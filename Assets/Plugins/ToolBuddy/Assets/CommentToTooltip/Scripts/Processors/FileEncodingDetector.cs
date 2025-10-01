using System;
using System.Text;
using Ude;

namespace ToolBuddy.CommentToTooltip.Processors
{
    public static class FileEncodingDetector
    {
        public static Encoding DetectFileEncoding(
            byte[] fileBytes)
        {
            // UTF-8 with BOM
            if (fileBytes.Length >= 3 && fileBytes[0] == 0xEF && fileBytes[1] == 0xBB && fileBytes[2] == 0xBF)
                return new UTF8Encoding(true);

            if (fileBytes.Length >= 2)
            {
                // UTF-16 LE
                if (fileBytes[0] == 0xFF && fileBytes[1] == 0xFE)
                    return new UnicodeEncoding(
                        false,
                        true
                    );

                // UTF-16 BE
                if (fileBytes[0] == 0xFE && fileBytes[1] == 0xFF)
                    return new UnicodeEncoding(
                        true,
                        true
                    );
            }

            if (fileBytes.Length >= 4)
            {
                // UTF-32 LE
                if (fileBytes[0] == 0xFF && fileBytes[1] == 0xFE && fileBytes[2] == 0x00 && fileBytes[3] == 0x00)
                    return new UTF32Encoding(
                        false,
                        true
                    );

                // UTF-32 BE
                if (fileBytes[0] == 0x00 && fileBytes[1] == 0x00 && fileBytes[2] == 0xFE && fileBytes[3] == 0xFF)
                    return new UTF32Encoding(
                        true,
                        true
                    );
            }

            // Use Ude to detect a non-BOM encoding
            string udeDetectedCharset = GetUdeDetectedCharset(fileBytes);

            if (string.IsNullOrEmpty(udeDetectedCharset))
                return new UTF8Encoding(false);

            Encoding udeDetectedEncoding = Encoding.GetEncoding(udeDetectedCharset);

            // If Ude says UTF-8 and no BOM was found above, return UTF-8 without BOM
            if (string.Equals(
                    udeDetectedEncoding.WebName,
                    "utf-8",
                    StringComparison.OrdinalIgnoreCase
                ))
                return new UTF8Encoding(false);

            return udeDetectedEncoding;
        }

        private static string GetUdeDetectedCharset(
            byte[] bytes)
        {
            CharsetDetector detector = new();
            detector.Feed(
                bytes,
                0,
                bytes.Length
            );
            detector.DataEnd();

            string udeDetectedCharset = detector.Charset;
            return udeDetectedCharset;
        }
    }
}