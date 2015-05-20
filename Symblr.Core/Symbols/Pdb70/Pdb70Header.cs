using System.Runtime.InteropServices;

namespace Symblr.Symbols.Pdb70
{
    /// <summary>
    /// Represents the header of a PDB 7.00 file.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Pdb70Header
    {
        /// <summary>
        /// The signature of the PDB 7.00 file.
        /// </summary>
        public static readonly byte[] Signature = new byte[] {
            0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66,
            0x74, 0x20, 0x43, 0x2F, 0x43, 0x2B, 0x2B, 0x20,
            0x4D, 0x53, 0x46, 0x20, 0x37, 0x2E, 0x30, 0x30,
            0x0D, 0x0A, 0x1A, 0x44, 0x53, 0x00, 0x00, 0x00
        };

        /// <summary>
        /// The size of a page, in bytes.
        /// </summary>
        public int PageSize;

        /// <summary>
        /// The page which holds the bitmap.
        /// </summary>
        public int BitmapPage;

        /// <summary>
        /// The number of pages in the file.
        /// </summary>
        public int PageCount;

        /// <summary>
        /// The number of bytes in the root stream.
        /// </summary>
        public int IndexBytes;

        /// <summary>
        /// Always zero.
        /// </summary>
        public int Reserved;

        /// <summary>
        /// The index of the index page.
        /// </summary>
        public int IndexPage;
    }
}
