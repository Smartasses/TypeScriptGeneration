namespace TypeScriptGeneration.TypeScriptTypes
{
    public abstract class TypeScriptType
    {
        public static TypeScriptType String => new BuiltInTypeScriptType("string");
        public static TypeScriptType Number => new BuiltInTypeScriptType("number");
        public static TypeScriptType Date => new BuiltInTypeScriptType("Date");
        public static TypeScriptType Any => new BuiltInTypeScriptType("any");
        public static TypeScriptType Boolean => new BuiltInTypeScriptType("boolean");

        public static TypeScriptType Array(TypeScriptType typeScriptType) => new ArrayTypeScriptType(typeScriptType);
        public static TypeScriptType Generic(TypeScriptType typeScriptType, params TypeScriptType[] genericArguments) => new GenericTypeScriptType(typeScriptType, genericArguments);
        public static TypeScriptType Dictionary(TypeScriptType key, TypeScriptType value) => new DictionaryTypeScriptType(key, value);

        public abstract string ToTypeScriptType();

        public override bool Equals(object obj)
        {
            return this.ToTypeScriptType().Equals((obj as TypeScriptType)?.ToTypeScriptType());
        }

        public override int GetHashCode()
        {
            return ToTypeScriptType().GetHashCode();
        }
    }
}