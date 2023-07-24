namespace CrudApp.Infrastructure.ChangeTracking;

[SkipChangeTracking]
public sealed class PropertyChangeEvent : EntityBase
{
    public EntityId EntityChangeEventId { get; set; }
    public string PropertyName { get; set; } = null!;
    [JsonValueConverter]
    public object? OldPropertyValue { get; set; }
    [JsonValueConverter]
    public object? NewPropertyValue { get; set; }

    public EntityChangeEvent EntityChangeEvent { get; set; } = null!;
}