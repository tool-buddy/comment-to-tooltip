using System;
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

        [MenuItem(
            MenuName + "Process a file...",
            priority = 0
        )]
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

        [MenuItem(
            MenuName + "Process a folder...",
            priority = 1
        )]
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

        [MenuItem(
            MenuName + "Preferences",
            priority = 2
        )]
        public static void OnPreferences() =>
            SettingsService.OpenUserPreferences($"Preferences/{AssetInformation.Name}");

        [MenuItem(
            MenuName + "Help",
            priority = 3
        )]
        public static void OnHelp()
        {
            //Search for ReadMe.txt under any "CommentToTooltip" folder in the project
            string[] guids = AssetDatabase.FindAssets("ReadMe t:TextAsset");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(
                        "ReadMe.txt",
                        StringComparison.OrdinalIgnoreCase
                    )
                    && path.IndexOf(
                        "/CommentToTooltip/",
                        StringComparison.OrdinalIgnoreCase
                    )
                    >= 0)
                {
                    TextAsset readme = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                    if (readme == null)
                        continue;

                    AssetDatabase.OpenAsset(readme);
                    return;
                }
            }

            UserNotifier.DisplayDialogBoxMessage("Could not find ReadMe.txt under a 'CommentToTooltip' folder.");
        }


        [MenuItem(
            PublisherItemsPath + "/Publisher Page"
        )]
        public static void OpenPublisherPage() =>
            Application.OpenURL(AssetInformation.PublisherURL);
    }
}