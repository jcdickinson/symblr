using Symblr.Symbols.Pdb70;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Symblr.Symbols
{
    /// <summary>
    /// Represents a way to read PDB 7.00 metadata.
    /// </summary>
    public sealed class Pdb70SymbolMetadataProvider : ISymbolMetadataProvider
    {
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
        public async Task<ISymbolMetadata> TryGetSymbolMetadataAsync(System.IO.Stream stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            var file = await Pdb70File.TryOpenAsync(stream, cancellationToken);
            if (file == null) return null;
            return new Pdb70SymbolMetadata(file);
        }

        /// <summary>
        /// Represents symbol metadata for a PDB 7.00 file.
        /// </summary>
        class Pdb70SymbolMetadata : ISymbolMetadata
        {
            /// <summary>
            /// The file.
            /// </summary>
            private readonly Pdb70File _file;

            /// <summary>
            /// Initializes a new instance of the <see cref="Pdb70SymbolMetadata"/> class.
            /// </summary>
            /// <param name="file">The file.</param>
            public Pdb70SymbolMetadata(Pdb70File file)
            {
                _file = file;

                var arr = _file.Guid.ToByteArray();
                var sb = new StringBuilder(arr.Length * 2 + 1);
                foreach (var b in arr) sb.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", b);
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}", _file.Age);
                Identifier = sb.ToString();
            }

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
                get { return true; }
            }

            /// <summary>
            /// Gets a value indicating whether the symbols have source server information.
            /// </summary>
            /// <value>
            /// <c>true</c> if the symbols have source server information; otherwise, <c>false</c>.
            /// </value>
            public bool HasSourceServerInformation
            {
                get { return _file.StreamExists("SRCSRV"); }
            }

            /// <summary>
            /// Gets the source information from the metadata.
            /// </summary>
            public SourceInformationCollection SourceInformation
            {
                get
                {
                    throw new System.NotImplementedException();
                }
                set
                {
                    throw new System.NotImplementedException();
                }
            }

            /// <summary>
            /// Asyncronously saves any changes made to the metadata.
            /// </summary>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>
            /// A <see cref="Task" /> that represents the pending save operation.
            /// </returns>
            public Task SaveAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                return _file.SaveAsync(cancellationToken);
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                _file.Dispose();
            }
        }
    }
}
