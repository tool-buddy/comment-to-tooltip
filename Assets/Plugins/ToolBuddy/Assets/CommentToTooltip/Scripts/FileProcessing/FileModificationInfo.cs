using System.Text;

namespace ToolBuddy.CommentToTooltip.FileProcessing
{
    /// <summary>
    /// Describes a file modification.
    /// </summary>
    public sealed class FileModificationInfo
    {
        /// <summary>
        /// Gets the file path.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Gets the file's new content.
        /// </summary>
        public string NewContent { get; }

        /// <summary>
        /// Gets the file's character encoding.
        /// </summary>
        public Encoding Encoding { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileModificationInfo"/> class.
        /// </summary>
        /// <param name="filePath">The absolute path of the file that was altered.</param>
        /// <param name="newContent">The content written to the file after processing.</param>
        /// <param name="encoding">The encoding that should be used when persisting the file contents.</param>
        public FileModificationInfo(
            string filePath,
            string newContent,
            Encoding encoding)
        {
            FilePath = filePath;
            NewContent = newContent;
            Encoding = encoding;
        }
    }
}