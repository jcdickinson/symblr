using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Symblr.Symbols
{
    /// <summary>
    /// Represents a collection of <see cref="SourceInformation"/>.
    /// </summary>
    public class SourceInformationCollection : Collection<SourceInformation>
    {
        /// <summary>
        /// Gets the server that the source files can be downloaded from.
        /// </summary>
        /// <value>
        /// The server that the source files can be downloaded from.
        /// </value>
        public string Server { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceInformationCollection" /> class.
        /// </summary>
        /// <param name="server">The server that the source files can be downloaded from.</param>
        public SourceInformationCollection(string server)
            : this(server, new SourceInformation[0])
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceInformationCollection" /> class.
        /// </summary>
        /// <param name="server">The server that the source files can be downloaded from.</param>
        /// <param name="list">The initial values within the collection.</param>
        /// <exception cref="System.ArgumentNullException">server</exception>
        public SourceInformationCollection(string server, IList<SourceInformation> list)
            : base(list)
        {
            if (string.IsNullOrEmpty(server)) throw new ArgumentNullException("server");
            Server = server;
        }
    }
}
