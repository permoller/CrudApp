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
            var errorMessage = $"Got response status {response.StatusCode}{expectedStatusMessage} from request {requestMessage}.";
            
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
            sb.Append(errorMessage);
            if (!string.IsNullOrEmpty(problem.Title))
                sb.AppendLine().Append("Title:  ").Append(problem.Title.ReplaceLineEndings(Environment.NewLine + "        "));
            if (!string.IsNullOrEmpty(problem.Detail))
                sb.AppendLine().Append("Detail: ").Append(problem.Detail.ReplaceLineEndings(Environment.NewLine + "        "));
            
            if (problem.TryGetExtension<Dictionary<string, string[]>>("errors", out var errors) && errors.Count > 0)
            {
                sb.AppendLine().Append("Errors:");
                foreach (var kvp in errors)
                {
                    foreach (var error in kvp.Value)
                    {
                        sb.AppendLine().Append(" * ").Append(kvp.Key).Append(": ").Append(error);
                    }
                }
            }
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

public class ApiException : HttpRequestException
{
    public ApiException(string message, Exception? innerException, HttpStatusCode? statusCode) : base(message, innerException, statusCode)
    {
    }
}
public class StringApiException : ApiException
{
    public ProblemDetails? Response => Data.Contains(nameof(Response)) ? (ProblemDetails?)Data[nameof(Response)] : null;

    public StringApiException(string message, Exception? innerException, HttpStatusCode? statusCode, string? response) : base(message, innerException, statusCode)
    {
        Data[nameof(Response)] = response;
    }
}

public class ProblemDetailsApiException : ApiException
{
    public ProblemDetails? ProblemDetails => Data.Contains(nameof(ProblemDetails)) ? (ProblemDetails?)Data[nameof(ProblemDetails)] : null;

    public ProblemDetailsApiException(string message, Exception? innerException, HttpStatusCode? statusCode, ProblemDetails? problemDetails) : base(message, innerException, statusCode)
    {
        Data[nameof(ProblemDetails)] = problemDetails;
    }
}
