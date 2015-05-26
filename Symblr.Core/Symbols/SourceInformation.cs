
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
        /// Gets the relative path which the file can be stored.
        /// </summary>
        /// <value>
        /// The relative path which the file can be stored.
        /// </value>
        public string TargetPath { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceInformation" /> struct.
        /// </summary>
        /// <param name="originalFile">The original file that was stored in the symbols during the build.</param>
        /// <exception cref="System.ArgumentNullException">
        /// originalFile
        /// or
        /// serverPath
        /// </exception>
        public SourceInformation(
            string originalFile)
            : this()
        {
            if (string.IsNullOrEmpty(originalFile)) throw new ArgumentNullException("originalFile");

            OriginalFile = originalFile;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceInformation" /> struct.
        /// </summary>
        /// <param name="originalFile">The original file that was stored in the symbols during the build.</param>
        /// <param name="targetPath">The relative path which the file can be stored.</param>
        /// <exception cref="System.ArgumentNullException">
        /// originalFile
        /// or
        /// serverPath
        /// </exception>
        public SourceInformation(
            string originalFile,
            string targetPath)
            : this()
        {
            if (string.IsNullOrEmpty(originalFile)) throw new ArgumentNullException("originalFile");
            if (string.IsNullOrEmpty(targetPath)) throw new ArgumentNullException("targetPath");

            OriginalFile = originalFile;
            TargetPath = targetPath;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}*{1}", OriginalFile, TargetPath);
        }
    }
}
