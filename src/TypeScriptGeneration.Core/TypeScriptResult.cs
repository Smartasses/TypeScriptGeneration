using System;
using System.Collections.Generic;
using TypeScriptGeneration.TypeScriptTypes;

namespace TypeScriptGeneration
{
    public class TypeScriptResult
    {
        public Type Type { get; set; }
        public ICollection<TypeScriptImport> Imports { get; set; }
        public string Content { get; set; }
        public string FilePath { get; set; }
        public Dictionary<TypeScriptType[], string> ExternalImports { get; set; }
    }
}