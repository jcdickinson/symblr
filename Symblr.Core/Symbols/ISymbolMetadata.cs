using System;
using System.Threading;
using System.Threading.Tasks;

namespace Symblr.Symbols
{
    /// <summary>
    /// Represents a way to access symbol metadata.
    /// </summary>
    public interface ISymbolMetadata : IDisposable
    {
        /// <summary>
        /// Gets the identifier of the symbol.
        /// </summary>
        /// <value>
        /// The identifier of the symbol.
        /// </value>
        string Identifier { get; }

        /// <summary>
        /// Gets a value indicating whether information is supported.
        /// </summary>
        /// <value>
        /// <c>true</c> if source information is supported; otherwise, <c>false</c>.
        /// </value>
        bool SupportsSourceServerInformation { get; }

        /// <summary>
        /// Gets a value indicating whether the symbols have source server information.
        /// </summary>
        /// <value>
        /// <c>true</c> if the symbols have source server information; otherwise, <c>false</c>.
        /// </value>
        bool HasSourceServerInformation { get; }

        /// <summary>
        /// Gets or sets the source information from the metadata.
        /// </summary>
        /// <returns>A list of the source information.</returns>
        /// <exception cref="System.NotSupportedException">Thrown if <see cref="SupportsSourceServerInformation"/> is null.</exception>
        SourceInformationCollection SourceInformation { get; set; }

        /// <summary>
        /// Asynchronously saves any changes made to the metadata.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task" /> that represents the pending save operation.
        /// </returns>
        Task SaveAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
