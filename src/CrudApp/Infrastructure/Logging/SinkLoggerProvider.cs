namespace CrudApp.Infrastructure.Logging;

public sealed class SinkLoggerProvider : ILoggerProvider
{
    private readonly List<ILogSink> _logSinks;
    private readonly AsyncLocal<LogScope?> _scopeAsyncLocal = new();

    public LogScope? CurrentScope
    {
        get => _scopeAsyncLocal.Value;
        set => _scopeAsyncLocal.Value = value;
    }
    
    public SinkLoggerProvider(IEnumerable<ILogSink> logSinks)
    {
        _logSinks = logSinks.ToList();
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new SinkLogger(this, categoryName);
    }

    public void Dispose()
    {
        // leave log-processor running
    }

    public void Write(LogEntry logEntry)
    {
        List<Exception>? exceptions = null;
        foreach (var logSink in _logSinks)
        {
            try
            {
                logSink.Write(logEntry);
            }
            catch (Exception ex)
            {
                exceptions ??= new();
                exceptions.Add(ex);
            }
        }
        if(exceptions is not null)
            throw new AggregateException("Error writing to one or more log sinks.", exceptions);
    }
}
