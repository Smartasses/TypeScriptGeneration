using System.Threading.Tasks;
using TypeScriptGeneration.FileSync;

namespace TypeScriptGeneration
{
    public static class ContextExtensions
    {
        public static async Task WriteFiles(this ConvertContext context, string outputFolder)
        {
            await new SyncFiles().DoSync(outputFolder, context.GetFiles());
        }
    }
}