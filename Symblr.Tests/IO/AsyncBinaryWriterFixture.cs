using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Symblr.IO
{
    public class AsyncBinaryWriterFixture
    {
        [Fact]
        public async Task When_writing_an_Int32()
        {
            var ms = new MemoryStream();
            var sut = new AsyncBinaryWriter(ms);
            await sut.WriteAsync(100, CancellationToken.None);
            Assert.Equal(new byte[] { 100, 0, 0, 0 }, ms.ToArray());
        }

        [Fact]
        public async Task When_writing_a_structure()
        {
            var ms = new MemoryStream();
            var sut = new AsyncBinaryWriter(ms);

            await sut.WriteStructureAsync(new Symblr.Symbols.Pdb70.Pdb70Header()
            {
                PageSize = 0,
                BitmapPage = 1,
                PageCount = 2,
                IndexBytes = 3,
                Reserved = 4,
                IndexPage = 5
            }, CancellationToken.None);

            Assert.Equal(new byte[] { 
                0, 0, 0, 0, 
                1, 0, 0, 0, 
                2, 0, 0, 0, 
                3, 0, 0, 0, 
                4, 0, 0, 0, 
                5, 0, 0, 0,
            }, ms.ToArray());
        }
    }
}
