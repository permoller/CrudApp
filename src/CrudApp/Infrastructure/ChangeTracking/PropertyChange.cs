using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json.Serialization;

namespace CrudApp.Infrastructure.ChangeTracking;

[SkipChangeTracking]
[EntityTypeConfiguration(typeof(Configuration))]
public sealed class PropertyChange : EntityBase
{
    public EntityId EntityChangeId { get; set; }

    public string PropertyName { get; set; } = null!;
    
    public string? OldPropertyValueAsJson { get; set; }
    
    public string? NewPropertyValueAsJson { get; set; }

    public override string DisplayName => $"{PropertyName} {OldPropertyValueAsJson} -> {NewPropertyValueAsJson}";

    public sealed class Configuration : IEntityTypeConfiguration<PropertyChange>
    {
        public void Configure(EntityTypeBuilder<PropertyChange> builder)
        {
            builder.HasOne<EntityChange>().WithMany().HasForeignKey(e => e.EntityChangeId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}