using Xunit.Abstractions;

namespace CrudApp.Tests;
public abstract class IntegrationTestsBase : IAsyncLifetime
{
    protected ITestOutputHelper TestOutputHelper { get; }

    protected WebAppFixture Fixture { get; }

    protected IntegrationTestsBase(ITestOutputHelper testOutputHelper, WebAppFixture webAppFixture)
    {
        TestOutputHelper = testOutputHelper;
        Fixture = webAppFixture;
    }

    public virtual async Task InitializeAsync()
    {
        await Fixture.StartTestAsync(TestOutputHelper);
    }

    public virtual async Task DisposeAsync()
    {
        await Fixture.StopTestAsync();
    }
}

public abstract class IntegrationTestsBase<T> : IntegrationTestsBase where T : WebAppFixture
{
    new protected T Fixture { get; }

    protected IntegrationTestsBase(ITestOutputHelper testOutputHelper, T webAppFixture) : base(testOutputHelper, webAppFixture)
    {
        Fixture = webAppFixture;
    }
}
