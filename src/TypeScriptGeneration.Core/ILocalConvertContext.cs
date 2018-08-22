using System;
using System.Collections.Generic;
using TypeScriptGeneration.TypeScriptTypes;

namespace TypeScriptGeneration
{
    public interface ILocalConvertContext
    {
        TypeScriptType GetTypeScriptType(Type t);
        ConvertConfiguration Configuration { get; }
        Dictionary<Type, TypeScriptResult> Imports { get; }
        Dictionary<TypeScriptType[], string> ExternalImports { get; }
    }
}