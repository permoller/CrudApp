using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace CrudApp.Infrastructure.Testing;

/// <summary>
/// This is used to test the application infrastructure.
/// </summary>
[EntityTypeConfiguration(typeof(Configuration))]
public class InfrastructureTestEntity : EntityBase
{
    [JsonConstructor]
    private InfrastructureTestEntity()
    {
        // used when deserializing from JSON and creating entities from database
        NonNullableOwnedEntity = null!;
    }

    public InfrastructureTestEntity(InfrastructureTestOwnedEntity nonNullableOwnedEntity)
    {
        NonNullableOwnedEntity = nonNullableOwnedEntity;
    }

    public string? TestProp { get; set; }

    public int? NullableInt { get; set; }
    public int NonNullableInt { get; set; }

    // Mapped to columns in the same table as the parrent
    public InfrastructureTestOwnedEntity? NullableOwnedEntity { get; set; }

    // Mapped to columns in the same table as the parrent
    public InfrastructureTestOwnedEntity NonNullableOwnedEntity { get; set; }

    public ICollection<InfrastructureTestChildEntity> CollectionOfOwnedEntities { get; set; } = new List<InfrastructureTestChildEntity>();

    public class Configuration : IEntityTypeConfiguration<InfrastructureTestEntity>
    {
        public void Configure(EntityTypeBuilder<InfrastructureTestEntity> builder)
        {
            builder.OwnsOne(e => e.NonNullableOwnedEntity);
            builder.OwnsOne(e => e.NullableOwnedEntity);
            builder.OwnsMany(e => e.CollectionOfOwnedEntities);
        }
    }
}

public class InfrastructureTestOwnedEntity
{
    public string RequiredProp { get; set; } = "test"; // at least one required property is needed for EF Core to know if the entity exists or is null
    public string? OwnedTestProp { get; set; }
}

public class InfrastructureTestChildEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public EntityId Id { get; set; }
    public string? TestProp { get; set; }
}