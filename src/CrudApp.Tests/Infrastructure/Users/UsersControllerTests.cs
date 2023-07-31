using CrudApp.Infrastructure.Authentication;
using CrudApp.Infrastructure.Http;
using CrudApp.Infrastructure.Users;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CrudApp.Tests.Infrastructure.Authentication;

public class UsersControllerTests
{
    [Fact]
    public async Task GetCurrentUser_ShouldReturnNull_WhenNotAuthenticated()
    {
        var fixture = await WebAppFixture.CreateAsync();
        var client = fixture.WebAppFactory.CreateClient();
        var response = await client.GetAsync("/api/users/current");
        Assert.Equal((int)HttpStatus.NoContent, (int)response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnUser_WhenAuthenticated()
    {
        var fixture = await WebAppFixture.CreateAsync();
        var client = fixture.WebAppFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(UserIdAuthenticationHandler.HttpAuthenticationScheme, fixture.InitialUserId.ToString());
        var response = await client.GetAsync("/api/users/current");
        Assert.Equal((int)HttpStatus.Ok, (int)response.StatusCode);
        var currentUser = await response.Content.ReadFromJsonAsync<User>();
        Assert.NotNull(currentUser);
        Assert.Equal(fixture.InitialUserId, currentUser.Id);
    }
}
