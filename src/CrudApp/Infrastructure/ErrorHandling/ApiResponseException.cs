using Microsoft.AspNetCore.Mvc;

namespace CrudApp.Infrastructure.ErrorHandling;

/// <summary>
/// Indicates the request did not succeed.
/// Contains the HTTP status code and optionally a message with details to be returned to the client.
/// Transformed to a <see cref="ProblemDetails"/> response in <see cref="ApiExceptionHandler"/>.
/// </summary>
public sealed class ApiResponseException : Exception
{
    public int HttpStatus
    {
        get => (int?)Data[nameof(HttpStatus)] ?? default;
        private set => Data[nameof(HttpStatus)] = value;
    }

    public ApiResponseException(int status, string? message) : this(status, message, null)
    {
    }

    public ApiResponseException(int status, string? message, Exception? innerException) : base(message, innerException)
    {
        HttpStatus = status;
    }

}
