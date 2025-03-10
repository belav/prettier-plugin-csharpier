using CSharpier.Cli;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.IO.Abstractions.TestingHelpers;
using System.Text;

namespace CSharpier.Tests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class CommandLineFormatterTests
{
    private const string UnformattedClassContent = "public class ClassName { public int Field; }";
    private const string FormattedClassContent =
        "public class ClassName\n{\n    public int Field;\n}\n";

    [Test]
    public void Format_Writes_Failed_To_Compile()
    {
        var context = new TestContext();
        context.WhenAFileExists("Invalid.cs", "asdfasfasdf");

        var result = Format(context);

        result
            .ErrorOutputLines.First()
            .Should()
            .Be("Error ./Invalid.cs - Failed to compile so was not formatted.");

        result.ExitCode.Should().Be(1);
    }

    [Test]
    public void Format_Writes_Failed_To_Compile_As_Warning()
    {
        var context = new TestContext();
        context.WhenAFileExists("Invalid.cs", "asdfasfasdf");

        var result = Format(context, compilationErrorsAsWarnings: true);

        result
            .OutputLines.First()
            .Should()
            .Be("Warning ./Invalid.cs - Failed to compile so was not formatted.");

        result.ExitCode.Should().Be(0);
    }

    [Test]
    public void Format_Writes_Failed_To_Compile_For_Subdirectory()
    {
        var context = new TestContext();
        context.WhenAFileExists("Subdirectory/Invalid.cs", "asdfasfasdf");

        var result = Format(context, directoryOrFilePaths: "Subdirectory");

        result
            .ErrorOutputLines.First()
            .Should()
            .Be("Error ./Subdirectory/Invalid.cs - Failed to compile so was not formatted.");
    }

    [Test]
    public void Format_Writes_Failed_To_Compile_For_FullPath()
    {
        var context = new TestContext();
        context.WhenAFileExists("Subdirectory/Invalid.cs", "asdfasfasdf");

        var result = Format(
            context,
            directoryOrFilePaths: Path.Combine(GetRootPath(), "Subdirectory")
        );

        result
            .ErrorOutputLines.First()
            .Should()
            .Be(
                $"Error {GetRootPath().Replace('\\', '/')}/Subdirectory/Invalid.cs - Failed to compile so was not formatted."
            );
    }

    [Test]
    public void Format_Writes_Failed_To_Compile_With_Directory()
    {
        var context = new TestContext();
        context.WhenAFileExists("Directory/Invalid.cs", "asdfasfasdf");

        var result = Format(context);

        result
            .ErrorOutputLines.First()
            .Should()
            .Be("Error ./Directory/Invalid.cs - Failed to compile so was not formatted.");
    }

    [Test]
    public void Format_Writes_Unsupported()
    {
        var context = new TestContext();
        context.WhenAFileExists("Unsupported.js", "asdfasfasdf");

        var result = Format(context, directoryOrFilePaths: "Unsupported.js");

        result
            .OutputLines.First()
            .Should()
            .Be("Warning ./Unsupported.js - Is an unsupported file type.");
    }

    [Test]
    public void Format_Does_Not_Write_Unsupported_When_Formatting_Directory()
    {
        var context = new TestContext();
        context.WhenAFileExists("Unsupported.js", "asdfasfasdf");

        var result = Format(context);

        result.OutputLines.First().Should().StartWith("Formatted 0 files");
    }

    [Test]
    public void Format_Writes_File_With_Directory_Path()
    {
        var context = new TestContext();
        var unformattedFilePath = "Unformatted.cs";
        context.WhenAFileExists(unformattedFilePath, UnformattedClassContent);

        Format(context);

        context.GetFileContent(unformattedFilePath).Should().Be(FormattedClassContent);
    }

    [Test]
    public void Formats_CSX_File()
    {
        var context = new TestContext();
        var unformattedFilePath = "Unformatted.csx";
        context.WhenAFileExists(
            unformattedFilePath,
            """
            #r "Microsoft.WindowsAzure.Storage"

            public static void Run()
            {
            }
            """
        );

        var result = Format(context);
        result.OutputLines.First().Should().StartWith("Formatted 1 files");

        context
            .GetFileContent(unformattedFilePath)
            .Should()
            .Be(
                """
                #r "Microsoft.WindowsAzure.Storage"

                public static void Run() { }

                """
            );
    }

    [Test]
    public void Formats_Overrides_File()
    {
        var context = new TestContext();
        var unformattedFilePath = "Unformatted.cst";
        context.WhenAFileExists(unformattedFilePath, UnformattedClassContent);
        context.WhenAFileExists(
            ".csharpierrc",
            """
            overrides:
              - files: "*.cst"
                formatter: "csharp"
            """
        );

        var result = Format(context);
        result.OutputLines.First().Should().StartWith("Formatted 1 files");

        context.GetFileContent(unformattedFilePath).Should().Be(FormattedClassContent);
    }

    [TestCase("0.9.0", false)]
    [TestCase("9999.0.0", false)]
    [TestCase("current", true)]
    public void Works_With_MSBuild_Version_Checking(string version, bool shouldPass)
    {
        var context = new TestContext();
        var currentVersion = typeof(CommandLineFormatter).Assembly.GetName().Version!.ToString(3);

        var versionToTest = version == "current" ? currentVersion : version;

        context.WhenAFileExists(
            "Test.csproj",
            $@"<Project Sdk=""Microsoft.NET.Sdk"">
    <ItemGroup>
        <PackageReference Include=""CSharpier.MsBuild"" Version=""{versionToTest}"" />
    </ItemGroup>
</Project>
"
        );

        var result = Format(context);

        if (shouldPass)
        {
            result.ExitCode.Should().Be(0);
            result.ErrorOutputLines.Should().BeEmpty();
        }
        else
        {
            result.ExitCode.Should().Be(1);
            result
                .ErrorOutputLines.First()
                .Should()
                .EndWith(
                    $@"Test.csproj uses version {version} of CSharpier.MsBuild which is a mismatch with version {currentVersion}"
                );
        }
    }

    [Test]
    public void Works_With_MSBuild_Version_Checking_When_No_Version_Specified()
    {
        var context = new TestContext();
        var currentVersion = typeof(CommandLineFormatter).Assembly.GetName().Version!.ToString(3);

        context.WhenAFileExists(
            "Test.csproj",
            $@"<Project Sdk=""Microsoft.NET.Sdk"">
    <ItemGroup>
        <PackageReference Include=""CSharpier.MsBuild"" />
    </ItemGroup>
</Project>
"
        );

        var result = Format(context);

        result.ExitCode.Should().Be(0);
        result
            .OutputLines.First()
            .Should()
            .EndWith($"Test.csproj uses an unknown version of CSharpier.MsBuild");
    }

    [TestCase("0.9.0", false)]
    [TestCase("9999.0.0", false)]
    [TestCase("current", true)]
    public void Works_With_MSBuild_Version_Checking_When_No_Version_Specified_With_Directory_Props(
        string version,
        bool shouldPass
    )
    {
        var context = new TestContext();
        var currentVersion = typeof(CommandLineFormatter).Assembly.GetName().Version!.ToString(3);

        var versionToTest = version == "current" ? currentVersion : version;

        context.WhenAFileExists(
            "Test.csproj",
            $@"<Project Sdk=""Microsoft.NET.Sdk"">
    <ItemGroup>
        <PackageReference Include=""CSharpier.MsBuild"" />
    </ItemGroup>
</Project>
"
        );

        context.WhenAFileExists(
            "Directory.Packages.props",
            $@"<Project>
  <ItemGroup>
    <PackageVersion Include=""CSharpier.MsBuild"" Version=""{versionToTest}"" />
  </ItemGroup>
</Project>
"
        );

        var result = Format(context);

        if (shouldPass)
        {
            result.ExitCode.Should().Be(0);
            result.ErrorOutputLines.Should().BeEmpty();
        }
        else
        {
            result.ExitCode.Should().Be(1);
            result
                .ErrorOutputLines.First()
                .Should()
                .EndWith(
                    $@"Test.csproj uses version {version} of CSharpier.MsBuild which is a mismatch with version {currentVersion}"
                );
        }
    }

    [Test]
    public void Works_With_MSBuild_Version_Checking_When_No_Version_Included()
    {
        var context = new TestContext();

        context.WhenAFileExists(
            "Test.csproj",
            $@"<Project Sdk=""Microsoft.NET.Sdk"">
    <ItemGroup>
        <PackageReference Include=""SomeOtherLibrary"" />
    </ItemGroup>
</Project>
"
        );

        var result = Format(context);

        result.ExitCode.Should().Be(0);
        result.ErrorOutputLines.Should().BeEmpty();
    }

    [Test]
    public void Format_Writes_File_With_File_Path()
    {
        var context = new TestContext();
        const string unformattedFilePath = "Unformatted.cs";
        context.WhenAFileExists(unformattedFilePath, UnformattedClassContent);

        Format(context, directoryOrFilePaths: "Unformatted.cs");

        context.GetFileContent(unformattedFilePath).Should().Be(FormattedClassContent);
    }

    [Test]
    public void Format_Supports_Skip_Write()
    {
        var context = new TestContext();
        const string unformattedFilePath = "Unformatted.cs";
        context.WhenAFileExists(unformattedFilePath, UnformattedClassContent);

        Format(context, skipWrite: true);

        context.GetFileContent(unformattedFilePath).Should().Be(UnformattedClassContent);
    }

    [Test]
    public void Format_Checks_Unformatted_File()
    {
        var context = new TestContext();
        const string unformattedFilePath = "Unformatted.cs";
        context.WhenAFileExists(unformattedFilePath, UnformattedClassContent);

        var result = Format(context, check: true);

        result.ExitCode.Should().Be(1);
        context.GetFileContent(unformattedFilePath).Should().Be(UnformattedClassContent);
        result
            .ErrorOutputLines.First()
            .Should()
            .StartWith("Error ./Unformatted.cs - Was not formatted.");
    }

    [Test]
    public void Format_Checks_Unformatted_File_With_MsBuildFormat_Message()
    {
        var context = new TestContext();
        const string unformattedFilePath = "Unformatted.cs";
        context.WhenAFileExists(unformattedFilePath, UnformattedClassContent);

        var result = Format(context, check: true, logFormat: LogFormat.MsBuild);

        result.ExitCode.Should().Be(1);
        context.GetFileContent(unformattedFilePath).Should().Be(UnformattedClassContent);
        result
            .ErrorOutputLines.First()
            .Should()
            .StartWith("./Unformatted.cs: error: Was not formatted.");
    }

    [TestCase("Src/node_modules/File.cs")]
    [TestCase("node_modules/File.cs")]
    [TestCase("node_modules/Folder/File.cs")]
    [TestCase("Src/obj/File.cs")]
    [TestCase("obj/File.cs")]
    [TestCase("obj/Folder/File.cs")]
    public void Format_Ignores_Files_In_Special_Folders(string filePath)
    {
        var context = new TestContext();
        context.WhenAFileExists(filePath, UnformattedClassContent);

        var result = Format(context, check: true);

        result.ExitCode.Should().Be(0);
    }

    [Test]
    public void Format_Checks_Formatted_File()
    {
        var context = new TestContext();
        const string formattedFilePath = "Formatted.cs";
        context.WhenAFileExists(formattedFilePath, FormattedClassContent);
        var result = Format(context, check: true);

        result.ExitCode.Should().Be(0);
    }

    [TestCase("TemporaryGeneratedFile_Tester.cs")]
    [TestCase("TestFile.designer.cs")]
    [TestCase("TestFile.generated.cs")]
    [TestCase("TestFile.g.cs")]
    [TestCase("TestFile.g.i.cs")]
    public void Format_Skips_Generated_Files(string fileName)
    {
        var context = new TestContext();
        var unformattedFilePath = fileName;
        context.WhenAFileExists(unformattedFilePath, UnformattedClassContent);

        var result = Format(context);

        result.OutputLines.FirstOrDefault().Should().StartWith("Formatted 0 files in ");
    }

    [TestCase("TemporaryGeneratedFile_Tester.cs")]
    [TestCase("TestFile.designer.cs")]
    [TestCase("TestFile.generated.cs")]
    [TestCase("TestFile.g.cs")]
    [TestCase("TestFile.g.i.cs")]
    public void Format_Formats_Generated_Files_When_Include_Generated(string fileName)
    {
        var context = new TestContext();
        var unformattedFilePath = fileName;
        context.WhenAFileExists(unformattedFilePath, UnformattedClassContent);

        var result = Format(context, includeGenerated: true);

        result.OutputLines.FirstOrDefault().Should().StartWith("Formatted 1 files in ");
    }

    [TestCase("// <autogenerated />")]
    [TestCase("/* <autogenerated /> */")]
    [TestCase("// <auto-generated />")]
    [TestCase("/* <auto-generated /> */")]
    public void Format_Skips_Auto_Generated_Comment_File(string comment)
    {
        var context = new TestContext();
        var unformattedContent = $"{comment}\n{UnformattedClassContent}";
        context.WhenAFileExists("AutoGenerated.cs", unformattedContent);

        var result = Format(context);

        result.ExitCode.Should().Be(0);
        context.GetFileContent("AutoGenerated.cs").Should().Be(unformattedContent);
    }

    [TestCase("// <autogenerated />")]
    [TestCase("/* <autogenerated /> */")]
    [TestCase("// <auto-generated />")]
    [TestCase("/* <auto-generated /> */")]
    public void Format_Formats_Auto_Generated_Comment_File_When_Include_Generated(string comment)
    {
        var context = new TestContext();
        var unformattedContent = $"{comment}\n{UnformattedClassContent}";
        context.WhenAFileExists("AutoGenerated.cs", unformattedContent);

        var result = Format(context, includeGenerated: true);

        result.ExitCode.Should().Be(0);
        context
            .GetFileContent("AutoGenerated.cs")
            .Should()
            .Be($"{comment}\n{FormattedClassContent}");
    }

    [TestCase("File.cs", "File.cs")]
    [TestCase("File.cs", "*.cs")]
    [TestCase("SubFolder/File.cs", "*.cs")]
    [TestCase("Debug/Logs/File.cs", "**/Logs")]
    [TestCase("Debug/Logs/File.cs", "Logs/")]
    [TestCase("Debug/Logs/File.cs", "Debug/Logs/File.cs")]
    [TestCase(
        @"Src/CSharpier.Playground/App_Data/Uploads/f45e11a81b926de2af29459af6974bb8.cs",
        "Uploads/"
    )]
    public void File_In_Ignore_Skips_Formatting(string fileName, string ignoreContents)
    {
        var context = new TestContext();
        var unformattedFilePath = fileName;
        context.WhenAFileExists(unformattedFilePath, UnformattedClassContent);
        context.WhenAFileExists(".csharpierignore", ignoreContents);

        var result = Format(context);

        result.OutputLines.FirstOrDefault().Should().StartWith("Formatted 0 files in ");
    }

    [TestCase("SubFolder/File.cs", "*.cs", "SubFolder")]
    [TestCase("SubFolder/File.cs", "SubFolder/File.cs", "SubFolder")]
    public void File_In_Ignore_Skips_Formatting_With_BaseDirectory(
        string fileName,
        string ignoreContents,
        string baseDirectory
    )
    {
        var context = new TestContext();
        var unformattedFilePath = fileName;
        context.WhenAFileExists(unformattedFilePath, UnformattedClassContent);
        context.WhenAFileExists(".csharpierignore", ignoreContents);

        var result = Format(
            context,
            directoryOrFilePaths: Path.Combine(GetRootPath(), baseDirectory)
        );

        result.OutputLines.FirstOrDefault().Should().StartWith("Formatted 0 files in ");
    }

    [Test]
    public void Multiple_Files_Should_Use_Root_Ignore()
    {
        var context = new TestContext();
        var unformattedFilePath1 = "SubFolder/1/File1.cs";
        var unformattedFilePath2 = "SubFolder/2/File2.cs";
        context.WhenAFileExists(unformattedFilePath1, UnformattedClassContent);
        context.WhenAFileExists(unformattedFilePath2, UnformattedClassContent);
        context.WhenAFileExists(".csharpierignore", "Subfolder/**/*.cs");

        var result = Format(
            context,
            directoryOrFilePaths: [unformattedFilePath1, unformattedFilePath2]
        );

        result.OutputLines.FirstOrDefault().Should().StartWith("Formatted 0 files in ");
    }

    [Test]
    public void Multiple_Files_Should_Use_Multiple_Ignores()
    {
        var context = new TestContext();
        var unformattedFilePath1 = "SubFolder/1/File1.cs";
        var unformattedFilePath2 = "SubFolder/2/File2.cs";
        context.WhenAFileExists(unformattedFilePath1, UnformattedClassContent);
        context.WhenAFileExists(unformattedFilePath2, UnformattedClassContent);
        context.WhenAFileExists("SubFolder/1/.csharpierignore", "File1.cs");
        context.WhenAFileExists("SubFolder/2/.csharpierignore", "File2.cs");

        var result = Format(
            context,
            directoryOrFilePaths: [unformattedFilePath1, unformattedFilePath2]
        );

        result.OutputLines.FirstOrDefault().Should().StartWith("Formatted 0 files in ");
    }

    [Test]
    public void Ignore_Should_Deal_With_Period()
    {
        var context = new TestContext();
        var unformattedFilePath1 = @"Directory.WithPeriod\File1.cs";
        context.WhenAFileExists(unformattedFilePath1, UnformattedClassContent);
        context.WhenAFileExists("Directory.WithPeriod/.csharpierignore", "File1.cs");

        var result = Format(context, directoryOrFilePaths: "Directory.WithPeriod");

        result.OutputLines.FirstOrDefault().Should().StartWith("Formatted 0 files in ");
    }

    [Test]
    public void Ignore_Should_Deal_With_Inconsistent_Slashes()
    {
        var context = new TestContext();
        var unformattedFilePath1 = @"SubFolder\1\File1.cs";
        context.WhenAFileExists(unformattedFilePath1, UnformattedClassContent);
        context.WhenAFileExists("SubFolder/1/.csharpierignore", "File1.cs");

        var result = Format(context, directoryOrFilePaths: unformattedFilePath1);

        result.OutputLines.FirstOrDefault().Should().StartWith("Formatted 0 files in ");
    }

    [Test]
    public void Ignore_Reports_Errors()
    {
        var context = new TestContext();
        context.WhenAFileExists("Test.cs", UnformattedClassContent);
        var path = context.WhenAFileExists(".csharpierignore", @"\Src\Uploads\*.cs");

        var result = Format(context);

        result.ExitCode.Should().Be(1);
        result
            .ErrorOutputLines.First()
            .Replace("\\", "/")
            .Should()
            .Contain(
                $"Error The .csharpierignore file at {path} could not be parsed due to the following line:"
            );
        result.ErrorOutputLines.Skip(1).First().Should().Contain(@"\Src\Uploads\*.cs");
    }

    [Test]
    public void Write_Stdout_Should_Only_Write_File()
    {
        var context = new TestContext();
        context.WhenAFileExists("file1.cs", UnformattedClassContent);

        var result = Format(context, writeStdout: true);

        result.OutputLines.Should().ContainSingle();
        result.OutputLines.First().Should().Be(FormattedClassContent);
    }

    [Test]
    public void Should_Format_StandardInput_When_Provided()
    {
        var context = new TestContext();
        var result = Format(context, standardInFileContents: UnformattedClassContent);

        result.OutputLines.Should().ContainSingle();
        result.OutputLines.First().Should().Be(FormattedClassContent);
    }

    [Test]
    public void File_With_Mismatched_Line_Endings_In_Verbatim_String_Should_Pass_Validation()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "file1.cs",
            "public class ClassName\n{\npublic string Value = @\"EndThisLineWith\r\nEndThisLineWith\n\";\n}"
        );

        var result = Format(context);

        result.ExitCode.Should().Be(0);
    }

    [Test]
    public void File_With_Compilation_Error_Should_Not_Lose_Code()
    {
        var context = new TestContext();
        var contents =
            @"#if DEBUG
?using System;
#endif
";
        context.WhenAFileExists("Invalid.cs", contents);

        var result = Format(context);

        context.GetFileContent("Invalid.cs").Should().Be(contents);
        result
            .ErrorOutputLines.First()
            .Should()
            .Be("Error ./Invalid.cs - Failed to compile so was not formatted.");
    }

    [TestCase(
        @"class ClassName
{
#if DEBUG
    private void MethodName() { }

    static public void ReorderModifiers() { }
#endif
}
"
    )]
    [TestCase(
        @"#if DEBUG

class ClassName
{
    private static string field1;

    static public string reorderedField;
}

#endif
"
    )]
    public void File_With_Reorder_Modifiers_In_If_Directive_Should_Pass_Validation(string contents)
    {
        var context = new TestContext();

        context.WhenAFileExists("file1.cs", contents);

        var result = Format(context);

        context
            .GetFileContent("file1.cs")
            .Should()
            .Be(contents.Replace("static public", "public static"));
        result.ErrorOutputLines.Should().BeEmpty();
        result.OutputLines.First().Should().StartWith("Formatted 1 files in");
    }

    [Test]
    public void File_With_Added_Trailing_Comma_Before_Comment_Should_Pass_Validation()
    {
        var context = new TestContext();

        var fileContents = """
            var someObject = new SomeObject()
            {
                Property1 = 1,
                Property2 = 2 // Trailing Comment
            };
            """;
        context.WhenAFileExists("file1.cs", fileContents);

        var result = Format(context);

        result.ErrorOutputLines.Should().BeEmpty();
        result.OutputLines.First().Should().StartWith("Formatted 1 files in");
    }

    [TestCase(".csharpierrc")]
    [TestCase(".csharpierrc.json")]
    [TestCase(".csharpierrc.yaml")]
    public void Empty_Config_Files_Should_Log_Warning(string configFileName)
    {
        var context = new TestContext();
        var configPath = context.WhenAFileExists(".csharpierrc", "");
        context.WhenAFileExists("file1.cs", "public class ClassName { }");

        var result = Format(context);

        result
            .OutputLines.First()
            .Replace("\\", "/")
            .Should()
            .Be($"Warning The configuration file at {configPath} was empty.");
    }

    [Test]
    public void Should_Support_Config_Path()
    {
        var context = new TestContext();
        var configPath = context.WhenAFileExists("config/.csharpierrc", "printWidth: 10");
        context.WhenAFileExists("file1.cs", "var myVariable = someLongValue;");

        Format(context, configPath: configPath);

        context.GetFileContent("file1.cs").Should().Be("var myVariable =\n    someLongValue;\n");
    }

    [Test]
    public void Should_Support_Config_Path_With_Editor_Config()
    {
        var context = new TestContext();
        var configPath = context.WhenAFileExists(
            "config/.editorconfig",
            """
            [*]
            max_line_length = 10
            """
        );
        var fileName = context.WhenAFileExists("file1.cs", "var myVariable = someLongValue;");

        Format(context, configPath: configPath);

        context.GetFileContent(fileName).Should().Be("var myVariable =\n    someLongValue;\n");
    }

    private static FormatResult Format(
        TestContext context,
        bool skipWrite = false,
        bool check = false,
        LogFormat logFormat = LogFormat.Console,
        bool writeStdout = false,
        bool includeGenerated = false,
        bool compilationErrorsAsWarnings = false,
        string? standardInFileContents = null,
        string? configPath = null,
        params string[] directoryOrFilePaths
    )
    {
        var originalDirectoryOrFilePaths = directoryOrFilePaths;
        if (directoryOrFilePaths.Length == 0)
        {
            directoryOrFilePaths = [GetRootPath()];
            originalDirectoryOrFilePaths = ["."];
        }
        else
        {
            directoryOrFilePaths = directoryOrFilePaths
                .Select(o => context.FileSystem.Path.Combine(GetRootPath(), o))
                .ToArray();
        }

        var fakeConsole = new TestConsole();
        var testLogger = new ConsoleLogger(fakeConsole, LogLevel.Information, logFormat);
        var exitCode = CommandLineFormatter
            .Format(
                new CommandLineOptions
                {
                    ConfigPath = configPath,
                    DirectoryOrFilePaths = directoryOrFilePaths,
                    OriginalDirectoryOrFilePaths = originalDirectoryOrFilePaths,
                    SkipWrite = skipWrite,
                    Check = check,
                    LogFormat = logFormat,
                    WriteStdout = writeStdout || standardInFileContents != null,
                    StandardInFileContents = standardInFileContents,
                    IncludeGenerated = includeGenerated,
                    CompilationErrorsAsWarnings = compilationErrorsAsWarnings,
                },
                context.FileSystem,
                fakeConsole,
                testLogger,
                CancellationToken.None
            )
            .Result;

        return new FormatResult(exitCode, fakeConsole.GetLines(), fakeConsole.GetErrorLines());
    }

    private static string GetRootPath()
    {
        return OperatingSystem.IsWindows() ? @"c:\test" : "/Test";
    }

    private sealed class TestContext
    {
        public readonly MockFileSystem FileSystem = new();

        public TestContext()
        {
            this.FileSystem.AddDirectory(GetRootPath());
        }

        public string WhenAFileExists(string path, string contents)
        {
            path = this.FileSystem.Path.Combine(GetRootPath(), path).Replace('\\', '/');
            this.FileSystem.AddFile(path, new MockFileData(contents));
            return path;
        }

        public string GetFileContent(string path)
        {
            path = this.FileSystem.Path.Combine(GetRootPath(), path);
            return this.FileSystem.File.ReadAllText(path);
        }
    }

    private sealed record FormatResult(
        int ExitCode,
        IList<string> OutputLines,
        IList<string> ErrorOutputLines
    );

    private sealed class TestConsole : IConsole
    {
        private readonly List<string> lines = [];
        private readonly List<string> errorLines = [];

        public List<string> GetLines()
        {
            this.FinishReadingLines();
            return this.lines;
        }

        public List<string> GetErrorLines()
        {
            this.FinishReadingLines();
            return this.errorLines;
        }

        private string nextLine = string.Empty;
        private string nextErrorLine = string.Empty;

        public void WriteLine(string? line = null)
        {
            while (line != null && line.Contains("  "))
            {
                line = line.Replace("  ", " ");
            }

            if (line != null)
            {
                this.nextLine += line;
                this.lines.Add(this.nextLine);
                this.nextLine = "";
            }
        }

        public void WriteErrorLine(string? line = null)
        {
            while (line != null && line.Contains("  "))
            {
                line = line.Replace("  ", " ");
            }

            if (line != null)
            {
                this.nextErrorLine += line;
                this.errorLines.Add(this.nextErrorLine);
                this.nextErrorLine = "";
            }
        }

        public void Write(string value)
        {
            this.nextLine += value;
        }

        public void WriteError(string value)
        {
            this.nextErrorLine += value;
        }

        public Encoding InputEncoding => Encoding.UTF8;
        public ConsoleColor ForegroundColor { get; set; }

        public void ResetColor() { }

        private void FinishReadingLines()
        {
            if (this.nextLine != string.Empty)
            {
                this.lines.Add(this.nextLine);
                this.nextLine = string.Empty;
            }
            if (this.nextErrorLine != string.Empty)
            {
                this.errorLines.Add(this.nextErrorLine);
                this.nextErrorLine = string.Empty;
            }
        }
    }
}
