using System;
using System.Globalization;
using ToolBuddy.CommentToTooltip.FileProcessing;
using UnityEditor;
using UnityEngine;

namespace ToolBuddy.CommentToTooltip.Editor
{
    /// <inheritdoc />
    /// <remarks>
    /// Uses Modal Dialogs for user notifications.
    /// Automatically notifies the user of processing progress, completion, cancellation and errors.
    /// </remarks>
    public sealed class EditorUserNotifier : IUserNotifier
    {
        private readonly IFileProcessor _processor;

        public EditorUserNotifier(
            IFileProcessor processor)
        {
            _processor = processor;

            _processor.NoFileToProcess += OnNoFileToProcess;
            _processor.NoCommentTypeSelected += OnNoCommentTypeSelected;
            _processor.ProcessingCompleted += OnProcessingCompleted;
            _processor.ProcessingError += OnProcessingError;
            _processor.CancellationCheck += OnCancellationCheck;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _processor.NoFileToProcess -= OnNoFileToProcess;
            _processor.NoCommentTypeSelected -= OnNoCommentTypeSelected;
            _processor.ProcessingCompleted -= OnProcessingCompleted;
            _processor.ProcessingError -= OnProcessingError;
            _processor.CancellationCheck -= OnCancellationCheck;
        }

        /// <inheritdoc />
        public void DisplayMessage(
            string message) =>
            EditorUtility.DisplayDialog(
                AssetInformation.Name,
                message,
                "Ok"
            );

        private void OnNoFileToProcess() =>
            DisplayMessage("No file(s) found to process.");

        private void OnNoCommentTypeSelected() =>
            DisplayMessage("No comment type(s) selected. Please select at least one in the Preferences menu.");

        private bool OnCancellationCheck(
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

        private void OnProcessingError(
            Exception e)
        {
            DisplayMessage(
                "An unexpected error occurred while processing a file. No changes were applied. You can find more details in the console."
            );
            Debug.LogException(e);
        }

        private void OnProcessingCompleted(
            FileProcessingResult result)
        {
            EditorUtility.ClearProgressBar();

            if (result.Canceled)
                DisplayMessage("The operation was cancelled by the user. No changes were applied.");
            else
            {
                DisplayMessage(GetOperationCompletionMessage(result));

                if (result.ModifiedFiles.Count != 0)
                {
                    Debug.Log("Tooltips added or modified in the following files:");
                    foreach (FileModificationInfo file in result.ModifiedFiles)
                        Debug.Log(file.FilePath);
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
            string message = $"Processing of {result.TotalFilesCount} file(s) completed successfully in {operationDuration}s. ";

            if (writtenFilesCount == 0)
                message += "No file(s) were modified. ";
            else
                message += $"{writtenFilesCount} file(s) modified. ";

            if (writtenFilesCount > 0)
                message += "You can find more details in the console.";

            return message;
        }
    }
}