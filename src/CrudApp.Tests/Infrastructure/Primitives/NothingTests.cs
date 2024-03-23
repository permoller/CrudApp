namespace CrudApp.Tests.Infrastructure.Primitives;
public class NothingTests
{
    [Fact]
    public void Multiple_instances_should_be_equal()
    {
        Assert.Equal(Nothing.Instance, new Nothing());
        Assert.Equal(default, new Nothing());
    }
}