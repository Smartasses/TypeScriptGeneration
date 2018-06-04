using System;
using System.Collections.Generic;

namespace TypeScriptGeneration
{
    public class TypeScriptResult
    {
        public Type Type { get; set; }
        public ICollection<TypeScriptResult> Imports { get; set; }
        public string Content { get; set; }
        public string FilePath { get; set; }
    }
}