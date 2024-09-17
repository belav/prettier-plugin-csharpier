using System.IO;
using DiffEngine;
using NUnit.Framework;

namespace CSharpier.Tests.FormattingTests;

[SetUpFixture]
public class SetUpFixture
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        EmptyFiles.FileExtensions.AddTextExtension(".test");

        DiffTools.AddToolBasedOn(
            DiffTool.WinMerge,
            name: "WritableWinMerge",
            launchArguments: new(LeftArguments, RightArguments)
        );
    }

    static string LeftArguments(string temp, string target)
    {
        var tempTitle = Path.GetFileName(temp);
        var targetTitle = Path.GetFileName(target);
        return $"/u /wr /e \"{target}\" \"{temp}\" /dl \"{targetTitle}\" /dr \"{tempTitle}\" /cfg Backup/EnableFile=0";
    }

    static string RightArguments(string temp, string target)
    {
        var tempTitle = Path.GetFileName(temp);
        var targetTitle = Path.GetFileName(target);
        return $"/u /wl /e \"{temp}\" \"{target}\" /dl \"{tempTitle}\" /dr \"{targetTitle}\" /cfg Backup/EnableFile=0";
    }
}
