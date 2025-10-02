using System;
using System.IO;
using ToolBuddy.CommentToTooltip.Editor.Settings;
using ToolBuddy.CommentToTooltip.FileProcessing;
using UnityEditor;
using UnityEngine;
using SettingsService = UnityEditor.SettingsService;

namespace ToolBuddy.CommentToTooltip.Editor
{
    public class MenuItems
    {
        private const string PublisherItemsPath = "Tools/" + AssetInformation.Publisher;
        private const string MenuName = PublisherItemsPath + "/" + AssetInformation.Name + "/";

        private static readonly IFileProcessor _fileProcessor;
        private static readonly ISettingsService _settings;
        private static readonly IUserNotifier _userNotifier;

        static MenuItems()
        {
            _fileProcessor = EditorCompositionRoot.Resolve<IFileProcessor>();
            _settings = EditorCompositionRoot.Resolve<ISettingsService>();
            _userNotifier = EditorCompositionRoot.Resolve<IUserNotifier>();
        }

        [MenuItem(
            MenuName + "Process a file...",
            priority = 0
        )]
        public static void OnProcessFile()
        {
            string filePath = EditorUtility.OpenFilePanel(
                "Select a file",
                _settings.InitialFolderPath,
                "cs"
            );

            if (File.Exists(filePath))
            {
                DirectoryInfo directoryInfo = new FileInfo(filePath).Directory;
                if (directoryInfo != null)
                    _settings.InitialFolderPath = directoryInfo.FullName;
                _fileProcessor.ProcessFile(
                    filePath,
                    _settings.ParsingSettings.GetCommentTypes()
                );
            }
        }

        [MenuItem(
            MenuName + "Process a folder...",
            priority = 1
        )]
        public static void OnProcessAFolder()
        {
            string folderPath = EditorUtility.OpenFolderPanel(
                "Select a folder",
                _settings.InitialFolderPath,
                ""
            );

            if (Directory.Exists(folderPath))
            {
                _settings.InitialFolderPath = folderPath;
                _fileProcessor.ProcessFolder(
                    folderPath,
                    _settings.ParsingSettings.GetCommentTypes()
                );
            }
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

            _userNotifier.DisplayMessage("Could not find ReadMe.txt under a 'CommentToTooltip' folder.");
        }


        [MenuItem(
            PublisherItemsPath + "/Publisher Page"
        )]
        public static void OpenPublisherPage() =>
            Application.OpenURL(AssetInformation.PublisherURL);
    }
}