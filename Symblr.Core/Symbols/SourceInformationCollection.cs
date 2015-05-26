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
        /// Initializes a new instance of the <see cref="SourceInformationCollection" /> class.
        /// </summary>
        /// <param name="server">The server that the source files can be downloaded from.</param>
        public SourceInformationCollection()
            : base()
        {

        }
    }
}
