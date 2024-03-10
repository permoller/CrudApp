using System.Runtime.Serialization;
using Microsoft.AspNetCore.Authentication;

namespace CrudApp.Infrastructure.ErrorHandling;

/// <summary>
/// Indicates authentication is required, but the current request is not authenticated.
/// Triggers a call to <see cref="IAuthenticationService.ChallengeAsync(HttpContext, string?, AuthenticationProperties?)"/>
/// in <see cref="ApiExceptionHandler"/>.
/// </summary>
[Serializable]
public sealed class NotAuthenticatedException : Exception
{
    public NotAuthenticatedException()
    {
    }

    private NotAuthenticatedException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
    {
    }
}
