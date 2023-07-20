using System.Runtime.Serialization;

namespace CrudApp.ErrorHandling;

[Serializable]
public sealed class BadRequestException : ProblemDetailsException
{
    public BadRequestException(string? message) : base(message)
    {
    }

    public BadRequestException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    private BadRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }


    public override HttpStatusCodes StatusCode => HttpStatusCodes.Status400BadRequest;
}

