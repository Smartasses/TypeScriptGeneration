using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TypeScriptGeneration.TypeMapping;
using TypeScriptGeneration.TypeScriptTypes;

namespace TypeScriptGeneration
{
    public class LocalContext : ILocalConvertContext
    {
        public ConvertConfiguration Configuration { get; }
        private readonly IConvertContext _convertContext;
        private readonly Type _type;

        public LocalContext(ConvertConfiguration configuration, IConvertContext convertContext, Type type)
        {
            Configuration = configuration;
            _convertContext = convertContext;
            _type = type;
            Imports = new Dictionary<Type, TypeScriptResult>();
        }
        
        public Dictionary<Type, TypeScriptResult> Imports { get; }
        
        public TypeScriptType GetTypeScriptType(Type type)
        {
            {
                if (!Configuration.ShouldConvertType(type))
                {
                    return TypeScriptType.Any;
                }
            }
            
            {
                if (Configuration.PredefinedMapping.IsPredefined(type, out var result))
                {
                    return result;
                }                
            }
                
            var actualType = Nullable.GetUnderlyingType(type) ?? type;
    
            {
                if (Configuration.PredefinedMapping.IsPredefined(actualType, out var result))
                {
                    return result;
                }                
            }
    
            {
                var dictionary = TypeHelper.GetDictionaryType(actualType);
                if (dictionary != null)
                {
                    return TypeScriptType.Dictionary(GetTypeScriptType(dictionary.GetTypeInfo().GetGenericArguments()[0]), GetTypeScriptType(dictionary.GetTypeInfo().GetGenericArguments()[1]));
                }                
            }
    
            {
                var enumerable = TypeHelper.GetEnumerableType(actualType);
                if (enumerable != null)
                {
                    return TypeScriptType.Array(GetTypeScriptType(enumerable.GetTypeInfo().GetGenericArguments()[0]));
                }
            }
    
            {
                var taskType = TypeHelper.GetTaskType(actualType);
                if (taskType != null)
                {
                    return GetTypeScriptType(taskType.GetTypeInfo().GetGenericArguments()[0]);
                }
            }

            {
                if (actualType.IsConstructedGenericType)
                {
                    var typedefinition = actualType.GetGenericTypeDefinition();
                    var typeDefinition = GetTypeScriptType(typedefinition);
                    var genericArguments = actualType.GetTypeInfo().GetGenericArguments();
                    return TypeScriptType.Generic(GetTypeScriptType(typedefinition), genericArguments.Select(GetTypeScriptType).ToArray());
                }
            }

            {
                if (actualType.IsGenericParameter)
                {
                    return new BuiltInTypeScriptType(actualType.Name);
                }
            }

            {
                if (!Imports.TryGetValue(actualType, out var result))
                {
                    Imports.Add(actualType, _convertContext.GetTypeScriptFile(actualType));
                }
                return new BuiltInTypeScriptType(Configuration.GetTypeName(actualType));
            }
        }
    }
}