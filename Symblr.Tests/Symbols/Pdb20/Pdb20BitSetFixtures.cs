using Microsoft.VisualStudio.TestTools.UnitTesting;
using Symblr.IO;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Symblr.Symbols.Pdb20
{
    [TestClass]
    public class Pdb20BitSetFixtures
    {
        [TestMethod, TestCategory("Pdb20")]
        public void When_creating_a_null_BitSet()
        {
            var sut = new Pdb20BitSet();
            Assert.IsNull(sut.Words, "it should initialize correctly.");
            Assert.IsTrue(sut.IsEmpty, "it should indicate that it is empty.");
        }

        [TestMethod, TestCategory("Pdb20")]
        public void When_creating_an_empty_BitSet()
        {
            var sut = new Pdb20BitSet(0);
            Assert.AreEqual(0, sut.Words.Length, "it should initialize correctly.");
            Assert.IsTrue(sut.IsEmpty, "it should indicate that it is empty.");
        }

        [TestMethod, TestCategory("Pdb20")]
        public void When_calling_ToString_on_a_null_BitSet()
        {
            var sut = new Pdb20BitSet();
            Assert.AreEqual("", sut.ToString(), "it should return the correct string representation.");
        }

        [TestMethod, TestCategory("Pdb20")]
        public void When_calling_ToString_on_an_empty_BitSet()
        {
            var sut = new Pdb20BitSet(0);
            Assert.AreEqual("", sut.ToString(), "it should return the correct string representation.");
        }

        [TestMethod, TestCategory("Pdb20")]
        public void When_calling_ToString_on_a_BitSet()
        {
            var sut = new Pdb20BitSet(0);
            sut[0] = true;
            sut[1] = true;
            sut[3] = true;
            sut[5] = true;
            sut[31] = true;
            Assert.AreEqual("1 1 0 1 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1",
                sut.ToString(), "it should return the correct string representation.");
        }

        [TestMethod, TestCategory("Pdb20")]
        public void When_expanding_a_null_BitSet_it_should_expand()
        {
            var sut = new Pdb20BitSet();
            sut[0] = true;

            Assert.IsNotNull(sut.Words, "it should expand correctly.");
            Assert.AreEqual(1, sut.Words.Length, "it should expand correctly.");
            Assert.IsFalse(sut.IsEmpty, "it should indicate that it is not empty.");

            Assert.AreEqual(1u, sut.Words[0], "the first bit should be set.");
            Assert.IsTrue(sut[0], "the first bit should be set.");

            sut[0] = false;

            Assert.AreEqual(0u, sut.Words[0], "the first bit should be unset.");
            Assert.IsFalse(sut[0], "the first bit should be unset.");

            sut[0x28] = true;

            Assert.AreEqual(2, sut.Words.Length, "it should expand correctly.");
            Assert.AreEqual(0x100u, sut.Words[1], "it should set the second word correctly.");
            Assert.IsTrue(sut[0x28], "the 40th bit should be set.");
        }

        [TestMethod, TestCategory("Pdb20")]
        public void When_expanding_an_empty_BitSet_it_should_expand()
        {
            var sut = new Pdb20BitSet(0);
            sut[0] = true;

            Assert.IsNotNull(sut.Words, "it should expand correctly.");
            Assert.AreEqual(1, sut.Words.Length, "it should expand correctly.");
            Assert.IsFalse(sut.IsEmpty, "it should indicate that it is not empty.");

            Assert.AreEqual(1u, sut.Words[0], "the first bit should be set.");
            Assert.IsTrue(sut[0], "the first bit should be set.");

            sut[0] = false;

            Assert.AreEqual(0u, sut.Words[0], "the first bit should be unset.");
            Assert.IsFalse(sut[0], "the first bit should be unset.");

            sut[0x28] = true;

            Assert.AreEqual(2, sut.Words.Length, "it should expand correctly.");
            Assert.AreEqual(0x100u, sut.Words[1], "it should set the second word correctly.");
            Assert.IsTrue(sut[0x28], "the 40th bit should be set.");
        }

        [TestMethod, TestCategory("Pdb20")]
        public void When_allocating_in_a_BitSet_free_list()
        {
            var sut = new Pdb20BitSet();
            sut[0x28] = true;

            var index = sut.Allocate();
            Assert.AreEqual(0x28, index, "it should allocate the first free item.");
            Assert.IsFalse(sut[0x28], "it should allocate the first free item.");

            index = sut.Allocate();
            Assert.AreEqual(0x40, index, "it should allocate the second free item.");
            Assert.AreEqual(3, sut.Words.Length, "it should expand the free list.");
            Assert.AreEqual(0xFFFFFFFFu ^ 0x1u, sut.Words[2], "it should mark the new entries as free.");
        }

        [TestMethod, TestCategory("Pdb20")]
        public void When_allocating_in_a_null_BitSet_free_list()
        {
            var sut = new Pdb20BitSet();

            var index = sut.Allocate();
            Assert.AreEqual(0x0, index, "it should allocate the first free item.");
            Assert.IsFalse(sut[0x0], "it should allocate the first free item.");
        }

        [TestMethod, TestCategory("Pdb20")]
        public void When_deallocating_in_a_BitSet_free_list()
        {
            var sut = new Pdb20BitSet();
            sut[0x28] = false; // Allocates two words

            sut.Deallocate(0x28);
            Assert.AreEqual(2, sut.Words.Length, "it should not expand the free list.");
            Assert.IsTrue(sut[0x28], "it should deallocate the item.");
        }

        [TestMethod, TestCategory("Pdb20")]
        public async Task When_reading_a_bit_set()
        {
            var reader = new AsyncBinaryReader(new TestStream(
                2, 0, 0, 0,
                1, 0, 0, 0,
                0, 1, 0, 0
            ));

            var sut = await Pdb20BitSet.ReadAsync(reader, CancellationToken.None);
            Assert.AreEqual(2, sut.Words.Length, "it should read both words.");
            Assert.IsTrue(sut[0x0], "it should read the correct values.");
            Assert.IsTrue(sut[0x28], "it should read the correct values.");
            Assert.IsFalse(sut[0x1], "it should read the correct values.");
        }

        [TestMethod, TestCategory("Pdb20")]
        public async Task When_writing_a_bit_set()
        {
            var sut = new Pdb20BitSet();
            sut[0x0] = true;
            sut[0x28] = true;

            var ms = new MemoryStream();
            var writer = new AsyncBinaryWriter(new TestStream(ms));
            await sut.WriteAsync(writer, CancellationToken.None);

            ArrayAssert.All(new byte[] {
                2, 0, 0, 0,
                1, 0, 0, 0,
                0, 1, 0, 0
            }, ms.ToArray(), (e, a) => Assert.AreEqual(e, a, "it should write the correct bytes."));
        }
    }
}
