using System;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CSharpier
{
    public class IgnoreFile
    {
        protected Ignore.Ignore Ignore { get; }
        protected string IgnoreBaseDirectoryPath { get; }

        protected IgnoreFile(Ignore.Ignore ignore, string ignoreBaseDirectoryPath)
        {
            this.Ignore = ignore;
            this.IgnoreBaseDirectoryPath = ignoreBaseDirectoryPath.Replace('\\', '/');
        }

        public bool IsIgnored(string filePath)
        {
            var normalizedFilePath = filePath.Replace('\\', '/');
            if (!normalizedFilePath.StartsWith(this.IgnoreBaseDirectoryPath))
            {
                throw new Exception(
                    "The filePath of "
                        + filePath
                        + " does not start with the ignoreBaseDirectoryPath of "
                        + this.IgnoreBaseDirectoryPath
                );
            }

            normalizedFilePath = normalizedFilePath[(this.IgnoreBaseDirectoryPath.Length + 1)..];

            return this.Ignore.IsIgnored(normalizedFilePath);
        }

        public static async Task<IgnoreFile?> Create(
            string baseDirectoryPath,
            IFileSystem fileSystem,
            ILogger logger,
            CancellationToken cancellationToken
        ) {
            var ignore = new Ignore.Ignore();
            var ignoreFilePath = FindIgnorePath(baseDirectoryPath, fileSystem);
            if (ignoreFilePath == null)
            {
                return new IgnoreFile(ignore, baseDirectoryPath);
            }

            foreach (
                var line in await fileSystem.File.ReadAllLinesAsync(
                    ignoreFilePath,
                    cancellationToken
                )
            ) {
                try
                {
                    ignore.Add(line);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        @$"The .csharpierignore file at {ignoreFilePath} could not be parsed due to the following line:
{line}
"
                    );
                    return null;
                }
            }

            return new IgnoreFile(ignore, fileSystem.Path.GetDirectoryName(ignoreFilePath));
        }

        private static string? FindIgnorePath(string baseDirectoryPath, IFileSystem fileSystem)
        {
            var directoryInfo = fileSystem.DirectoryInfo.FromDirectoryName(baseDirectoryPath);
            while (directoryInfo != null)
            {
                var ignoreFilePath = fileSystem.Path.Combine(
                    directoryInfo.FullName,
                    ".csharpierignore"
                );
                if (fileSystem.File.Exists(ignoreFilePath))
                {
                    return ignoreFilePath;
                }

                directoryInfo = directoryInfo.Parent;
            }

            return null;
        }
    }
}
