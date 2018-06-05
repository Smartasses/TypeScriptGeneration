using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TypeScriptGeneration.TypeMapping
{
    static class TypeHelper
    {
        public static Type GetTaskType(Type actualType)
        {
            return actualType.GetTypeInfo().IsGenericType && actualType.GetGenericTypeDefinition() == typeof(Task<>) ? actualType :
                actualType.GetTypeInfo().GetInterfaces().FirstOrDefault(t => IntrospectionExtensions.GetTypeInfo(t).IsGenericType && t.GetGenericTypeDefinition() == typeof(Task<>));
        }
        public static Type GetEnumerableType(Type actualType)
        {
            return actualType.GetTypeInfo().IsGenericType && actualType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ? actualType :
                actualType.GetTypeInfo().GetInterfaces().FirstOrDefault(t => IntrospectionExtensions.GetTypeInfo(t).IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }
        public static Type GetDictionaryType(Type actualType)
        {
            return actualType.GetTypeInfo().IsGenericType && actualType.GetGenericTypeDefinition() == typeof(IDictionary<,>) ? actualType :
                actualType.GetTypeInfo().GetInterfaces().FirstOrDefault(t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }
    }
}