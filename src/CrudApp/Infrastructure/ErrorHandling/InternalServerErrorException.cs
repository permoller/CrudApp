
namespace CrudApp.Infrastructure.ErrorHandling;

/// <summary>
/// Indicates an internal server error. 
/// </summary>
public sealed class InternalServerErrorException : Exception
{
    public InternalServerErrorException(string message) : base(message)
    {
    }
}
