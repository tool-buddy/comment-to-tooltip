namespace ToolBuddy.CommentToTooltip.Editor.Settings
{
    public class ParsingSettings
    {
        public bool ParseSingleLineDocumentationComments { get; set; }
        public bool ParseDelimitedDocumentationComments { get; set; }
        public bool ParseSingleLineComments { get; set; }

        public ParsingSettings(
            bool parseSingleLineDocumentationComments,
            bool parseDelimitedDocumentationComments,
            bool parseSingleLineComments)
        {
            ParseSingleLineDocumentationComments = parseSingleLineDocumentationComments;
            ParseDelimitedDocumentationComments = parseDelimitedDocumentationComments;
            ParseSingleLineComments = parseSingleLineComments;
        }

        public bool Update(
            bool parseSingleLineDocumentationComments,
            bool parseDelimitedDocumentationComments,
            bool parseSingleLineComments)
        {
            bool updated = false;
            if (ParseSingleLineDocumentationComments != parseSingleLineDocumentationComments)
            {
                ParseSingleLineDocumentationComments = parseSingleLineDocumentationComments;
                updated = true;
            }

            if (ParseDelimitedDocumentationComments != parseDelimitedDocumentationComments)
            {
                ParseDelimitedDocumentationComments = parseDelimitedDocumentationComments;
                updated = true;
            }

            if (ParseSingleLineComments != parseSingleLineComments)
            {
                ParseSingleLineComments = parseSingleLineComments;
                updated = true;
            }

            return updated;
        }

        public CommentTypes GetCommentTypes()
        {
            CommentTypes result = CommentTypes.None;
            if (ParseSingleLineDocumentationComments)
                result |= CommentTypes.SingleLineDocumentation;
            if (ParseDelimitedDocumentationComments)
                result |= CommentTypes.DelimitedDocumentation;
            if (ParseSingleLineComments)
                result |= CommentTypes.SingleLine;
            return result;
        }
    }
}