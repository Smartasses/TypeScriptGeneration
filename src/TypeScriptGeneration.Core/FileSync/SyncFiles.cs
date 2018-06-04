using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TypeScriptGeneration.FileSync
{

    public class SyncFiles
    {

        public async Task DoSync(string path, IEnumerable<KeyValuePair<string, string>> files)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            
            var allFiles = files
                .GroupBy(x => Path.GetFullPath(Path.Combine(path, x.Key)), x => x.Value)
                .ToArray();

            var duplicates = allFiles.Where(x => x.Count() > 1).ToArray();
            
            var dict = allFiles.ToDictionary(x => x.Key, x => x.First());
            
            RemoveUnusedFilesAndFolders(path, dict);
            CreateDirectories(dict);

            foreach (var file in dict.Select(x => new {FilePath = x.Key, Content = x.Value, Hash = GetHash(x.Value)}))
            {
                var hashComment = $"// hash: {file.Hash}";
                bool needsWrite = false;
                await RetryTimes(async () => {
                    using (var strm = new FileStream(file.FilePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
                    using (var fileReader = new StreamReader(strm))
                    {
                        var hash = (await fileReader.ReadLineAsync())?.Trim();
                        needsWrite = hash != hashComment;
                    }
                    if (needsWrite)
                    {
                        Console.WriteLine("Writing file: " + file.FilePath);
                        await RetryTimes(async () => {
                            using (var strm = new FileStream(file.FilePath, FileMode.Create, FileAccess.Write,
                                FileShare.Write))
                            using (var fileWriter = new StreamWriter(strm))
                            {
                                await fileWriter.WriteLineAsync(hashComment);
                                await fileWriter.WriteAsync(file.Content);
                                await fileWriter.FlushAsync();
                            }
                        }, 3);
                    }
                    else
                    {
                        Console.WriteLine("Skip file: " + file.FilePath);
                    }
                }, 3);
            }
        }

        private async Task RetryTimes(Func<Task> action, int times)
        {
            var tryCount = 0;
            bool succeeded = false;
            do
            {
                try
                {
                    await action();
                    succeeded = true;
                }
                catch
                {
                    tryCount++;
                    await Task.Delay(tryCount * 50 + 50);
                }
            } while (!succeeded && times > tryCount);
        }
        
        private string GetHash(string content)
        {
            var messageBytes = Encoding.UTF8.GetBytes(content);
#if NET45
            var sha1Managed = new SHA1Managed();
            var hash = sha1Managed.ComputeHash(messageBytes);
#else
            var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(messageBytes);
#endif
            return Convert.ToBase64String(hash);
        }

        private static void CreateDirectories(Dictionary<string, string> dict)
        {
            var dirHash = new HashSet<string>();
            foreach (var file in dict)
            {
                var dir = Path.GetDirectoryName(file.Key);
                if (!dirHash.Contains(dir))
                {
                    dirHash.Add(dir);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }
            }
        }

        private static void RemoveUnusedFilesAndFolders(string path, Dictionary<string, string> dict)
        {
            Directory.GetFileSystemEntries(path, "*.ts", SearchOption.AllDirectories)
                .Where(x => !dict.ContainsKey(Path.GetFullPath(x)))
                .ToList()
                .ForEach(File.Delete);
            
            Directory.GetDirectories(path, "*", SearchOption.AllDirectories)
                .Where(x => Directory.GetFiles(x).Length == 0)
                .ToList()
                .ForEach(
                    x =>
                    {
                        if (Directory.Exists(x))
                            Directory.Delete(x, true);
                    });
        }
    }
}