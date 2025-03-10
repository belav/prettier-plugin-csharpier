using CSharpier.Cli.Server;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;

namespace CSharpier.Cli;

internal class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = CommandLineOptions.Create();

        rootCommand.Handler = CommandHandler.Create(new CommandLineOptions.Handler(Run));

        return await rootCommand.InvokeAsync(args);
    }

    // TODO at some point (1.0?) the options should be cleaned up
    // and use sub commands
    public static async Task<int> Run(
        string[]? directoryOrFile,
        bool check,
        LogFormat logFormat,
        bool fast,
        bool skipWrite,
        bool writeStdout,
        bool pipeMultipleFiles,
        bool server,
        int? serverPort,
        bool noCache,
        bool noMSBuildCheck,
        bool includeGenerated,
        bool compilationErrorsAsWarnings,
        string configPath,
        LogLevel logLevel,
        CancellationToken cancellationToken
    )
    {
        // System.CommandLine passes string.empty instead of null when this isn't supplied even if we use string?
        var actualConfigPath = string.IsNullOrEmpty(configPath) ? null : configPath;

        var console = new SystemConsole();
        var logger = new ConsoleLogger(console, logLevel, logFormat);

        if (pipeMultipleFiles)
        {
            return await PipeMultipleFilesFormatter.StartServer(
                console,
                logger,
                actualConfigPath,
                cancellationToken
            );
        }

        if (server)
        {
            return await ServerFormatter.StartServer(serverPort, logger, actualConfigPath);
        }

        var directoryOrFileNotProvided = directoryOrFile is null or { Length: 0 };
        var originalDirectoryOrFile = directoryOrFile;

        string? standardInFileContents = null;
        if (Console.IsInputRedirected && directoryOrFileNotProvided)
        {
            using var streamReader = new StreamReader(
                Console.OpenStandardInput(),
                console.InputEncoding
            );
            standardInFileContents = await streamReader.ReadToEndAsync(
#if NET7_0_OR_GREATER
                cancellationToken
#endif
            );

            directoryOrFile = [Directory.GetCurrentDirectory()];
            originalDirectoryOrFile = [Directory.GetCurrentDirectory()];
        }
        else
        {
            directoryOrFile = directoryOrFile!.Select(Path.GetFullPath).ToArray();
        }

        var commandLineOptions = new CommandLineOptions
        {
            DirectoryOrFilePaths = directoryOrFile.ToArray(),
            OriginalDirectoryOrFilePaths = originalDirectoryOrFile!,
            StandardInFileContents = standardInFileContents,
            Check = check,
            LogFormat = logFormat,
            NoCache = noCache,
            NoMSBuildCheck = noMSBuildCheck,
            Fast = fast,
            SkipWrite = skipWrite,
            WriteStdout = writeStdout || standardInFileContents != null,
            IncludeGenerated = includeGenerated,
            ConfigPath = actualConfigPath,
            CompilationErrorsAsWarnings = compilationErrorsAsWarnings,
        };

        return await CommandLineFormatter.Format(
            commandLineOptions,
            new FileSystem(),
            console,
            logger,
            cancellationToken
        );
    }
}
