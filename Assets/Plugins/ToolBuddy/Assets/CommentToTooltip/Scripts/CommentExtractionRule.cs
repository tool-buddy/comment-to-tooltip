using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace ToolBuddy.CommentToTooltip
{
    /// <summary>
    /// Defines the parameters for processing (parsing) code to extract the comments to be put in the tooltips
    /// </summary>
    public class CommentExtractionRule
    {
        /// <summary>
        /// Regex that extracts from the code the comment line(s)
        /// </summary>
        /// <remarks>
        /// Capture groups are:
        /// beginning => tabulations and spaces at line beginning
        /// documentation => each documentation line
        /// tooltip => existing tooltip attribute
        /// tooltipContent => existing tooltip attribute's string content
        /// field => field declaration
        /// </remarks>
        [NotNull]
        public Regex CommentedFieldPattern { get; private set; }

        /// <summary>
        /// Regex that extracts the documentation capture group into the actual comment.
        /// Is optional: If no documentation to comment transformation needed, set this to null
        /// </summary>
        [CanBeNull]
        public Regex CommentExtractor { get; private set; }

        /// <summary>
        /// The <see cref="SupportedTypes"/> that are processed by this
        /// </summary>
        public CommentTypes SupportedTypes { get; private set; }

        public CommentExtractionRule(
            [NotNull] Regex commentedFieldPattern,
            [CanBeNull] Regex commentExtractor,
            CommentTypes supportedType)
        {
            CommentedFieldPattern = commentedFieldPattern;
            CommentExtractor = commentExtractor;
            SupportedTypes = supportedType;
        }
    }
}