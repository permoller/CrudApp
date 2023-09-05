using System.Text.Json;

namespace CrudApp.Infrastructure.Logging;

public class TextWriterLogSink : ILogSink
{
    public enum Format { PlainText, Json }

    private readonly TextWriter _textWriter;
    private readonly Func<LogEntry, string> _formatter;
    private readonly object _lock = new();
    

    public TextWriterLogSink(TextWriter textWriter, Format format)
    {
        _textWriter = textWriter;
        _formatter = format == Format.Json ? FormatAsJson : FormatAsPlainText;
    }
    public void Write(LogEntry logEntry)
    {
        var line = _formatter(logEntry);
        lock (_lock)
        {
            _textWriter.WriteLine(line);
        }
    }

    private static string FormatAsPlainText(LogEntry logEntry)
        => $"[{logEntry.Timestamp:O}] [{logEntry.Trace?.Id,-32}] [{logEntry.Log?.Level,-11}] [{logEntry.Log?.Logger,-75}] {logEntry.Message?.ReplaceLineEndings("  [newline]  ")}";

    private static string FormatAsJson(LogEntry logEntry)
        => JsonSerializer.Serialize(logEntry);
}
