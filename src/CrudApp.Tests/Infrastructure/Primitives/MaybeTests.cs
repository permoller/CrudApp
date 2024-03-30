using CrudApp.Infrastructure.Testing;

namespace CrudApp.Tests.Infrastructure.Primitives;
public class MaybeTests
{
    [Fact]
    public void GetValueOrNullable()
    {
        var intValue = 3;
        var objectValue = new InfrastructureTestEntity(new());

        var actual = ((IInfrastructureMaybe)new Maybe<int>(intValue)).GetValueOrNull();
        Assert.IsType<int>(actual);
        Assert.Equal(intValue, actual);

        actual = ((IInfrastructureMaybe)new Maybe<InfrastructureTestEntity>(objectValue)).GetValueOrNull();
        Assert.IsType<InfrastructureTestEntity>(actual);
        Assert.Equal(objectValue, actual);

        actual = ((IInfrastructureMaybe)new Maybe<int>()).GetValueOrNull();
        Assert.Null(actual);

        actual = ((IInfrastructureMaybe)new Maybe<InfrastructureTestEntity>()).GetValueOrNull();
        Assert.Null(actual);
    }
}
