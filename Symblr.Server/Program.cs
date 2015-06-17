
using Symblr.Symbols;
using System.IO;
using System.Text;
namespace Symblr
{
    class Program
    {
        static void Main(string[] args)
        {
            var md = new Pdb70SymbolMetadataProvider();
            var mz = new MZSymbolMetadataProvider();
            
            using (var fs = File.OpenRead(@"E:\gh\symblr\Symblr.Server\bin\Debug\symblr.server.exe"))
            {
                var mt = mz.TryGetSymbolMetadataAsync(fs).Result;
            }
        }
    }
}

