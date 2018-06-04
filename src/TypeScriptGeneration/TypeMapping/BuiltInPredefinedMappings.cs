using System;
using System.Collections.Generic;
using TypeScriptGeneration.TypeScriptTypes;

namespace TypeScriptGeneration.TypeMapping
{
    public class BuiltInPredefinedMappings : IPredefinedMapping
    {
        private readonly IReadOnlyDictionary<Type, TypeScriptType> _mappings;
        public BuiltInPredefinedMappings()
        {
            _mappings = new Dictionary<Type, TypeScriptType>
            {
                // Number types
                { typeof(byte), TypeScriptType.Number },
                { typeof(int), TypeScriptType.Number },
                { typeof(uint), TypeScriptType.Number },
                { typeof(short), TypeScriptType.Number },
                { typeof(ushort), TypeScriptType.Number },
                { typeof(long), TypeScriptType.Number },
                { typeof(ulong), TypeScriptType.Number },
                { typeof(float), TypeScriptType.Number },
                { typeof(decimal), TypeScriptType.Number },
                
                // String types
                { typeof(string), TypeScriptType.String },
                { typeof(char), TypeScriptType.String },
                { typeof(Guid), TypeScriptType.String },
                
                // Boolean types
                { typeof(Boolean), TypeScriptType.Boolean },
                
                // Date types
                { typeof(DateTime), TypeScriptType.Date },
                
                // Any types
                { typeof(object), TypeScriptType.Any }
            };
        }

        public bool IsPredefined(Type input, out TypeScriptType typeScriptType)
            =>_mappings.TryGetValue(input, out typeScriptType);
    }
}