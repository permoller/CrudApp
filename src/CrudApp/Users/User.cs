using CrudApp.Authorization;

namespace CrudApp.Users;

public sealed class User : EntityBase
{
    public ICollection<AuthorizationGroupMembership> AuthorizationGroupMemberships { get; set; } = new List<AuthorizationGroupMembership>();
}
