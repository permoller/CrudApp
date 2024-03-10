using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace CrudApp.Infrastructure.ErrorHandling;

/// <summary>
/// Indicates the request did not succeed.
/// Contains the HTTP status code and optionally a message with details to be returned to the client.
/// Transformed to a <see cref="ProblemDetails"/> response in <see cref="ApiExceptionHandler"/>.
/// </summary>
[Serializable]
public sealed class ApiResponseException : Exception
{
    public int HttpStatus { get; }
    public bool HasMessage { get; private set; }

    public ApiResponseException(int status, string? message) : this(status, message, null)
    {
    }

    public ApiResponseException(int status, string? message, Exception? innerException) : base(message, innerException)
    {
        HasMessage = !string.IsNullOrEmpty(message);
        HttpStatus = status;
    }

    private ApiResponseException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        HttpStatus = info.GetInt32(nameof(HttpStatus));
        HasMessage = info.GetBoolean(nameof(HasMessage));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(HasMessage), HasMessage);
        info.AddValue(nameof(HttpStatus), HttpStatus);
        base.GetObjectData(info, context);
    }

}
