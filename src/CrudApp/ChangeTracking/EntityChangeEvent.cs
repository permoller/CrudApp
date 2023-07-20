namespace CrudApp.ChangeTracking;
public enum ChangeType { EntityCreated, EntityUpdated, EntityDeleted }

[SkipChangeTracking]
public sealed class EntityChangeEvent : EntityBase
{
    [EnumValueConverter]
    public ChangeType ChangeType { get; set; }
    public string? EntityType { get; set; }
    public EntityId? EntityId { get; set; }
    public EntityId? AuthPrincipalId { get; set; }
    public DateTimeOffset Time { get; set; }
    public string? ActivityId { get; set; }

    public ICollection<PropertyChangeEvent> PropertyChangeEvents { get; set; } = new List<PropertyChangeEvent>();
}
