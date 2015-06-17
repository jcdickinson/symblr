using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Symblr.Symbols.Pdb70;
using Xunit;

namespace Symblr.Symbols
{
    public class Pdb70SymbolMetadataProviderFixtures
    {
        [Fact]
        public async Task When_reading_a_file_without_source_server_information()
        {
            var sut = new Pdb70SymbolMetadataProvider();

            var ms = new MemoryStream();
            var stream = new TestStream(ms);
            await stream.WriteAsync(Pdbs.NoSrcSrv, 0, Pdbs.NoSrcSrv.Length);
            stream.Position = 0;

            using (var metadata = await sut.TryGetSymbolMetadataAsync(stream))
            {
                Assert.NotNull(metadata);
                Assert.True(metadata.SupportsSourceServerInformation);
                Assert.Equal("63ba9bb5992dfc429f6bcc52135dbb091", metadata.Identifier);
                Assert.False(metadata.HasSourceServerInformation);
                Assert.Equal(6, metadata.SourceInformation.Count);
                Assert.True(metadata.SourceInformation.Any(x => string.IsNullOrEmpty(x.TargetPath)));
            }
        }

        [Fact]
        public async Task When_reading_a_file_with_source_server_information()
        {
            var sut = new Pdb70SymbolMetadataProvider();

            var ms = new MemoryStream();
            var stream = new TestStream(ms);
            await stream.WriteAsync(Pdbs.RealSrcSrv, 0, Pdbs.RealSrcSrv.Length);
            stream.Position = 0;

            using (var metadata = await sut.TryGetSymbolMetadataAsync(stream))
            {
                Assert.NotNull(metadata);
                Assert.True(metadata.SupportsSourceServerInformation);
                Assert.Equal("63ba9bb5992dfc429f6bcc52135dbb092", metadata.Identifier);
                Assert.True(metadata.HasSourceServerInformation);
                Assert.Equal(581, metadata.SourceInformation.Count);
                Assert.False(metadata.SourceInformation.Any(x => string.IsNullOrEmpty(x.TargetPath)));
            }
        }

        [Fact]
        public async Task When_reading_a_non_PDB()
        {
            var sut = new Pdb70SymbolMetadataProvider();
            
            var stream = new TestStream(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);

            using (var metadata = await sut.TryGetSymbolMetadataAsync(stream))
            {
                Assert.Null(metadata);
            }
        }
    }
}
