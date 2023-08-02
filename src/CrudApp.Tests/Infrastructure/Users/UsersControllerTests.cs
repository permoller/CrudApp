using CrudApp.Infrastructure.Http;
using CrudApp.Infrastructure.Users;
using System.Net.Http.Json;

namespace CrudApp.Tests.Infrastructure.Authentication;

public class UsersControllerTests
{
    [Fact]
    public async Task GetCurrentUser_ShouldReturnNull_WhenNotAuthenticated()
    {
        var fixture = await WebAppFixture.CreateAsync();
        var client = fixture.CreateHttpClient();
        var response = await client.GetAsync("/api/users/current");
        Assert.Equal((int)HttpStatus.NoContent, (int)response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnUser_WhenAuthenticated()
    {
        var fixture = await WebAppFixture.CreateAsync();
        var client = fixture.CreateHttpClient(fixture.InitialUserId);
        var response = await client.GetAsync("/api/users/current");
        Assert.Equal((int)HttpStatus.Ok, (int)response.StatusCode);
        var currentUser = await response.Content.ReadFromJsonAsync<User>();
        Assert.NotNull(currentUser);
        Assert.Equal(fixture.InitialUserId, currentUser.Id);
    }
}
