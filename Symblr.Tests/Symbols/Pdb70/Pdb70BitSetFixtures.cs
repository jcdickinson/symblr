using Symblr.IO;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Symblr.Symbols.Pdb70
{
    public class Pdb70BitSetFixtures
    {
        [Fact]
        public void When_creating_a_null_BitSet()
        {
            var sut = new Pdb70BitSet();
            Assert.Null(sut.Words);
            Assert.True(sut.IsEmpty);
        }

        [Fact]
        public void When_creating_an_empty_BitSet()
        {
            var sut = new Pdb70BitSet(0);
            Assert.Equal(0, sut.Words.Length);
            Assert.True(sut.IsEmpty);
        }

        [Fact]
        public void When_calling_ToString_on_a_null_BitSet()
        {
            var sut = new Pdb70BitSet();
            Assert.Equal("", sut.ToString());
        }

        [Fact]
        public void When_calling_ToString_on_an_empty_BitSet()
        {
            var sut = new Pdb70BitSet(0);
            Assert.Equal("", sut.ToString());
        }

        [Fact]
        public void When_calling_ToString_on_a_BitSet()
        {
            var sut = new Pdb70BitSet(0);
            sut[0] = true;
            sut[1] = true;
            sut[3] = true;
            sut[5] = true;
            sut[31] = true;
            Assert.Equal(
                "1 1 0 1 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1",
                sut.ToString());
        }

        [Fact]
        public void When_expanding_a_null_BitSet_it_should_expand()
        {
            var sut = new Pdb70BitSet();
            sut[0] = true;

            Assert.NotNull(sut.Words);
            Assert.Equal(1, sut.Words.Length);
            Assert.False(sut.IsEmpty);

            Assert.Equal(1u, sut.Words[0]);
            Assert.True(sut[0]);

            sut[0] = false;

            Assert.Equal(0u, sut.Words[0]);
            Assert.False(sut[0]);

            sut[0x28] = true;

            Assert.Equal(2, sut.Words.Length);
            Assert.Equal(0x100u, sut.Words[1]);
            Assert.True(sut[0x28]);
        }

        [Fact]
        public void When_expanding_an_empty_BitSet_it_should_expand()
        {
            var sut = new Pdb70BitSet(0);
            sut[0] = true;

            Assert.NotNull(sut.Words);
            Assert.Equal(1, sut.Words.Length);
            Assert.False(sut.IsEmpty);

            Assert.Equal(1u, sut.Words[0]);
            Assert.True(sut[0]);

            sut[0] = false;

            Assert.Equal(0u, sut.Words[0]);
            Assert.False(sut[0]);

            sut[0x28] = true;

            Assert.Equal(2, sut.Words.Length);
            Assert.Equal(0x100u, sut.Words[1]);
            Assert.True(sut[0x28]);
        }

        [Fact]
        public void When_retrieving_an_item_past_the_end_of_a_bitset()
        {
            var sut = new Pdb70BitSet();
            sut.Words = new uint[] { 0u };

            Assert.False(sut[64]);
            Assert.Equal(1, sut.Words.Length);
        }

        [Fact]
        public void When_allocating_in_a_BitSet_free_list()
        {
            var sut = new Pdb70BitSet();
            sut[0x28] = true;

            var index = sut.Allocate();
            Assert.Equal(0x28, index);
            Assert.False(sut[0x28]);

            index = sut.Allocate();
            Assert.Equal(0x40, index);
            Assert.Equal(3, sut.Words.Length);
            Assert.Equal(0xFFFFFFFFu ^ 0x1u, sut.Words[2]);
        }

        [Fact]
        public void When_allocating_in_a_null_BitSet_free_list()
        {
            var sut = new Pdb70BitSet();

            var index = sut.Allocate();
            Assert.Equal(0x0, index);
            Assert.False(sut[0x0]);
        }

        [Fact]
        public void When_deallocating_in_a_BitSet_free_list()
        {
            var sut = new Pdb70BitSet();
            sut[0x28] = false; // Allocates two words

            sut.Deallocate(0x28);
            Assert.Equal(2, sut.Words.Length);
            Assert.True(sut[0x28]);
        }

        [Fact]
        public async Task When_reading_a_bit_set()
        {
            var reader = new AsyncBinaryReader(new TestStream(
                2, 0, 0, 0,
                1, 0, 0, 0,
                0, 1, 0, 0
            ));

            var sut = await Pdb70BitSet.ReadAsync(reader, CancellationToken.None);
            Assert.Equal(2, sut.Words.Length);
            Assert.True(sut[0x0]);
            Assert.True(sut[0x28]);
            Assert.False(sut[0x1]);
        }

        [Fact]
        public async Task When_writing_a_bit_set()
        {
            var sut = new Pdb70BitSet();
            sut[0x0] = true;
            sut[0x28] = true;

            var ms = new MemoryStream();
            var writer = new AsyncBinaryWriter(new TestStream(ms));
            await sut.WriteAsync(writer, CancellationToken.None);

            Assert.Equal(new byte[] {
                2, 0, 0, 0,
                1, 0, 0, 0,
                0, 1, 0, 0
            }, ms.ToArray());
        }

        [Fact]
        public void When_calculating_log_2()
        {
            var inc = 1u;
            for (var i = 0ul; i < uint.MaxValue; i += inc)
            {
                var expected = i == 0 ? 0 : (uint)Math.Log(i, 2);
                Assert.Equal(expected, Pdb70BitSet.Log2((uint)i));
                if (i % 128 == 0) inc++;
            }
        }

        [Fact]
        public void When_allocating_an_empty_free_list()
        {
            var sut = default(Pdb70BitSet);
            sut.Words = new uint[0];

            var i = sut.Allocate();
            Assert.Equal(0, i);
        }

        [Fact]
        public void When_allocating_with_the_29th_bit_free()
        {
            var sut = default(Pdb70BitSet);
            sut.Words = new uint[] { 536870912u };

            var i = sut.Allocate();
            Assert.Equal(29, i);
        }

        [Fact]
        public void When_allocating_with_the_3rd_bit_free()
        {
            var sut = default(Pdb70BitSet);
            sut.Words = new uint[] { 8u };

            var i = sut.Allocate();
            Assert.Equal(3, i);
        }

        [Fact]
        public void When_allocating_with_a_full_first_word_in_a_free_list()
        {
            var sut = default(Pdb70BitSet);
            sut.Words = new uint[] { 0u };

            var i = sut.Allocate();
            Assert.Equal(32, i);
        }

        [Fact]
        public void When_allocating_with_a_full_second_word_in_a_free_list()
        {
            var sut = default(Pdb70BitSet);
            sut.Words = new uint[] { 0u, 0u };

            var i = sut.Allocate();
            Assert.Equal(64, i);
        }
    }
}
