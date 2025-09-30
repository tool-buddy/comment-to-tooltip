using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace ToolBuddy.CommentToTooltip.Tests
{
    public class TextProcessorTests
    {
        private static string LoadText(
            string filePath)
        {
            string fullPath = Path.Combine(
                Application.dataPath,
                filePath
            );

            Assert.IsTrue(
                File.Exists(fullPath),
                $"Missing test data file at path: {fullPath}"
            );

            return File.ReadAllText(fullPath);
        }

        public static IEnumerable<TestCaseData> GetAllCases()
        {
            string basePath = "Plugins/ToolBuddy/Assets/CommentToTooltip/Tests/TestData/";

            string fullBasePath = Path.Combine(
                Application.dataPath,
                basePath
            );

            if (!Directory.Exists(fullBasePath))
                Assert.Fail($"Missing test data directory at path: {fullBasePath}");

            List<string> inputs = new(
                Directory.GetFiles(
                    fullBasePath,
                    "*.input.cs.txt"
                )
            );
            inputs.Sort();

            foreach (string fullInputPath in inputs)
            {
                string fileName = Path.GetFileName(fullInputPath);
                string expectedFileName = fileName.Replace(
                    ".input.cs.txt",
                    ".expected.cs.txt"
                );
                string expectedFileFullPath = Path.Combine(
                    fullBasePath,
                    expectedFileName
                );

                Assert.IsTrue(
                    File.Exists(expectedFileFullPath),
                    $"Missing expected file for input '{fileName}'"
                );

                yield return new TestCaseData(
                    basePath + fileName,
                    basePath + expectedFileName
                ).SetName(
                    //triple extension removal
                    Path.GetFileNameWithoutExtension(
                        Path.GetFileNameWithoutExtension(
                            Path.GetFileNameWithoutExtension(fileName)))
                );
            }
        }

        [Test]
        [TestCaseSource(nameof(GetAllCases))]
        public void GeneratedOutput_EqualsExpected(
            string inputFilePath,
            string expectedFilePath)
        {
            string input = LoadText(inputFilePath);
            string expected = LoadText(expectedFilePath);

            TextProcessor processor = new();
            bool modified = processor.TryProcessText(
                input,
                out string output,
                CommentTypes.All
            );

            Assert.AreEqual(
                expected,
                output,
                $"Generated output does not match expected for case: {inputFilePath}"
            );

            // Verify modified flag correctness against input/expected difference
            Assert.AreEqual(
                expected != input,
                modified,
                "Modified flag does not reflect whether output differs from input."
            );
        }
    }
}