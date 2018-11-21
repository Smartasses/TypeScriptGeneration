using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TypeScriptGeneration
{
    public class InheritanceDiscriminatorConfiguration
    {
        internal readonly Dictionary<Type, SubTypesAndDiscriminator> _config;

        public InheritanceDiscriminatorConfiguration()
        {
            _config = new Dictionary<Type, SubTypesAndDiscriminator>();
        }

        public InheritanceDiscriminatorConfiguration AddWithStaticTypeProperty<TBaseType>(
            Expression<Func<TBaseType, object>> discriminator,
            params Type[] subTypes) => InternalAdd(discriminator, true, subTypes);
        public InheritanceDiscriminatorConfiguration Add<TBaseType>(
            Expression<Func<TBaseType, object>> discriminator,
            params Type[] subTypes) => InternalAdd(discriminator, false, subTypes);
        
        private InheritanceDiscriminatorConfiguration InternalAdd<TBaseType>(Expression<Func<TBaseType, object>> discriminator, bool staticTypeProperty,
            params Type[] subTypes)
        {
            var propertyInfo = GetPropertyFromExpression(discriminator);
            var baseType = typeof(TBaseType);
            return Add(baseType, propertyInfo, subTypes, staticTypeProperty);
        }

        public bool TryGetValue(Type type, out SubTypesAndDiscriminator result)
            => _config.TryGetValue(type, out result);


        private InheritanceDiscriminatorConfiguration Add(Type baseType, PropertyInfo propertyInfo, Type[] subTypes, bool addStaticTypeProperty)
        {
            TryGetDiscriminatorValue(baseType, propertyInfo, out var rootValue);
            var subTypesAndDiscriminator = new SubTypesAndDiscriminator(propertyInfo, rootValue, addStaticTypeProperty);

            foreach (var subType in subTypes.Where(x => x != baseType && baseType.IsAssignableFrom(x)))
            {
                if (TryGetDiscriminatorValue(subType, propertyInfo, out var discriminatorValue))
                {
                    if (subTypesAndDiscriminator.SubTypesWithDiscriminatorValue.Values.Contains(discriminatorValue))
                    {
                        throw new ArgumentException(
                            $"DiscriminatorValue already exists: type: {subType}, value: {discriminatorValue}");
                    }

                    subTypesAndDiscriminator.SubTypesWithDiscriminatorValue.Add(subType, discriminatorValue);
                }
                Add(subType, propertyInfo, subTypes, addStaticTypeProperty);
            }

            _config.Add(baseType, subTypesAndDiscriminator);
            return this;
        }

        private static bool TryGetDiscriminatorValue(Type subType, PropertyInfo methodInfo, out object value)
        {
            if (subType.IsClass && !subType.IsAbstract)
            {
                try
                {
                    var instance = CreateInstance(subType);
                    value = methodInfo.GetValue(instance);
                    return true;
                }
                catch (Exception ex)
                {
                    var message = $"Could not create a new instance of type '{subType.Name}'. Make sure it has a parameterless constructor (doesn't need to be public) or it has only one constructor with parameters that accepts default values.";
                    throw new MissingMethodException(message, ex);
                }
            }

            value = null;
            return false;
        }

        private static object CreateInstance(Type type)
        {
            var constructor = GetConstructor(type);

            try
            {
                var constructorParams = constructor.GetParameters().Select(p => GetParameterValue(p));
                var instance = constructor.Invoke(constructorParams.ToArray());
                return instance;
            }
            catch (Exception ex)
            {
                var message = $"Could not create a new instance of type '{type.Name}'. Make sure it has a parameterless constructor (doesn't need to be public) or it has only one constructor with parameters that accepts default values.";
                throw new MissingMethodException(message, ex);
            }
        }

        private static ConstructorInfo GetConstructor(Type type)
        {
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var constructor = constructors.FirstOrDefault(x => x.GetParameters().Length == 0);
            if (constructor == null)
            {
                if (constructors.Length != 1)
                {
                    var message = $"Could not find a suitable constructor for type '{type.Name}'. Make sure it has a parameterless constructor (doesn't need to be public) or it has only one constructor with parameters that accepts default values.";
                    throw new MissingMethodException(message);
                }

                constructor = constructors[0];
            }

            return constructor;
        }

        private static object GetParameterValue(ParameterInfo parameter)
        {
            if (parameter.HasDefaultValue)
                return parameter.DefaultValue;
            if (parameter.ParameterType.IsValueType && Nullable.GetUnderlyingType(parameter.ParameterType) == null)
                return Activator.CreateInstance(parameter.ParameterType);
            return null;
        }

        private PropertyInfo GetPropertyFromExpression<T>(Expression<Func<T, object>> expression)
        {
            MemberExpression resultExpression;

            switch (expression.Body)
            {
                case UnaryExpression unaryExpression when unaryExpression.Operand is MemberExpression memberExpression:
                    resultExpression = memberExpression;
                    break;
                case MemberExpression memberExpression:
                    resultExpression = memberExpression;
                    break;
                default:
                    throw new ArgumentException();
            }

            return (PropertyInfo)resultExpression.Member;
        }
    }
}