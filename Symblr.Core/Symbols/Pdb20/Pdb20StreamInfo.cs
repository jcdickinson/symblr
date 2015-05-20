using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symblr.Symbols.Pdb20
{
    /// <summary>
    /// Represents information about a stream within a PDB file.
    /// </summary>
    [DebuggerDisplay("{StreamLength} Count = {Count}")]
    sealed class Pdb20StreamInfo : Collection<int>
    {
        /// <summary>
        /// Gets or sets the length of the stream.
        /// </summary>
        public int StreamLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pdb20StreamInfo"/> class.
        /// </summary>
        public Pdb20StreamInfo(int streamLength = 0)
        {
            StreamLength = streamLength;
        }
    }
}
