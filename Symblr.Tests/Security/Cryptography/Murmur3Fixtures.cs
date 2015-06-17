using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Symblr.Security.Cryptography
{
    public class Murmur3Fixtures
    {
        [Fact]
        public void When_performing_the_standard_test()
        {
            // Adapted from: https://code.google.com/p/smhasher/source/browse/trunk/KeysetTest.cpp#13
            const uint Murmur3_x64_128 = 0x6384BA69u;

            using (var hash = new Murmur3())
            // Also test that Merkle incremental hashing works.
            using (var cs = new CryptoStream(Stream.Null, hash, CryptoStreamMode.Write))
            {
                var key = new byte[256];

                for (var i = 0; i < 256; i++)
                {
                    key[i] = (byte)i;
                    using (var m = new Murmur3(256 - i))
                    {
                        var computed = m.ComputeHash(key, 0, i);
                        // Also check that your implementation deals with incomplete
                        // blocks.
                        cs.Write(computed, 0, 5);
                        cs.Write(computed, 5, computed.Length - 5);
                    }
                }

                cs.FlushFinalBlock();
                var final = hash.Hash;
                var verification = ((uint)final[0]) | ((uint)final[1] << 8) | ((uint)final[2] << 16) | ((uint)final[3] << 24);
                Assert.Equal(Murmur3_x64_128, verification);
            }
        }
    }
}
