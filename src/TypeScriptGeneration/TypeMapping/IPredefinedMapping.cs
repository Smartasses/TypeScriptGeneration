using System;
using TypeScriptGeneration.TypeScriptTypes;

namespace TypeScriptGeneration.TypeMapping
{
    public interface IPredefinedMapping
    {
        bool IsPredefined(Type input, out TypeScriptType typeScriptType);
    }
}