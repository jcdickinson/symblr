using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Symblr
{
    /// <summary>
    /// Represents the base class for all Symblr exceptions.
    /// </summary>
    [Serializable]
    public class SymblrException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SymblrException"/> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public SymblrException()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymblrException"/> class.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        [ExcludeFromCodeCoverage]
        public SymblrException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymblrException" /> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not a
        /// null reference, the current exception is raised in a catch block that handles the inner exception.
        /// </param>
        [ExcludeFromCodeCoverage]
        public SymblrException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymblrException"/> class.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        [ExcludeFromCodeCoverage]
        protected SymblrException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
