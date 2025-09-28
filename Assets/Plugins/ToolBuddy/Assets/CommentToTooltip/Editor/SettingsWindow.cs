using UnityEditor;
using UnityEngine;

namespace ToolBuddy.CommentToTooltip.Editor
{
    public class SettingsWindow : EditorWindow
    {
        private const string Label = "Select the comment types to process";
        private const string Toggle1Text = @"/// <summary> Comment here </summary>";
        private const string Toggle2Text = @"/**<summary> Comment here </summary>*/";
        private const string Toggle3Text = @"// Comment here";

        public SettingsWindow() =>
            minSize = new Vector2(
                350,
                100
            );

        public void OnDestroy() =>
            Menu.Settings.WriteToEditorPreferences();

        public void OnGUI()
        {
            GUILayout.Label(
                Label,
                EditorStyles.boldLabel
            );

            EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth * 0.9f;

            Menu.Settings.ParseSingleLineDocumentationComments = EditorGUILayout.Toggle(
                Toggle1Text,
                Menu.Settings.ParseSingleLineDocumentationComments
            );
            Menu.Settings.ParseDelimitedDocumentationComments = EditorGUILayout.Toggle(
                Toggle2Text,
                Menu.Settings.ParseDelimitedDocumentationComments
            );
            Menu.Settings.ParseSingleLineComments = EditorGUILayout.Toggle(
                Toggle3Text,
                Menu.Settings.ParseSingleLineComments
            );
        }

        public void OnFocus() =>
            Menu.Settings.ReadFromEditorPreferences();

        public void OnLostFocus() =>
            Menu.Settings.WriteToEditorPreferences();
    }
}