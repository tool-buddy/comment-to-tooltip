using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace ToolBuddy.CommentToTooltip.Editor.Settings
{
    public sealed class EditoPrefsSettingsService : ISettingsService
    {
        private const string EditorPreferencesKey1 = "C2T_SingleLineDocumentation";
        private const string EditorPreferencesKey2 = "C2T_DelimitedDocumentation";
        private const string EditorPreferencesKey3 = "C2T_SingleLine";
        private const string InitialFolderPathPreferenceKey = "C2T_InitialFolderPath";

        private readonly ParsingSettings _parsingSettings;

        private string _initialFolderPath;

        public EditoPrefsSettingsService()
        {
            _parsingSettings = ReadFromEditorPreferences();
            _initialFolderPath = EditorPrefs.GetString(
                InitialFolderPathPreferenceKey,
                Application.dataPath
            );
        }

        /// <inheritdoc />
        public string InitialFolderPath
        {
            get => _initialFolderPath;
            set
            {
                if (_initialFolderPath == value)
                    return;

                _initialFolderPath = value;
                EditorPrefs.SetString(
                    InitialFolderPathPreferenceKey,
                    value
                );
            }
        }

        public ParsingSettings ParsingSettings => _parsingSettings;

        public void UpdateParsingSettings(
            bool parseSingleLineDocumentationComments,
            bool parseDelimitedDocumentationComments,
            bool parseSingleLineComments)
        {
            bool wasUpdated = _parsingSettings.Update(
                parseSingleLineDocumentationComments,
                parseDelimitedDocumentationComments,
                parseSingleLineComments
            );

            if (wasUpdated)
                WriteToEditorPreferences(_parsingSettings);
        }

        [NotNull]
        private static ParsingSettings ReadFromEditorPreferences() =>
            new(
                EditorPrefs.GetBool(
                    EditorPreferencesKey1,
                    true
                ),
                EditorPrefs.GetBool(
                    EditorPreferencesKey2,
                    true
                ),
                EditorPrefs.GetBool(
                    EditorPreferencesKey3,
                    true
                )
            );

        private static void WriteToEditorPreferences(
            [NotNull] ParsingSettings parsingSettings)
        {
            if (parsingSettings == null)
                throw new ArgumentNullException(nameof(parsingSettings));

            EditorPrefs.SetBool(
                EditorPreferencesKey1,
                parsingSettings.ParseSingleLineDocumentationComments
            );
            EditorPrefs.SetBool(
                EditorPreferencesKey2,
                parsingSettings.ParseDelimitedDocumentationComments
            );
            EditorPrefs.SetBool(
                EditorPreferencesKey3,
                parsingSettings.ParseSingleLineComments
            );
        }
    }
}