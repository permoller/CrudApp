using System.Runtime.Serialization;

namespace CrudApp.Infrastructure.ErrorHandling;


/// <summary>
/// Indicates an internal server error. 
/// </summary>
[Serializable]
public sealed class ErrorException : Exception
{
    public ErrorException(string message) : base(message)
    {
    }

    public ErrorException(string message, Exception innerException) : base(message, innerException)
    {
    }

    private ErrorException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
    {
    }

}
