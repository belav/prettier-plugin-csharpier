using System.IO.Abstractions.TestingHelpers;
using CSharpier.Cli.Options;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace CSharpier.Tests;

[TestFixture]
[Parallelizable(ParallelScope.Fixtures)]
public class OptionsProviderTests
{
    [Test]
    public async Task Should_Return_Default_Options_With_Empty_Json()
    {
        var context = new TestContext();
        context.WhenAFileExists("c:/test/.csharpierrc", "{}");

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");

        ShouldHaveDefaultOptions(result);
    }

    [Test]
    public async Task Should_Return_Default_Options_With_No_File_And_Known_Extension()
    {
        var context = new TestContext();
        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");

        ShouldHaveDefaultOptions(result);
    }

    [Test]
    public async Task Should_Return_Default_Options_With_No_File_And_Unknown_Extension()
    {
        var context = new TestContext();
        var result = async () =>
            await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cst");

        await result.Should().ThrowAsync<Exception>();
    }

    [TestCase(".csharpierrc")]
    [TestCase(".csharpierrc.json")]
    [TestCase(".csharpierrc.yaml")]
    public async Task Should_Return_Default_Options_With_Empty_File(string fileName)
    {
        var context = new TestContext();
        context.WhenAFileExists($"c:/test/{fileName}", string.Empty);
        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");

        ShouldHaveDefaultOptions(result);
    }

    [Test]
    public async Task Should_Return_Json_Extension_Options()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/.csharpierrc.json",
            @"{ 
    ""printWidth"": 10, 
    ""endOfLine"": ""crlf""
}"
        );

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");

        result.Width.Should().Be(10);
        result.EndOfLine.Should().Be(EndOfLine.CRLF);
    }

    [TestCase("yaml")]
    [TestCase("yml")]
    public async Task Should_Return_Yaml_Extension_Options(string extension)
    {
        var context = new TestContext();
        context.WhenAFileExists(
            $"c:/test/.csharpierrc.{extension}",
            @"
printWidth: 10
endOfLine: crlf
"
        );

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");

        result.Width.Should().Be(10);
        result.EndOfLine.Should().Be(EndOfLine.CRLF);
    }

    [TestCase("{ \"printWidth\": 10 }")]
    [TestCase("printWidth: 10")]
    public async Task Should_Read_ExtensionLess_File(string contents)
    {
        var context = new TestContext();
        context.WhenAFileExists($"c:/test/.csharpierrc", contents);

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");

        result.Width.Should().Be(10);
    }

    [TestCase("", "printWidth: 10")]
    [TestCase("", "{ \"printWidth\": 10 }")]
    [TestCase(".yml", "printWidth: 10")]
    [TestCase(".yaml", "printWidth: 10")]
    [TestCase(".json", "{ \"printWidth\": 10 }")]
    public async Task Should_Find_Configuration_In_Parent_Directory(
        string extension,
        string contents
    )
    {
        var context = new TestContext();
        context.WhenAFileExists($"c:/test/.csharpierrc{extension}", contents);

        var result = await context.CreateProviderAndGetOptionsFor(
            "c:/test/subfolder",
            "c:/test/subfolder/test.cs"
        );

        result.Width.Should().Be(10);
    }

    [Test]
    public async Task Should_Prefer_No_Extension()
    {
        var context = new TestContext();
        context.WhenAFileExists("c:/test/.csharpierrc", "{ \"printWidth\": 1 }");

        context.WhenAFileExists("c:/test/.csharpierrc.json", "{ \"printWidth\": 2 }");
        context.WhenAFileExists("c:/test/.csharpierrc.yaml", "printWidth: 3");

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");

        result.Width.Should().Be(1);
    }

    [Test]
    public async Task Should_Return_PrintWidth_With_Json()
    {
        var context = new TestContext();
        context.WhenAFileExists("c:/test/.csharpierrc", "{ \"printWidth\": 10 }");

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");

        result.Width.Should().Be(10);
    }

    [Test]
    public async Task Should_Return_TabWidth_With_Json()
    {
        var context = new TestContext();
        context.WhenAFileExists("c:/test/.csharpierrc", "{ \"tabWidth\": 10 }");

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");

        result.IndentSize.Should().Be(10);
    }

    [Test]
    public async Task Should_Return_UseTabs_With_Json()
    {
        var context = new TestContext();
        context.WhenAFileExists("c:/test/.csharpierrc", "{ \"useTabs\": true }");

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");

        result.UseTabs.Should().BeTrue();
    }

    [Test]
    public async Task Should_Return_PrintWidth_With_Yaml()
    {
        var context = new TestContext();
        context.WhenAFileExists("c:/test/.csharpierrc", "printWidth: 10");

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");

        result.Width.Should().Be(10);
    }

    [Test]
    public async Task Should_Return_TabWidth_With_Yaml()
    {
        var context = new TestContext();
        context.WhenAFileExists("c:/test/.csharpierrc", "tabWidth: 10");

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");

        result.IndentSize.Should().Be(10);
    }

    [Test]
    public async Task Should_Return_UseTabs_With_Yaml()
    {
        var context = new TestContext();
        context.WhenAFileExists("c:/test/.csharpierrc", "useTabs: true");

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");

        result.UseTabs.Should().BeTrue();
    }

    [Test]
    public async Task Should_Return_TabWidth_For_Override()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/.csharpierrc",
            """
            overrides:
                - files: "*.cst"
                  formatter: "csharp"
                  tabWidth: 2
            """
        );

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cst");

        result.IndentSize.Should().Be(2);
    }

    [Test]
    public async Task Should_Support_EditorConfig_Basic()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/.editorconfig",
            @"
[*]
indent_style = space
indent_size = 2
max_line_length = 10
end_of_line = crlf
"
        );

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");

        result.UseTabs.Should().BeFalse();
        result.IndentSize.Should().Be(2);
        result.Width.Should().Be(10);
        result.EndOfLine.Should().Be(EndOfLine.CRLF);
    }

    [Test]
    public async Task Should_Support_EditorConfig_With_Comments()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/.editorconfig",
            @"
# EditorConfig is awesome: https://EditorConfig.org

# top-most EditorConfig file
root = true

[*]
indent_style = space
indent_size = 2
max_line_length = 10
; Windows-style line endings
end_of_line = crlf
"
        );

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");

        result.UseTabs.Should().BeFalse();
        result.IndentSize.Should().Be(2);
        result.Width.Should().Be(10);
        result.EndOfLine.Should().Be(EndOfLine.CRLF);
    }

    [Test]
    public async Task Should_Support_EditorConfig_With_Duplicated_Sections()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/.editorconfig",
            @"
[*]
indent_size = 2

[*]
indent_size = 4
"
        );

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");

        result.IndentSize.Should().Be(4);
    }

    [Test]
    public async Task Should_Support_EditorConfig_With_Duplicated_Rules()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/.editorconfig",
            @"
[*]
indent_size = 2
indent_size = 4
"
        );

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");

        result.IndentSize.Should().Be(4);
    }

    [Test]
    public async Task Should_Not_Fail_With_Bad_EditorConfig()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/.editorconfig",
            @"
[*
indent_size==
"
        );

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");

        result.IndentSize.Should().Be(4);
    }

    [TestCase("tab_width")]
    [TestCase("indent_size")]
    public async Task Should_Support_EditorConfig_Tabs(string propertyName)
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/.editorconfig",
            $@"
    [*]
    indent_style = tab
    {propertyName} = 2
    "
        );

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");

        result.UseTabs.Should().BeTrue();
        result.IndentSize.Should().Be(2);
    }

    [Test]
    public async Task Should_Support_EditorConfig_Tabs_With_Tab_Width()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/.editorconfig",
            @"
    [*]
    indent_style = tab
    indent_size = 1
    tab_width = 3
    "
        );

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");

        result.UseTabs.Should().BeTrue();
        result.IndentSize.Should().Be(3);
    }

    [Test]
    public async Task Should_Support_EditorConfig_Tabs_With_Indent_Size_Tab()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/.editorconfig",
            @"
    [*]
    indent_size = tab
    tab_width = 3
    "
        );

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");

        result.IndentSize.Should().Be(3);
    }

    [Test]
    public async Task Should_Support_EditorConfig_Tabs_With_Multiple_Files()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/subfolder/.editorconfig",
            @"
    [*]
    indent_size = 1
    "
        );

        context.WhenAFileExists(
            "c:/test/.editorconfig",
            @"
    [*]
    indent_size = 2
    max_line_length = 10
    "
        );

        var result = await context.CreateProviderAndGetOptionsFor(
            "c:/test/subfolder",
            "c:/test/subfolder/test.cs"
        );
        result.IndentSize.Should().Be(1);
        result.Width.Should().Be(10);
    }

    [Test]
    public async Task Should_Support_EditorConfig_Tabs_With_Multiple_Files_And_Unset()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/subfolder/.editorconfig",
            @"
    [*]
    indent_size = unset
    "
        );

        context.WhenAFileExists(
            "c:/test/.editorconfig",
            @"
    [*]
    indent_size = 2
    "
        );

        var result = await context.CreateProviderAndGetOptionsFor(
            "c:/test/subfolder",
            "c:/test/subfolder/test.cs"
        );
        result.IndentSize.Should().Be(4);
    }

    [Test]
    public async Task Should_Support_EditorConfig_Root()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/subfolder/.editorconfig",
            @"
    root = true

    [*]
    indent_size = 2
    "
        );

        context.WhenAFileExists(
            "c:/test/.editorconfig",
            @"
    [*]
    max_line_length = 2
    "
        );

        var result = await context.CreateProviderAndGetOptionsFor(
            "c:/test/subfolder",
            "c:/test/subfolder/test.cs"
        );
        result.Width.Should().Be(100);
    }

    [Test]
    public async Task Should_Support_EditorConfig_Tabs_With_Globs()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/.editorconfig",
            @"
[*]
indent_size = 1

[*.cs]
indent_size = 2
"
        );

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");
        result.IndentSize.Should().Be(2);
    }

    [Test]
    public async Task Should_Support_EditorConfig_Tabs_With_Glob_Braces()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/.editorconfig",
            @"
[*]
indent_size = 1

[*.{cs}]
indent_size = 2
"
        );

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");
        result.IndentSize.Should().Be(2);
    }

    [Test]
    public async Task Should_Support_EditorConfig_Tabs_With_Glob_Braces_Multiples()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/.editorconfig",
            @"
[*]
indent_size = 1

[*.{csx,cs}]
indent_size = 2
"
        );

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");
        result.IndentSize.Should().Be(2);
    }

    [Test]
    public async Task Should_Support_EditorConfig_Tabs_With_Glob_Braces_Only()
    {
        // TODO should we support | ? it seems to work when it isn't the first pattern in a file
        // probably easier to just only support , - it is the standard way of doing glob patterns and I accidentally ended up with | in the documentation.
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/.editorconfig",
            """
            [*.{cs|csx}]
            indent_style = space
            indent_size = 2
            max_line_length = 120
            """
        );

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");
        result.IndentSize.Should().Be(2);
    }

    [Test]
    public async Task Should_Return_IndentSize_For_Formatter_In_Editorconfig()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/.editorconfig",
            """
            [*.cst]
            indent_size = 2
            csharpier_formatter = csharp
            """
        );

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cst");

        result.IndentSize.Should().Be(2);
    }

    [Test]
    public async Task Should_Find_EditorConfig_In_Parent_Directory()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/.editorconfig",
            @"
[*.cs]
indent_size = 2
"
        );

        var result = await context.CreateProviderAndGetOptionsFor(
            "c:/test/subfolder",
            "c:/test/subfolder/test.cs"
        );
        result.IndentSize.Should().Be(2);
    }

    [Test]
    public async Task Should_Prefer_CSharpierrc_In_SameFolder()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/.editorconfig",
            @"
[*.cs]
indent_size = 2
"
        );
        context.WhenAFileExists("c:/test/.csharpierrc", "tabWidth: 1");

        var result = await context.CreateProviderAndGetOptionsFor(
            "c:/test/subfolder",
            "c:/test/test.cs"
        );
        result.IndentSize.Should().Be(1);
    }

    [Test]
    public async Task Should_Not_Prefer_Closer_EditorConfig()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/subfolder/.editorconfig",
            @"
[*.cs]
indent_size = 2
"
        );
        context.WhenAFileExists("c:/test/.csharpierrc", "tabWidth: 1");

        var result = await context.CreateProviderAndGetOptionsFor(
            "c:/test",
            "c:/test/subfolder/test.cs"
        );
        result.IndentSize.Should().Be(1);
    }

    [Test]
    public async Task Should_Ignore_Invalid_EditorConfig_Lines()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/.editorconfig",
            @"
[*]
indent_size = 2
INVALID
"
        );

        var result = await context.CreateProviderAndGetOptionsFor("c:/test", "c:/test/test.cs");

        result.IndentSize.Should().Be(2);
    }

    [Test]
    public async Task Should_Ignore_Ignored_EditorConfig()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/subfolder/.editorconfig",
            @"
    [*]
    indent_size = 2
    "
        );

        context.WhenAFileExists(
            "c:/test/.editorconfig",
            @"
    [*]
    indent_size = 1
    "
        );

        context.WhenAFileExists("c:/test/.csharpierignore", "/subfolder/.editorconfig");

        var result = await context.CreateProviderAndGetOptionsFor(
            "c:/test",
            "c:/test/subfolder/test.cs"
        );
        result.IndentSize.Should().Be(1);
    }

    [Test]
    public async Task Should_Prefer_Closer_CSharpierrc()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/.editorconfig",
            @"
[*.cs]
indent_size = 2
"
        );
        context.WhenAFileExists("c:/test/subfolder/.csharpierrc", "tabWidth: 1");

        var result = await context.CreateProviderAndGetOptionsFor(
            "c:/test",
            "c:/test/subfolder/test.cs"
        );
        result.IndentSize.Should().Be(1);
    }

    [Test]
    public async Task Should_Not_Look_For_Subfolders_EditorConfig_When_Limited()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/subfolder/.editorconfig",
            @"
    [*]
    indent_size = 1
    "
        );

        // this shouldn't happen in the real world, but validates we correctly limit
        // the search to the top directory only
        var result = await context.CreateProviderAndGetOptionsFor(
            "c:/test/",
            "c:/test/subfolder/test.cs",
            limitEditorConfigSearch: true
        );
        result.IndentSize.Should().Be(4);
    }

    [Test]
    public async Task Should_Not_Look_For_Subfolders_CSharpierRc_When_Limited()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/subfolder/.csharpierrc",
            @"
    [*]
    tabWidth: 1
    "
        );

        // this shouldn't happen in the real world, but validates we correctly limit
        // the search to the top directory only
        var result = await context.CreateProviderAndGetOptionsFor(
            "c:/test/",
            "c:/test/subfolder/test.cs",
            limitEditorConfigSearch: true
        );
        result.IndentSize.Should().Be(4);
    }

    [Test]
    public async Task Should_Look_For_Subfolders_When_Limited()
    {
        var context = new TestContext();
        context.WhenAFileExists(
            "c:/test/.editorconfig",
            @"
    [*]
    indent_size = 1
    "
        );

        var result = await context.CreateProviderAndGetOptionsFor(
            "c:/test/subfolder",
            "c:/test/subfolder/test.cs",
            limitEditorConfigSearch: true
        );
        result.IndentSize.Should().Be(1);
    }

    private static void ShouldHaveDefaultOptions(PrinterOptions printerOptions)
    {
        printerOptions.Width.Should().Be(100);
        printerOptions.IndentSize.Should().Be(4);
        printerOptions.UseTabs.Should().BeFalse();
        printerOptions.EndOfLine.Should().Be(EndOfLine.Auto);
    }

    private class TestContext
    {
        private readonly MockFileSystem fileSystem = new();

        public void WhenAFileExists(string path, string contents)
        {
            if (!OperatingSystem.IsWindows())
            {
                path = path.Replace("c:", string.Empty);
            }

            this.fileSystem.AddFile(path, new MockFileData(contents));
        }

        public async Task<PrinterOptions> CreateProviderAndGetOptionsFor(
            string directoryName,
            string filePath,
            bool limitEditorConfigSearch = false
        )
        {
            if (!OperatingSystem.IsWindows())
            {
                directoryName = directoryName.Replace("c:", string.Empty);
                filePath = filePath.Replace("c:", string.Empty);
            }

            this.fileSystem.AddDirectory(directoryName);
            var provider = await OptionsProvider.Create(
                directoryName,
                null,
                this.fileSystem,
                NullLogger.Instance,
                CancellationToken.None,
                limitEditorConfigSearch
            );

            var printerOptions = provider.GetPrinterOptionsFor(filePath);

            if (printerOptions is null)
            {
                throw new Exception("PrinterOptions was null");
            }

            return printerOptions;
        }
    }
}
