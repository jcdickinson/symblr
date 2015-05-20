using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Symblr.Symbols.Pdb20
{
    /// <summary>
    /// Represents the PDB signature header.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Pdb20SignatureHeader
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
