using System;
using System.Collections.Generic;
using TypeScriptGeneration.TypeScriptTypes;

namespace TypeScriptGeneration
{
    public interface ILocalConvertContext
    {
        TypeScriptType GetTypeScriptType(Type t, bool convert = true);
        ConvertConfiguration Configuration { get; }
        Dictionary<Type, TypeScriptImport> Imports { get; }
        Dictionary<TypeScriptType[], string> ExternalImports { get; }
    }
}