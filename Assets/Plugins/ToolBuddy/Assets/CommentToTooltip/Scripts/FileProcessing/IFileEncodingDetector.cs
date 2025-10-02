using System.Text;

namespace ToolBuddy.CommentToTooltip.FileProcessing
{
    public interface IFileEncodingDetector
    {
        Encoding DetectFileEncoding(
            byte[] fileBytes);
    }
}