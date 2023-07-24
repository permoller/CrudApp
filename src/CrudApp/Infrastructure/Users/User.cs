namespace CrudApp.Infrastructure.Users;

public sealed class User : EntityBase
{
    public ICollection<AuthorizationGroupMembership> AuthorizationGroupMemberships { get; set; } = new List<AuthorizationGroupMembership>();
}
