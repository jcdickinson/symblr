using Symblr.IO;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Symblr.Symbols.Pdb70
{
    /// <summary>
    /// Represents a set of bits.
    /// </summary>
    struct Pdb70BitSet
    {
        private static readonly int[] Lookup = new int[256];

        /// <summary>
        /// Initializes the <see cref="Pdb70BitSet"/> struct.
        /// </summary>
        static Pdb70BitSet()
        {
            for (int i = 1; i < 256; i++)
                Lookup[i] = (int)(Math.Log(i) / Math.Log(2));
        }

        /// <summary>
        /// Finds Log2 of the specified number.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        private static int Log2(uint i)
        {
            if (i >= 0x1000000u) { return Lookup[i >> 24] + 24; }
            else if (i >= 0x10000u) { return Lookup[i >> 16] + 16; }
            else if (i >= 0x100u) { return Lookup[i >> 8] + 8; }
            else { return Lookup[i]; }
        }

        /// <summary>
        /// Gets the words that represent the bitset.
        /// </summary>
        public uint[] Words;

        /// <summary>
        /// Gets or sets the <see cref="System.Boolean"/> at the specified bit index.
        /// </summary>
        /// <value>
        /// The <see cref="System.Boolean"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The bit value.</returns>
        public bool this[int index]
        {
            get
            {
                var word = index / 32;
                if (word >= Words.Length) return false;
                return ((Words[word]) & GetBit(index)) != 0;
            }
            set
            {
                var word = index / 32;
                if (Words == null)
                    Words = new uint[0];
                if (word >= Words.Length)
                    Array.Resize(ref Words, word + 1);

                if (value)
                    Words[word] |= GetBit(index);
                else
                    Words[word] &= ~GetBit(index);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty
        {
            get { return Words == null || Words.Length == 0; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pdb70BitSet"/> struct.
        /// </summary>
        /// <param name="size">The size.</param>
        public Pdb70BitSet(int size)
        {
            Words = new uint[size];
        }

        /// <summary>
        /// Deallocates the specified bit indicating that it is free.
        /// </summary>
        /// <param name="index">The index.</param>
        public void Deallocate(int index)
        {
            if (!this[index])
                this[index] = true;
        }

        /// <summary>
        /// Allocates a new bit where there is a free bit.
        /// </summary>
        /// <returns>The allcoated bit.</returns>
        public int Allocate()
        {
            if (Words == null) Words = new uint[] { 0xFFFFFFFFu };

            for (var i = 0; i < Words.Length; i++)
            {
                // This is a **FREE** list, so we need to find the
                // first 1 bit.
                var item = Words[i] ^ 0xFFFFFFFFu;
                if (item != 0xFFFFFFFFu)
                {
                    // Find first non-set bit.
                    var inv = item ^ 0xFFFFFFFFu;
                    var two = item + 1;
                    var bit = (inv & two);
                    if (bit != 0)
                    {
                        Words[i] &= ~bit;
                        return i * 32 + Log2(bit);
                    }
                }
            }

            var index = this.Words.Length;
            Array.Resize(ref Words, index + 1);
            Words[index] = 0xFFFFFFFFu ^ 1;
            return index * 32;
        }

        /// <summary>
        /// Asynchronously reads a bitset from the specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task{Pdb20BitSet}"/> that represents the asynchronous read operation.
        /// </returns>
        public static async Task<Pdb70BitSet> ReadAsync(AsyncBinaryReader reader, CancellationToken cancellationToken)
        {
            var result = new Pdb70BitSet(await reader.ReadInt32Async(cancellationToken));
            if (result.Words.Length != 0)
            {
                var bytes = await reader.ReadBytesAsync(result.Words.Length * sizeof(int), cancellationToken);
                Buffer.BlockCopy(bytes, 0, result.Words, 0, bytes.Length);
            }
            return result;
        }

        /// <summary>
        /// Asynchronously writes the bitset to the specified writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous write operation.
        /// </returns>
        public async Task WriteAsync(AsyncBinaryWriter writer, CancellationToken cancellationToken)
        {
            await writer.WriteAsync(Words.Length, cancellationToken);
            var bytes = new byte[Words.Length * sizeof(int)];
            Buffer.BlockCopy(Words, 0, bytes, 0, bytes.Length);
            await writer.BaseStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
        }

        /// <summary>
        /// Gets the bit at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The value of the bit.</returns>
        private static uint GetBit(int index)
        {
            return 1u << (index % 32);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            for (var i = 0; Words != null && i < Words.Length * 32; i++)
            {
                if (sb.Length != 0) sb.Append(' ');
                sb.Append(this[i] ? '1' : '0');
            }
            return sb.ToString();
        }
    }
}
