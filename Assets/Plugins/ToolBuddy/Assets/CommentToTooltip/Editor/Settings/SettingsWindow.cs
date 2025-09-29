using UnityEditor;
using UnityEngine;

namespace ToolBuddy.CommentToTooltip.Editor.Settings
{
    public class SettingsWindow : EditorWindow
    {
        public SettingsWindow() =>
            minSize = new Vector2(
                350,
                100
            );

        public void OnGUI()
        {
            Settings settings = SettingsStorage.Settings;

            GUILayout.Label(
                "Select the comment types to process",
                EditorStyles.boldLabel
            );

            EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth * 0.9f;

            bool parseSingleLineDocumentation = EditorGUILayout.Toggle(
                "/// <summary> Comment here </summary>",
                settings.ParseSingleLineDocumentationComments
            );
            bool parseDelimitedDocumentation = EditorGUILayout.Toggle(
                "/**<summary> Comment here </summary>*/",
                settings.ParseDelimitedDocumentationComments
            );
            bool parseSingleLine = EditorGUILayout.Toggle(
                "// Comment here",
                settings.ParseSingleLineComments
            );

            SettingsStorage.Update(
                parseSingleLineDocumentation,
                parseDelimitedDocumentation,
                parseSingleLine
            );
        }
    }
}