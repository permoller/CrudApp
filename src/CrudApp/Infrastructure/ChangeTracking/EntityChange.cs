using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

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

    public ICollection<PropertyChange> PropertyChanges { get; set; } = new List<PropertyChange>();

    /// <summary>
    /// Configure a shadow property (column) on <see cref="EntityChange"/> to contain a foreign key to an <see cref="EntityBase"/> subtype.
    /// This is used by EF Core for the relation from the entity types <see cref="EntityBase.EntityChanges"/> navigation property to the <see cref="EntityChange"/> entities.
    /// </summary>
    /// <param name="entityTypeBuilder"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void ConfigureEntityChangesRelation(EntityTypeBuilder entityTypeBuilder)
    {
        var entityBaseSubtype = entityTypeBuilder.Metadata.ClrType;

        if (!entityBaseSubtype.IsSubclassOf(typeof(EntityBase)))
            throw new ArgumentException($"{entityBaseSubtype.Name} is not a subtype of {nameof(EntityBase)}.");

        // If change tracking is disabled, we do not add the shadow property and tells EF Core to ignore the navigation property.
        if (!ChangeTrackingHelper.IsChangeTrackingEnabled(entityBaseSubtype))
        {
            entityTypeBuilder.Ignore(nameof(EntityChanges));
            return;
        }

        // We need to use a name that does not conflict with any of the existing properties
        var shadowPropertyName = nameof(EntityId) + "Of" + entityBaseSubtype.Name;
        if (entityBaseSubtype.GetProperty(shadowPropertyName) is not null)
            throw new InvalidOperationException($"{nameof(EntityChange)} allready has a property named {shadowPropertyName}.");

        entityTypeBuilder
            .HasMany(nameof(EntityBase.EntityChanges))
            .WithOne()// no navigation property to entity
            .HasForeignKey(shadowPropertyName)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
