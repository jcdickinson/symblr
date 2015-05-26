using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symblr.Symbols.Pdb70
{
    /// <summary>
    /// Represents reasons why a PDB70 cannot be loaded.
    /// </summary>
    enum Pdb70LoadErrorCode
    {
        /// <summary>
        /// The error is unanticipated.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// The PDB is using an unsupported feature.
        /// </summary>
        UnsupportedFeature = 1,
        /// <summary>
        /// The PDB is assumed to be corrupt.
        /// </summary>
        AssumedCorrupt = 2,
    }
}
