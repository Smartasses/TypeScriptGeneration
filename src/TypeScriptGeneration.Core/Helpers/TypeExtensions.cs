using System;

namespace TypeScriptGeneration
{
    public static class TypeExtensions
    {
        public static string GetCleanName(this Type type)
        {
            return type.Name.Split('`')[0];
        }
    }
}