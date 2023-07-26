namespace CrudApp.Infrastructure.ChangeTracking;
public enum ChangeType { EntityCreated, EntityUpdated, EntityDeleted }

[SkipChangeTracking]
public sealed class EntityChange : EntityBase
{
    [EnumValueConverter]
    public ChangeType ChangeType { get; set; }
    public string? EntityType { get; set; }
    public EntityId? EntityId { get; set; }
    public EntityId? AuthPrincipalId { get; set; }
    public DateTimeOffset Time { get; set; }
    public string? ActivityId { get; set; }

    public ICollection<PropertyChange> PropertyChanges { get; set; } = new List<PropertyChange>();
}
