using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace ToolBuddy.CommentToTooltip.Processors
{
    public static class FileProcessor
    {
        //todo consider making this class non-static

        private static readonly TextProcessor _textProcessor = new();

        // Events for UI listeners
        public static event Action NoFileToProcess;
        public static event Action NoCommentTypeSelected;
        public static event Action<FileProcessingResult> ProcessingCompleted;
        public static event Action<Exception> ProcessingError;
        public static event Func<int, int, string, bool> CancellationCheck;

        public static void ProcessFile(
            string filePath,
            CommentTypes commentTypes) =>
            ProcessFiles(
                new[] { filePath },
                commentTypes
            );


        public static void ProcessFolder(
            string folderPath,
            CommentTypes commentTypes)
        {
            string[] filePaths = Directory.GetFiles(
                folderPath,
                "*.cs",
                SearchOption.AllDirectories
            );
            ProcessFiles(
                filePaths,
                commentTypes
            );
        }

        private static void ProcessFiles(
            string[] filePaths,
            CommentTypes commentTypes)
        {
            List<string> validatedFilePaths = filePaths.Where(s => !String.IsNullOrEmpty(s)).ToList();

            if (validatedFilePaths.Count == 0)
                NoFileToProcess?.Invoke();
            else if (commentTypes == CommentTypes.None)
                NoCommentTypeSelected?.Invoke();
            else
                try
                {
                    FileProcessingResult result = GetModifiedFiles(
                        validatedFilePaths,
                        commentTypes
                    );

                    if (!result.Canceled)
                        WriteFilesToDisk(result.ModifiedFiles);

                    ProcessingCompleted?.Invoke(result);
                }
                catch (Exception e)
                {
                    ProcessingError?.Invoke(e);
                }
        }


        private static FileProcessingResult GetModifiedFiles(
            [NotNull] List<string> filePaths,
            CommentTypes commentTypes)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();

            int processed = 0;
            bool canceled = false;

            List<FileModificationInfo> fileModifications = new(
                filePaths.Count
            );

            foreach (string filePath in filePaths)
            {
                canceled = (CancellationCheck != null
                            && CancellationCheck.Invoke(
                                processed + 1,
                                filePaths.Count,
                                filePath
                            ));

                if (canceled)
                    break;

                byte[] fileBytes = File.ReadAllBytes(filePath);

                Encoding fileEncoding = FileEncodingDetector.DetectFileEncoding(fileBytes);

                string fileContent = fileEncoding.GetString(fileBytes);

                if (_textProcessor.TryProcessText(
                        fileContent,
                        out string modifiedFileContent,
                        commentTypes
                    ))
                    fileModifications.Add(
                        new FileModificationInfo(
                            filePath,
                            modifiedFileContent,
                            fileEncoding
                        )
                    );

                processed++;
            }

            stopwatch.Stop();

            return new FileProcessingResult(
                fileModifications,
                filePaths.Count,
                canceled,
                stopwatch.Elapsed.TotalSeconds
            );
        }


        private static void WriteFilesToDisk(
            List<FileModificationInfo> fileModifications)
        {
            foreach (FileModificationInfo fileModificationInfo in fileModifications)
                using (StreamWriter writer = new(
                           fileModificationInfo.FilePath,
                           false,
                           fileModificationInfo.Encoding
                       ))
                {
                    writer.Write(fileModificationInfo.NewContent);
                }
        }
    }
}