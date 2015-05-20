using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Symblr.IO
{
    [TestClass]
    public class AsyncBinaryReaderFixtures
    {
        [TestMethod, TestCategory("IO")]
        public async Task When_reading_bytes()
        {
            var sut = new AsyncBinaryReader(new TestStream(
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9
            ));

            var ba = await sut.ReadBytesAsync(10, CancellationToken.None);
            Assert.AreEqual(10, ba.Length, "it should read all the bytes.");
            for (var i = 0; i < 10; i++)
                Assert.AreEqual(i, ba[i], "it should read the correct bytes.");
        }

        [TestMethod, TestCategory("IO")]
        [ExpectedException(typeof(EndOfStreamException), "it should throw an end-of-file exception.")]
        public async Task When_reading_bytes_from_a_stream_that_is_too_short()
        {
            var sut = new AsyncBinaryReader(new TestStream(
                0, 1, 2, 3, 4, 5
            ));

            await sut.ReadBytesAsync(10, CancellationToken.None);
        }

        [TestMethod, TestCategory("IO")]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "it should throw an argument out of range exception.")]
        public async Task When_reading_negative_amounts_of_bytes()
        {
            var sut = new AsyncBinaryReader(new TestStream(
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9
            ));

            var ba = await sut.ReadBytesAsync(-1, CancellationToken.None);
        }

        [TestMethod, TestCategory("IO")]
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

            var str = await sut.ReadStructureAsync<Symblr.Symbols.Pdb20.Pdb20Header>(CancellationToken.None);
            Assert.AreEqual(0, str.PageSize, "it should read the correct bytes.");
            Assert.AreEqual(1, str.BitmapPage, "it should read the correct bytes.");
            Assert.AreEqual(2, str.PageCount, "it should read the correct bytes.");
            Assert.AreEqual(3, str.IndexBytes, "it should read the correct bytes.");
            Assert.AreEqual(4, str.Reserved, "it should read the correct bytes.");
            Assert.AreEqual(5, str.IndexPage, "it should read the correct bytes.");
        }

        [TestMethod, TestCategory("IO")]
        [ExpectedException(typeof(EndOfStreamException), "it should throw an end-of-file exception.")]
        public async Task When_reading_a_structure_from_a_stream_that_is_too_short()
        {
            var sut = new AsyncBinaryReader(new TestStream(
                0, 0, 0, 0,
                1, 0, 0, 0,
                2, 0, 0, 0,
                3, 0, 0, 0,
                4, 0, 0, 0
            ));

            await sut.ReadStructureAsync<Symblr.Symbols.Pdb20.Pdb20Header>(CancellationToken.None);
        }

        [TestMethod, TestCategory("IO")]
        public async Task When_reading_an_Int32()
        {
            var sut = new AsyncBinaryReader(new TestStream(
                1, 0, 0, 0
            ));

            var i = await sut.ReadInt32Async(CancellationToken.None);
            Assert.AreEqual(1, i, "it should read all the bytes.");
        }

        [TestMethod, TestCategory("IO")]
        [ExpectedException(typeof(EndOfStreamException), "it should throw an end-of-file exception.")]
        public async Task When_reading_an_Int32_from_a_stream_that_is_too_short()
        {
            var sut = new AsyncBinaryReader(new TestStream(
                1, 0, 0
            ));

            await sut.ReadInt32Async(CancellationToken.None);
        }

        [TestMethod, TestCategory("IO")]
        public async Task When_reading_an_UInt32()
        {
            var sut = new AsyncBinaryReader(new TestStream(
                1, 0, 0, 0
            ));

            var i = await sut.ReadInt32Async(CancellationToken.None);
            Assert.AreEqual(1, i, "it should read all the bytes.");
        }

        [TestMethod, TestCategory("IO")]
        [ExpectedException(typeof(EndOfStreamException), "it should throw an end-of-file exception.")]
        public async Task When_reading_an_UInt32_from_a_stream_that_is_too_short()
        {
            var sut = new AsyncBinaryReader(new TestStream(
                1, 0, 0
            ));

            await sut.ReadUInt32Async(CancellationToken.None);
        }
    }
}
