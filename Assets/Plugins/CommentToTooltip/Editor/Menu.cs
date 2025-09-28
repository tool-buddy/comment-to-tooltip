using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ToolBuddy.CommentToTooltip.Editor
{
    public static class Menu
    {
        private static readonly TooltipGenerator _tooltipGenerator = new();

        private static string LogsFilePath =>
            String.Format(
                "{0}/{1}_logs.txt",
                Application.dataPath,
                ToolName
            );

        public static string ToolName =>
            "Comment To Tooltip";

        static Menu() =>
            _settings.ReadFromEditorPreferences();

        #region editor settings

        private static readonly Settings _settings = new();

        public static Settings Settings =>
            _settings;

        #endregion

        #region files and folders panel

        /// <summary>
        /// The folder that is displayed when opening a file or folder panel
        /// </summary>
        private static string InitialFolderPath
        {
            get => EditorPrefs.GetString(
                InitialFolderPathPreferenceKey,
                Application.dataPath
            );
            set => EditorPrefs.SetString(
                InitialFolderPathPreferenceKey,
                value
            );
        }

        private const string InitialFolderPathPreferenceKey = "C2T_InitialFolderPath";

        private const string SupportedFileExtension = "cs";

        #endregion

        #region batch info

        private static int _batchFilesCount;
        private static int _batchProcessedFilesCount;

        #endregion

        #region ui texts

        //menu items
        private const string MenuName = "Tools/Comment To Tooltip";
        private const string ProcessFileMenuItem = "Process a file...";
        private const string ProcessFolderMenuItem = "Process a folder...";
        private const string AboutMenuItem = "Help";
        private const string SettingsMenuItem = "Settings";

        //windows titles
        private const string FileSelectionTitle = "Select a file";
        private const string FolderSelectionTitle = "Select a folder";
        private const string ProcessingInProgressTitle = "File processing";
        private const string SettingsWindowTitle = "Settings";

        //windows content
        private const string HelpFileNotFoundDialogMessage = "Help can be found in the ReadMe.txt file";

        private const string NoCommentTypesSelectedDialogMessage =
            "No comment types selected. Please select at least one in the {0} menu.";

        private const string NoFilesFoundDialogMessage = "No files found to process.";
        private const string ProcessingInProgressDialogMessage = "Processing file {0} out of {1} : {2}";

        private const string ProcessingInteruptedDialogMessage =
            "The operation was stopped by the user. No changes were applied.";

        private const string ProcessingSuccededDialogMessage = "Processing of {0} file{1} completed successfully in {2}s. ";

        private const string ProcessingSucceededWithRecoverableErrorDialogMessage =
            "Errors were encountered for one or more files. These files were ignored. ";

        private const string ProcessingSuccededFilesModifiedDialogMessage = "{0} file{1} modified. ";
        private const string ProcessingSuccededNoFilesModifiedDialogMessage = "No files were modified. ";

        private const string ProcessingSuccededSeeLogsDialogMessage =
            "You can find more details in the console or in the logs file.";

        private const string ProcessingEncountredCriticalErrorDialogMessage =
            "An unexpected error occurred while processing a file. No changes were applied. You can find more details in the console.";

        private const string OkButtonDialogBox = "Ok";

        //console messages
        private const string FilesModifiedConsoleMessage = "Tooltips added or modified in the following files:";
        private const string LogsUpdatedConsoleMessage = "Messages logged in the following file: '{0}'.";

        private const string RecoverableErrorEncoutredConsoleMessage =
            "An exception occured while processing the following file '{0}'. This file will be ignored.";

        //log file only messages
        private const string BatchStartedLogMessage = "Started processing file(s).";

        #endregion

        #region menu items

        [MenuItem(MenuName + "/" + ProcessFileMenuItem)]
        public static void OnProcessFile()
        {
            string filePath = EditorUtility.OpenFilePanel(
                FileSelectionTitle,
                InitialFolderPath,
                SupportedFileExtension
            );
            if (File.Exists(filePath))
            {
                InitialFolderPath = new FileInfo(filePath).Directory.FullName;
                ProcessFiles(new[] { filePath });
            }
        }

        [MenuItem(MenuName + "/" + ProcessFolderMenuItem)]
        public static void OnProcessAFolder()
        {
            string folderPath = EditorUtility.OpenFolderPanel(
                FolderSelectionTitle,
                InitialFolderPath,
                ""
            );
            if (Directory.Exists(folderPath))
            {
                InitialFolderPath = folderPath;
                string[] filePaths = Directory.GetFiles(
                    folderPath,
                    "*." + SupportedFileExtension,
                    SearchOption.AllDirectories
                );
                ProcessFiles(filePaths);
            }
        }

        [MenuItem(MenuName + "/" + SettingsMenuItem)]
        public static void OnSettings() =>
            EditorWindow.GetWindow<SettingsWindow>(
                true,
                SettingsWindowTitle,
                true
            );

        [MenuItem(MenuName + "/" + AboutMenuItem)]
        public static void OnHelp() =>
            DisplayDialogBoxMessage(
                HelpFileNotFoundDialogMessage,
                false
            );

        #endregion

        #region processing files

        private static void ProcessFiles(
            string[] filePaths)
        {
            CommentTypes commentTypes = Settings.GetCommenTypes();

            List<string> validatedFilePaths = filePaths.AsEnumerable().Where(s => !String.IsNullOrEmpty(s)).ToList();

            LogToFile(BatchStartedLogMessage);

            if (validatedFilePaths.Count == 0)
                DisplayDialogBoxMessage(
                    NoFilesFoundDialogMessage,
                    true
                );
            else if (commentTypes == CommentTypes.None)
                DisplayDialogBoxMessage(
                    String.Format(
                        CultureInfo.InvariantCulture,
                        NoCommentTypesSelectedDialogMessage,
                        SettingsMenuItem
                    ),
                    true
                );
            else
            {
                //start batch
                _batchFilesCount = validatedFilePaths.Count;
                _batchProcessedFilesCount = 0;

                List<KeyValuePair<string, string>> filesToWrite = new();
                try
                {
                    filesToWrite = GetModifiedFiles(
                        validatedFilePaths,
                        commentTypes
                    );
                }
                catch (Exception)
                {
                    filesToWrite.Clear();
                    DisplayDialogBoxMessage(
                        ProcessingEncountredCriticalErrorDialogMessage,
                        true
                    );
                    throw;
                }
                finally
                {
                    WriteFilesToDisk(filesToWrite);
                    LogModifiedFilesList(filesToWrite);
                    EditorUtility.ClearProgressBar();
                    //end batch
                    _batchFilesCount = 0;
                    _batchProcessedFilesCount = 0;
                }
            }
        }

        /// <summary>
        /// Returns the files that were modified to add/update tooltips.
        /// This method doesn't write on disk the modified files
        /// </summary>
        /// <param name="filePaths"></param>
        /// <param name="commentTypes"></param>
        /// <returns>A list of pairs. The pair's key is the file's path, and the pair's value is the modified file's content</returns>
        private static List<KeyValuePair<string, string>> GetModifiedFiles(
            List<string> filePaths,
            CommentTypes commentTypes)
        {
            Stopwatch stopWatch = new();
            stopWatch.Start();

            List<KeyValuePair<string, string>> modifiedFiles = new(_batchFilesCount);

            bool operationWasCanceled = false;
            bool exceptionCatched = false;
            foreach (string filePath in filePaths)
            {
                //canceling handling
                if (DisplayCancelableProgressBar(filePath))
                {
                    operationWasCanceled = true;
                    break;
                }

                try
                {
                    string readFileContent = File.ReadAllText(
                        filePath,
                        Encoding.Default
                    );
                    string modifiedFileContent;
                    if (_tooltipGenerator.TryProcessText(
                            readFileContent,
                            out modifiedFileContent,
                            commentTypes
                        ))
                        modifiedFiles.Add(
                            new KeyValuePair<string, string>(
                                filePath,
                                modifiedFileContent
                            )
                        );
                }
                catch (Exception exception)
                {
                    DisplayConsoleMessage(
                        String.Format(
                            RecoverableErrorEncoutredConsoleMessage,
                            filePath
                        ),
                        true
                    );
                    DisplayConsoleException(exception);
                    exceptionCatched = true;
                }
                finally
                {
                    _batchProcessedFilesCount++;
                }
            }

            if (operationWasCanceled)
            {
                modifiedFiles.Clear();
                DisplayDialogBoxMessage(
                    ProcessingInteruptedDialogMessage,
                    true
                );
            }
            else
                DisplayOperationCompletedMessage(
                    modifiedFiles.Count,
                    stopWatch.Elapsed.TotalSeconds,
                    exceptionCatched
                );

            stopWatch.Stop();

            return modifiedFiles;
        }

        private static bool DisplayCancelableProgressBar(
            string fileName) =>
            EditorUtility.DisplayCancelableProgressBar(
                ProcessingInProgressTitle,
                String.Format(
                    CultureInfo.InvariantCulture,
                    ProcessingInProgressDialogMessage,
                    (_batchProcessedFilesCount + 1),
                    _batchFilesCount,
                    fileName
                ),
                (float)_batchProcessedFilesCount / _batchFilesCount
            );

        private static void LogModifiedFilesList(
            List<KeyValuePair<string, string>> filesToWrite)
        {
            if (filesToWrite.Count != 0)
            {
                DisplayConsoleMessage(
                    String.Format(
                        CultureInfo.InvariantCulture,
                        FilesModifiedConsoleMessage
                    ),
                    true
                );
                foreach (string file in filesToWrite.Select(pair => pair.Key).ToList())
                    DisplayConsoleMessage(
                        file,
                        true
                    );
                Debug.Log(
                    String.Format(
                        CultureInfo.InvariantCulture,
                        LogsUpdatedConsoleMessage,
                        LogsFilePath
                    )
                );
            }
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

        private static void DisplayOperationCompletedMessage(
            int writtenFilesCount,
            double operationDurationInSeconds,
            bool wasExceptionCatched)
        {
            string operationCompletedMessage = String.Format(
                CultureInfo.InvariantCulture,
                ProcessingSuccededDialogMessage,
                _batchProcessedFilesCount,
                _batchProcessedFilesCount == 1
                    ? String.Empty
                    : "s",
                operationDurationInSeconds.ToString(
                    "F",
                    CultureInfo.InvariantCulture
                )
            );

            if (wasExceptionCatched)
                operationCompletedMessage += ProcessingSucceededWithRecoverableErrorDialogMessage;

            if (writtenFilesCount == 0)
                operationCompletedMessage += ProcessingSuccededNoFilesModifiedDialogMessage;
            else
                operationCompletedMessage += String.Format(
                    CultureInfo.InvariantCulture,
                    ProcessingSuccededFilesModifiedDialogMessage,
                    writtenFilesCount,
                    writtenFilesCount == 1
                        ? String.Empty
                        : "s"
                );

            if (writtenFilesCount > 0 || wasExceptionCatched)
                operationCompletedMessage += ProcessingSuccededSeeLogsDialogMessage;

            DisplayDialogBoxMessage(
                operationCompletedMessage,
                true
            );
        }

        #endregion

        #region messages displaying

        private static void DisplayDialogBoxMessage(
            string message,
            bool logToFile)
        {
            if (logToFile)
                LogToFile(message);
            EditorUtility.DisplayDialog(
                ToolName,
                message,
                OkButtonDialogBox
            );
        }

        private static void DisplayConsoleMessage(
            string message,
            bool logToFile)
        {
            if (logToFile)
                LogToFile(message);
            Debug.Log(message);
        }

        private static void DisplayConsoleException(
            Exception exception)
        {
            LogToFile(exception.ToString());
            Debug.LogException(exception);
        }

        private static void LogToFile(
            string message)
        {
            using (StreamWriter writer = new(
                       LogsFilePath,
                       true
                   ))
            {
                writer.WriteLine(
                    "{0} : {1}",
                    DateTime.Now,
                    message
                );
            }
        }

        #endregion
    }
}