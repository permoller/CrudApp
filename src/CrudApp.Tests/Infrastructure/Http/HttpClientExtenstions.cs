using CrudApp.Infrastructure.UtilityCode;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using System.Text.Json;

namespace CrudApp.Tests.Infrastructure.Http;
internal static class HttpClientExtenstions
{
    public static async Task EnsureSuccessAsync(this HttpResponseMessage response, int? expectedStatusCode = null)
    {
        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException statusException)
        {
            // Expect a response body from the server
            string responseString;
            try
            {
                responseString = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                // We could not read the response body. Return the status-error message in an exception.
                throw new ApiException(statusException.Message, ex, response.StatusCode);
            }

            // If no response body received. Return the status-error message in an exception.
            if (string.IsNullOrEmpty(responseString))
                throw new ApiException(statusException.Message, null, response.StatusCode);

            // Expect the response-body to be a ProblemDetails object
            ProblemDetails? problem;
            try
            {
                problem = JsonSerializer.Deserialize<ProblemDetails>(responseString, JsonUtils.ApiJsonSerializerOptions);
            }
            catch (Exception ex)
            {
                // We could not parse the respnse. Return the status-error message and the string content in an exception.
                throw new StringApiException(statusException.Message, ex, response.StatusCode, responseString);
            }

            // Format ProblemDetails as an error message
            var messageBuilder = new StringBuilder();
            messageBuilder.Append(problem?.Title);
            if (!string.IsNullOrEmpty(problem?.Detail))
                messageBuilder.AppendLine().Append("Detail: ").Append(problem.Detail);
            if (problem?.Extensions.TryGetValue("errors", out var errorsObj) == true && errorsObj is JsonElement errorsElm && errorsElm.ValueKind == JsonValueKind.Object)
            {
                var errors = errorsElm.Deserialize<IDictionary<string, string[]>>(JsonUtils.ApiJsonSerializerOptions);
                messageBuilder.AppendLine().Append("Errors:");
                foreach (var kvp in errors)
                {
                    foreach (var error in kvp.Value)
                    {
                        messageBuilder.AppendLine().Append(" * ").Append(kvp.Key).Append(": ").Append(error);
                    }
                }
            }
            if (problem?.Extensions.TryGetValue("exceptionMessages", out var exceptionObj) == true && exceptionObj is IEnumerable<string> exceptionMessages)
            {
                messageBuilder.AppendLine().Append("Exception:");
                foreach(var exceptionMessage in exceptionMessages)
                {
                    messageBuilder.AppendLine().Append(" * ").Append(exceptionMessage);
                }
            }
            var message = messageBuilder.ToString();
            
            // If we got nothing from the ProblemDetails, fallback to the status-error message.
            if (string.IsNullOrWhiteSpace(message))
                message = statusException.Message;

            throw new ProblemDetailsApiException(message, null, response.StatusCode, problem);
        }

        if (expectedStatusCode.HasValue)
        {
            Assert.Equal(expectedStatusCode.Value, (int)response.StatusCode);
        }
    }

    public static async Task<T?> ReadContentAsync<T>(this HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.NoContent)
            return default;

        string contentString;
        try
        {
            contentString = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            throw new ApiException("Error reading response content.", ex, response.StatusCode);
        }

        if (string.IsNullOrEmpty(contentString))
            return default;

        try
        {
            var result = JsonSerializer.Deserialize<T>(contentString, JsonUtils.ApiJsonSerializerOptions);
            return result;
        }
        catch (Exception ex)
        {
            throw new StringApiException("Error deserializing response content.", ex, response.StatusCode, contentString);
        }
    }

    public static HttpRequestException WrapWithRequestDetails(this HttpRequestException exception, string httpMethod, string uri)
    {
        return new HttpRequestException($"Error calling {httpMethod} {uri}", exception, exception.StatusCode);
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
    public string? Response { get; }

    public StringApiException(string message, Exception? innerException, HttpStatusCode? statusCode, string? response) : base(message, innerException, statusCode)
    {
        Response = response;
    }
}

public class ProblemDetailsApiException : ApiException
{
    public ProblemDetails? Response { get; }

    public ProblemDetailsApiException(string message, Exception? innerException, HttpStatusCode? statusCode, ProblemDetails? response) : base(message, innerException, statusCode)
    {
        Response = response;
    }
}
