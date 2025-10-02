namespace ToolBuddy.CommentToTooltip.Editor.Settings
{
    public interface ISettingsService
    {
        /// <summary>
        /// The folder that is displayed when opening a file or folder panel
        /// </summary>
        string InitialFolderPath { get; set; }

        ParsingSettings ParsingSettings { get; }


        void UpdateParsingSettings(
            bool parseSingleLineDocumentationComments,
            bool parseDelimitedDocumentationComments,
            bool parseSingleLineComments);
    }
}