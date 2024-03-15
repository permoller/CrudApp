using CrudApp.Infrastructure.UtilityCode;
using System.Diagnostics;

namespace CrudApp.Infrastructure.Logging;

public sealed class SinkLogger : ILogger
{
    private readonly SinkLoggerProvider _provider;
    private readonly string _category;

    public SinkLogger(SinkLoggerProvider provider, string category)
    {
        _provider = provider;
        _category = category;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        var dictionary = StateToDictionary(state);
        return new LogScope(_provider, state.ToString(), dictionary);
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _provider.Write(CreateLogEntry(logLevel, _category, formatter(state, exception), state, exception));
    }

    private LogEntry CreateLogEntry<TState>(LogLevel logLevel, string category, string message, TState state, Exception? exception)
    {
        var logEntry = new LogEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Message = message,
            State = StateToDictionary(state),
            Log = new()
            {
                Level = GetLogLevelString(logLevel),
                Logger = category,
            }
        };

        var activity = Activity.Current;
        if (activity is not null)
        {
            logEntry.Span = new() { Id = activity.SpanId.ToHexString() };
            logEntry.Trace = new() { Id = activity.TraceId.ToHexString() };
        }

        if (exception is not null)
        {
            logEntry.Error = new()
            {
                Message = exception.GetMessagesIncludingData(),
                StackTrace = exception.ToString(),
                Type = exception.GetType().Name,
            };
        }

        var scope = _provider.CurrentScope;
        if (scope is not null)
        {
            logEntry.Scopes = scope.GetSelfAndAncestors().Select(s => new Scope { Message = s.Message, State = s.State}).ToList();

            // Add all the key-value pairs from all the scopes state-dictionaries as labels. Making them easier to use in queries.
            // If the same key exists in multiple scopes, the value from the inner scope is used.
            logEntry.Labels = new();
            foreach(var kvp in scope.GetSelfAndAncestors().Where(s => s.State is not null).SelectMany(s => s.State!))
                logEntry.Labels.TryAdd(kvp.Key, kvp.Value);
        }
        return logEntry;
    }

    // Use a switch to get log level strings... gives better performance than ToString()
    private static string GetLogLevelString(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Trace => "Trace",
        LogLevel.Debug => "Debug",
        LogLevel.Information => "Information",
        LogLevel.Warning => "Warning",
        LogLevel.Error => "Error",
        LogLevel.Critical => "Critical",
        LogLevel.None => "None",
        _ => logLevel.ToString(),
    };

    private static Dictionary<string, string?>? StateToDictionary<TState>(TState state)
    {
        if (state is IEnumerable<KeyValuePair<string, object?>> stateProperties)
        {
            var dictionary = new Dictionary<string, string?>();
            foreach (var property in stateProperties)
            {
                dictionary[property.Key] = property.Value?.ToString();
            }
            return dictionary;
        }
        return null;
    }
}
