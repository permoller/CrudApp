using CrudApp.Infrastructure.UtilityCode;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CrudApp.Tests.Infrastructure.Http;
internal static class HttpClientExtenstions
{
    public static Task<HttpResponseMessage> ApiGetAsync(this HttpClient client, string requestUri)
    {
        return client.ApiSendAsync(new(HttpMethod.Get, requestUri));
    }

    public static Task<HttpResponseMessage> ApiPostAsJsonAsync<T>(this HttpClient client, string requestUri, T value)
    {
        return client.ApiSendAsync(new(HttpMethod.Post, requestUri) { Content = JsonContent.Create(value, options: JsonUtils.ApiJsonSerializerOptions) });
    }

    public static Task<HttpResponseMessage> ApiPutAsJsonAsync<T>(this HttpClient client, string requestUri, T value)
    {
        return client.ApiSendAsync(new(HttpMethod.Put, requestUri) { Content = JsonContent.Create(value, options: JsonUtils.ApiJsonSerializerOptions) });
    }

    public static Task<HttpResponseMessage> ApiDeleteAsync(this HttpClient client, string requestUri)
    {
        return client.ApiSendAsync(new(HttpMethod.Delete, requestUri));
    }

    public static async Task<HttpResponseMessage> ApiSendAsync(this HttpClient client, HttpRequestMessage request)
    {
        try
        {
            var response = await client.SendAsync(request);
            return response;
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException($"Error sending request to {request.Method} {request.RequestUri}.", ex, ex.StatusCode);
        }
    }

    public static async Task ApiEnsureSuccessAsync(this HttpResponseMessage response, int? expectedStatusCode = null)
    {
        var isSuccess = expectedStatusCode.HasValue ? expectedStatusCode.Value == (int)response.StatusCode : response.IsSuccessStatusCode;
        if (!isSuccess)
        {
            var requestMessage = $"{response.RequestMessage?.Method} {response.RequestMessage?.RequestUri}";
            var expectedStatusMessage = expectedStatusCode.HasValue ? $" (expected {expectedStatusCode})" : "";
            var errorMessage = $"Got response status {(int)response.StatusCode}{expectedStatusMessage} from request {requestMessage}.";
            
            // Expect a response body from the server
            string responseString;
            try
            {
                responseString = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                // We could not read the response body.
                throw new ApiException(errorMessage, ex, response.StatusCode);
            }

            // If no response body received.
            if (string.IsNullOrEmpty(responseString))
                throw new ApiException(errorMessage, null, response.StatusCode);

            // Expect the response-body to be a ProblemDetails object
            ProblemDetails? problem;
            try
            {
                problem = JsonSerializer.Deserialize<ProblemDetails>(responseString, JsonUtils.ApiJsonSerializerOptions);
                ArgumentNullException.ThrowIfNull(problem);
            }
            catch (Exception ex)
            {
                // We could not parse the respnse. Return the string content in an exception.
                throw new StringApiException(errorMessage, ex, response.StatusCode, responseString);
            }

            // Format ProblemDetails as an error message
            var sb = new StringBuilder();
            
            if (!string.IsNullOrEmpty(problem.Title))
                sb.AppendLine(problem.Title);

            if (!string.IsNullOrEmpty(problem.Detail))
                sb.AppendLine(problem.Detail);
            
            if (problem.TryGetData(out var data) && data.Count > 0)
            {
                sb.AppendLine().AppendLine("Data:");
                foreach (var kvp in data)
                    sb.Append(" * ").Append(kvp.Key).Append(": ").Append(kvp.Value).AppendLine();
            }

            if (problem.TryGetErrors(out var errors) && errors.Count > 0)
            {
                sb.AppendLine().Append("Errors:");
                foreach (var kvp in errors)
                    foreach (var error in kvp.Value)
                        sb.Append(" * ").Append(kvp.Key).Append(": ").Append(error).AppendLine();
            }

            sb.AppendLine().AppendLine("Debug Info:");
            var debugInfo = new List<KeyValuePair<string, object?>>
            {
                new("request", requestMessage),
                new("responseStatus", (int)response.StatusCode),
                new("errorType", problem.Type),
                new("errorId", problem.Instance),
                new("errorStatus", problem.Status),
            };
            foreach (var kvp in problem.Extensions.Where(kvp => kvp.Key != "errors" && kvp.Key != "properties"))
                debugInfo.Add(kvp);

            foreach(var kvp in debugInfo)
                sb.Append(" * ").Append(kvp.Key).Append(": ").Append(kvp.Value).AppendLine();
            
            var message = sb.ToString();

            throw new ProblemDetailsApiException(message, null, response.StatusCode, problem);
        }
    }

    public static async Task<T?> ApiReadContentAsync<T>(this HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.NoContent)
            return default;

        string responseString;
        try
        {
            responseString = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            throw new ApiException("Error reading response content from request to {response.RequestMessage?.Method} {response.RequestMessage?.RequestUri}.", ex, response.StatusCode);
        }

        if (string.IsNullOrEmpty(responseString))
            return default;

        try
        {
            var result = JsonSerializer.Deserialize<T>(responseString, JsonUtils.ApiJsonSerializerOptions);
            return result;
        }
        catch (Exception ex)
        {
            throw new StringApiException($"Error deserializing response content from request to {response.RequestMessage?.Method} {response.RequestMessage?.RequestUri}.", ex, response.StatusCode, responseString);
        }
    }
}

public class ApiException(string message, Exception? innerException, HttpStatusCode? statusCode) 
    : HttpRequestException(message, innerException, statusCode) { }

public class StringApiException : ApiException
{
    public string Response => (string)Data[nameof(Response)]!;

    public StringApiException(string message, Exception? innerException, HttpStatusCode? statusCode, string response) : base(message, innerException, statusCode)
    {
        Data[nameof(Response)] = response;
    }
}

public class ProblemDetailsApiException : ApiException
{
    public ProblemDetails ProblemDetails => (ProblemDetails)Data[nameof(ProblemDetails)]!;

    public ProblemDetailsApiException(string message, Exception? innerException, HttpStatusCode? statusCode, ProblemDetails problemDetails) : base(message, innerException, statusCode)
    {
        Data[nameof(ProblemDetails)] = problemDetails;
    }
}
