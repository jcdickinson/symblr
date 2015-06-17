using Symblr.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Symblr.Symbols
{
    /// <summary>
    /// Represents a symbol metadata provider for files that are not supported
    /// by other providers.
    /// </summary>
    public class FallbackSymbolsMetadataProvider : ISymbolMetadataProvider
    {
        /// <summary>
        /// Represents the symbol metadata for <see cref="FallbackSymbolsMetadataProvider"/>.
        /// </summary>
        private class FallbackSymbolMetadata : ISymbolMetadata
        {
            /// <summary>
            /// Gets the identifier of the symbol.
            /// </summary>
            /// <value>
            /// The identifier of the symbol.
            /// </value>
            public string Identifier
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets a value indicating whether information is supported.
            /// </summary>
            /// <value>
            /// <c>true</c> if source information is supported; otherwise, <c>false</c>.
            /// </value>
            public bool SupportsSourceServerInformation
            {
                get { return false; }
            }

            /// <summary>
            /// Gets a value indicating whether the symbols have source server information.
            /// </summary>
            /// <value>
            /// <c>true</c> if the symbols have source server information; otherwise, <c>false</c>.
            /// </value>
            public bool HasSourceServerInformation
            {
                get { return false; }
            }

            /// <summary>
            /// Gets the source information from the metadata.
            /// </summary>
            /// <exception cref="System.NotSupportedException">Thrown if the value is set.</exception>
            public SourceInformationCollection SourceInformation
            {
                get { return SourceInformationCollection.Empty; }
                set { throw new NotSupportedException(Resources.NotSupportedException_SourceInformation); }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="FallbackSymbolMetadata"/> class.
            /// </summary>
            /// <param name="identifier">The identifier.</param>
            public FallbackSymbolMetadata(string identifier)
            {
                Identifier = identifier;
            }

            /// <summary>
            /// Asyncronously saves any changes made to the metadata.
            /// </summary>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>
            /// A <see cref="Task" /> that represents the pending save operation.
            /// </returns>
            /// <exception cref="System.NotSupportedException">Always thrown.</exception>
            public Task SaveAsync(System.Threading.CancellationToken cancellationToken = default(CancellationToken))
            {
                throw new NotSupportedException(Resources.NotSupportedException_SourceInformation);
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {

            }
        }

        /// <summary>
        /// Asynchronously attempts to read a stream and return the symbol metadata from it.
        /// </summary>
        /// <param name="stream">The stream to read the metadata from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task{ISymbolMetadata}" /> that represents the pending read operation. Once the task completes
        /// <see cref="Task{ISymbolMetadata}.Result" /> will contain the metadata if it was succesfully read, or <c>null</c>
        /// if the metadata could not be read.
        /// </returns>
        public async Task<ISymbolMetadata> TryGetSymbolMetadataAsync(System.IO.Stream stream, System.Threading.CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var hash = new Murmur3())
            using (var cs = new CryptoStream(Stream.Null, hash, CryptoStreamMode.Write))
            {
                await stream.CopyToAsync(cs, Environment.SystemPageSize, cancellationToken);
                cs.FlushFinalBlock();

                var final = hash.Hash;
                var s = new StringBuilder(32);
                for (var i = 0; i < 16; i++)
                    s.Append(final[i].ToString("x2", CultureInfo.InvariantCulture));

                return new FallbackSymbolMetadata(s.ToString());
            }
        }

    }
}
