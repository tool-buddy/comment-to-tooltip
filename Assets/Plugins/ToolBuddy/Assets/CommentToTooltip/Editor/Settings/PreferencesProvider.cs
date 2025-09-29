using UnityEditor;
using UnityEngine;

namespace ToolBuddy.CommentToTooltip.Editor.Settings
{
    internal static class PreferencesProvider
    {
        private static GUIStyle _headerStyle;
        private static GUIStyle _descStyle;

        private static void EnsureStyles()
        {
            if (_headerStyle == null)
                _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12
                };

            if (_descStyle == null)
                _descStyle = new GUIStyle(EditorStyles.wordWrappedLabel);
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider() =>
            new(
                $"Preferences/{AssetInformation.Name}",
                SettingsScope.User
            )
            {
                label = AssetInformation.Name,
                guiHandler = GUIHandler,
                keywords = AssetInformation.Name.Split(' ')
            };

        private static void GUIHandler(
            string ctx)
        {
            EnsureStyles();


            GUILayout.Space(6);

            GUILayout.Label(
                "Select which comment types will be parsed into [Tooltip] attributes.",
                _descStyle
            );

            GUILayout.Space(6);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(4);

            DrawParsingSettings(
                SettingsStorage.ParsingSettings,
                out bool parseSingleLineDocumentation,
                out bool parseDelimitedDocumentation,
                out bool parseSingleLine
            );

            GUILayout.Space(4);
            EditorGUILayout.EndVertical();

            // Footer actions
            if (GUILayout.Button(
                    "Reset to Defaults"
                ))
            {
                // Recommended defaults
                parseSingleLineDocumentation = true;
                parseDelimitedDocumentation = true;
                parseSingleLine = true;
                GUI.FocusControl(null);
            }

            SettingsStorage.UpdateParsingSettings(
                parseSingleLineDocumentation,
                parseDelimitedDocumentation,
                parseSingleLine
            );
        }

        private static void DrawParsingSettings(
            ParsingSettings parsingSettings,
            out bool parseSingleLineDocumentation,
            out bool parseDelimitedDocumentation,
            out bool parseSingleLine)
        {
            // XML doc (triple-slash)
            parseSingleLineDocumentation = EditorGUILayout.ToggleLeft(
                "XML doc comments (triple-slash)",
                parsingSettings.ParseSingleLineDocumentationComments
            );
            EditorGUILayout.HelpBox(
                "/// <summary> Comment here </summary>",
                MessageType.None
            );

            GUILayout.Space(4);

            // XML doc (block)
            parseDelimitedDocumentation = EditorGUILayout.ToggleLeft(
                "XML doc comments (block)",
                parsingSettings.ParseDelimitedDocumentationComments
            );
            EditorGUILayout.HelpBox(
                "/**<summary> Comment here </summary>*/",
                MessageType.None
            );

            GUILayout.Space(4);

            // Line comment (//)
            parseSingleLine = EditorGUILayout.ToggleLeft(
                "Line comments (//)",
                parsingSettings.ParseSingleLineComments
            );
            EditorGUILayout.HelpBox(
                "// Comment here",
                MessageType.None
            );
        }
    }
}