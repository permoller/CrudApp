using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace CrudApp.Tests.Infrastructure.Http;
internal static class HttpClientExtenstions
{
    public static async Task EnsureSuccessAsync(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        string responseString;
        try
        {
            responseString = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            throw new ApiException("Error reading error response content.", ex, response.StatusCode);
        }
        if (string.IsNullOrEmpty(responseString))
            throw new ApiException($"Status code {response.StatusCode} indicates an error.", null, response.StatusCode);
        try
        {
            var problem = JsonSerializer.Deserialize<ProblemDetails>(responseString);
            throw new ApiException<ProblemDetails>("Status code {response.StatusCode} indicates an error.", null, response.StatusCode, problem);
        }
        catch (Exception ex)
        {
            throw new ApiException<string>($"Status code {response.StatusCode} indicates an error.", ex, response.StatusCode, responseString);
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
        try
        {
            return JsonSerializer.Deserialize<T>(contentString);
        }
        catch (Exception ex)
        {
            throw new ApiException<string>("Error deserializing response content.", ex, response.StatusCode, contentString);
        }
    }
}

public class ApiException : HttpRequestException
{
    public ApiException(string message, Exception? innerException, HttpStatusCode? statusCode) : base(message, innerException, statusCode)
    {
    }
}
public class ApiException<T> : ApiException
{
    public ApiException(string message, Exception? innerException, HttpStatusCode? statusCode, T? content) : base(message, innerException, statusCode)
    {
        Content = content;
    }

    public T? Content { get; }
}
