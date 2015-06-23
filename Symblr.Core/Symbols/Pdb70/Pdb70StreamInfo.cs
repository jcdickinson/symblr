using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Symblr.Symbols.Pdb70
{
    /// <summary>
    /// Represents information about a stream within a PDB 7.00 file.
    /// </summary>
    [DebuggerDisplay("{StreamLength} Count = {Count}")]
    internal sealed class Pdb70StreamInfo : Collection<int>
    {
        /// <summary>
        /// Gets or sets the length of the stream.
        /// </summary>
        public int StreamLength
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pdb70StreamInfo" /> class.
        /// </summary>
        /// <param name="streamLength">The length of the stream.</param>
        public Pdb70StreamInfo(int streamLength = 0)
        {
            StreamLength = streamLength;
        }
    }
}
