using CrudApp.Infrastructure.Users;

namespace CrudApp.Infrastructure.Authorization;

/// <summary>
/// Provides information about the user that should be used for authorization.
/// This is normally the same as the authenticated user.
/// </summary>
public record AuthorizationContext
{
    private static readonly AsyncLocal<AuthorizationContext?> _current = new();

    /// <summary>
    /// Gets or sets the current autorization context. This flows across async calls.
    /// Null if no user is authenticated.
    /// </summary>
    public static AuthorizationContext? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }


    public EntityId UserId { get; }

    public AuthorizationContext(EntityId userId)
    {
        UserId = userId;
    }
}
