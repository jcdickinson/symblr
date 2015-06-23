using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Symblr.Security.Cryptography
{
    /// <summary>
    /// Represents the Murmur 3 Hash Algorithm.
    /// </summary>
    /// <remarks>
    /// Murmur 3 is not cryptographically secure, instead it is used
    /// for content identification.
    /// </remarks>
    public sealed class Murmur3 : HashAlgorithm
    {
        private const int InputBlockSizeBytes = 16;
        private const ulong C1 = 0x87c37b91114253d5ul;
        private const ulong C2 = 0x4cf5ad432745937ful;

        private readonly ulong _seed;
        private ulong _h1;
        private ulong _h2;
        private ulong _length;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether multiple blocks can be transformed.
        /// </summary>
        /// <returns>true if multiple blocks can be transformed; otherwise, false.</returns>
        public override bool CanTransformMultipleBlocks
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the size, in bits, of the computed hash code.
        /// </summary>
        /// <returns>The size, in bits, of the computed hash code.</returns>
        public override int HashSize
        {
            get { return 128; }
        }

        /// <summary>
        /// Gets the input block size.
        /// </summary>
        /// <returns>The input block size.</returns>
        public override int InputBlockSize
        {
            get { return InputBlockSizeBytes; }
        }

        /// <summary>
        /// Gets the output block size.
        /// </summary>
        /// <returns>The output block size.</returns>
        public override int OutputBlockSize
        {
            get { return InputBlockSizeBytes; }
        }

        /// <summary>
        /// Gets a value indicating whether the current transform can be reused.
        /// </summary>
        /// <returns>Always true.</returns>
        public override bool CanReuseTransform
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the seed used in the algorithm.
        /// </summary>
        /// <value>
        /// The seed used in the algorithm.
        /// </value>
        public long Seed
        {
            get { return unchecked((long)_seed); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Murmur3"/> class.
        /// </summary>
        public Murmur3()
            : this(0)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Murmur3" /> class.
        /// </summary>
        /// <param name="seed">The seed for the hash.</param>
        public Murmur3(long seed)
        {
            _seed = unchecked((ulong)seed);
            Initialize();
        }

        /// <summary>
        /// Creates an instance of the default implementation of <see cref="Murmur3"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="Murmur3"/>.</returns>
        public static new Murmur3 Create()
        {
            // TODO: X86/X64.
            return new Murmur3();
        }

        /// <summary>
        /// Creates an instance of the specified implementation of <see cref="Murmur3"/>.
        /// </summary>
        /// <param name="hashName">The name of the specific implementation of <see cref="Murmur3"/>
        /// to be used.</param>
        /// <returns>A new instance of <see cref="Murmur3"/> using the specified implementation.</returns>
        public static new Murmur3 Create(string hashName)
        {
            // TODO: X86/X64 + 32/64/128.
            return new Murmur3();
        }

        /// <summary>
        /// Routes data written to the object into the hash algorithm for computing the hash.
        /// </summary>
        /// <param name="array">The input to compute the hash code for.</param>
        /// <param name="ibStart">The offset into the byte array from which to begin using data.</param>
        /// <param name="cbSize">The number of bytes in the byte array to use as data.</param>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1107:CodeMustNotContainMultipleStatementsOnOneLine",
            Justification = "State machine. Improves readability.")]
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            _length += (uint)cbSize;

            unsafe
            {
                fixed (byte* pbyte = &array[ibStart])
                {
                    byte* ppbyte = pbyte;

                    var k1 = 0ul;
                    var k2 = 0ul;
                    while (cbSize >= InputBlockSizeBytes)
                    {
                        k1 = *((ulong*)ppbyte);
                        ppbyte += 8;

                        k2 = *((ulong*)ppbyte);
                        ppbyte += 8;

                        cbSize -= 16;

                        _h1 = _h1 ^ (Rol(k1 * C1, 31) * C2);
                        _h1 = 0x52dce729 + ((Rol(_h1, 27) + _h2) * 5);

                        _h2 = _h2 ^ (Rol(k2 * C2, 33) * C1);
                        _h2 = 0x38495ab5 + ((Rol(_h2, 31) + _h1) * 5);
                    }

                    // ------------ TAIL ALGORITHM ------------
                    k1 = 0;
                    k2 = 0;
                    switch (cbSize & 15)
                    {
                        case 15: k2 ^= (ulong)(*(ppbyte + 14)) << 48; goto case 14;
                        case 14: k2 ^= (ulong)(*(ppbyte + 13)) << 40; goto case 13;
                        case 13: k2 ^= (ulong)(*(ppbyte + 12)) << 32; goto case 12;
                        case 12: k2 ^= (ulong)(*(ppbyte + 11)) << 24; goto case 11;
                        case 11: k2 ^= (ulong)(*(ppbyte + 10)) << 16; goto case 10;
                        case 10: k2 ^= (ulong)(*(ppbyte + 9)) << 8; goto case 9;
                        case 9:
                            k2 ^= (ulong)(*(ppbyte + 8));
                            _h2 ^= Rol(k2 * C2, 33) * C1;
                            goto case 8;
                        case 8: k1 ^= (ulong)(*(ppbyte + 7)) << 56; goto case 7;
                        case 7: k1 ^= (ulong)(*(ppbyte + 6)) << 48; goto case 6;
                        case 6: k1 ^= (ulong)(*(ppbyte + 5)) << 40; goto case 5;
                        case 5: k1 ^= (ulong)(*(ppbyte + 4)) << 32; goto case 4;
                        case 4: k1 ^= (ulong)(*(ppbyte + 3)) << 24; goto case 3;
                        case 3: k1 ^= (ulong)(*(ppbyte + 2)) << 16; goto case 2;
                        case 2: k1 ^= (ulong)(*(ppbyte + 1)) << 8; goto case 1;
                        case 1:
                            k1 ^= (ulong)(*ppbyte);
                            _h1 ^= Rol(k1 * C1, 31) * C2;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Finalizes the hash computation after the last data is processed by the cryptographic stream object.
        /// </summary>
        /// <returns>
        /// The computed hash code.
        /// </returns>
        protected override byte[] HashFinal()
        {
            _h1 ^= _length;
            _h2 ^= _length;

            _h1 += _h2;
            _h2 += _h1;

            _h1 = FinalMix(_h1);
            _h2 = FinalMix(_h2);

            _h1 += _h2;
            _h2 += _h1;

            var result = new byte[16];
            UInt64(result, 0, _h1);
            UInt64(result, 8, _h2);

            return result;
        }

        /// <summary>Performs the final mix.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The mixed value.</returns>
        private static ulong FinalMix(ulong value)
        {
            value = (value ^ (value >> 33)) * 0xff51afd7ed558ccdul;
            value = (value ^ (value >> 33)) * 0xc4ceb9fe1a85ec53ul;
            value ^= value >> 33;
            return value;
        }

        /// <summary>
        /// Initializes an implementation of the <see cref="T:System.Security.Cryptography.HashAlgorithm" /> class.
        /// </summary>
        public override void Initialize()
        {
            _h1 = _h2 = _seed;
            _length = 0ul;
        }

        #region Math Utils
        /// <summary>Rolls the specified value left by the specified amount of bits.</summary>
        /// <param name="value">The value.</param>
        /// <param name="bits">The bits.</param>
        /// <returns>The rolled value.</returns>
        private static ulong Rol(ulong value, byte bits)
        {
            return (value << bits) | (value >> (64 - bits));
        }

        /// <summary>Sets a <see cref="UInt64"/> in the specified byte array at the specified position.</summary>
        /// <param name="bytes">The byte array.</param>
        /// <param name="pos">The position.</param>
        /// <param name="value">The value.</param>
        private static unsafe void UInt64(byte[] bytes, int pos, ulong value)
        {
            fixed (byte* pbyte = &bytes[pos])
            {
                *((ulong*)pbyte) = value;
            }
        }
        #endregion
    }
}
