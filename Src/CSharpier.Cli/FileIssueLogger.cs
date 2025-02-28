using Microsoft.Extensions.Logging;

namespace CSharpier.Cli;

internal class FileIssueLogger(string filePath, ILogger logger, bool isMsBuildFormat)
{
    static string FormatMsBuildError(LogState logState, Exception? exception) =>
        $"{logState.Path}: error: {logState.Message}";

    record class LogState
    {
        public required string Path;
        public required string Message;
    }

    public void WriteError(string value, Exception? exception = null)
    {
        if (isMsBuildFormat)
        {
            var logState = new LogState() { Path = this.GetPath(), Message = value };

            logger.Log(LogLevel.Error, 0, logState, exception, FormatMsBuildError);
        }
        else
        {
            logger.LogError(exception, $"{this.GetPath()} - {value}");
        }
    }

    public void WriteWarning(string value)
    {
        logger.LogWarning($"{this.GetPath()} - {value}");
    }

    private string GetPath()
    {
        return filePath;
    }
}
