using CrudApp.Infrastructure.Users;

namespace CrudApp.Infrastructure.Authentication;

/// <summary>
/// Provides information about the currently authenticated user.
/// </summary>
public record AuthenticationContext
{
    private static readonly AsyncLocal<AuthenticationContext?> _current = new();

    /// <summary>
    /// Gets or sets the current authentication context. This flows across async calls.
    /// Null if no user is authenticated.
    /// </summary>
    public static AuthenticationContext? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }


    public User User { get; }

    public AuthenticationContext(User user)
    {
        User = user;
    }
}
