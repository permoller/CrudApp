﻿using CrudApp.Infrastructure.Http;
using CrudApp.Infrastructure.Users;
using CrudApp.Tests.Infrastructure.Http;
using Xunit.Abstractions;

namespace CrudApp.Tests.Infrastructure.Authentication;

public class UsersControllerTests : IntegrationTestsBase, IClassFixture<WebAppFixture>
{
    public UsersControllerTests(ITestOutputHelper testOutputHelper, WebAppFixture fixture) : base(testOutputHelper, fixture) { }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnNull_WhenNotAuthenticated()
    {
        var client = Fixture.CreateHttpClient();
        var response = await client.GetAsync("/api/users/current");
        await response.EnsureSuccessAsync(HttpStatus.NoContent);
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnUser_WhenAuthenticated()
    {
        var client = Fixture.CreateHttpClient(Fixture.InitialUserId);
        var response = await client.GetAsync("/api/users/current");
        await response.EnsureSuccessAsync(HttpStatus.Ok);
        var currentUser = await response.ReadContentAsync<User>();
        Assert.NotNull(currentUser);
        Assert.Equal(Fixture.InitialUserId, currentUser.Id);
    }
}
