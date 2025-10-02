using System;

namespace ToolBuddy.CommentToTooltip.FileProcessing
{
    /// <summary>
    /// Converts comments to tooltips in files and folders.
    /// </summary>
    public interface IFileProcessor
    {
        /// <summary>
        /// Raised when no candidate files are found to process.
        /// </summary>
        event Action NoFileToProcess;

        /// <summary>
        /// Raised when processing is requested without any comment types selected.
        /// </summary>
        event Action NoCommentTypeSelected;

        /// <summary>
        /// Raised once processing finishes, providing a detailed result.
        /// </summary>
        event Action<FileProcessingResult> ProcessingCompleted;

        /// <summary>
        /// Raised when an unexpected error occurs during processing.
        /// </summary>
        event Action<Exception> ProcessingError;

        /// <summary>
        /// Invoked periodically to determine whether the user requested processing cancellation.
        /// </summary>
        event Func<int, int, string, bool> CancellationCheck;

        /// <summary>
        /// Processes the specified file and attempts to convert comments to tooltips.
        /// </summary>
        /// <param name="filePath">Absolute path of the file to process.</param>
        /// <param name="commentTypes">The comment types that should be parsed.</param>
        void ProcessFile(
            string filePath,
            CommentTypes commentTypes);

        /// <summary>
        /// Processes every .cs file located under the specified folder.
        /// </summary>
        /// <param name="folderPath">Absolute path of the folder whose files should be processed.</param>
        /// <param name="commentTypes">The comment types that should be parsed.</param>
        void ProcessFolder(
            string folderPath,
            CommentTypes commentTypes);
    }
}