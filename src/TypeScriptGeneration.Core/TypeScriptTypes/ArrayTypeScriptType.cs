namespace TypeScriptGeneration.TypeScriptTypes
{
    public class ArrayTypeScriptType : GenericTypeScriptType
    {
        public ArrayTypeScriptType(TypeScriptType itemTypeScriptType) : base(new BuiltInTypeScriptType("Array"), itemTypeScriptType)
        {
        }
    }
}