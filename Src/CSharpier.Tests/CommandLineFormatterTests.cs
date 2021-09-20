using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace CSharpier.Tests
{
    [TestFixture]
    public class CommandLineFormatterTests
    {
        private MockFileSystem fileSystem;
        private const string UnformattedClassContent =
            "public class ClassName { public int Field; }";
        private const string FormattedClassContent =
            "public class ClassName\n{\n    public int Field;\n}\n";

        [SetUp]
        public void Setup()
        {
            this.fileSystem = new MockFileSystem();
        }

        [Test]
        public void Format_Writes_Failed_To_Compile()
        {
            WhenAFileExists("Invalid.cs", "asdfasfasdf");

            var (_, lines) = this.Format();

            lines.First()
                .Should()
                .Be(
                    $"Warning {Path.DirectorySeparatorChar}Invalid.cs - Failed to compile so was not formatted."
                );
        }

        [Test]
        public void Format_Writes_Unsupported()
        {
            WhenAFileExists("Unsupported.js", "asdfasfasdf");

            var (exitCode, lines) = this.Format(directoryOrFilePaths: "Unsupported.js");

            lines.First().Should().Be(@"Error /Unsupported.js - Is an unsupported file type.");
        }

        [Test]
        public void Format_Writes_File()
        {
            const string unformattedFilePath = "Unformatted.cs";
            WhenAFileExists(unformattedFilePath, UnformattedClassContent);

            this.Format();

            this.GetFileContent(unformattedFilePath).Should().Be(FormattedClassContent);
        }

        [Test]
        public void Format_Supports_Skip_Write()
        {
            const string unformattedFilePath = "Unformatted.cs";
            WhenAFileExists(unformattedFilePath, UnformattedClassContent);

            this.Format(skipWrite: true);

            this.GetFileContent(unformattedFilePath).Should().Be(UnformattedClassContent);
        }

        [Test]
        public void Format_Checks_Unformatted_File()
        {
            const string unformattedFilePath = "Unformatted.cs";
            WhenAFileExists(unformattedFilePath, UnformattedClassContent);

            var (exitCode, lines) = this.Format(check: true);

            exitCode.Should().Be(1);
            this.GetFileContent(unformattedFilePath).Should().Be(UnformattedClassContent);
            lines.First()
                .Should()
                .StartWith(
                    $"Warning {Path.DirectorySeparatorChar}Unformatted.cs - Was not formatted."
                );
        }

        [Test]
        public void Format_Checks_Formatted_File()
        {
            const string formattedFilePath = "Formatted.cs";
            WhenAFileExists(formattedFilePath, FormattedClassContent);
            var (exitCode, _) = this.Format(check: true);

            exitCode.Should().Be(0);
        }

        [TestCase("TemporaryGeneratedFile_Tester.cs")]
        [TestCase("TestFile.designer.cs")]
        [TestCase("TestFile.generated.cs")]
        [TestCase("TestFile.g.cs")]
        [TestCase("TestFile.g.i.cs")]
        public void Format_Skips_Generated_Files(string fileName)
        {
            var unformattedFilePath = fileName;
            WhenAFileExists(unformattedFilePath, UnformattedClassContent);

            var (_, lines) = this.Format();

            lines.Should().Contain("Total files: 0 ");
        }

        [TestCase("File.cs", "File.cs")]
        [TestCase("File.cs", "*.cs")]
        [TestCase("SubFolder/File.cs", "*.cs")]
        [TestCase("Debug/Logs/File.cs", "**/Logs")]
        [TestCase("Debug/Logs/File.cs", "Logs/")]
        [TestCase("Debug/Logs/File.cs", "Debug/Logs/File.cs")]
        [TestCase(
            @"\Src\CSharpier.Playground\App_Data\Uploads\f45e11a81b926de2af29459af6974bb8.cs",
            "Uploads/"
        )]
        public void File_In_Ignore_Skips_Formatting(string fileName, string ignoreContents)
        {
            var unformattedFilePath = fileName;
            WhenAFileExists(unformattedFilePath, UnformattedClassContent);
            WhenAFileExists(".csharpierignore", ignoreContents);

            var (_, lines) = this.Format();

            lines.FirstOrDefault(o => o.StartsWith("Total files")).Should().Be("Total files: 0 ");
        }

        [TestCase("SubFolder/File.cs", "*.cs", "SubFolder")]
        [TestCase("SubFolder/File.cs", "SubFolder/File.cs", "SubFolder")]
        public void File_In_Ignore_Skips_Formatting_With_BaseDirectory(
            string fileName,
            string ignoreContents,
            string baseDirectory
        ) {
            var unformattedFilePath = fileName;
            WhenAFileExists(unformattedFilePath, UnformattedClassContent);
            WhenAFileExists(".csharpierignore", ignoreContents);

            var (_, lines) = this.Format(
                directoryOrFilePaths: Path.Combine(GetRootPath(), baseDirectory)
            );

            lines.FirstOrDefault(o => o.StartsWith("Total files")).Should().Be("Total files: 0 ");
        }

        [Test]
        public void Multiple_Files_Should_Use_Root_Ignore()
        {
            var unformattedFilePath1 = "SubFolder/1/File1.cs";
            var unformattedFilePath2 = "SubFolder/2/File2.cs";
            WhenAFileExists(unformattedFilePath1, UnformattedClassContent);
            WhenAFileExists(unformattedFilePath2, UnformattedClassContent);
            WhenAFileExists(".csharpierignore", "Subfolder/**/*.cs");

            var (_, lines) = this.Format(
                directoryOrFilePaths: new[] { unformattedFilePath1, unformattedFilePath2 }
            );

            lines.FirstOrDefault(o => o.StartsWith("Total files")).Should().Be("Total files: 0 ");
        }

        [Test]
        public void Multiple_Files_Should_Use_Multiple_Ignores()
        {
            var unformattedFilePath1 = "SubFolder/1/File1.cs";
            var unformattedFilePath2 = "SubFolder/2/File2.cs";
            WhenAFileExists(unformattedFilePath1, UnformattedClassContent);
            WhenAFileExists(unformattedFilePath2, UnformattedClassContent);
            WhenAFileExists("SubFolder/1/.csharpierignore", "File1.cs");
            WhenAFileExists("SubFolder/2/.csharpierignore", "File2.cs");

            var (_, lines) = this.Format(
                directoryOrFilePaths: new[] { unformattedFilePath1, unformattedFilePath2 }
            );

            lines.FirstOrDefault(o => o.StartsWith("Total files")).Should().Be("Total files: 0 ");
        }

        [Test]
        public void Ignore_Should_Deal_With_Inconsistent_Slashes()
        {
            var unformattedFilePath1 = @"SubFolder\1\File1.cs";
            WhenAFileExists(unformattedFilePath1, UnformattedClassContent);
            WhenAFileExists("SubFolder/1/.csharpierignore", "File1.cs");

            var (_, lines) = this.Format(directoryOrFilePaths: unformattedFilePath1);

            lines.FirstOrDefault(o => o.StartsWith("Total files")).Should().Be("Total files: 0 ");
        }

        [Test]
        public void Ignore_Reports_Errors()
        {
            WhenAFileExists(".csharpierignore", @"\Src\Uploads\*.cs");

            var (exitCode, lines) = this.Format();

            var path = this.fileSystem.Path.Combine(GetRootPath(), ".csharpierignore");

            exitCode.Should().Be(1);
            lines.First()
                .Should()
                .StartWith(
                    $"Error The .csharpierignore file at {path} could not be parsed due to the following line:"
                );
            lines.Skip(1).First().Should().Contain(@"\Src\Uploads\*.cs");
        }

        [Test]
        public void Write_Stdout_Should_Only_Write_File()
        {
            WhenAFileExists("file1.cs", UnformattedClassContent);

            var (_, lines) = this.Format(writeStdout: true);

            lines.Should().ContainSingle();
            lines.First().Should().Be(FormattedClassContent);
        }

        [Test]
        public void Should_Format_StandardInput_When_Provided()
        {
            var (_, lines) = this.Format(standardInFileContents: UnformattedClassContent);

            lines.Should().ContainSingle();
            lines.First().Should().Be(FormattedClassContent);
        }

        [Test]
        public void File_With_Mismatched_Line_Endings_In_Verbatim_String_Should_Pass_Validation()
        {
            WhenAFileExists(
                "file1.cs",
                "public class ClassName\n{\npublic string Value = @\"EndThisLineWith\r\nEndThisLineWith\n\";\n}"
            );

            var (exitCode, _) = this.Format();

            exitCode.Should().Be(0);
        }

        [Test]
        public void File_With_Compilation_Error_In_If_Should_Not_Lose_Code()
        {
            var contents =
                @"#if DEBUG
?using System;
#endif
";
            WhenAFileExists("Invalid.cs", contents);

            var (_, lines) = this.Format();

            var result = GetFileContent("Invalid.cs");

            result.Should().Be(contents);

            lines.First()
                .Should()
                .Be(
                    $"Warning {Path.DirectorySeparatorChar}Invalid.cs - Failed to compile so was not formatted."
                );
        }

        [Test]
        public void File_Should_Format_With_Supplied_Symbols()
        {
            WhenAFileExists(".csharpierrc", @"{ ""preprocessorSymbolSets"": [""FORMAT""] }");
            WhenAFileExists(
                "file1.cs",
                @"public class ClassName
{
#if FORMAT
    public     string ShortPropertyName;
#elif NO_FORMAT
    public     string ShortPropertyName;
#else
    public     string ShortPropertyName;
#endif
}
"
            );

            this.Format();

            var result = GetFileContent("file1.cs");

            result.Should()
                .Be(
                    @"public class ClassName
{
#if FORMAT
    public string ShortPropertyName;
#elif NO_FORMAT
    public     string ShortPropertyName;
#else
    public     string ShortPropertyName;
#endif
}
"
                );
        }

        [TestCase(".csharpierrc")]
        [TestCase(".csharpierrc.json")]
        [TestCase(".csharpierrc.yaml")]
        public void Empty_Config_Files_Should_Log_Warning(string configFileName)
        {
            var configPath = WhenAFileExists(".csharpierrc", "");
            WhenAFileExists("file1.cs", "public class ClassName { }");

            var (_, lines) = this.Format();

            lines.First()
                .Replace("\\", "/")
                .Should()
                .Be($"Warning The configuration file at {configPath} was empty.");
        }

        private (int exitCode, IList<string> lines) Format(
            bool skipWrite = false,
            bool check = false,
            bool writeStdout = false,
            string standardInFileContents = null,
            params string[] directoryOrFilePaths
        ) {
            if (directoryOrFilePaths.Length == 0)
            {
                directoryOrFilePaths = new[] { GetRootPath() };
            }
            else
            {
                directoryOrFilePaths = directoryOrFilePaths.Select(
                        o => this.fileSystem.Path.Combine(GetRootPath(), o)
                    )
                    .ToArray();
            }

            var fakeConsole = new TestConsole();
            var testLogger = new ConsoleLogger(fakeConsole);
            var result =
                CommandLineFormatter.Format(
                    new CommandLineOptions
                    {
                        DirectoryOrFilePaths = directoryOrFilePaths,
                        SkipWrite = skipWrite,
                        Check = check,
                        WriteStdout = writeStdout,
                        StandardInFileContents = standardInFileContents,
                    },
                    this.fileSystem,
                    fakeConsole,
                    testLogger,
                    CancellationToken.None
                ).Result;

            fakeConsole.Close();

            return (result, fakeConsole.Lines);
        }

        private string GetRootPath()
        {
            var result = OperatingSystem.IsWindows() ? @"c:\test" : "/Test";
            this.fileSystem.AddDirectory(result);
            return result;
        }

        private string GetFileContent(string path)
        {
            path = this.fileSystem.Path.Combine(GetRootPath(), path);
            return this.fileSystem.File.ReadAllText(path);
        }

        private string WhenAFileExists(string path, string contents)
        {
            path = this.fileSystem.Path.Combine(GetRootPath(), path).Replace('\\', '/');
            this.fileSystem.AddFile(path, new MockFileData(contents));
            return path;
        }

        private class TestConsole : IConsole
        {
            public readonly List<string> Lines = new();

            private string nextLine = "";

            public void WriteLine(string line = null)
            {
                while (line != null && line.Contains("  "))
                {
                    line = line.Replace("  ", " ");
                }

                if (line != null)
                {
                    nextLine += line;
                    this.Lines.Add(nextLine);
                    nextLine = "";
                }
            }

            public void Write(string value)
            {
                nextLine += value;
            }

            public Encoding InputEncoding => Encoding.UTF8;
            public ConsoleColor ForegroundColor { get; set; }
            public void ResetColor() { }

            public void Close()
            {
                if (nextLine != "")
                {
                    this.Lines.Add(nextLine);
                }
            }
        }
    }
}
