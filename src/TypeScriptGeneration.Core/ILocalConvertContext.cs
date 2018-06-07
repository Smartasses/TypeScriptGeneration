using System;
using System.Collections.Generic;
using TypeScriptGeneration.TypeScriptTypes;

namespace TypeScriptGeneration
{
    public interface ILocalConvertContext
    {
        TypeScriptType GetTypeScriptType(Type t);
        ConvertConfiguration Configuration { get; }
        Dictionary<TypeScriptType[], string> ExternalImports { get; }
    }
}