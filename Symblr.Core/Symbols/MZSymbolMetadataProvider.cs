using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Symblr.Symbols
{
    /// <summary>
    /// Represents a way to read MZ (DLL, EXE, COM) metadata.
    /// </summary>
    public class MZSymbolMetadataProvider : ISymbolMetadataProvider
    {
        public Task<ISymbolMetadata> TryGetSymbolMetadataAsync(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            return MZ.MZMetadata.TryOpenAsync(stream, cancellationToken);
        }
    }
}
