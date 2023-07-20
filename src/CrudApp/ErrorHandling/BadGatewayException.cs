using System.Runtime.Serialization;

namespace CrudApp.ErrorHandling;

[Serializable]
public sealed class BadGatewayException : ProblemDetailsException
{
    public BadGatewayException(string? message) : base(message)
    {
    }

    public BadGatewayException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    private BadGatewayException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }


    public override HttpStatusCodes StatusCode => HttpStatusCodes.Status502BadGateway;
}

