namespace CrudApp.Infrastructure.Logging;

public sealed class LogScope : IDisposable
{
    private readonly SinkLoggerProvider _provider;

    public LogScope? Parent { get; }
    public string? Message { get; }
    public Dictionary<string, string?>? State { get; }

    public LogScope(SinkLoggerProvider provider, string? message, Dictionary<string, string?>? state)
    {
        _provider = provider;
        Message = message;
        Parent = provider.CurrentScope;
        State = state;
        provider.CurrentScope = this;
    }

    public void Dispose()
    {
        _provider.CurrentScope = Parent;
    }

    public IEnumerable<LogScope> GetSelfAndAncestors()
    {
        var scope = this;
        while (scope is not null)
        {
            yield return scope;
            scope = scope.Parent;
        }
    }
}
