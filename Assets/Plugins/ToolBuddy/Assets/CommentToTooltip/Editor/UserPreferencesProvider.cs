using ToolBuddy.CommentToTooltip.Editor.Settings;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ToolBuddy.CommentToTooltip.Editor
{
    public sealed class UserPreferencesProvider : SettingsProvider
    {
        private GUIStyle _descStyle;
        private GUIStyle _headerStyle;

        private ISettingsService _settings;

        public UserPreferencesProvider()
            : base(
                $"Preferences/{AssetInformation.Name}",
                SettingsScope.User
            )
        {
            label = AssetInformation.Name;
            keywords = AssetInformation.Name.Split(' ');
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider() => new UserPreferencesProvider();

        public override void OnActivate(
            string searchContext,
            VisualElement rootElement)
        {
            _settings = EditorCompositionRoot.Resolve<ISettingsService>();

            if (_headerStyle == null)
                _headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12 };

            if (_descStyle == null)
                _descStyle = new GUIStyle(EditorStyles.wordWrappedLabel);
        }

        public override void OnGUI(
            string searchContext)
        {
            if (_settings == null)
            {
                EditorGUILayout.HelpBox(
                    "Settings service is not available.",
                    MessageType.Error
                );
                return;
            }

            GUILayout.Space(6);
            GUILayout.Label(
                "Select which comment types will be parsed into [Tooltip] attributes.",
                _descStyle
            );
            GUILayout.Space(6);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(4);

            DrawParsingSettings(
                _settings.ParsingSettings,
                out bool parseSingleLineDocumentation,
                out bool parseDelimitedDocumentation,
                out bool parseSingleLine
            );

            GUILayout.Space(4);
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Reset to Defaults"))
            {
                parseSingleLineDocumentation = true;
                parseDelimitedDocumentation = true;
                parseSingleLine = true;
                GUI.FocusControl(null);
            }

            _settings.UpdateParsingSettings(
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