namespace TypeScriptGeneration.TypeScriptTypes
{
    public class BuiltInTypeScriptType : TypeScriptType
    {
        private readonly string _name;

        public BuiltInTypeScriptType(string name)
        {
            _name = name;
        }

        public override string ToTypeScriptType()
        {
            return _name;
        }
    }
}