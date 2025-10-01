using System;
using System.Text.RegularExpressions;

namespace ToolBuddy.CommentToTooltip.CommentParsing
{
    public static class ParsingConfigBuilder
    {
        private const string NewLineRegex = @"(?:\r)?\n";

        /// <summary>
        /// Builds a <see cref="ParsingConfig"/> for the specified comment type.
        /// </summary>
        /// <param name="commentType">The type of comment for which the rule is being created.</param>
        /// <returns>A <see cref="ParsingConfig"/> configured for the specified <paramref name="commentType"/>.</returns>
        public static ParsingConfig BuildConfigFor(
            CommentTypes commentType) =>
            new(
                GetCommentedFieldPattern(commentType),
                GetDocumentationCommentPattern(commentType),
                commentType
            );

        /// <summary>
        /// Creates a <see cref="Regex"/> pattern to match commented field declarations based on the specified comment
        /// type.
        /// </summary>
        /// <param name="commentTypes">The type of comments to match, such as single-line, delimited, or documentation comments.</param>
        /// <returns>A <see cref="Regex"/> instance configured to match field declarations with optional comments, attributes,
        /// and tooltips.</returns>
        /// <exception cref="commentTypes">Thrown if the <paramref name="commentTypes"/> parameter specifies an unsupported comment type.</exception>
        private static Regex GetCommentedFieldPattern(
            CommentTypes commentTypes)
        {
            /* capture groups:
            /// beginning => tabulations and spaces at line beginning
            /// documentation => each documentation line
            /// tooltip => existing tooltip attribute
            /// tooltipContent => existing tooltip attribute's string content
            /// field => field declaration
            */

            const string nonTooltipAttributes =
                @"(?<attributes>^[ \t]*\[(?!([ \t]*(?:UnityEngine.)?Tooltip))[^]]+\]\s*(?=^))*";

            const string existingTooltip =
                @"(?<tooltip>^[ \t]*\[(?:UnityEngine.)?Tooltip\(""(?<tooltipContent>[^""]*)""\)\]\s*(?=^))?";

            const string fieldDeclaration =
                @"(?<field>(?<beginning>^[ \t]*)((?<modifier>public|private|internal|protected)\s+)?[^\s;=]+\s+[^\s;=\\(]+\s*(?:=(?!>)[^;]+)?;)";

            const string nonCommentRegexPart =
                @"\s*(?=^)"
                + nonTooltipAttributes
                + existingTooltip
                + nonTooltipAttributes
                + fieldDeclaration;

            Regex result;
            //todo avoid switching on a flags enum
            switch (commentTypes)
            {
                case CommentTypes.SingleLineDocumentation:
                    result = new Regex(
                        $@"(?>^[ \t]*///[ \t]?(?>(?<documentation>[^\r\n]*)){NewLineRegex})+{nonCommentRegexPart}",
                        RegexOptions.Multiline | RegexOptions.Compiled
                    );
                    break;
                case CommentTypes.DelimitedDocumentation:

                    result = new Regex(
                        $@"(?>^[ \t]*/\*)(?:(?>[ \t]*\*[ \t]?)(?<documentation>[^\r\n]*)(?:{NewLineRegex})?)+[ \t]*\*/[ \t]*{NewLineRegex}{nonCommentRegexPart}",
                        RegexOptions.Multiline | RegexOptions.Compiled
                    );
                    break;
                case CommentTypes.SingleLine:
                    result = new Regex(
                        $@"(?>^[ \t]*//(?!/)[ \t]?(?>(?<documentation>[^\r\n]*)){NewLineRegex})+{nonCommentRegexPart}",
                        RegexOptions.Multiline | RegexOptions.Compiled
                    );
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(commentTypes),
                        commentTypes,
                        null
                    );
            }

            return result;
        }

        /// <summary>
        /// Returns a Regex that extracts the comment from an XML documentation.
        /// </summary>
        private static Regex GetDocumentationCommentPattern(
            CommentTypes commentTypes)
        {
            Regex result;
            //todo avoid switching on a flags enum
            switch (commentTypes)
            {
                case CommentTypes.SingleLineDocumentation:
                case CommentTypes.DelimitedDocumentation:
                    // Match across actual newlines using NewLineRegex and [\s\S]*? for non-greedy multiline capture
                    result = new Regex(
                        $@"\s*<summary>\s*(?:{NewLineRegex})?(?<comment>[\s\S]*?(?=(?:{NewLineRegex})?\s*</summary\s*))",
                        RegexOptions.Multiline | RegexOptions.Compiled
                    );
                    break;
                case CommentTypes.SingleLine:
                    result = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(commentTypes),
                        commentTypes,
                        null
                    );
            }

            return result;
        }
    }
}