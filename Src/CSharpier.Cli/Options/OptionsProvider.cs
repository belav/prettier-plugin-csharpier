using System.IO.Abstractions;
using System.Text.Json;
using CSharpier.Cli.EditorConfig;
using Microsoft.Extensions.Logging;

namespace CSharpier.Cli.Options;

internal class OptionsProvider
{
    private readonly IList<EditorConfigSections> editorConfigs;
    private readonly List<CSharpierConfigData> csharpierConfigs;
    private readonly IgnoreFile ignoreFile;
    private readonly ConfigurationFileOptions? specifiedConfigFile;
    private readonly IFileSystem fileSystem;

    private OptionsProvider(
        IList<EditorConfigSections> editorConfigs,
        List<CSharpierConfigData> csharpierConfigs,
        IgnoreFile ignoreFile,
        ConfigurationFileOptions? specifiedPrinterOptions,
        IFileSystem fileSystem
    )
    {
        this.editorConfigs = editorConfigs;
        this.csharpierConfigs = csharpierConfigs;
        this.ignoreFile = ignoreFile;
        this.specifiedConfigFile = specifiedPrinterOptions;
        this.fileSystem = fileSystem;
    }

    public static async Task<OptionsProvider> Create(
        string directoryName,
        string? configPath,
        IFileSystem fileSystem,
        ILogger logger,
        CancellationToken cancellationToken,
        bool limitConfigSearch = false
    )
    {
        var filename = configPath is not null ? Path.GetFileName(configPath) : null;
        var csharpierConfigPath = configPath;
        string? editorConfigPath = null;

        if (configPath is not null && Path.GetFileName(configPath) == ".editorconfig")
        {
            csharpierConfigPath = null;
            editorConfigPath = configPath;
        }

        var specifiedConfigFile = csharpierConfigPath is not null
            ? ConfigFileParser.Create(csharpierConfigPath, fileSystem, logger)
            : null;

        var csharpierConfigs = csharpierConfigPath is null
            ? ConfigFileParser.FindForDirectoryName(
                directoryName,
                fileSystem,
                logger,
                limitConfigSearch
            )
            : [];

        IList<EditorConfigSections>? editorConfigSections = null;

        var ignoreFile = await IgnoreFile.Create(directoryName, fileSystem, cancellationToken);

        try
        {
            // TODO #1456 can probably add tests to OptionsProviderTests instead of the other two spots
            // TODO #1456 because there is a specified file, we should always be using that one
            // kind of like above it passes a "specifiedConfigFile"
            // right how this just uses the specified one to get the list, but then later
            // the files that we format don't exist in the same directory so aren't formatted with it
            editorConfigSections = editorConfigPath is null
                ? EditorConfigParser.FindForDirectoryName(
                    directoryName,
                    fileSystem,
                    limitConfigSearch,
                    ignoreFile
                )
                : EditorConfigParser.FindForDirectoryName(
                    Path.GetDirectoryName(editorConfigPath)!,
                    fileSystem,
                    true,
                    ignoreFile
                );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failure parsing editorconfig files for {DirectoryName}",
                directoryName
            );
        }

        return new OptionsProvider(
            editorConfigSections ?? Array.Empty<EditorConfigSections>(),
            csharpierConfigs,
            ignoreFile,
            specifiedConfigFile,
            fileSystem
        );
    }

    public PrinterOptions? GetPrinterOptionsFor(string filePath)
    {
        if (this.specifiedConfigFile is not null)
        {
            return this.specifiedConfigFile.ConvertToPrinterOptions(filePath);
        }

        var directoryName = this.fileSystem.Path.GetDirectoryName(filePath);

        ArgumentNullException.ThrowIfNull(directoryName);

        var resolvedEditorConfig = this.editorConfigs.FirstOrDefault(o =>
            directoryName.StartsWith(o.DirectoryName)
        );
        var resolvedCSharpierConfig = this.csharpierConfigs.FirstOrDefault(o =>
            directoryName.StartsWith(o.DirectoryName)
        );

        if (resolvedCSharpierConfig is not null)
        {
            return resolvedCSharpierConfig.CSharpierConfig.ConvertToPrinterOptions(filePath);
        }

        if (resolvedEditorConfig is not null)
        {
            DebugLogger.Log("has editorconfig");
            return resolvedEditorConfig.ConvertToPrinterOptions(filePath);
        }

        if (filePath.EndsWith(".cs") || filePath.EndsWith(".csx"))
        {
            return new PrinterOptions { Formatter = "csharp" };
        }

        return null;
    }

    public bool IsIgnored(string actualFilePath)
    {
        return this.ignoreFile.IsIgnored(actualFilePath);
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(
            new
            {
                specified = this.specifiedConfigFile,
                csharpierConfigs = this.csharpierConfigs,
                editorConfigs = this.editorConfigs,
            }
        );
    }
}
