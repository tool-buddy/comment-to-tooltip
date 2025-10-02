namespace ToolBuddy.CommentToTooltip.TextProcessing
{
    /// <summary>
    /// Provides functionality for transforming text based on selected comment parsing rules.
    /// </summary>
    public interface ITextProcessor
    {
        /// <summary>
        /// Attempts to process the supplied text with the specified comment parsing options.
        /// </summary>
        /// <param name="textToProcess">The raw text to inspect and transform.</param>
        /// <param name="processedText">The processed text output.</param>
        /// <param name="commentTypes">Which comment types should be considered during processing.</param>
        /// <returns><see langword="true" /> when the text is transformed; otherwise, <see langword="false" />.</returns>
        bool TryProcessText(
            string textToProcess,
            out string processedText,
            CommentTypes commentTypes);
    }
}