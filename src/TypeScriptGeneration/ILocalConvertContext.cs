using System;
using TypeScriptGeneration.TypeScriptTypes;

namespace TypeScriptGeneration
{
    public interface ILocalConvertContext
    {
        TypeScriptType GetTypeScriptType(Type t);
        ConvertConfiguration Configuration { get; }
    }
}