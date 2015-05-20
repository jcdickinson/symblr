using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Symblr.IO
{
    [TestClass]
    public class AsyncBinaryWriterFixture
    {
        [TestMethod, TestCategory("IO")]
        public async Task When_writing_an_Int32()
        {
            var ms = new MemoryStream();
            var sut = new AsyncBinaryWriter(ms);
            await sut.WriteAsync(100, CancellationToken.None);
            var arr = ms.ToArray();

            ArrayAssert.All(new byte[] { 100, 0, 0, 0 }, arr, (e, a) => Assert.AreEqual(e, a, "it should have the correct bytes."));
        }

        [TestMethod, TestCategory("IO")]
        public async Task When_writing_a_structure()
        {
            var ms = new MemoryStream();
            var sut = new AsyncBinaryWriter(ms);

            await sut.WriteStructureAsync(new Symblr.Symbols.Pdb20.Pdb20Header()
            {
                PageSize = 0,
                BitmapPage = 1,
                PageCount = 2,
                IndexBytes = 3,
                Reserved = 4,
                IndexPage = 5
            }, CancellationToken.None);

            var arr = ms.ToArray();
            ArrayAssert.All(new byte[] { 
                0, 0, 0, 0, 
                1, 0, 0, 0, 
                2, 0, 0, 0, 
                3, 0, 0, 0, 
                4, 0, 0, 0, 
                5, 0, 0, 0,
            }, arr, (e, a) => Assert.AreEqual(e, a, "it should have the correct bytes."));
        }
    }
}
