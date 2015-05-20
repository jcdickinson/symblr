using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Symblr
{
    /// <summary>
    /// Represents native methods.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    static class NativeMethods
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr memset(byte[] dest, int c, int count);

        /// <summary>
        /// Determines if two pieces of memory are equal.
        /// </summary>
        /// <param name="b1">The first piece of memory.</param>
        /// <param name="b2">The second piece of memory.</param>
        /// <returns>A value indicating whether the memory is equal.</returns>
        public static bool MemoryEquals(byte[] b1, byte[] b2)
        {
            if (object.ReferenceEquals(b1, b2)) return true;
            if (b1 == null || b2 == null) return false;
            if (b1.Length != b2.Length) return false;
            if (b1.Length == 0) return true;
            if (b1[0] != b2[0]) return false; // Quick check before incurring P/Invoke cost.

            return memcmp(b1, b2, b1.Length) == 0;
        }

        /// <summary>
        /// Set all bytes within the specified piece of memory to a specific value.
        /// </summary>
        /// <param name="b">The memory.</param>
        /// <param name="c">The value to set each byte to.</param>
        public static void MemorySet(byte[] b, int c = 0)
        {
            if (b == null) throw new ArgumentNullException("b");
            memset(b, c, b.Length);
        }
    }
}
