using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ToolBuddy.CommentToTooltip
{
    /// <summary>
    /// A class containing a set of methods for generating Unity's tooltips from existing code comments.
    /// </summary>
    public class TooltipGenerator
    {
        private readonly CommentExtractionRule[] _extractionRules;
        private readonly StringBuilder _documentationBuilder;
        private readonly string _lineEnding;
        private readonly StringBuilder _tooltipTagBuilder;

        private const string NewLineRegex = @"(?:\r)?\n";


        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public TooltipGenerator()
        {
            _tooltipTagBuilder = new StringBuilder();
            _documentationBuilder = new StringBuilder();
            _lineEnding = Environment.NewLine;
            _extractionRules = GetCommentExtractionRules();
        }

        /// <summary>
        /// Processes the given text by updating it with tooltips generated from valid comments.
        /// </summary>
        /// <param name="textToProcess"> The input text. </param>
        /// <param name="processedText"> The output text. If method returns false, this text will be equal to <paramref name="textToProcess"/>. </param>
        /// <param name="commentTypes"> The <see cref="CommentTypes"/> to be considered while generating the tooltips. </param>
        /// <returns> True if the text was updated.</returns>
        public bool TryProcessText(
            string textToProcess,
            out string processedText,
            CommentTypes commentTypes)
        {
            processedText = textToProcess;

            bool fileWasModified = false;

            foreach (CommentExtractionRule codeProcessor in _extractionRules)
            {
                if ((codeProcessor.SupportedTypes & commentTypes) == CommentTypes.None)
                    continue;

                int insertedTextLength = 0;
                MatchCollection matches = codeProcessor.CommentedFieldPattern.Matches(processedText);
                for (int matchIndex = 0; matchIndex < matches.Count; matchIndex++)
                {
                    Match match = matches[matchIndex];
                    GroupCollection groups = match.Groups;
                    string tooltipContent = BuildTooltipContent(
                        groups["documentation"].Captures,
                        codeProcessor.CommentExtractor
                    );

                    //if existing tooltip is different from the generated one
                    if (tooltipContent != groups["tooltipContent"].ToString())
                    {
                        _tooltipTagBuilder.Append(groups["beginning"]);
                        //tooltip attribute beginning
                        _tooltipTagBuilder.Append("[UnityEngine.Tooltip(\"");
                        _tooltipTagBuilder.Append(tooltipContent);
                        //tooltip attribute end
                        _tooltipTagBuilder.Append("\")]");
                        _tooltipTagBuilder.Append(_lineEnding);
                        string tooltip = _tooltipTagBuilder.ToString();
                        _tooltipTagBuilder.Length = 0;
                        _tooltipTagBuilder.Capacity = 0;

                        //remove possible old tooltip
                        Group oldTooltipGroup = groups["tooltip"];
                        int oldTooltipLength = oldTooltipGroup.Length;
                        if (oldTooltipLength > 0)
                        {
                            processedText = processedText.Remove(
                                insertedTextLength + oldTooltipGroup.Index,
                                oldTooltipLength
                            );
                            insertedTextLength -= oldTooltipLength;
                        }

                        //insert tooltip in text
                        processedText = processedText.Insert(
                            insertedTextLength + groups["field"].Index,
                            tooltip
                        );
                        insertedTextLength += tooltip.Length;

                        fileWasModified = true;
                    }
                }
            }

            return fileWasModified;
        }

        private CommentExtractionRule[] GetCommentExtractionRules() =>
            new[]
            {
                new CommentExtractionRule(
                    GetCommentedFieldPattern(CommentTypes.SingleLineDocumentation),
                    GetDocumentationCommentPattern(),
                    CommentTypes.SingleLineDocumentation
                ),
                new CommentExtractionRule(
                    GetCommentedFieldPattern(CommentTypes.DelimitedDocumentation),
                    GetDocumentationCommentPattern(),
                    CommentTypes.DelimitedDocumentation
                ),
                new CommentExtractionRule(
                    GetCommentedFieldPattern(CommentTypes.SingleLine),
                    null,
                    CommentTypes.SingleLine
                )
            };

        /// <summary>
        /// Creates a <see cref="Regex"/> pattern to match commented field declarations based on the specified comment
        /// type.
        /// </summary>
        /// <param name="commentTypes">The type of comments to match, such as single-line, delimited, or documentation comments.</param>
        /// <returns>A <see cref="Regex"/> instance configured to match field declarations with optional comments, attributes,
        /// and tooltips.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="commentTypes"/> parameter specifies an unsupported comment type.</exception>
        private Regex GetCommentedFieldPattern(
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
            const string nonCommentRegexPart =
                @"\s*(?=^)"
                + nonTooltipAttributes
                + @"(?<tooltip>^[ \t]*\[(?:UnityEngine.)?Tooltip\(""(?<tooltipContent>[^""]*)""\)\]\s*(?=^))?"
                + nonTooltipAttributes
                + @"(?<field>(?<beginning>^[ \t]*)public\s+[^\s;=]+\s+[^\s;=]+\s*(?>=[^;]+)?;)";

            Regex result;
            //todo avoid switching on a flags enum
            switch (commentTypes)
            {
                case CommentTypes.SingleLineDocumentation:
                    result = new Regex(
                        $@"(?>^[ \t]*///[ \t]?(?>(?<documentation>[^\r\n]*)){NewLineRegex})+{nonCommentRegexPart}",
                        RegexOptions.Multiline
                    );
                    break;
                case CommentTypes.DelimitedDocumentation:

                    result = new Regex(
                        $@"(?>^[ \t]*/\*)(?:(?>[ \t]*\*[ \t]?)(?<documentation>[^\r\n]*)(?:{NewLineRegex})?)+[ \t]*\*/[ \t]*{NewLineRegex}{nonCommentRegexPart}",
                        RegexOptions.Multiline
                    );
                    break;
                case CommentTypes.SingleLine:
                    result = new Regex(
                        $@"(?>^[ \t]*//(?!/)[ \t]?(?>(?<documentation>[^\r\n]*)){NewLineRegex})+{nonCommentRegexPart}",
                        RegexOptions.Multiline
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
        private Regex GetDocumentationCommentPattern()
        {
            string doubleEscapedNewLine = _lineEnding.Replace(
                "\r",
                @"\\r"
            ).Replace(
                "\n",
                @"\\n"
            );

            return new(
                $@"\s*<summary>\s*(?:{doubleEscapedNewLine})?(?<comment>.*?(?=(?:{doubleEscapedNewLine})?\s*</summary\s*))",
                RegexOptions.Multiline
            );
        }

        private string BuildTooltipContent(
            CaptureCollection documentationCaptures,
            Regex commentExtractor)
        {
            string escapedNewLineLiteral = _lineEnding.Replace(
                "\r",
                @"\r"
            ).Replace(
                "\n",
                @"\n"
            );

            //constructing the documentation text
            int capturesCount = documentationCaptures.Count;
            for (int captureIndex = 0; captureIndex < capturesCount; captureIndex++)
            {
                Capture capturedLine = documentationCaptures[captureIndex];
                _documentationBuilder.Append(capturedLine);

                //new line if there is other lines to add
                if (captureIndex != capturesCount - 1)
                    _documentationBuilder.Append(escapedNewLineLiteral);
            }

            string documentation = _documentationBuilder.ToString();
            _documentationBuilder.Clear();

            string tooltipContent;
            if (commentExtractor != null)
            {
                //extracting the significant meaningful part of the documentation
                Match match = commentExtractor.Match(documentation);
                if (!match.Success)
                    throw new InvalidOperationException(
                        String.Format(
                            CultureInfo.InvariantCulture,
                            "Could not parse the following documentation xml '{0}'",
                            documentation
                        )
                    );

                tooltipContent = match.Groups["comment"].ToString();
            }
            else
                tooltipContent = documentation;

            return tooltipContent;
        }
    }
}