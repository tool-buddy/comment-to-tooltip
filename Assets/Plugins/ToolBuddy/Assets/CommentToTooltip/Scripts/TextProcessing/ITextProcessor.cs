namespace ToolBuddy.CommentToTooltip.TextProcessing
{
    public interface ITextProcessor
    {
        bool TryProcessText(
            string textToProcess,
            out string processedText,
            CommentTypes commentTypes);
    }
}