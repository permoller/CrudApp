using CrudApp.Infrastructure.Users;
using CrudApp.Tests.Infrastructure.WebApi;
using Xunit.Abstractions;
using CrudApp.Infrastructure.Primitives;

namespace CrudApp.Tests.Infrastructure.Authentication;

public class UsersControllerTests : IntegrationTestsBase, IClassFixture<WebAppFixture>
{
    public UsersControllerTests(ITestOutputHelper testOutputHelper, WebAppFixture fixture) : base(testOutputHelper, fixture) { }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnNull_WhenNotAuthenticated()
    {
        var client = Fixture.CreateHttpClient();
        var response = await client.GetAsync("/api/users/current");
        await response.ApiEnsureSuccessAsync(HttpStatus.NoContent);
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnUser_WhenAuthenticated()
    {
        var client = Fixture.CreateHttpClient(Fixture.RootUserId);
        var response = await client.GetAsync("/api/users/current");
        await response.ApiEnsureSuccessAsync(HttpStatus.Ok);
        var currentUser = await response.ApiReadContentAsync<User>();
        Assert.NotNull(currentUser);
        Assert.Equal(Fixture.RootUserId, currentUser.Id);
    }
}
