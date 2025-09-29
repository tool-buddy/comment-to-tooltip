using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using ToolBuddy.CommentToTooltip.Editor.Settings;

namespace ToolBuddy.CommentToTooltip.Editor.FileProcessing
{
    public static class FileProcessor
    {
        //todo consider making this class non-static

        private static readonly TooltipGenerator _tooltipGenerator = new();

        // Events for UI listeners
        public static event Action NoFileToProcess;
        public static event Action NoCommentTypeSelected;
        public static event Action<FileProcessingResult> ProcessingCompleted;
        public static event Action<Exception> ProcessingError;
        public static event Func<int, int, string, bool> CancellationCheck;

        public static void ProcessFile(
            string filePath)
        {
            DirectoryInfo directoryInfo = new FileInfo(filePath).Directory;
            if (directoryInfo != null)
                SettingsStorage.InitialFolderPath = directoryInfo.FullName;
            ProcessFiles(new[] { filePath });
        }


        public static void ProcessFolder(
            string folderPath)
        {
            SettingsStorage.InitialFolderPath = folderPath;

            string[] filePaths = Directory.GetFiles(
                folderPath,
                "*.cs",
                SearchOption.AllDirectories
            );
            ProcessFiles(filePaths);
        }

        private static void ProcessFiles(
            string[] filePaths)
        {
            CommentTypes commentTypes = SettingsStorage.ParsingSettings.GetCommentTypes();

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

            List<KeyValuePair<string, string>> modifiedFiles = new(
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

                string fileContent = File.ReadAllText(
                    filePath,
                    Encoding.Default
                );

                if (_tooltipGenerator.TryProcessText(
                        fileContent,
                        out string modifiedFileContent,
                        commentTypes
                    ))
                    modifiedFiles.Add(
                        new KeyValuePair<string, string>(
                            filePath,
                            modifiedFileContent
                        )
                    );

                processed++;
            }

            stopwatch.Stop();

            return new FileProcessingResult(
                modifiedFiles,
                filePaths.Count,
                canceled,
                stopwatch.Elapsed.TotalSeconds
            );
        }

        private static void WriteFilesToDisk(
            List<KeyValuePair<string, string>> filesToWrite)
        {
            foreach (KeyValuePair<string, string> pair in filesToWrite)
                using (StreamWriter writer = new(
                           pair.Key,
                           false,
                           Encoding.Default
                       ))
                {
                    writer.Write(pair.Value);
                }
        }
    }
}