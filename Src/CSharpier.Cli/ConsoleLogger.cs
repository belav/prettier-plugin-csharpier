using Microsoft.Extensions.Logging;

namespace CSharpier.Cli;

internal class ConsoleLogger(IConsole console, LogLevel loggingLevel, LogFormat logFormat)
    : ILogger
{
    private static readonly object ConsoleLock = new();

    public virtual void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception, string> formatter
    )
    {
        if (logLevel < loggingLevel)
        {
            return;
        }

        void Write(string value)
        {
            if (logLevel >= LogLevel.Error)
            {
                console.WriteError(value);
            }
            else
            {
                console.Write(value);
            }
        }

        void WriteLine(string? value = null)
        {
            if (logLevel >= LogLevel.Error)
            {
                console.WriteErrorLine(value);
            }
            else
            {
                console.WriteLine(value);
            }
        }

        if (!this.IsEnabled(logLevel))
        {
            return;
        }

        lock (ConsoleLock)
        {
            var message = formatter(state, exception!);

            if (logFormat == LogFormat.Console && logLevel >= LogLevel.Warning)
            {
                console.ForegroundColor = GetColorLevel(logLevel);
                Write($"{logLevel} ");
                console.ResetColor();
            }

            var stringReader = new StringReader(message);
            var line = stringReader.ReadLine();
            WriteLine(line);
            while ((line = stringReader.ReadLine()) != null)
            {
                WriteLine("  " + line);
            }

            if (exception == null)
            {
                return;
            }

            WriteLine("  " + exception.Message);
            if (exception.StackTrace != null)
            {
                stringReader = new StringReader(exception.StackTrace);
                while ((line = stringReader.ReadLine()) != null)
                {
                    WriteLine("  " + line);
                }
            }

            WriteLine();
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
        where TState : notnull
    {
        throw new NotImplementedException();
    }
}
