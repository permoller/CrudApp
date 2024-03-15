using System.Text.Json;

namespace CrudApp.Infrastructure.Logging;

public class TextWriterLogSink : ILogSink
{
    public enum Format { PlainText, Json }

    private readonly TextWriter _textWriter;
    private readonly Func<LogEntry, string> _formatter;
    private readonly object _lock = new();
    private readonly string? _onlyShowLastSegmentOfCategoriesStartingWith;

    public TextWriterLogSink(TextWriter textWriter, Format format, string? onlyPrintLastSegmentOfCategoriesStartingWith = null)
    {
        _textWriter = textWriter;
        _formatter = format == Format.Json ? FormatAsJson : FormatAsPlainText;
        _onlyShowLastSegmentOfCategoriesStartingWith = onlyPrintLastSegmentOfCategoriesStartingWith;
    }
    public void Write(LogEntry logEntry)
    {
        var line = _formatter(logEntry);
        lock (_lock)
        {
            _textWriter.WriteLine(line);
        }
    }

    private string FormatAsPlainText(LogEntry logEntry)
    {
        var errorLabel = logEntry.Error is null ? null : " Error: ";
        var category = logEntry.Log?.Logger;
        if (_onlyShowLastSegmentOfCategoriesStartingWith is not null && category is not null && category.StartsWith(_onlyShowLastSegmentOfCategoriesStartingWith))
        {
            var i = category.LastIndexOf('.');
            if (i > 0 && i < category.Length)
                category = category.Substring(i + 1);
        }
        
        return $"{logEntry.Timestamp:yyyy-MM-dd HH:mm:ss zzz} | {logEntry.Trace?.Id,-32} | {logEntry.Log?.Level,-11} | {category,-40} | {logEntry.Message}{errorLabel}{logEntry.Error?.Message}".ReplaceLineEndings("  [newline]  ");
    }




    private static string FormatAsJson(LogEntry logEntry)
        => JsonSerializer.Serialize(logEntry);
}
