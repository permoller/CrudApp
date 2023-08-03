using CrudApp.Infrastructure.UtilityCode;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace CrudApp.Tests.Infrastructure.Http;
internal static class HttpClientExtenstions
{
    public static async Task EnsureSuccessAsync(this HttpResponseMessage response)
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

            // Return the status-error message and the ProblemDetails in an exception.
            throw new ProblemDetailsApiException(statusException.Message, null, response.StatusCode, problem);
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
