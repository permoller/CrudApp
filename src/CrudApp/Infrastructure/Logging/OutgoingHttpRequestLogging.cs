using System.Diagnostics;

namespace CrudApp.Infrastructure.Logging;

public class OutgoingHttpRequestLogging : DelegatingHandler
{
    private readonly ILogger<OutgoingHttpRequestLogging> _logger;

    public OutgoingHttpRequestLogging(ILogger<OutgoingHttpRequestLogging> logger)
        : base()
    {
        _logger = logger;
    }

    public OutgoingHttpRequestLogging(HttpMessageHandler innerHandler, ILogger<OutgoingHttpRequestLogging> logger)
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
