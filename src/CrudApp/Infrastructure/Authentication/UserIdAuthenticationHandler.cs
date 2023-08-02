using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Security.Claims;

namespace CrudApp.Infrastructure.Authentication;

/// <summary>
/// Do not use this authentication handler in a real application.
/// It just looks for a user id in the authorization header.
/// </summary>
public class UserIdAuthenticationHandler : IAuthenticationHandler
{
    public const string AuthenticationType = nameof(UserIdAuthenticationHandler);
    public const string UserIdClaimType = "UserId";
    public const string HttpAuthenticationScheme = "UserId";
    

    private readonly ILogger<UserIdAuthenticationHandler> _logger;
    private readonly ProblemDetailsFactory _problemDetailsFactory;
    private AuthenticationScheme? _scheme;
    private HttpContext? _context;

    private HttpContext Context => _context ?? throw new InvalidOperationException("Context not initialized.");
    private AuthenticationScheme Scheme => _scheme ?? throw new InvalidOperationException("Scheme not initialized.");

    public UserIdAuthenticationHandler(
        ILogger<UserIdAuthenticationHandler> logger,
        ProblemDetailsFactory problemDetailsFactory)
    {
        _logger = logger;
        _problemDetailsFactory = problemDetailsFactory;
    }

    public async Task<AuthenticateResult> AuthenticateAsync()
    {
        var authHeader = Context.Request.Headers.Authorization.ToString();
        if (authHeader?.StartsWith(HttpAuthenticationScheme + " ") != true)
            return AuthenticateResult.NoResult();

        var userIdString = authHeader.Substring(HttpAuthenticationScheme.Length);
        if (string.IsNullOrEmpty(userIdString))
            return AuthenticateResult.Fail("Missing user id.");

        if (!long.TryParse(userIdString, out var userId))
            return AuthenticateResult.Fail("Invalid user id.");

        var identity = new ClaimsIdentity(AuthenticationType);
        identity.AddClaim(new Claim(UserIdClaimType, userId.ToString()));
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }

    public async Task ChallengeAsync(AuthenticationProperties? properties)
    {
        // Add header telling the client which authentication scheme to user.
        Context.Response.Headers.WWWAuthenticate = HttpAuthenticationScheme;

        // Return 401 Unauthorized and a problem details object.
        await WriteProblemDetailsResponseAsync(HttpStatus.Unauthorized);
    }

    public async Task ForbidAsync(AuthenticationProperties? properties)
    {
        // Return 403 Forbidden and a problem details object.
        await WriteProblemDetailsResponseAsync(HttpStatus.Forbidden);
    }

    private async Task WriteProblemDetailsResponseAsync(int statusCode)
    {
        var problemDetails = _problemDetailsFactory.CreateProblemDetails(Context, statusCode: statusCode);
        Context.Response.StatusCode = statusCode;
        await Context.Response.WriteAsJsonAsync(problemDetails);
    }

    public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
    {
        _scheme = scheme;
        _context = context;
        return Task.CompletedTask;
    }
}

