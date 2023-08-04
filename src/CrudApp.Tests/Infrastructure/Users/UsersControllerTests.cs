using CrudApp.Infrastructure.Http;
using CrudApp.Infrastructure.Users;
using CrudApp.Tests.Infrastructure.Http;

namespace CrudApp.Tests.Infrastructure.Authentication;

public class UsersControllerTests
{
    [Fact]
    public async Task GetCurrentUser_ShouldReturnNull_WhenNotAuthenticated()
    {
        var fixture = await WebAppFixture.CreateAsync();
        var client = fixture.CreateHttpClient();
        var response = await client.GetAsync("/api/users/current");
        await response.EnsureSuccessAsync(HttpStatus.NoContent);
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnUser_WhenAuthenticated()
    {
        var fixture = await WebAppFixture.CreateAsync();
        var client = fixture.CreateHttpClient(fixture.InitialUserId);
        var response = await client.GetAsync("/api/users/current");
        await response.EnsureSuccessAsync(HttpStatus.Ok);
        var currentUser = await response.ReadContentAsync<User>();
        Assert.NotNull(currentUser);
        Assert.Equal(fixture.InitialUserId, currentUser.Id);
    }
}
