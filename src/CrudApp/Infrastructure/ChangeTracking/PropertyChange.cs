using System.Text.Json.Serialization;

namespace CrudApp.Infrastructure.ChangeTracking;

[SkipChangeTracking]
public sealed class PropertyChange : EntityBase
{
    public EntityId EntityChangeId { get; set; }
    public string PropertyName { get; set; } = null!;
    
    public string? OldPropertyValueAsJson { get; set; }
    
    public string? NewPropertyValueAsJson { get; set; }

    [JsonIgnore]
    public EntityChange? EntityChange { get; set; }

    public override string DisplayName => $"{PropertyName} {OldPropertyValueAsJson} -> {NewPropertyValueAsJson}";
}