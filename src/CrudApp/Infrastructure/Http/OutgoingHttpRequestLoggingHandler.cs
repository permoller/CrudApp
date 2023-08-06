using System.Diagnostics;

namespace CrudApp.Infrastructure.Http;

public class OutgoingHttpRequestLoggingHandler : DelegatingHandler
{
    private readonly ILogger<OutgoingHttpRequestLoggingHandler> _logger;

    public OutgoingHttpRequestLoggingHandler(ILogger<OutgoingHttpRequestLoggingHandler> logger)
        : base()
    {
        _logger = logger;
    }

    public OutgoingHttpRequestLoggingHandler(HttpMessageHandler innerHandler, ILogger<OutgoingHttpRequestLoggingHandler> logger)
        : base(innerHandler)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage? response = null;
        var sw = Stopwatch.StartNew();
        try
        {
            response = await base.SendAsync(request, cancellationToken);
            sw.Stop();
            HttpRequestLogging.HttpRequestCompleted(
                _logger,
                "Outgoing",
                request.Method.ToString(),
                request.RequestUri?.Scheme,
                request.RequestUri?.Host,
                null,
                request.RequestUri?.AbsolutePath,
                request.RequestUri?.Query,
                request.RequestUri?.Fragment,
                (int)response.StatusCode,
                sw.ElapsedMilliseconds,
                null);
            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            HttpRequestLogging.HttpRequestCompleted(
                _logger,
                "Outgoing",
                request.Method.ToString(),
                request.RequestUri?.Scheme,
                request.RequestUri?.Host,
                null,
                request.RequestUri?.AbsolutePath,
                request.RequestUri?.Query,
                request.RequestUri?.Fragment,
                (int?)response?.StatusCode,
                sw.ElapsedMilliseconds,
                ex);
            throw;
        }
        
    }
}
