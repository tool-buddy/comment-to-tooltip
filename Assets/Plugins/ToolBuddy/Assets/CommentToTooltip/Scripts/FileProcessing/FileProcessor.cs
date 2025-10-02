using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using ToolBuddy.CommentToTooltip.TextProcessing;

namespace ToolBuddy.CommentToTooltip.FileProcessing
{
    /// <inheritdoc />
    public sealed class FileProcessor : IFileProcessor
    {
        private readonly ITextProcessor _textProcessor;
        private readonly IFileEncodingDetector _fileEncodingDetector;

        public FileProcessor(
            ITextProcessor textProcessor,
            IFileEncodingDetector fileEncodingDetector)
        {
            _textProcessor = textProcessor ?? throw new ArgumentNullException(nameof(textProcessor));
            _fileEncodingDetector = fileEncodingDetector ?? throw new ArgumentNullException(nameof(fileEncodingDetector));
        }

        /// <inheritdoc />
        public event Action NoFileToProcess;

        /// <inheritdoc />
        public event Action NoCommentTypeSelected;

        /// <inheritdoc />
        public event Action<FileProcessingResult> ProcessingCompleted;

        /// <inheritdoc />
        public event Action<Exception> ProcessingError;

        /// <inheritdoc />
        public event Func<int, int, string, bool> CancellationCheck;

        /// <inheritdoc />
        public void ProcessFile(
            string filePath,
            CommentTypes commentTypes) =>
            ProcessFiles(
                new[] { filePath },
                commentTypes
            );

        /// <inheritdoc />
        public void ProcessFolder(
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

        private void ProcessFiles(
            string[] filePaths,
            CommentTypes commentTypes)
        {
            List<string> validatedFilePaths = filePaths.Where(s => !String.IsNullOrEmpty(s)).ToList();

            if (validatedFilePaths.Count == 0)
            {
                NoFileToProcess?.Invoke();
                return;
            }

            if (commentTypes == CommentTypes.None)
            {
                NoCommentTypeSelected?.Invoke();
                return;
            }

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

        private FileProcessingResult GetModifiedFiles(
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
                canceled = CancellationCheck?.Invoke(
                               processed + 1,
                               filePaths.Count,
                               filePath
                           )
                           ?? false;

                if (canceled)
                    break;

                byte[] fileBytes = File.ReadAllBytes(filePath);

                Encoding fileEncoding = _fileEncodingDetector.DetectFileEncoding(fileBytes);

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