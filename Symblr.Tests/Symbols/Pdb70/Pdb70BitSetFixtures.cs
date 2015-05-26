using Microsoft.VisualStudio.TestTools.UnitTesting;
using Symblr.IO;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Symblr.Symbols.Pdb70
{
    [TestClass]
    public class Pdb70BitSetFixtures
    {
        [TestMethod, TestCategory("PDB70")]
        public void When_creating_a_null_BitSet()
        {
            var sut = new Pdb70BitSet();
            Assert.IsNull(sut.Words, "it should initialize correctly.");
            Assert.IsTrue(sut.IsEmpty, "it should indicate that it is empty.");
        }

        [TestMethod, TestCategory("PDB70")]
        public void When_creating_an_empty_BitSet()
        {
            var sut = new Pdb70BitSet(0);
            Assert.AreEqual(0, sut.Words.Length, "it should initialize correctly.");
            Assert.IsTrue(sut.IsEmpty, "it should indicate that it is empty.");
        }

        [TestMethod, TestCategory("PDB70")]
        public void When_calling_ToString_on_a_null_BitSet()
        {
            var sut = new Pdb70BitSet();
            Assert.AreEqual("", sut.ToString(), "it should return the correct string representation.");
        }

        [TestMethod, TestCategory("PDB70")]
        public void When_calling_ToString_on_an_empty_BitSet()
        {
            var sut = new Pdb70BitSet(0);
            Assert.AreEqual("", sut.ToString(), "it should return the correct string representation.");
        }

        [TestMethod, TestCategory("PDB70")]
        public void When_calling_ToString_on_a_BitSet()
        {
            var sut = new Pdb70BitSet(0);
            sut[0] = true;
            sut[1] = true;
            sut[3] = true;
            sut[5] = true;
            sut[31] = true;
            Assert.AreEqual("1 1 0 1 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1",
                sut.ToString(), "it should return the correct string representation.");
        }

        [TestMethod, TestCategory("PDB70")]
        public void When_expanding_a_null_BitSet_it_should_expand()
        {
            var sut = new Pdb70BitSet();
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

        [TestMethod, TestCategory("PDB70")]
        public void When_expanding_an_empty_BitSet_it_should_expand()
        {
            var sut = new Pdb70BitSet(0);
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

        [TestMethod, TestCategory("PDB70")]
        public void When_retrieving_an_item_past_the_end_of_a_bitset()
        {
            var sut = new Pdb70BitSet();
            sut.Words = new uint[] { 0u };

            Assert.IsFalse(sut[64], "it should indicate false for the bit.");
            Assert.AreEqual(1, sut.Words.Length, "it should not expand the bitset.");
        }

        [TestMethod, TestCategory("PDB70")]
        public void When_allocating_in_a_BitSet_free_list()
        {
            var sut = new Pdb70BitSet();
            sut[0x28] = true;

            var index = sut.Allocate();
            Assert.AreEqual(0x28, index, "it should allocate the first free item.");
            Assert.IsFalse(sut[0x28], "it should allocate the first free item.");

            index = sut.Allocate();
            Assert.AreEqual(0x40, index, "it should allocate the second free item.");
            Assert.AreEqual(3, sut.Words.Length, "it should expand the free list.");
            Assert.AreEqual(0xFFFFFFFFu ^ 0x1u, sut.Words[2], "it should mark the new entries as free.");
        }

        [TestMethod, TestCategory("PDB70")]
        public void When_allocating_in_a_null_BitSet_free_list()
        {
            var sut = new Pdb70BitSet();

            var index = sut.Allocate();
            Assert.AreEqual(0x0, index, "it should allocate the first free item.");
            Assert.IsFalse(sut[0x0], "it should allocate the first free item.");
        }

        [TestMethod, TestCategory("PDB70")]
        public void When_deallocating_in_a_BitSet_free_list()
        {
            var sut = new Pdb70BitSet();
            sut[0x28] = false; // Allocates two words

            sut.Deallocate(0x28);
            Assert.AreEqual(2, sut.Words.Length, "it should not expand the free list.");
            Assert.IsTrue(sut[0x28], "it should deallocate the item.");
        }

        [TestMethod, TestCategory("PDB70")]
        public async Task When_reading_a_bit_set()
        {
            var reader = new AsyncBinaryReader(new TestStream(
                2, 0, 0, 0,
                1, 0, 0, 0,
                0, 1, 0, 0
            ));

            var sut = await Pdb70BitSet.ReadAsync(reader, CancellationToken.None);
            Assert.AreEqual(2, sut.Words.Length, "it should read both words.");
            Assert.IsTrue(sut[0x0], "it should read the correct values.");
            Assert.IsTrue(sut[0x28], "it should read the correct values.");
            Assert.IsFalse(sut[0x1], "it should read the correct values.");
        }

        [TestMethod, TestCategory("PDB70")]
        public async Task When_writing_a_bit_set()
        {
            var sut = new Pdb70BitSet();
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

        [TestMethod, TestCategory("PDB70")]
        public void When_calculating_log_2()
        {
            var inc = 1u;
            for (var i = 0ul; i < uint.MaxValue; i += inc)
            {
                var expected = i == 0 ? 0 : (uint)Math.Log(i, 2);
                Assert.AreEqual(expected, Pdb70BitSet.Log2((uint)i), "it should calculate log2 correctly.");
                if (i % 128 == 0) inc++;
            }
        }


        [TestMethod, TestCategory("PDB70")]
        public void When_allocating_an_empty_free_list()
        {
            var sut = default(Pdb70BitSet);
            sut.Words = new uint[0];

            var i = sut.Allocate();
            Assert.AreEqual(0, i, "it should allocate the first bit.");
        }

        [TestMethod, TestCategory("PDB70")]
        public void When_allocating_with_the_29th_bit_free()
        {
            var sut = default(Pdb70BitSet);
            sut.Words = new uint[] { 536870912u };

            var i = sut.Allocate();
            Assert.AreEqual(29, i, "it should allocate the 29th bit.");
        }

        [TestMethod, TestCategory("PDB70")]
        public void When_allocating_with_the_3rd_bit_free()
        {
            var sut = default(Pdb70BitSet);
            sut.Words = new uint[] { 8u };

            var i = sut.Allocate();
            Assert.AreEqual(3, i, "it should allocate the 3rd bit.");
        }

        [TestMethod, TestCategory("PDB70")]
        public void When_allocating_with_a_full_first_word_in_a_free_list()
        {
            var sut = default(Pdb70BitSet);
            sut.Words = new uint[] { 0u };

            var i = sut.Allocate();
            Assert.AreEqual(32, i, "it should allocate the 32nd bit.");
        }

        [TestMethod, TestCategory("PDB70")]
        public void When_allocating_with_a_full_second_word_in_a_free_list()
        {
            var sut = default(Pdb70BitSet);
            sut.Words = new uint[] { 0u, 0u };

            var i = sut.Allocate();
            Assert.AreEqual(64, i, "it should allocate the 64th bit.");
        }
    }
}
