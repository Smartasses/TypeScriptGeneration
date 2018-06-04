using System.Linq;

namespace TypeScriptGeneration.TypeScriptTypes
{
    public class GenericTypeScriptType : TypeScriptType
    {
        private TypeScriptType _typeScriptType;
        private TypeScriptType[] _genericArguments;

        public GenericTypeScriptType(TypeScriptType typeScriptType, params TypeScriptType[] genericArguments)
        {
            _typeScriptType = typeScriptType;
            _genericArguments = genericArguments;
        }

        public override string ToTypeScriptType()
        {
            return $"{_typeScriptType.ToTypeScriptType()}<{string.Join(", ", _genericArguments.Select(x => x.ToTypeScriptType()))}>";
        }
    }
}