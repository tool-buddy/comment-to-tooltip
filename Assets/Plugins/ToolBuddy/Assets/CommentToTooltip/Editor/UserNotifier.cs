using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace ToolBuddy.CommentToTooltip.Editor
{
    [InitializeOnLoad]
    internal static class UserNotifier
    {
        static UserNotifier()
        {
            FileProcessor.NoFileToProcess += OnNoFileToProcess;
            FileProcessor.NoCommentTypeSelected += OnNoCommentTypeSelected;
            FileProcessor.ProcessingCompleted += OnProcessingCompleted;
            FileProcessor.ProcessingError += OnProcessingError;
            FileProcessor.CancellationCheck += OnCancellationCheck;
        }

        private static void OnNoFileToProcess() =>
            DisplayDialogBoxMessage(
                "No file(s) found to process."
            );

        private static void OnNoCommentTypeSelected() =>
            DisplayDialogBoxMessage(
                "No comment type(s) selected. Please select at least one in the Preferences menu."
            );

        private static bool OnCancellationCheck(
            int nextIndex,
            int total,
            string path) =>
            EditorUtility.DisplayCancelableProgressBar(
                "File processing",
                $"Processing file {nextIndex} out of {total} : {path}",
                total <= 0
                    ? 0f
                    : (float)(nextIndex - 1) / total
            );

        private static void OnProcessingError(
            Exception e)
        {
            DisplayDialogBoxMessage(
                "An unexpected error occurred while processing a file. No changes were applied. You can find more details in the console."
            );
            Debug.LogException(e);
        }

        private static void OnProcessingCompleted(
            FileProcessingResult result)
        {
            EditorUtility.ClearProgressBar();

            if (result.Canceled)
                DisplayDialogBoxMessage(
                    "The operation was cancelled by the user. No changes were applied."
                );
            else
            {
                DisplayDialogBoxMessage(
                    GetOperationCompletionMessage(result)
                );

                if (result.ModifiedFiles.Count != 0)
                {
                    Debug.Log(
                        "Tooltips added or modified in the following files:"
                    );

                    foreach (KeyValuePair<string, string> pair in result.ModifiedFiles)
                    {
                        string filePath = pair.Key;
                        Debug.Log(filePath);
                    }
                }
            }
        }

        private static string GetOperationCompletionMessage(
            FileProcessingResult result)
        {
            int writtenFilesCount = result.ModifiedFiles.Count;

            string operationDuration = result.DurationSeconds.ToString(
                "F",
                CultureInfo.InvariantCulture
            );
            string operationCompletedMessage =
                $"Processing of {result.TotalFilesCount} file(s) completed successfully in {operationDuration}s. ";

            if (writtenFilesCount == 0)
                operationCompletedMessage += "No file(s) were modified. ";
            else
                operationCompletedMessage += $"{writtenFilesCount} file(s) modified. ";

            if (writtenFilesCount > 0)
                operationCompletedMessage += "You can find more details in the console.";

            return operationCompletedMessage;
        }

        public static void DisplayDialogBoxMessage(
            string message) =>
            EditorUtility.DisplayDialog(
                AssetInformation.Name,
                message,
                "Ok"
            );
    }
}