using System.Text;

namespace ToolBuddy.CommentToTooltip.Processors
{
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