namespace CrudApp.Infrastructure.ChangeTracking;

[SkipChangeTracking]
public sealed class PropertyChange : EntityBase
{
    public EntityId EntityChangeId { get; set; }
    public string PropertyName { get; set; } = null!;
    [JsonValueConverter]
    public object? OldPropertyValue { get; set; }
    [JsonValueConverter]
    public object? NewPropertyValue { get; set; }

    public EntityChange? EntityChange { get; set; }
}