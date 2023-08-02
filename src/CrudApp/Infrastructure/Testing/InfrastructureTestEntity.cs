namespace CrudApp.Infrastructure.Testing;

/// <summary>
/// This is used to test the application infrastructure.
/// </summary>
public class InfrastructureTestEntity : EntityBase
{
    public int? NullableInt { get; set; }
    public int NonNullableInt { get; set; } = 0;

    public EntityId? NullableRefId { get; set; }
    public InfrastructureTestRefEntity? NullableRef { get; set; }
    
    public EntityId NonNullableRefId { get; set; }
    public InfrastructureTestRefEntity NonNullableRef { get; set; } = new InfrastructureTestRefEntity();
    
    public ICollection<InfrastructureTestChildEntity>? Children { get; set; }
}

public class InfrastructureTestRefEntity : EntityBase
{
}

public class InfrastructureTestChildEntity : EntityBase
{
    public EntityId InfrastructureTestEntityId { get; set; }
    public InfrastructureTestEntity? InfrastructureTestEntity { get; set; }
}