using CrudApp.Infrastructure.Users;

namespace CrudApp.Infrastructure.Authorization;

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


    public User User { get; }

    public AuthorizationContext(User user)
    {
        User = user;
    }
}
