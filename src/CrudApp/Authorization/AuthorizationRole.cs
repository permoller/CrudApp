namespace CrudApp.Authorization;

/// <summary>
/// Defines a set of access rights.
/// </summary>
public class AuthorizationRole : EntityBase
{
    public ICollection<AuthorizationGroupMembership> AuthorizationGroupMemberships { get; set; } = new List<AuthorizationGroupMembership>();
}
