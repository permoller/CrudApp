using Microsoft.EntityFrameworkCore;

namespace CrudApp.Authorization;

/// <summary>
/// Defines that an entity is included in a group.
/// </summary>
// Index used to find the the id of entities in a group.
[Index(nameof(AuthorizationGroupId), nameof(EntityId))]
// Index used to find the groups that an enitty is included in.
[Index(nameof(EntityId), nameof(AuthorizationGroupId))]
public class AuthorizationGroupEntity : EntityBase
{
    public EntityId AuthorizationGroupId { get; set; }
    public EntityId EntityId { get; set; }
    public string EntityType { get; set; } = null!;

    public AuthorizationGroup AuthorizationGroup { get; set; } = null!;
}

