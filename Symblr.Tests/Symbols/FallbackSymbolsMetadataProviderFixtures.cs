using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Symblr.Symbols
{
    public class FallbackSymbolsMetadataProviderFixtures
    {
        [Fact]
        public async Task When_getting_the_metadata_for_unsupported_data()
        {
            var sut = new FallbackSymbolsMetadataProvider();

            var stream = new TestStream(MZ.Exes.NuGet);
            using (var file = await sut.TryGetSymbolMetadataAsync(stream, CancellationToken.None))
            {
                Assert.Equal("7f71c74f53a9138b31495107e33077d1", file.Identifier);
                Assert.False(file.SupportsSourceServerInformation);
            }
        }
    }
}
