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
        /// Gets an empty <see cref="SourceInformationCollection"/>.
        /// </summary>
        public static SourceInformationCollection Empty = new SourceInformationCollection(new SourceInformation[0]);

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceInformationCollection" /> class.
        /// </summary>
        /// <param name="server">The server that the source files can be downloaded from.</param>
        public SourceInformationCollection()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceInformationCollection"/> class.
        /// </summary>
        /// <param name="underlyingCollection">The underlying collection.</param>
        public SourceInformationCollection(IList<SourceInformation> underlyingCollection)
            : base(underlyingCollection)
        {

        }
    }
}
