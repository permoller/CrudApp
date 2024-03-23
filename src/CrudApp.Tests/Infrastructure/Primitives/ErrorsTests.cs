using CrudApp.Infrastructure.Testing;

namespace CrudApp.Tests.Infrastructure.Primitives;
public class ErrorsTests
{
    [Fact]
    public void Test()
    {
        var error = new Error.EntityNotFound(typeof(InfrastructureTestEntity), 3);
        Assert.Equal(HttpStatus.NotFound, error.HttpStatucCode);
        Assert.Equal(nameof(Error.EntityNotFound), error.TypeName);
        Assert.Equal("Not Found", error.Title);
        Assert.Equal("Entity not found.", error.Details);
        Assert.Equal(nameof(InfrastructureTestEntity), Assert.Contains("entityType", error.Data));
        Assert.Equal(3, (EntityId)Assert.Contains("entityId", error.Data)!);
    }
}
