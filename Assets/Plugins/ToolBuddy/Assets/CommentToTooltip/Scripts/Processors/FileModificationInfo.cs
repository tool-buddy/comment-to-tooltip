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

        public FileModificationInfo(
            string filePath,
            string newContent)
        {
            FilePath = filePath;
            NewContent = newContent;
        }
    }
}