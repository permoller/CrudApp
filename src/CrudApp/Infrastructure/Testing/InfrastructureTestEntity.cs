using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
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
        NonNullableOwned = null!;
    }

    public InfrastructureTestEntity(InfrastructureTestOwnedEntity nonNullableOwned)
    {
        NonNullableOwned = nonNullableOwned;
    }

    public string? TestProp { get; set; }

    public int? NullableInt { get; set; }
    public int NonNullableInt { get; set; }

    public InfrastructureTestOwnedEntity? NullableOwned { get; set; }
    
    public InfrastructureTestOwnedEntity NonNullableOwned { get; set; }

    public ICollection<InfrastructureTestChildEntity> Children { get; set; } = new List<InfrastructureTestChildEntity>();

    public class Configuration : IEntityTypeConfiguration<InfrastructureTestEntity>
    {
        public void Configure(EntityTypeBuilder<InfrastructureTestEntity> builder)
        {
            builder.OwnsOne(e => e.NonNullableOwned).WithOwner();
            builder.OwnsOne(e => e.NullableOwned).WithOwner();
            builder.HasMany(e => e.Children).WithOne().HasForeignKey(child => child.OwnerId);
        }
    }
}

public class InfrastructureTestOwnedEntity
{
    public string RequiredProp { get; set; } = "test"; // at least one required property is needed for EF Core to know if 
    public string? OwnedTestProp { get; set; }
}

public class InfrastructureTestChildEntity
{
    public EntityId Id { get; set; }
    public EntityId OwnerId { get; set; }
    public string? TestProp { get; set; }
}