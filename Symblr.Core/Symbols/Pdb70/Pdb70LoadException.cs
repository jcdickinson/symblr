using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Symblr.Symbols.Pdb70
{
    /// <summary>
    /// Represents an exception about a failed load attempt
    /// on a PDB.
    /// </summary>
    [Serializable]
    sealed class Pdb70LoadException : SymblrException
    {
        /// <summary>
        /// Gets the error code.
        /// </summary>
        /// <value>
        /// The error code.
        /// </value>
        public Pdb70LoadErrorCode ErrorCode
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pdb70LoadException" /> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        public Pdb70LoadException(Pdb70LoadErrorCode errorCode)
            : this(errorCode, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pdb70LoadException" /> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="innerException">The inner exception.</param>
        public Pdb70LoadException(Pdb70LoadErrorCode errorCode, Exception innerException)
            : base(GetMessage(errorCode), innerException)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pdb70LoadException"/> class.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        private Pdb70LoadException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ErrorCode = (Pdb70LoadErrorCode)info.GetInt32("ErrorCode");
        }

        /// <summary>
        /// Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
        /// </PermissionSet>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ErrorCode", (int)ErrorCode);
        }

        /// <summary>
        /// Gets the message for a specific error code.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <returns>The error message.</returns>
        private static string GetMessage(Pdb70LoadErrorCode errorCode)
        {
            switch (errorCode)
            {
                case Pdb70LoadErrorCode.UnsupportedFeature: return Resources.LoadException_Message_UnsupportedFeature;
                case Pdb70LoadErrorCode.AssumedCorrupt: return Resources.LoadException_Message_PotentiallyCorrupt;
                default: return Resources.LoadException_Message_Unknown;
            }
        }
    }
}
