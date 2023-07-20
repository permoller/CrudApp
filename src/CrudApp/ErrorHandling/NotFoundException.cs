using System.Runtime.Serialization;

namespace CrudApp.ErrorHandling;

[Serializable]
public sealed class NotFoundException : ProblemDetailsException
{
    public NotFoundException(string? message) : base(message)
    {
    }

    public NotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    private NotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public override HttpStatusCodes StatusCode => HttpStatusCodes.Status404NotFound;
}

