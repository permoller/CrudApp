using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CrudApp.Infrastructure.ChangeTracking;
public enum ChangeType { EntityCreated, EntityUpdated, EntityDeleted }

[SkipChangeTracking]
[Index(nameof(EntityId))]
[Index(nameof(EntityType))]
public sealed class EntityChange : EntityBase
{
    [EnumValueConverter]
    public ChangeType ChangeType { get; set; }
    public string? EntityType { get; set; }
    public EntityId? EntityId { get; set; }
    public EntityId? UserId { get; set; }
    public DateTimeOffset Time { get; set; }
    public string? ActivityId { get; set; }

    public override string DisplayName => $"{ChangeType} {EntityType}[{EntityId}]";
}
