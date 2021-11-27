using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace CSharpier.Cli.Tests;

// TODO update workflow to run these
public class CliTests
{
    private const string TestFileDirectory = "TestFiles";

    [SetUp]
    public void BeforeEachTest()
    {
        if (Directory.Exists(TestFileDirectory))
        {
            Directory.Delete(TestFileDirectory, true);
        }

        Directory.CreateDirectory(TestFileDirectory);
    }

    [TestCase("\n")]
    [TestCase("\r\n")]
    public async Task Should_Format_Basic_File(string lineEnding)
    {
        var formattedContent = "public class ClassName { }" + lineEnding;
        var unformattedContent = $"public class ClassName {{{lineEnding}{lineEnding}}}";

        await WriteFileAsync("BasicFile.cs", unformattedContent);

        using var process = new CsharpierProcess("BasicFile.cs");

        var (output, exitCode) = await process.GetOutputWithExitCode();
        var result = await ReadAllTextAsync("BasicFile.cs");

        output.Should().StartWith("Total time:");
        output
            .Should()
            .Contain(
                "Total files:                                                                           1"
            );
        exitCode.Should().Be(0);
        result.Should().Be(formattedContent);
    }

    [Test]
    public async Task Should_Respect_Ignore_File_With_Subdirectory_When_DirectorOrFile_Is_Dot()
    {
        var unformattedContent = "public class Unformatted {     }";
        var filePath = "Subdirectory/IgnoredFile.cs";
        await WriteFileAsync(filePath, unformattedContent);
        await WriteFileAsync(".csharpierignore", filePath);

        // TODO easier way to run this and get output
        using var process = new CsharpierProcess(".");
        await process.GetOutputWithExitCode();
        var result = await ReadAllTextAsync(filePath);

        result.Should().Be(unformattedContent, $"The file at {filePath} should have been ignored");
    }

    [Test]
    public async Task Should_Return_Error_When_No_DirectoryOrFile_And_Not_Piping_StdIn()
    {
        throw new Exception("TODO CSharpierProcess always pipes input, we need to change that");
        using var process = new CsharpierProcess();
        var (output, exitCode) = await process.GetOutputWithExitCode();

        exitCode.Should().Be(1);
        output.Should().Contain("directoryOrFile is required when not piping stdin to CSharpier");
    }

    [TestCase("\n")]
    [TestCase("\r\n")]
    public async Task Should_Format_Piped_File(string lineEnding)
    {
        var formattedContent1 = "public class ClassName1 { }" + lineEnding;
        var unformattedContent1 = $"public class ClassName1 {{{lineEnding}{lineEnding}}}";

        using var process = new CsharpierProcess();

        process.Write(unformattedContent1);

        var result = await process.GetOutput();

        result.Should().Be(formattedContent1);
    }

    [Test]
    public async Task With_Check_Should_Write_Unformatted_File()
    {
        var unformattedContent = "public class ClassName1 {\n\n}";

        await WriteFileAsync("CheckUnformatted.cs", unformattedContent);

        using var process = new CsharpierProcess("CheckUnformatted.cs --check");
        var (result, exitCode) = await process.GetOutputWithExitCode();

        result.Should().StartWith("Warning /CheckUnformatted.cs - Was not formatted.");
        exitCode.Should().Be(1);
    }

    // TODO file with compilation error should spit out warning for regular use
    // TODO file with compilation error should return file for piping, multi file and single file

    // TODO test with ignored file?
    // TODO why doesn't the final /n show up? is it trimmed by the process junk?
    [TestCase("\n")]
    [TestCase("\r\n")]
    public async Task Cli_Should_Format_Multiple_Piped_Files(string lineEnding)
    {
        var formattedContent1 = "public class ClassName1 { }" + lineEnding;
        var formattedContent2 = "public class ClassName2 { }" + lineEnding;
        ;
        var unformattedContent1 = $"public class ClassName1 {{{lineEnding}{lineEnding}}}";
        var unformattedContent2 = $"public class ClassName2 {{{lineEnding}{lineEnding}}}";

        using var process = new CsharpierProcess("--pipe-multiple-files");

        process.Write("Test2.cs");
        process.Write('\u0003');
        process.Write(unformattedContent1);
        process.Write('\u0003');

        var result = await process.GetOutputWithoutClosing();

        result.Should().Be(formattedContent1);

        process.Write("Test2.cs");
        process.Write('\u0003');
        process.Write(unformattedContent2);
        process.Write('\u0003');

        result = await process.GetOutputWithoutClosing();

        result.Should().Be(formattedContent2);
    }

    private async Task WriteFileAsync(string path, string content)
    {
        var fileInfo = new FileInfo(Path.Combine(TestFileDirectory, path));
        EnsureExists(fileInfo.Directory!);

        await File.WriteAllTextAsync(fileInfo.FullName, content);
    }

    private async Task<string> ReadAllTextAsync(string path)
    {
        return await File.ReadAllTextAsync(Path.Combine(TestFileDirectory, path));
    }

    private void EnsureExists(DirectoryInfo directoryInfo)
    {
        if (directoryInfo.Parent != null)
        {
            EnsureExists(directoryInfo.Parent);
        }

        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }
    }

    // cli wrap does not give a clean way to do this because it is hard to distinguish
    // between files that come back from it
    // but using this with the regular tests was causing troubles
    private class CsharpierProcess : IDisposable
    {
        private string output = string.Empty;
        private readonly StreamWriter standardInput;
        private readonly Process process;

        public CsharpierProcess(string? arguments = null)
        {
            const string path = "dotnet-csharpier.exe";

            void OutputHandler(object sender, DataReceivedEventArgs e)
            {
                Console.WriteLine("got data: " + e.Data + "-----");
                this.output += e.Data;
            }

            this.process = new Process();
            this.process.StartInfo.WorkingDirectory = Path.Combine(
                Directory.GetCurrentDirectory(),
                TestFileDirectory
            );
            this.process.StartInfo.FileName = path;
            this.process.StartInfo.Arguments = arguments;
            this.process.StartInfo.UseShellExecute = false;
            this.process.StartInfo.RedirectStandardInput = true;
            this.process.StartInfo.RedirectStandardOutput = true;
            this.process.StartInfo.RedirectStandardError = true;
            this.process.ErrorDataReceived += OutputHandler;

            this.process.Start();

            this.standardInput = this.process.StandardInput;

            this.process.BeginErrorReadLine();
        }

        public void Write(string value)
        {
            this.standardInput.Write(value);
            Console.WriteLine("Write: " + value);
        }

        public void Write(char value)
        {
            this.standardInput.Write(value);
            Console.WriteLine("Write: " + value);
        }

        public Task<string> GetOutputWithoutClosing()
        {
            return this.GetOutput(false);
        }

        public Task<string> GetOutput()
        {
            return this.GetOutput(true);
        }

        public async Task<(string, int)> GetOutputWithExitCode()
        {
            await this.process.WaitForExitAsync();
            return (await this.process.StandardOutput.ReadToEndAsync(), this.process.ExitCode);
        }

        private async Task<string> GetOutput(bool close)
        {
            if (close)
            {
                this.standardInput.Close();
                await this.process.WaitForExitAsync();
                return await this.process.StandardOutput.ReadToEndAsync();
            }

            var result = new StringBuilder();
            while (true)
            {
                result.Append(Convert.ToChar(this.process.StandardOutput.Read()));

                // wait 10ms to see if there will be another character to read
                var x = 0;
                while (this.process.StandardOutput.Peek() < 0)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    x++;
                    if (x > 10)
                    {
                        return result.ToString();
                    }
                }
            }
        }

        public void Dispose()
        {
            this.standardInput.Dispose();
            this.process.Kill();
            this.process.Dispose();
        }
    }
}
