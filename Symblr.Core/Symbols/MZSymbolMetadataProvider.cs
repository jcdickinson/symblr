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
        /// <summary>
        /// Asynchronously attempts to read a stream and return the symbol metadata from it.
        /// </summary>
        /// <param name="stream">The stream to read the metadata from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task{ISymbolMetadata}" /> that represents the pending read operation. Once the task completes
        /// <see cref="Task{ISymbolMetadata}.Result" /> will contain the metadata if it was successfully read, or <c>null</c>
        /// if the metadata could not be read.
        /// </returns>
        public Task<ISymbolMetadata> TryGetSymbolMetadataAsync(
            Stream stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            return MZ.MZMetadata.TryOpenAsync(stream, cancellationToken);
        }
    }
}
