using System.Runtime.Serialization;

namespace CrudApp.ErrorHandling;

[Serializable]
public sealed class ConflictException : ProblemDetailsException
{
    public ConflictException(string? message) : base(message)
    {
    }

    public ConflictException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    private ConflictException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }


    public override HttpStatusCodes StatusCode => HttpStatusCodes.Status409Conflict;
}

