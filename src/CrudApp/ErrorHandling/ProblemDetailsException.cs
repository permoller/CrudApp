using System.Runtime.Serialization;

namespace CrudApp.ErrorHandling;

[Serializable]
public abstract class ProblemDetailsException : Exception
{
    protected ProblemDetailsException(string? message) : base(message)
    {
    }

    protected ProblemDetailsException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected ProblemDetailsException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public abstract HttpStatusCodes StatusCode { get; }

    public enum HttpStatusCodes {
        Status400BadRequest = 400,
        Status403Forbidden = 403,
        Status404NotFound = 404,
        Status409Conflict = 409,
        Status502BadGateway = 502,
        Status503ServiceUnavailable = 503
    }
}



