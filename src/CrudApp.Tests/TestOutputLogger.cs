using Microsoft.Extensions.Logging;
using System.Text;

namespace CrudApp.Tests;


public sealed class TestOutputLogger : ILogger
{
    private readonly Provider _provider;
    private readonly string? _categoryName;

    public TestOutputLogger(Provider provider, string? categoryName)
    {
        _provider = provider;
        _categoryName = categoryName;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var msg = formatter(state, exception);
        msg = "       " + msg?.ReplaceLineEndings(Environment.NewLine + "       ");
        var exceptionString = "       " + exception?.ToString()?.ReplaceLineEndings(Environment.NewLine + "       ");

        // Estimate the space required for the final message, so the string builder does not need to keep alocating more memory
        var stringBuilderCapacity = (_categoryName?.Length ?? 0) + msg.Length + (exceptionString?.Length ?? 0) + 50;

        var sb = new StringBuilder(stringBuilderCapacity)
            .AppendLine()
            .Append(FormatLogLevel(logLevel)).AppendLine(_categoryName);

        if (!string.IsNullOrWhiteSpace(msg))
            sb.AppendLine(msg);
        
        if (!string.IsNullOrWhiteSpace(exceptionString))
            sb.AppendLine("Exception:").AppendLine(exceptionString);

        _provider.WriteLine(sb.ToString());
    }

    private static string FormatLogLevel(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Trace => "Trace: ",
        LogLevel.Debug => "Debug: ",
        LogLevel.Information => "Info:  ",
        LogLevel.Warning => "Warn:  ",
        LogLevel.Error => "Error: ",
        LogLevel.Critical => "Fatal: ",
        LogLevel.None => "None: ",
        LogLevel x => x.ToString().ToUpperInvariant()
    };

    public sealed class Provider : ILoggerProvider
    {
        
        public Action<string> WriteLine { get; }

        public Provider(Action<string> writeLine)
        {
            WriteLine = writeLine;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TestOutputLogger(this, categoryName);
        }

        public void Dispose()
        {
            //
        }
    }
}