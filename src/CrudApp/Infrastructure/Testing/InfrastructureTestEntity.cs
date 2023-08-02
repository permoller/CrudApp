using System.Text.Json.Serialization;

namespace CrudApp.Infrastructure.Testing;

/// <summary>
/// This is used to test the application infrastructure.
/// </summary>
public class InfrastructureTestEntity : EntityBase
{
    [JsonConstructor]
    private InfrastructureTestEntity()
    {
        // used when deserializing from JSON and creating entities from database
        NonNullableRef = null!;
    }

    public InfrastructureTestEntity(InfrastructureTestRefEntity nonNullableRef)
    {
        NonNullableRef = nonNullableRef;
    }

    public string? TestProp { get; set; }

    public int? NullableInt { get; set; }
    public int NonNullableInt { get; set; }

    public EntityId? NullableRefId { get; set; }
    public InfrastructureTestRefEntity? NullableRef { get; set; }
    
    public EntityId NonNullableRefId { get; set; }
    public InfrastructureTestRefEntity NonNullableRef { get; set; }
    
    public ICollection<InfrastructureTestChildEntity>? Children { get; set; }
}

public class InfrastructureTestRefEntity : EntityBase
{
    public string? TestProp { get; set; }
}

public class InfrastructureTestChildEntity : EntityBase
{
    public string? TestProp { get; set; }
    public EntityId InfrastructureTestEntityId { get; set; }
    public InfrastructureTestEntity? InfrastructureTestEntity { get; set; }
}