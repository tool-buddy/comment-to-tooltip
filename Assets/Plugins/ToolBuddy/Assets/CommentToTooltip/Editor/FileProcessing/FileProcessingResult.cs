using System.Collections.Generic;

namespace ToolBuddy.CommentToTooltip.Editor.FileProcessing
{
    public sealed class FileProcessingResult
    {
        //todo use a ModifiedFile type
        public List<KeyValuePair<string, string>> ModifiedFiles { get; }
        public int TotalFilesCount { get; }
        public bool Canceled { get; }
        public double DurationSeconds { get; }

        public FileProcessingResult(
            List<KeyValuePair<string, string>> modifiedFiles,
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