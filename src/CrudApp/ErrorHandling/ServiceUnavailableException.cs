using System.Runtime.Serialization;

namespace CrudApp.ErrorHandling;

[Serializable]
public sealed class ServiceUnavailableException : ProblemDetailsException
{
    public ServiceUnavailableException(string? message) : base(message)
    {
    }

    public ServiceUnavailableException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    private ServiceUnavailableException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }


    public override HttpStatusCodes StatusCode => HttpStatusCodes.Status503ServiceUnavailable;
}

