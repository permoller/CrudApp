namespace CrudApp.Infrastructure.Authorization;

/// <summary>
/// An <see cref="AuthorizationGroup"/> is a scope that identifies a collection of entities
/// and a collection of users that have access to the entities in the group.
/// The specific access rights for a user to the entities is defined by the roles the user has in the group.
/// The many-to-many relation between entities and groups are defined by <see cref="AuthorizationGroupEntityRelation"/>.
/// The many-to-many relation between users and groups are defined by <see cref="AuthorizationGroupUserRelation"/>.
/// The role of the user in the group is also defined by <see cref="AuthorizationGroupUserRelation"/>.
/// </summary>
public class AuthorizationGroup : EntityBase
{
    public ICollection<AuthorizationGroupEntityRelation>? AuthorizationGroupEntityRelations { get; set; }
    public ICollection<AuthorizationGroupUserRelation>? AuthorizationGroupUserRelations { get; set; }
}
