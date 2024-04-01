using CrudApp.Infrastructure.Testing;

namespace CrudApp.Tests.Infrastructure.Primitives;
public class MaybeTests
{
    [Fact]
    public void GetValueOrNull()
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

    [Fact]
    public void GetValueOrDefault()
    {
        var intValue = 3;
        var objectValue = new InfrastructureTestEntity(new());

        Assert.Equal(intValue, new Maybe<int>(intValue).GetValueOrDefault());
        Assert.Equal(default(int), new Maybe<int>().GetValueOrDefault());
        Assert.Equal(objectValue, new Maybe<InfrastructureTestEntity>(objectValue).GetValueOrDefault());
        Assert.Null(new Maybe<InfrastructureTestEntity>().GetValueOrDefault());
    }

    [Fact]
    public void ToMaybe()
    {
        var intValue = 3;
        var objectValue = new InfrastructureTestEntity(new());

        Assert.True(intValue.ToMaybe().Match(v => v == intValue, () => false));
        Assert.True(((int?)intValue).ToMaybe().Match(v => v == intValue, () => false));
        Assert.True(((int?)null).ToMaybe().Match(v => false, () => true));
        Assert.True(objectValue.ToMaybe().Match(v => v == objectValue, () => false));
        Assert.True(((InfrastructureTestEntity?)null).ToMaybe().Match(v => false, () => true));
    }

    [Fact]
    public void Select()
    {
        var intValue = 3;
        var objectValue = new InfrastructureTestEntity(new());

        Assert.True(new Maybe<int>().Select(v => Fail()).Match(v => Fail(), () => true));
        Assert.True(new Maybe<int>(intValue).Select(i => i + 1).Match(v => v == intValue + 1, () => Fail()));
        Assert.True(new Maybe<int>(intValue).Select(i => (int?)null).Match(v => Fail(), () => true));
        Assert.True(new Maybe<int>(intValue).Select(i => objectValue).Match(v => true, () => Fail()));
        Assert.True(new Maybe<InfrastructureTestEntity>().Select(o => Fail()).Match(v => Fail(), () => true));
        Assert.True(new Maybe<InfrastructureTestEntity>(objectValue).Select(o => new { Entity = o }).Match(v => v.Entity == objectValue, () => Fail()));
        Assert.True(new Maybe<InfrastructureTestEntity>(objectValue).Select(i => (int?)null).Match(v => Fail(), () => true));

        Assert.True(((int?)intValue).Select(v => v + 1).Match(v => v == intValue + 1, () => Fail()));
        Assert.True(((int?)null).Select(v => Fail()).Match(v => Fail(), () => true));

        static bool Fail()
        {
            Assert.Fail("Should not be called");
            throw new InvalidOperationException();
        }
    }
}
