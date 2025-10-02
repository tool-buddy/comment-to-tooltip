namespace ToolBuddy.CommentToTooltip.Editor.Settings
{
    /// <summary>
    /// Provides access to persisted configuration.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// The folder that is displayed when opening a file or folder panel.
        /// </summary>
        string InitialFolderPath { get; set; }

        /// <summary>
        /// Gets the parsing behaviour that should be applied when processing comments.
        /// </summary>
        ParsingSettings ParsingSettings { get; }


        /// <summary>
        /// Persists new parsing preferences that control how comments are interpreted.
        /// </summary>
        /// <param name="parseSingleLineDocumentationComments">Whether single line documentation comments should be parsed.</param>
        /// <param name="parseDelimitedDocumentationComments">Whether delimited documentation comments should be parsed.</param>
        /// <param name="parseSingleLineComments">Whether single line comments should be parsed.</param>
        void UpdateParsingSettings(
            bool parseSingleLineDocumentationComments,
            bool parseDelimitedDocumentationComments,
            bool parseSingleLineComments);
    }
}