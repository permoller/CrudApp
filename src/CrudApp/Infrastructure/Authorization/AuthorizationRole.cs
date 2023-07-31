namespace CrudApp.Infrastructure.Authorization;

/// <summary>
/// Defines a set of access rights.
/// </summary>
public class AuthorizationRole : EntityBase
{
    public ICollection<AuthorizationGroupUserRelation> AuthorizationGroupUserRelations { get; set; } = new List<AuthorizationGroupUserRelation>();
}
