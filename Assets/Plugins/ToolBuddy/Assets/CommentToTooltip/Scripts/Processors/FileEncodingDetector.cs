using System;
using System.IO;
using System.Text;
using Ude;

namespace ToolBuddy.CommentToTooltip.Processors
{
    public static class FileEncodingDetector
    {
        public static Encoding DetectFileEncoding(
            string filePath)
        {
            byte[] bytes = File.ReadAllBytes(filePath);

            // UTF-8 with BOM
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                return new UTF8Encoding(true); 

            if (bytes.Length >= 2)
            {
                // UTF-16 LE
                if (bytes[0] == 0xFF && bytes[1] == 0xFE)
                    return new UnicodeEncoding(
                        false,
                        true
                    );

                // UTF-16 BE
                if (bytes[0] == 0xFE && bytes[1] == 0xFF)
                    return new UnicodeEncoding(
                        true,
                        true
                    ); 
            }

            if (bytes.Length >= 4)
            {
                // UTF-32 LE
                if (bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0x00 && bytes[3] == 0x00)
                    return new UTF32Encoding(
                        false,
                        true
                    );

                // UTF-32 BE
                if (bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF)
                    return new UTF32Encoding(
                        true,
                        true
                    ); 
            }

            // Use Ude to detect a non-BOM encoding
            string udeDetectedCharset = GetUdeDetectedCharset(bytes);

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