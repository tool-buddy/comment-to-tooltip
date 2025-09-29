using System.IO;
using ToolBuddy.CommentToTooltip.Editor.Settings;
using UnityEditor;
using UnityEngine;

namespace ToolBuddy.CommentToTooltip.Editor
{
    public static class MenuItems
    {
        private const string PublisherItemsPath = "Tools/" + AssetInformation.Publisher;
        private const string MenuName = PublisherItemsPath + "/" + AssetInformation.Name + "/";

        [MenuItem(MenuName + "Process a file...")]
        public static void OnProcessFile()
        {
            string filePath = EditorUtility.OpenFilePanel(
                "Select a file",
                SettingsStorage.InitialFolderPath,
                "cs"
            );

            if (File.Exists(filePath))
                FileProcessor.ProcessFile(filePath);
        }

        [MenuItem(MenuName + "Process a folder...")]
        public static void OnProcessAFolder()
        {
            string folderPath = EditorUtility.OpenFolderPanel(
                "Select a folder",
                SettingsStorage.InitialFolderPath,
                ""
            );

            if (Directory.Exists(folderPath))
                FileProcessor.ProcessFolder(folderPath);
        }

        [MenuItem(MenuName + "Preferences")]
        public static void OnPreferences() =>
            SettingsService.OpenUserPreferences($"Preferences/{AssetInformation.Name}");

        [MenuItem(MenuName + "Help")]
        public static void OnHelp() =>
            UserNotifier.DisplayDialogBoxMessage(
                "Help can be found in the ReadMe.txt file"
            );

        [MenuItem(
            PublisherItemsPath + "/Publisher Page"
        )]
        public static void OpenPublisherPage() =>
            Application.OpenURL(AssetInformation.PublisherURL);

    }
}