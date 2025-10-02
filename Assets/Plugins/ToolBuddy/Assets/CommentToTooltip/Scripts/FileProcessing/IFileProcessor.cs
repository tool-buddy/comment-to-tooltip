using System;

namespace ToolBuddy.CommentToTooltip.FileProcessing
{
    public interface IFileProcessor
    {
        event Action NoFileToProcess;
        event Action NoCommentTypeSelected;
        event Action<FileProcessingResult> ProcessingCompleted;
        event Action<Exception> ProcessingError;
        event Func<int, int, string, bool> CancellationCheck;

        void ProcessFile(
            string filePath,
            CommentTypes commentTypes);

        void ProcessFolder(
            string folderPath,
            CommentTypes commentTypes);
    }
}