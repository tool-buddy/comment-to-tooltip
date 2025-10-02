using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ToolBuddy.CommentToTooltip.TextProcessing
{
    /// <summary>
    /// A class containing a set of methods for generating Unity's tooltips from existing code comments.
    /// </summary>
    public sealed class RegexTextProcessor : ITextProcessor
    {
        private readonly StringBuilder _documentationBuilder;
        private readonly StringBuilder _escapingBuilder;
        private readonly ParsingConfig[] _parsingConfigs;
        private readonly StringBuilder _tooltipTagBuilder;


        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public RegexTextProcessor()
        {
            _tooltipTagBuilder = new StringBuilder();
            _documentationBuilder = new StringBuilder();
            _escapingBuilder = new StringBuilder();
            _parsingConfigs = new[]
            {
                ParsingConfigBuilder.BuildConfigFor(CommentTypes.SingleLineDocumentation),
                ParsingConfigBuilder.BuildConfigFor(CommentTypes.DelimitedDocumentation),
                ParsingConfigBuilder.BuildConfigFor(CommentTypes.SingleLine)
            };
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

            foreach (ParsingConfig config in _parsingConfigs)
            {
                if ((config.SupportedTypes & commentTypes) == CommentTypes.None)
                    continue;

                fileWasModified |= ProcessCommentType(
                    config,
                    ref processedText
                );
            }

            return fileWasModified;
        }

        private bool ProcessCommentType(
            ParsingConfig parsingConfig,
            ref string processedText)
        {
            int insertedTextLength = 0;
            bool textWasModified = false;

            MatchCollection matches = parsingConfig.CommentedFieldPattern.Matches(processedText);

            for (int matchIndex = 0; matchIndex < matches.Count; matchIndex++)
                textWasModified |= ProcessFieldMatch(
                    matches[matchIndex],
                    parsingConfig.CommentExtractor,
                    ref processedText,
                    ref insertedTextLength
                );

            return textWasModified;
        }

        private bool ProcessFieldMatch(
            Match match,
            Regex commentExtractor,
            ref string processedText,
            ref int insertedTextLength)
        {
            GroupCollection groups = match.Groups;

            if (!FieldHasPublicAccess(groups)
                && !HasSerializeFieldAttribute(groups))
                return false;

            string documentation = GetDocumentation(groups["documentation"].Captures);

            string rawTooltipContent = GetTooltipContent(
                documentation,
                commentExtractor
            );
            string escapedTooltipContent = EscapeText(rawTooltipContent);
            string oldTooltipContent = groups["tooltipContent"].ToString();

            if (escapedTooltipContent == oldTooltipContent)
                return false;

            string tooltip = GetTooltipLine(
                groups,
                escapedTooltipContent
            );

            Group oldTooltipGroup = groups["tooltip"];
            if (oldTooltipGroup.Length > 0)
                ReplaceExistingTooltip(
                    ref processedText,
                    ref insertedTextLength,
                    oldTooltipGroup,
                    tooltip
                );
            else
                InsertNewTooltip(
                    ref processedText,
                    ref insertedTextLength,
                    groups,
                    tooltip
                );

            return true;
        }


        private static bool HasSerializeFieldAttribute(
            GroupCollection groups)
        {
            CaptureCollection attributeCaptures = groups["attributes"].Captures;
            for (int index = 0; index < attributeCaptures.Count; index++)
                if (attributeCaptures[index].Value.IndexOf(
                        "SerializeField",
                        StringComparison.Ordinal
                    )
                    >= 0)
                    return true;

            return false;
        }

        private static bool FieldHasPublicAccess(
            GroupCollection groups) =>
            groups["modifier"].Value == "public";

        private void ReplaceExistingTooltip(
            ref string processedText,
            ref int insertedTextLength,
            Group oldTooltipGroup,
            string replacement)
        {
            int replaceIndex = insertedTextLength + oldTooltipGroup.Index;
            processedText = processedText.Remove(
                replaceIndex,
                oldTooltipGroup.Length
            );
            processedText = processedText.Insert(
                replaceIndex,
                replacement
            );
            insertedTextLength += replacement.Length - oldTooltipGroup.Length;
        }

        private static void InsertNewTooltip(
            ref string processedText,
            ref int insertedTextLength,
            GroupCollection groups,
            string tooltip)
        {
            processedText = processedText.Insert(
                insertedTextLength + groups["field"].Index,
                tooltip
            );
            insertedTextLength += tooltip.Length;
        }

        private string GetTooltipLine(
            GroupCollection groups,
            string tooltipContent)
        {
            _tooltipTagBuilder.Clear();

            _tooltipTagBuilder.Append(groups["beginning"]);
            //tooltip attribute beginning
            _tooltipTagBuilder.Append("[UnityEngine.Tooltip(\"");
            _tooltipTagBuilder.Append(tooltipContent);
            //tooltip attribute end
            _tooltipTagBuilder.Append("\")]");
            _tooltipTagBuilder.Append(Environment.NewLine);
            return _tooltipTagBuilder.ToString();
        }

        private string EscapeText(
            string text)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            _escapingBuilder.Clear();

            foreach (char c in text)
                switch (c)
                {
                    case '\\': _escapingBuilder.Append(@"\\"); break;
                    case '\"': _escapingBuilder.Append("\\\""); break;
                    case '\r': _escapingBuilder.Append(@"\r"); break;
                    case '\n': _escapingBuilder.Append(@"\n"); break;
                    case '\t': _escapingBuilder.Append(@"\t"); break;
                    default: _escapingBuilder.Append(c); break;
                }

            return _escapingBuilder.ToString();
        }

        private static string GetTooltipContent(
            string documentation,
            Regex commentExtractor)
        {
            if (commentExtractor == null)
                return documentation;

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

            return match.Groups["comment"].ToString();
        }

        private string GetDocumentation(
            CaptureCollection documentationCaptures)
        {
            _documentationBuilder.Clear();

            //constructing the documentation text
            int capturesCount = documentationCaptures.Count;
            for (int captureIndex = 0; captureIndex < capturesCount; captureIndex++)
            {
                Capture capturedLine = documentationCaptures[captureIndex];
                _documentationBuilder.Append(capturedLine);

                //new line if there is other lines to add
                if (captureIndex != capturesCount - 1)
                    _documentationBuilder.Append(Environment.NewLine);
            }

            return _documentationBuilder.ToString();
        }
    }
}