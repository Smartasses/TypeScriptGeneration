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
            ExternalImports = new Dictionary<TypeScriptType[], string>();
        }
        
        public Dictionary<Type, TypeScriptResult> Imports { get; }
        public Dictionary<TypeScriptType[], string> ExternalImports { get; }
        
        public TypeScriptType GetTypeScriptType(Type type, bool import = true)
        {
            if (!Configuration.ShouldConvertType(type))
            {
                return TypeScriptType.Any;
            }
            
            if (Configuration.PredefinedMapping.IsPredefined(type, out var typeResult))
            {
                return typeResult;
            }
                
            var actualType = Nullable.GetUnderlyingType(type) ?? type;
    
            if (Configuration.PredefinedMapping.IsPredefined(actualType, out var actualResult))
            {
                return actualResult;
            }
    
            var dictionary = TypeHelper.GetDictionaryType(actualType);
            if (dictionary != null)
            {
                return TypeScriptType.Dictionary(GetTypeScriptType(dictionary.GetTypeInfo().GetGenericArguments()[0]), GetTypeScriptType(dictionary.GetTypeInfo().GetGenericArguments()[1]));
            }
    
            var enumerable = TypeHelper.GetEnumerableType(actualType);
            if (enumerable != null)
            {
                return TypeScriptType.Array(GetTypeScriptType(enumerable.GetTypeInfo().GetGenericArguments()[0]));
            }
    
            var taskType = TypeHelper.GetTaskType(actualType);
            if (taskType != null)
            {
                return GetTypeScriptType(taskType.GetTypeInfo().GetGenericArguments()[0]);
            }

            if (actualType.IsConstructedGenericType)
            {
                var typeDefinition = actualType.GetGenericTypeDefinition();
                var tsTypeDefinition = GetTypeScriptType(typeDefinition);
                var genericArguments = actualType.GetTypeInfo().GetGenericArguments();
                return TypeScriptType.Generic(tsTypeDefinition, genericArguments.Select(x => GetTypeScriptType(x)).ToArray());
            }

            if (actualType.IsGenericParameter)
            {
                return new BuiltInTypeScriptType(actualType.Name);
            }

            if (_type != type && !Imports.ContainsKey(actualType))
            {
                var typeScriptResult = _convertContext.GetTypeScriptFile(actualType);
                if (import)
                {
                    Imports.Add(actualType, typeScriptResult);
                }
            }
            
            return new BuiltInTypeScriptType(Configuration.GetTypeName(actualType));
        }
    }
}