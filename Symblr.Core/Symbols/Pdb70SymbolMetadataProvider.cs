﻿using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Symblr.Symbols.Pdb70;

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
        /// <see cref="Task{ISymbolMetadata}.Result" /> will contain the metadata if it was successfully read, or <c>null</c>
        /// if the metadata could not be read.
        /// </returns>
        public async Task<ISymbolMetadata> TryGetSymbolMetadataAsync(
            Stream stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            var file = await Pdb70File.TryOpenAsync(stream, cancellationToken);
            if (file == null) return null;

            var md = new Pdb70SymbolMetadata(file);
            await md.ReadSourceServerInformationAsync(cancellationToken);
            return md;
        }

        /// <summary>
        /// Represents symbol metadata for a PDB 7.00 file.
        /// </summary>
        private class Pdb70SymbolMetadata : ISymbolMetadata
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
                var sb = new StringBuilder((arr.Length * 2) + 1);
                foreach (var b in arr) sb.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", b);
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}", _file.Age);
                Identifier = sb.ToString();
            }

            /// <summary>
            /// Asynchronously reads source server information.
            /// </summary>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>
            /// A <see cref="Task"/> that represents the asynchronous read operation.
            /// </returns>
            public async Task ReadSourceServerInformationAsync(CancellationToken cancellationToken)
            {
                const string sourceFiles = "/src/files/";
                const int sourceFilesLength = 11;

                if (_file.StreamExists("srcsrv"))
                    SourceInformation = await SrcSrvParser.ParseAsync(_file, cancellationToken);

                if (SourceInformation == null)
                {
                    SourceInformation = new SourceInformationCollection();
                    foreach (var item in _file.StreamNames.Where(x => x.StartsWith(sourceFiles)))
                    {
                        SourceInformation.Add(new Symbols.SourceInformation(item.Substring(sourceFilesLength)));
                    }
                }
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
                get { return SourceInformation.Any(x => !string.IsNullOrEmpty(x.TargetPath)); }
            }

            /// <summary>
            /// Gets or sets the source information from the metadata.
            /// </summary>
            public SourceInformationCollection SourceInformation
            {
                get;
                set;
            }

            /// <summary>
            /// Asynchronously saves any changes made to the metadata.
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
