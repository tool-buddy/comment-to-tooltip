using UnityEditor;

namespace ToolBuddy.CommentToTooltip.Editor
{
    public class Settings
    {
        private const string EditorPreferencesKey1 = "C2T_SingleLineDocumentation";
        private const string EditorPreferencesKey2 = "C2T_DelimitedDocumentation";
        private const string EditorPreferencesKey3 = "C2T_SingleLine";

        public bool ParseSingleLineDocumentationComments { get; set; }
        public bool ParseDelimitedDocumentationComments { get; set; }
        public bool ParseSingleLineComments { get; set; }

        public void ReadFromEditorPreferences()
        {
            ParseSingleLineDocumentationComments = EditorPrefs.GetBool(
                EditorPreferencesKey1,
                true
            );
            ParseDelimitedDocumentationComments = EditorPrefs.GetBool(
                EditorPreferencesKey2,
                true
            );
            ParseSingleLineComments = EditorPrefs.GetBool(
                EditorPreferencesKey3,
                true
            );
        }

        public void WriteToEditorPreferences()
        {
            EditorPrefs.SetBool(
                EditorPreferencesKey1,
                ParseSingleLineDocumentationComments
            );
            EditorPrefs.SetBool(
                EditorPreferencesKey2,
                ParseDelimitedDocumentationComments
            );
            EditorPrefs.SetBool(
                EditorPreferencesKey3,
                ParseSingleLineComments
            );
        }

        public CommentTypes GetCommenTypes()
        {
            CommentTypes result = CommentTypes.None;
            if (ParseSingleLineDocumentationComments)
                result = result | CommentTypes.SingleLineDocumentation;
            if (ParseDelimitedDocumentationComments)
                result = result | CommentTypes.DelimitedDocumentation;
            if (ParseSingleLineComments)
                result = result | CommentTypes.SingleLine;
            return result;
        }
    }
}