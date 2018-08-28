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
        public InheritanceDiscriminatorConfiguration Add<TBaseType>(Expression<Func<TBaseType, object>> discriminator,
            params Type[] subTypes)
        {
            var propertyInfo = GetPropertyFromExpression(discriminator);
            var baseType = typeof(TBaseType);
            return Add(baseType, propertyInfo, subTypes);
        }

        public bool TryGetValue(Type type, out SubTypesAndDiscriminator result)
            => _config.TryGetValue(type, out result);


        private InheritanceDiscriminatorConfiguration Add(Type baseType, PropertyInfo propertyInfo, Type[] subTypes)
        {
            TryGetDiscriminatorValue(baseType, propertyInfo, out var rootValue);
            var subTypesAndDiscriminator = new SubTypesAndDiscriminator(propertyInfo, rootValue);

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
                Add(subType, propertyInfo, subTypes);
            }

            _config.Add(baseType, subTypesAndDiscriminator);
            return this;
        }

        private static bool TryGetDiscriminatorValue(Type subType, PropertyInfo methodInfo, out object value)
        {
            if (subType.IsClass && !subType.IsAbstract)
            {
                var instance = Activator.CreateInstance(subType);
                value = methodInfo.GetValue(instance);
                return true;
            }

            value = null;
            return false;
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