using System.Collections.Generic;

namespace ToolBuddy.CommentToTooltip.FileProcessing
{
    public sealed class FileProcessingResult
    {
        public List<FileModificationInfo> ModifiedFiles { get; }
        public int TotalFilesCount { get; }
        public bool Canceled { get; }
        public double DurationSeconds { get; }

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