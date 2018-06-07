using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeScriptGeneration
{
    public class ConvertContext : IConvertContext
    {
        private readonly Dictionary<Type, TypeScriptResult> _generatedTypes;

        public ConvertContext()
        {
            _generatedTypes = new Dictionary<Type, TypeScriptResult>();
            
            Configuration = new ConvertConfiguration();
        }
        
        public ConvertConfiguration Configuration { get; }

        public void GenerateForTypes(params Type[] types)
        {
            var abuseLocalContext = new LocalContext(Configuration, this, typeof(object));
            foreach (var type in types)
            {
                abuseLocalContext.GetTypeScriptType(type);
            }
        }
        
        TypeScriptResult IConvertContext.GetTypeScriptFile(Type type)
        {
            if (!_generatedTypes.TryGetValue(type, out var result))
            {
                var localContext = new LocalContext(Configuration, this, type);
                var directory = Configuration.GetFileDirectory(type).Replace("\\", "/").TrimEnd('/') + "/";
                if (!directory.StartsWith("/"))
                {
                    throw new Exception("GetFileDirectory should always start with a /");
                }
                
                var fileName = Configuration.GetFileName(type);
                var originalFileName = fileName;
                var usedFileNames = _generatedTypes.ToDictionary(x => x.Value.FilePath);
                for (var tryCount = 1; usedFileNames.ContainsKey(directory + fileName); tryCount++)
                {
                    fileName = originalFileName + "_" + ++tryCount;                        
                }
                result = new TypeScriptResult
                {
                    Type = type,
                    FilePath = directory + fileName
                };
                _generatedTypes.Add(type, result);
                try
                {
                    var converter = Configuration.Converters.First(x => x.CanConvertType(type));
                    var ts = converter.ConvertType(type, localContext);
                    result.Imports = localContext.Imports.Values;
                    result.ExternalImports = localContext.ExternalImports;
                    result.Content = ts.Trim();
                }
                catch
                {
                    throw new Exception($"Failed to convert {type}");
                }
            }
            return result;
        }

        public TypeScriptResult[] GeneratedResults => _generatedTypes.Values.ToArray();

        public IEnumerable<KeyValuePair<string, string>> GetFiles()
        {
            foreach (var typeScriptResult in GeneratedResults)
            {
                var sb = new StringBuilder();
                if (typeScriptResult.Imports.Any() || typeScriptResult.ExternalImports.Any())
                {
                    foreach (var import in typeScriptResult.ExternalImports)
                    {
                        sb.AppendLine($"import {{ {string.Join(", ", import.Key.Select(x => x.ToTypeScriptType()))} }} from '{import.Value}';");
                    }
                    foreach (var import in typeScriptResult.Imports)
                    {
                        var relativePath = GetRelativePath(typeScriptResult.FilePath, import.FilePath);
                        
                        sb.AppendLine($"import {{ {import.Type.GetCleanName()} }} from '{relativePath}';");
                    }
                    sb.AppendLine();
                }
                sb.AppendLine(typeScriptResult.Content);
                yield return new KeyValuePair<string, string>(typeScriptResult.FilePath.Substring(1) + ".ts", sb.ToString());
            }
        }

        private string GetRelativePath(string filePath, string importFilePath)
        {
            var current = new Uri("http://root" + filePath);
            var target = new Uri("http://root" + importFilePath);
            var result = current.MakeRelativeUri(target);
            var asString = Uri.UnescapeDataString(result.ToString());
            if (!asString.StartsWith("../"))
            {
                asString = "./" + asString;
            }
            return asString;
        }
    }
}