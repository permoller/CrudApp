namespace CrudApp.Infrastructure.Authorization;

/// <summary>
/// Defines that an entity is included in a group.
/// </summary>
public class AuthorizationGroupEntityRelation : EntityBase
{
    public EntityId AuthorizationGroupId { get; set; }
    public EntityId EntityId { get; set; }
    public string EntityType { get; set; } = null!;

    public AuthorizationGroup? AuthorizationGroup { get; set; }
}

