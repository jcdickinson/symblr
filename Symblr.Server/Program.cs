
using Symblr.Symbols;
using System.IO;
namespace Symblr
{
    class Program
    {
        static void Main(string[] args)
        {
            var md = new Pdb70SymbolMetadataProvider();
            using (var fs = File.Open(@"E:\DeletedBitSet.pdb", FileMode.Open))
            {
                var mt = md.TryGetSymbolMetadataAsync(fs).Result;
                mt.SaveAsync().Wait();
            }
        }
    }
}

