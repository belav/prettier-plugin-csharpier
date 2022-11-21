using System.IO.Abstractions;
using System.Text;
using CSharpier.Cli;
using CSharpier.SyntaxPrinter;
using DiffEngine;
using FluentAssertions;
using NUnit.Framework;

namespace CSharpier.Tests.FormattingTests;

public class BaseTest
{
#pragma warning disable CS8618
    private DirectoryInfo rootDirectory;
#pragma warning restore CS8618

    [OneTimeSetUp]
    public void Setup()
    {
        this.rootDirectory = DirectoryFinder.FindParent("CSharpier.Tests");
    }

    protected void RunTest(string fileName, bool useTabs = false)
    {
        var filePath = Path.Combine(
            this.rootDirectory.FullName,
            "FormattingTests",
            "TestFiles",
            fileName + ".cst"
        );
        var fileReaderResult = FileReader
            .ReadFile(filePath, new FileSystem(), CancellationToken.None)
            .Result;

        PreprocessorSymbols.Reset();

        var result = CodeFormatter.Format(
            fileReaderResult.FileContents,
            new PrinterOptions { Width = PrinterOptions.WidthUsedByTests, UseTabs = useTabs }
        );

        var actualFilePath = filePath.Replace(".cst", ".actual.cst");
        File.WriteAllText(actualFilePath, result.Code, fileReaderResult.Encoding);

        var filePathToChange = filePath;
        var expectedFilePath = actualFilePath.Replace(".actual.", ".expected.");

        var expectedCode = fileReaderResult.FileContents;

        if (File.Exists(expectedFilePath))
        {
            expectedCode = File.ReadAllText(expectedFilePath, Encoding.UTF8);
            filePathToChange = expectedFilePath;
        }

        var normalizedCode = result.Code;

        if (Environment.GetEnvironmentVariable("NormalizeLineEndings") != null)
        {
            expectedCode = expectedCode.Replace("\r\n", "\n");
            normalizedCode = normalizedCode.Replace("\r\n", "\n");
        }

        var comparer = new SyntaxNodeComparer(expectedCode, normalizedCode, CancellationToken.None);

        result.Errors.Should().BeEmpty();
        result.FailureMessage.Should().BeEmpty();

        if (normalizedCode != expectedCode && !BuildServerDetector.Detected)
        {
            DiffRunner.Launch(filePathToChange, actualFilePath);
        }
        normalizedCode.Should().Be(expectedCode);

        var compareResult = comparer.CompareSource();
        compareResult.Should().BeNullOrEmpty();
    }
}
