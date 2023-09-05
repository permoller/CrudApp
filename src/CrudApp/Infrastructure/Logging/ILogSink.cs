namespace CrudApp.Infrastructure.Logging;
public interface ILogSink
{
    void Write(LogEntry logEntry);
}