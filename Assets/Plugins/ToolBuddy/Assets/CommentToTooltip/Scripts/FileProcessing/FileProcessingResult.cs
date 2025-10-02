using System.Collections.Generic;

namespace ToolBuddy.CommentToTooltip.FileProcessing
{
    /// <summary>
    /// Represents the aggregated outcome of a file processing operation.
    /// </summary>
    public sealed class FileProcessingResult
    {
        /// <summary>
        /// Gets the collection of files that were altered during processing.
        /// </summary>
        public List<FileModificationInfo> ModifiedFiles { get; }

        /// <summary>
        /// Gets the total number of files that were inspected.
        /// </summary>
        public int TotalFilesCount { get; }

        /// <summary>
        /// Gets a value indicating whether the operation was cancelled before completion.
        /// </summary>
        public bool Canceled { get; }

        /// <summary>
        /// Gets the time taken to process the files, expressed in seconds.
        /// </summary>
        public double DurationSeconds { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileProcessingResult"/> class.
        /// </summary>
        /// <param name="modifiedFiles">The files that were modified.</param>
        /// <param name="totalFilesCount">The total number of files that were inspected.</param>
        /// <param name="canceled">Whether the operation was cancelled.</param>
        /// <param name="durationSeconds">The duration of the processing operation, in seconds.</param>
        public FileProcessingResult(
            List<FileModificationInfo> modifiedFiles,
            int totalFilesCount,
            bool canceled,
            double durationSeconds)
        {
            ModifiedFiles = modifiedFiles;
            TotalFilesCount = totalFilesCount;
            Canceled = canceled;
            DurationSeconds = durationSeconds;
        }
    }
}