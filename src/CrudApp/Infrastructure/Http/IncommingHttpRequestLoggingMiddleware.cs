using System.Diagnostics;

namespace CrudApp.Infrastructure.Http;

public class IncommingHttpRequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IncommingHttpRequestLoggingMiddleware> _logger;

    public IncommingHttpRequestLoggingMiddleware(RequestDelegate next, ILogger<IncommingHttpRequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await _next(context);
            sw.Stop();
            HttpRequestLogging.HttpRequestCompleted(
                _logger,
                "Incomming",
                context.Request.Method,
                context.Request.Scheme,
                context.Request.Host.ToString(),
                context.Request.PathBase,
                context.Request.Path,
                context.Request.QueryString.ToString(),
                null,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                null);
        }
        catch(Exception ex)
        {
            sw.Stop();
            HttpRequestLogging.HttpRequestCompleted(
                _logger,
                "Incomming",
                context.Request.Method,
                context.Request.Scheme,
                context.Request.Host.ToString(),
                context.Request.PathBase,
                context.Request.Path,
                context.Request.QueryString.ToString(),
                null,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                ex);
            throw;
        }
    }
}
