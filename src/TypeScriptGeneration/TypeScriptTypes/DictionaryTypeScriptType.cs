using System;

namespace TypeScriptGeneration.TypeScriptTypes
{
    public class DictionaryTypeScriptType : TypeScriptType
    {
        private readonly TypeScriptType _key;
        private readonly TypeScriptType _value;

        public DictionaryTypeScriptType(TypeScriptType key, TypeScriptType value)
        {
            _key = key;
            _value = value;
            if (!Equals(key, TypeScriptType.String))
            {
                throw new Exception("TODO");
            }
        }
        public override string ToTypeScriptType()
        {
            return $"{{ [key: {_key.ToTypeScriptType()}]: {_value.ToTypeScriptType()} }}";
        }
    }
}