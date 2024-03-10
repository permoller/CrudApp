using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrudApp.Infrastructure.Logging;

/// <summary>
/// Handles sending log entries in batches to OpenSearch.
/// Each batch is prepared in <see cref="OpenSearchBufferLogSink"/>.
/// </summary>
public sealed class OpenSearchSender : BackgroundService
{
    public const string HttpClientName = "OpenSearchSender";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OpenSearchBufferLogSink _buffer;

    public OpenSearchSender(IHttpClientFactory httpClientFactory, OpenSearchBufferLogSink buffer)
    {
        _httpClientFactory = httpClientFactory;
        _buffer = buffer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(2000, stoppingToken);
            await SendDataInBuffer();
        }
    }

    private async ValueTask SendDataInBuffer()
    {
        if (_buffer.TryGetStream(out var stream))
        {
            // index name must be lower case
            var indexName = string.Format("crudapp-logs-{0:yyyy-MM-dd}", DateTimeOffset.UtcNow);
            var uri = new Uri(indexName + "/_bulk", UriKind.Relative);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = uri,
                Content = new StreamContent(stream)
            };
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;charset=utf-8");
            var httpClient = _httpClientFactory.CreateClient(HttpClientName);
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var bulkResponse = JsonSerializer.Deserialize<OpenSearchBulkResponse>(responseContent);
            if (bulkResponse?.HasErrors != false)
                throw new HttpRequestException($"Got error response when sending log entries to {httpClient.BaseAddress}{uri}.{Environment.NewLine}{responseContent}");
        }
    }
    private sealed class OpenSearchBulkResponse
    {
        [JsonPropertyName("took")]
        public long Took { get; set; }

        [JsonPropertyName("errors")]
        public bool HasErrors { get; set; }

        [JsonPropertyName("items")]
        public JsonElement[] Items { get; set; }
    }
}
