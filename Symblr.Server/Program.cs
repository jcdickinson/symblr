
using Symblr.Symbols;
using System.IO;
namespace Symblr
{
    class Program
    {
        static void Main(string[] args)
        {
            var md = new Pdb70SymbolMetadataProvider();
            using(var fs = File.Open(@"C:\dbg\symbols\MicrosoftPublicSymbols\System.Windows.Forms.pdb\3E99F0309A1943249FC6430BCE0ACD041\System.Windows.Forms.pdb", FileMode.Open))
            {
                var mt = md.TryGetSymbolMetadataAsync(fs).Result;

            }
        }
    }
}

