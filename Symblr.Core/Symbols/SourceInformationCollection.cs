using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Symblr.Symbols
{
    /// <summary>
    /// Represents a collection of <see cref="SourceInformation"/>.
    /// </summary>
    public class SourceInformationCollection : Collection<SourceInformation>
    {
        private static readonly SourceInformationCollection _empty = new SourceInformationCollection(new SourceInformation[0]);

        /// <summary>
        /// Gets an empty <see cref="SourceInformationCollection"/>.
        /// </summary>
        public static SourceInformationCollection Empty
        {
            get { return _empty; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceInformationCollection" /> class.
        /// </summary>
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
