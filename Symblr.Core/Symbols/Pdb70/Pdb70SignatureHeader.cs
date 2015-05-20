using System;
using System.Runtime.InteropServices;

namespace Symblr.Symbols.Pdb70
{
    /// <summary>
    /// Represents the PDB 7.00 signature header.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Pdb70SignatureHeader
    {
        /// <summary>
        /// The version of the PDB.
        /// </summary>
        public int Version;

        /// <summary>
        /// The signature of the PDB (legacy).
        /// </summary>
        public int Signature;

        /// <summary>
        /// The age of the PDB.
        /// </summary>
        public int Age;

        /// <summary>
        /// The unique identifier of the PDB.
        /// </summary>
        public Guid Guid;
    }
}
