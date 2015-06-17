using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Symblr.IO
{
    public class AsyncBinaryReaderFixtures
    {
        [Fact]
        public async Task When_reading_bytes()
        {
            var sut = new AsyncBinaryReader(new TestStream(
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9
            ));

            var ba = await sut.ReadBytesAsync(10, CancellationToken.None);
            Assert.Equal(10, ba.Length);
            for (var i = 0; i < 10; i++)
                Assert.Equal(i, ba[i]);
        }

        [Fact]
        public async Task When_reading_bytes_from_a_stream_that_is_too_short()
        {
            var sut = new AsyncBinaryReader(new TestStream(
                0, 1, 2, 3, 4, 5
            ));

            await Assert.ThrowsAsync<EndOfStreamException>(async () =>
            {
                await sut.ReadBytesAsync(10, CancellationToken.None);
            });
        }

        [Fact]
        public async Task When_reading_negative_amounts_of_bytes()
        {
            var sut = new AsyncBinaryReader(new TestStream(
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9
            ));


            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                var ba = await sut.ReadBytesAsync(-1, CancellationToken.None);
            });
        }

        [Fact]
        public async Task When_reading_a_structure()
        {
            var sut = new AsyncBinaryReader(new TestStream(
                0, 0, 0, 0,
                1, 0, 0, 0,
                2, 0, 0, 0,
                3, 0, 0, 0,
                4, 0, 0, 0,
                5, 0, 0, 0
            ));

            var str = await sut.ReadStructureAsync<Symblr.Symbols.Pdb70.Pdb70Header>(CancellationToken.None);
            Assert.Equal(0, str.PageSize);
            Assert.Equal(1, str.BitmapPage);
            Assert.Equal(2, str.PageCount);
            Assert.Equal(3, str.IndexBytes);
            Assert.Equal(4, str.Reserved);
            Assert.Equal(5, str.IndexPage);
        }

        [Fact]
        public async Task When_reading_a_structure_from_a_stream_that_is_too_short()
        {
            var sut = new AsyncBinaryReader(new TestStream(
                0, 0, 0, 0,
                1, 0, 0, 0,
                2, 0, 0, 0,
                3, 0, 0, 0,
                4, 0, 0, 0
            ));

            await Assert.ThrowsAsync<EndOfStreamException>(async () =>
            {
                await sut.ReadStructureAsync<Symblr.Symbols.Pdb70.Pdb70Header>(CancellationToken.None);
            });
        }

        [Fact]
        public async Task When_reading_an_Int32()
        {
            var sut = new AsyncBinaryReader(new TestStream(
                1, 0, 0, 0
            ));

            var i = await sut.ReadInt32Async(CancellationToken.None);
            Assert.Equal(1, i);
        }

        [Fact]
        public async Task When_reading_an_Int32_from_a_stream_that_is_too_short()
        {
            var sut = new AsyncBinaryReader(new TestStream(
                1, 0, 0
            ));

            await Assert.ThrowsAsync<EndOfStreamException>(async () =>
            {
                await sut.ReadInt32Async(CancellationToken.None);
            });
        }

        [Fact]
        public async Task When_reading_an_UInt32()
        {
            var sut = new AsyncBinaryReader(new TestStream(
                1, 0, 0, 0
            ));

            var i = await sut.ReadInt32Async(CancellationToken.None);
            Assert.Equal(1, i);
        }

        [Fact]
        public async Task When_reading_an_UInt32_from_a_stream_that_is_too_short()
        {
            var sut = new AsyncBinaryReader(new TestStream(
                1, 0, 0
            ));

            await Assert.ThrowsAsync<EndOfStreamException>(async () =>
            {
                await sut.ReadUInt32Async(CancellationToken.None);
            });
        }

        [Fact]
        public async Task When_reading_an_UInt16()
        {
            var sut = new AsyncBinaryReader(new TestStream(
                1, 0
            ));

            var i = await sut.ReadInt16Async(CancellationToken.None);
            Assert.Equal(1, i);
        }

        [Fact]
        public async Task When_reading_an_UInt16_from_a_stream_that_is_too_short()
        {
            var sut = new AsyncBinaryReader(new TestStream(
                1
            ));

            await Assert.ThrowsAsync<EndOfStreamException>(async () =>
            {
                await sut.ReadInt16Async(CancellationToken.None);
            });
        }

        [Fact]
        public async Task When_reading_an_Int16()
        {
            var sut = new AsyncBinaryReader(new TestStream(
                1, 0
            ));

            var i = await sut.ReadInt16Async(CancellationToken.None);
            Assert.Equal(1, i);
        }

        [Fact]
        public async Task When_reading_an_Int16_from_a_stream_that_is_too_short()
        {
            var sut = new AsyncBinaryReader(new TestStream(
                1
            ));


            await Assert.ThrowsAsync<EndOfStreamException>(async () =>
            {
                await sut.ReadInt16Async(CancellationToken.None);
            });
        }
    }
}
