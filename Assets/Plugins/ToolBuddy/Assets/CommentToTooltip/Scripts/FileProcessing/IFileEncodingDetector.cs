using System.Text;

namespace ToolBuddy.CommentToTooltip.FileProcessing
{
    /// <summary>
    /// Determines the encoding used for a given file so it can be read and written safely.
    /// </summary>
    public interface IFileEncodingDetector
    {
        /// <summary>
        /// Detects the encoding for the specified file contents.
        /// </summary>
        /// <param name="fileBytes">The raw bytes read from the file.</param>
        /// <returns>The encoding that best represents the supplied bytes.</returns>
        Encoding DetectFileEncoding(
            byte[] fileBytes);
    }
}