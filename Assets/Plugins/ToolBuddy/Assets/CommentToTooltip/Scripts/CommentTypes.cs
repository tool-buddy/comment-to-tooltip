using System;

namespace ToolBuddy.CommentToTooltip
{
    /// <summary>
    /// The supported comment types for parsing.
    /// </summary>
    [Flags]
    public enum CommentTypes
    {
        /// <summary>
        /// No comments.
        /// </summary>
        None = 0,

        /// <summary>
        /// Comments of type XML documentation single-line.
        /// More information can be found in Annex E: "Documentation Comments" of the C# Language Specification.
        /// </summary>
        /// <example>
        /// /// &lt;summary&gt; Comment here &lt;/summary&gt;
        /// </example>
        SingleLineDocumentation = 1 << 0,

        /// <summary>
        /// Comments of type XML documentation delimited.
        /// More information can be found in Annex E: "Documentation Comments" of the C# Language Specification.
        /// </summary>
        /// <example>
        /// /** &lt;summary&gt; Comment here &lt;/summary&gt; */
        /// </example>
        DelimitedDocumentation = 1 << 1,

        /// <summary>
        /// Regular single-line comments.
        /// </summary>
        /// <example>
        /// // Comment here
        /// </example>
        SingleLine = 1 << 2,

        /// <summary>
        /// All supported comments.
        /// </summary>
        All = SingleLineDocumentation | DelimitedDocumentation | SingleLine
    }
}