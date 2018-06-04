using System;
using System.Collections.Generic;
using System.Text;

namespace TypeScriptGeneration
{
    public static class _
    {
        public static string Foreach<T>(IEnumerable<T> source, Func<T, string> format)
        {
            var sb = new StringBuilder();
            foreach (var item in source)
            {
                sb.Append(format(item));
            }
            return sb.ToString();
        }
    }
}