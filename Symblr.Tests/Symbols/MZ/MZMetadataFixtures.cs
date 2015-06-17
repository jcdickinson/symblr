using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Symblr.Symbols.MZ
{
    public class MZMetadataFixtures
    {
        [Fact]
        public async Task When_reading_NuGet_exe()
        {
            var sut = new MZSymbolMetadataProvider();
            // Authoritative identifiers can be calculated with:
            // "C:\Program Files (x86)\Windows Kits\8.1\Debuggers\x64\symstore.exe" add /f NuGet.exe /s "C:\symbols" /t symblr
            // Make sure the file is larger than 32k.

            var stream = new TestStream(Exes.NuGet);
            using (var file = await sut.TryGetSymbolMetadataAsync(stream, CancellationToken.None))
            {
                Assert.Equal("550953A8c000", file.Identifier);
                Assert.False(file.SupportsSourceServerInformation);
            }
        }
    }
}
