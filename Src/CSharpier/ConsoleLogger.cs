using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace CSharpier
{
    public class ConsoleLogger : ILogger
    {
        private static readonly object ConsoleLock = new();

        private readonly IConsole console;

        public ConsoleLogger(IConsole console)
        {
            this.console = console;
        }

        public virtual void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception, string> formatter
        ) {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            lock (ConsoleLock)
            {
                var message = formatter(state, exception!);

                if (logLevel >= LogLevel.Warning)
                {
                    this.console.ForegroundColor = GetColorLevel(logLevel);
                    this.console.Write($"{logLevel} ");
                    this.console.ResetColor();
                }

                var stringReader = new StringReader(message);
                var line = stringReader.ReadLine();
                this.console.WriteLine(line);
                while ((line = stringReader.ReadLine()) != null)
                {
                    this.console.WriteLine("  " + line);
                }

                if (exception == null)
                {
                    return;
                }

                this.console.WriteLine("  " + exception.Message);
                if (exception.StackTrace != null)
                {
                    stringReader = new StringReader(exception.StackTrace);
                    while ((line = stringReader.ReadLine()) != null)
                    {
                        this.console.WriteLine("  " + line);
                    }
                }

                this.console.WriteLine();
            }
        }

        private static ConsoleColor GetColorLevel(LogLevel logLevel) =>
            logLevel switch
            {
                LogLevel.Critical => ConsoleColor.DarkRed,
                LogLevel.Error => ConsoleColor.DarkRed,
                LogLevel.Warning => ConsoleColor.DarkYellow,
                _ => ConsoleColor.White,
            };

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }
}
