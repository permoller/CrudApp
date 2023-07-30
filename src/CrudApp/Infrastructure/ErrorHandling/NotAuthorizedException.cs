using System.Runtime.Serialization;
using Microsoft.AspNetCore.Authentication;

namespace CrudApp.Infrastructure.ErrorHandling;

/// <summary>
/// Indicates the current request is authenticated, but not authorized to perform the operation.
/// Triggers a call to <see cref="IAuthenticationService.ForbidAsync(HttpContext, string?, AuthenticationProperties?)"/>
/// in <see cref="ApiExceptionHandler"/>.
/// </summary>
[Serializable]
public sealed class NotAuthorizedException : Exception
{
    public NotAuthorizedException() : base()
    {
    }

    private NotAuthorizedException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
    {
    }
}