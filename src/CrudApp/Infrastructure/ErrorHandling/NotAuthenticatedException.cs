using Microsoft.AspNetCore.Authentication;

namespace CrudApp.Infrastructure.ErrorHandling;

/// <summary>
/// Indicates authentication is required, but the current request is not authenticated.
/// Triggers a call to <see cref="IAuthenticationService.ChallengeAsync(HttpContext, string?, AuthenticationProperties?)"/>
/// in <see cref="ApiExceptionHandler"/>.
/// </summary>
public sealed class NotAuthenticatedException : Exception
{
    public NotAuthenticatedException()
    {
    }
}
