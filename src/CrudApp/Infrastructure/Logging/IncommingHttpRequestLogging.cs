using System.Diagnostics;

namespace CrudApp.Infrastructure.Logging;

public class IncommingHttpRequestLogging : IMiddleware
{
    private readonly ILogger<IncommingHttpRequestLogging> _logger;

    public IncommingHttpRequestLogging(ILogger<IncommingHttpRequestLogging> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await next(context);
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
        catch (Exception ex)
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
