using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ToolBuddy.CommentToTooltip
{
    /// <summary>
    /// A class containing a set of methods for generating Unity's tooltips from existing code comments.
    /// </summary>
    public class TooltipGenerator
    {
        private readonly CodeProcessingConfiguration[] _codeProcessors;
        private readonly StringBuilder _documentationBuilder;
        private readonly string _escapedNewLineInGeneratedCode;
        private readonly string _newLineInGeneratedCode;
        private readonly StringBuilder _tooltipTagBuilder;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public TooltipGenerator()
        {
            _tooltipTagBuilder = new StringBuilder();
            _documentationBuilder = new StringBuilder();

            const RegexOptions regexOptions = RegexOptions.Multiline;

            _newLineInGeneratedCode = Environment.NewLine;

            _escapedNewLineInGeneratedCode = _newLineInGeneratedCode.Replace(
                "\r",
                @"\r"
            );
            _escapedNewLineInGeneratedCode = _escapedNewLineInGeneratedCode.Replace(
                "\n",
                @"\n"
            );

            string doubleEscapedNewLine = _newLineInGeneratedCode.Replace(
                "\r",
                @"\\r"
            );
            doubleEscapedNewLine = doubleEscapedNewLine.Replace(
                "\n",
                @"\\n"
            );

            Regex commentExtractingRegexp =
                new(
                    @"\s*<summary>\s*(?:"
                    + doubleEscapedNewLine
                    + @")?(?<comment>.*?(?=(?:"
                    + doubleEscapedNewLine
                    + @")?\s*</summary\s*))",
                    regexOptions
                );


            /* capture groups:
            beginning => tabulations and spaces at line beginning
            documentation => each documentation line
            tooltip => existing tooltip attribute
            tooltipContent => existing tooltip attribute's string content
            field => field declaration
            */

            const string nonTooltipAttributes =
                @"(?<attributes>^[ \t]*\[(?!([ \t]*(?:UnityEngine.)?Tooltip))[^]]+\]\s*(?=^))*";
            const string nonCommentRegexPart =
                @"\s*(?=^)"
                + nonTooltipAttributes
                + @"(?<tooltip>^[ \t]*\[(?:UnityEngine.)?Tooltip\(""(?<tooltipContent>[^""]*)""\)\]\s*(?=^))?"
                + nonTooltipAttributes
                + @"(?<field>(?<beginning>^[ \t]*)public\s+[^\s;=]+\s+[^\s;=]+\s*(?>=[^;]+)?;)";


            string newLineRegex = @"(?:\r)?\n";
            CodeProcessingConfiguration singleLineDocCommentCSharpProcessingConfiguration = new(
                new Regex(
                    @"(?>^[ \t]*///[ \t]?(?>(?<documentation>[^\r\n]*))" + newLineRegex + @")+" + nonCommentRegexPart,
                    regexOptions
                ),
                commentExtractingRegexp,
                CommentTypes.SingleLineDocumentation
            );

            CodeProcessingConfiguration delimitedDocCommentCSharpProcessingConfiguration = new(
                new Regex(
                    @"(?>^[ \t]*/\*)(?:(?>[ \t]*\*[ \t]?)(?<documentation>[^\r\n]*)(?:"
                    + newLineRegex
                    + @")?)+[ \t]*\*/[ \t]*"
                    + newLineRegex
                    + @""
                    + nonCommentRegexPart,
                    regexOptions
                ),
                commentExtractingRegexp,
                CommentTypes.DelimitedDocumentation
            );

            CodeProcessingConfiguration singleLineCommentProcessingConfiguration = new(
                new Regex(
                    @"(?>^[ \t]*//(?!/)[ \t]?(?>(?<documentation>[^\r\n]*))" + newLineRegex + @")+" + nonCommentRegexPart,
                    regexOptions
                ),
                null,
                CommentTypes.SingleLine
            );

            _codeProcessors = new[]
            {
                singleLineDocCommentCSharpProcessingConfiguration,
                delimitedDocCommentCSharpProcessingConfiguration,
                singleLineCommentProcessingConfiguration
            };
        }

        /// <summary>
        /// Processes the file's content, and if any tooltip was generated, updates the file.
        /// </summary>
        /// <param name="filePath"> The path of the file to update. </param>
        /// <param name="commentTypes"> The <see cref="CommentTypes"/> to be considered while generating the tooltips. </param>
        /// <returns> True if the file was updated. </returns>
        public bool TryProcessFile(
            string filePath,
            CommentTypes commentTypes) =>
            TryProcessFile(
                filePath,
                filePath,
                Encoding.Default,
                commentTypes
            );

        /// <summary>
        /// Processes the file's content, and if any tooltip was generated, updates the file.
        /// </summary>
        /// <param name="filePath"> The path of the file to update. </param>
        /// <param name="fileEncoding"> The <see cref="Encoding"/> of the file. </param>
        /// <param name="commentTypes"> The <see cref="CommentTypes"/> to be considered while generating the tooltips. </param>
        /// <returns> True if an output file with updated content was created. </returns>
        public bool TryProcessFile(
            string filePath,
            Encoding fileEncoding,
            CommentTypes commentTypes) =>
            TryProcessFile(
                filePath,
                filePath,
                fileEncoding,
                commentTypes
            );

        /// <summary>
        /// Processes the input file's content, and if any tooltip was generated, an output file will be created containing the updated content.
        /// </summary>
        /// <param name="inputFilePath"> The path of the input file. </param>
        /// <param name="outputFilePath"> The path of the output file. </param>
        /// <param name="commentTypes"> The <see cref="CommentTypes"/> to be considered while generating the tooltips. </param>
        /// <returns> True if an output file with updated content was created. </returns>
        public bool TryProcessFile(
            string inputFilePath,
            string outputFilePath,
            CommentTypes commentTypes) =>
            TryProcessFile(
                inputFilePath,
                outputFilePath,
                Encoding.Default,
                commentTypes
            );

        /// <summary>
        /// Processes the input file's content, and if any tooltip was generated, an output file will be created containing the updated content.
        /// </summary>
        /// <param name="inputFilePath"> The path of the input file. </param>
        /// <param name="outputFilePath"> The path of the output file. </param>
        /// <param name="fileEncoding"> The <see cref="Encoding"/> of the input file and output file. </param>
        /// <param name="commentTypes"> The <see cref="CommentTypes"/> to be considered while generating the tooltips. </param>
        /// <returns> True if an output file with updated content was created. </returns>
        public bool TryProcessFile(
            string inputFilePath,
            string outputFilePath,
            Encoding fileEncoding,
            CommentTypes commentTypes)
        {
            string inputFileContent;
            using (StreamReader streamReader = new(
                       inputFilePath,
                       fileEncoding
                   ))
            {
                inputFileContent = streamReader.ReadToEnd();
            }

            string outputFileContent;
            bool fileWasModified = TryProcessText(
                inputFileContent,
                out outputFileContent,
                commentTypes
            );

            if (fileWasModified)
                using (StreamWriter writer = new(
                           outputFilePath,
                           false,
                           fileEncoding
                       ))
                {
                    writer.Write(outputFileContent);
                }

            return fileWasModified;
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

            foreach (CodeProcessingConfiguration codeProcessor in _codeProcessors)
            {
                if ((codeProcessor.CommentTypes & commentTypes) == CommentTypes.None)
                    continue;

                int insertedTextLength = 0;
                MatchCollection matches = codeProcessor.Parser.Matches(processedText);
                int matchesCount = matches.Count;
                for (int matchIndex = 0; matchIndex < matchesCount; matchIndex++)
                {
                    Match match = matches[matchIndex];
                    GroupCollection groups = match.Groups;
                    string tooltipContent = BuildTooltipContent(
                        groups["documentation"].Captures,
                        codeProcessor.CommentExtractor
                    );

                    //if existing tooltip is different than the generated one
                    if (tooltipContent != groups["tooltipContent"].ToString())
                    {
                        _tooltipTagBuilder.Append(groups["beginning"]);
                        //tooltip attribute beginning
                        _tooltipTagBuilder.Append("[UnityEngine.Tooltip(\"");
                        _tooltipTagBuilder.Append(tooltipContent);
                        //tooltip attribute end
                        _tooltipTagBuilder.Append("\")]");
                        _tooltipTagBuilder.Append(_newLineInGeneratedCode);
                        string tooltip = _tooltipTagBuilder.ToString();
                        _tooltipTagBuilder.Length = 0;
                        _tooltipTagBuilder.Capacity = 0;

                        //remove possible old tooltip
                        Group oldTooltipGroup = groups["tooltip"];
                        int oldTolltipLength = oldTooltipGroup.Length;
                        if (oldTolltipLength > 0)
                        {
                            processedText = processedText.Remove(
                                insertedTextLength + oldTooltipGroup.Index,
                                oldTolltipLength
                            );
                            insertedTextLength -= oldTolltipLength;
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

        private string BuildTooltipContent(
            CaptureCollection documentationCaptures,
            Regex commentExtractor)
        {
            //constructing the documentation text
            int capturesCount = documentationCaptures.Count;
            for (int captureIndex = 0; captureIndex < capturesCount; captureIndex++)
            {
                Capture capturedLine = documentationCaptures[captureIndex];
                _documentationBuilder.Append(capturedLine);

                //new line if there is other lines to add
                if (captureIndex != capturesCount - 1)
                    _documentationBuilder.Append(_escapedNewLineInGeneratedCode);
            }

            string documentation = _documentationBuilder.ToString();
            _documentationBuilder.Length = 0;
            _documentationBuilder.Capacity = 0;

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