
using System;
namespace Symblr.Symbols
{
    /// <summary>
    /// Represents source information.
    /// </summary>
    public struct SourceInformation
    {
        /// <summary>
        /// Gets the original file that was stored in the symbols during the build.
        /// </summary>
        /// <value>
        /// The original file that was stored in the symbols during the build.
        /// </value>
        public string OriginalFile { get; private set; }

        /// <summary>
        /// Gets the local relative path in which the downloaded source file will be stored.
        /// </summary>
        /// <value>
        /// The local path in which the downloaded source file will be stored.
        /// </value>
        public string LocalPath { get; private set; }

        /// <summary>
        /// Gets a string that uniquely identifies the version of the file.
        /// </summary>
        /// <value>
        /// The string that uniquely identifier the version of the file.
        /// </value>
        public string LocalVersion { get; private set; }

        /// <summary>
        /// Gets the relative server path which the file can be retrieved.
        /// </summary>
        /// <value>
        /// The relative server path which the file can be retrieved.
        /// </value>
        public string ServerPath { get; private set; }

        /// <summary>
        /// Gets a string that uniquely identifies the version of the file on the server.
        /// </summary>
        /// <value>
        /// The string that uniquely identifier the version of the file on the server.
        /// </value>
        public string ServerVersion { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceInformation" /> struct.
        /// </summary>
        /// <param name="originalFile">The original file that was stored in the symbols during the build.</param>
        /// <param name="localPath">The local relative path in which the downloaded source file will be stored.</param>
        /// <param name="localVersion">A string that uniquely identifies the version of the file.</param>
        /// <param name="server">The server from which the file can be retrieved.</param>
        /// <param name="serverPath">The relative server path which the file can be retrieved.</param>
        /// <param name="serverVersion">A string that uniquely identifies the version of the file on the server.</param>
        public SourceInformation(
            string originalFile,
            string localPath,
            string localVersion,
            string serverPath,
            string serverVersion)
            : this()
        {
            if (string.IsNullOrEmpty(originalFile)) throw new ArgumentNullException("originalFile");

            if (string.IsNullOrEmpty(localPath)) throw new ArgumentNullException("localPath");
            if (string.IsNullOrEmpty(localVersion)) throw new ArgumentNullException("localVersion");

            if (string.IsNullOrEmpty(serverPath)) throw new ArgumentNullException("serverPath");
            if (string.IsNullOrEmpty(serverVersion)) throw new ArgumentNullException("serverVersion");

            OriginalFile = originalFile;

            LocalPath = localPath;
            LocalVersion = localVersion;

            ServerPath = serverPath;
            ServerVersion = serverVersion;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}: {1}", ServerVersion, ServerPath);
        }
    }
}
