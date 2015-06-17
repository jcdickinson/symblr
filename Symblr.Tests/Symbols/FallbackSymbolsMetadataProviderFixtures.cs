using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Symblr.Symbols
{
    [TestClass]
    public class FallbackSymbolsMetadataProviderFixtures
    {
        [TestMethod]
        public async Task When_getting_the_metadata_for_unsupported_data()
        {
            var sut = new FallbackSymbolsMetadataProvider();

            var stream = new TestStream(MZ.Exes.NuGet);
            using (var file = await sut.TryGetSymbolMetadataAsync(stream, CancellationToken.None))
            {
                Assert.AreEqual("7f71c74f53a9138b31495107e33077d1", file.Identifier, "it should return the correct identifier.");
                Assert.AreEqual(false, file.SupportsSourceServerInformation, "it should indicate no support for source server information.");
            }
        }
    }
}
