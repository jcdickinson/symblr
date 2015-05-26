using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Symblr.Symbols.Pdb70;

namespace Symblr.Symbols
{
    [TestClass]
    public class Pdb70SymbolMetadataProviderFixtures
    {
        [TestMethod, TestCategory("Symbols")]
        public async Task When_reading_a_file_without_source_server_information()
        {
            var sut = new Pdb70SymbolMetadataProvider();

            var ms = new MemoryStream();
            var stream = new TestStream(ms);
            await stream.WriteAsync(Pdbs.NoSrcSrv, 0, Pdbs.NoSrcSrv.Length);
            stream.Position = 0;

            using (var metadata = await sut.TryGetSymbolMetadataAsync(stream))
            {
                Assert.IsNotNull(metadata, "it should read the metadata");
                Assert.IsTrue(metadata.SupportsSourceServerInformation, "the provider should advertise support for source server information.");
                Assert.AreEqual("63ba9bb5992dfc429f6bcc52135dbb091", metadata.Identifier, "it should read the identifier correctly.");
                Assert.IsFalse(metadata.HasSourceServerInformation, "it should indicate that no source information is available.");
                Assert.AreEqual(6, metadata.SourceInformation.Count, "it should read all the source files.");
                ArrayAssert.AllAreFalse(metadata.SourceInformation, x => !string.IsNullOrEmpty(x.TargetPath), "it should have no files with source.");
            }
        }

        [TestMethod, TestCategory("Symbols")]
        public async Task When_reading_a_file_with_source_server_information()
        {
            var sut = new Pdb70SymbolMetadataProvider();

            var ms = new MemoryStream();
            var stream = new TestStream(ms);
            await stream.WriteAsync(Pdbs.RealSrcSrv, 0, Pdbs.RealSrcSrv.Length);
            stream.Position = 0;

            using (var metadata = await sut.TryGetSymbolMetadataAsync(stream))
            {
                Assert.IsNotNull(metadata, "it should read the metadata");
                Assert.IsTrue(metadata.SupportsSourceServerInformation, "the provider should advertise support for source server information.");
                Assert.AreEqual("63ba9bb5992dfc429f6bcc52135dbb092", metadata.Identifier, "it should read the identifier correctly.");
                Assert.IsTrue(metadata.HasSourceServerInformation, "it should indicate that source information is available.");
                Assert.AreEqual(581, metadata.SourceInformation.Count, "it should read all the source files.");
                ArrayAssert.AnyIsTrue(metadata.SourceInformation, x => !string.IsNullOrEmpty(x.TargetPath), "it should have files with source.");
            }
        }

        [TestMethod, TestCategory("Symbols")]
        public async Task When_reading_a_non_PDB()
        {
            var sut = new Pdb70SymbolMetadataProvider();
            
            var stream = new TestStream(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);

            using (var metadata = await sut.TryGetSymbolMetadataAsync(stream))
            {
                Assert.IsNull(metadata, "it should return a null value.");
            }
        }
    }
}
