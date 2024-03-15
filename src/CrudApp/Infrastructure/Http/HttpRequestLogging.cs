namespace CrudApp.Infrastructure.Http;

public static partial class HttpRequestLogging
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "{Direction} HTTP request completed {Method} {Scheme}://{Host}{PathBase}{Path}{Query}{Fragment} with status {StatusCode} in {ElapsedMilliseconds}ms")]
    public static partial void HttpRequestCompleted(
        ILogger logger,
        string direction,
        string method,
        string? scheme,
        string? host,
        string? pathBase,
        string? path,
        string? query,
        string? fragment,
        int? statusCode,
        long elapsedMilliseconds,
        Exception? exception);
}
