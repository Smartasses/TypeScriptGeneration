using System;

namespace TypeScriptGeneration
{
    public interface IConvertContext
    {
        TypeScriptResult GetTypeScriptFile(Type type);
    }
}