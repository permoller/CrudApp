using System.Runtime.Serialization;

namespace CrudApp.ErrorHandling;

[Serializable]
public sealed class ForbiddenException : ProblemDetailsException
{
    public ForbiddenException(string? message) : base(message)
    {
    }

    public ForbiddenException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    private ForbiddenException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public override HttpStatusCodes StatusCode => HttpStatusCodes.Status403Forbidden;
}

