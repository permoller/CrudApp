using System.Runtime.Serialization;

namespace CrudApp.Infrastructure.ErrorHandling;

/// <summary>
/// Indicates the request did not succeed.
/// Contains that HTTP status code and optionally a message with details to be returned to the client.
/// </summary>
[Serializable]
public sealed class ApiResponseException : Exception
{
    public bool HasMessage { get; private set; }

    public ApiResponseException(HttpStatus status) : this(status, null)
    {
    }

    public ApiResponseException(HttpStatus status, string? message) : this(status, message, null)
    {
    }

    public ApiResponseException(HttpStatus status, string? message, Exception? innerException) : base(message, innerException)
    {
        HasMessage = !string.IsNullOrEmpty(message);
        HttpStatus = status;
    }

    private ApiResponseException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        HttpStatus = (HttpStatus)info.GetValue(nameof(HttpStatus), typeof(HttpStatus))!;
        HasMessage = (bool)info.GetValue(nameof(HasMessage), typeof(bool))!;
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(HasMessage), HasMessage);
        info.AddValue(nameof(HttpStatus), HttpStatus);
        base.GetObjectData(info, context);
    }

    public HttpStatus HttpStatus { get; }
}
