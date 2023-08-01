using CrudApp.Infrastructure.Authentication;
using CrudApp.Infrastructure.Http;
using CrudApp.Infrastructure.Users;
using System.Net.Http.Headers;

namespace CrudApp.Tests.Infrastructure.OpenApi;

[UsesVerify]
public class OpenApiTests
{
    [Fact]
    public async Task GetOpenApiDocumentation()
    {
        var fixture = await WebAppFixture.CreateAsync();
        var client = fixture.WebAppFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(UserIdAuthenticationHandler.HttpAuthenticationScheme, fixture.InitialUserId.ToString());
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal((int)HttpStatus.Ok, (int)response.StatusCode);
        var openApiDoc = await response.Content.ReadAsStringAsync();
        await Verify(openApiDoc);
    }
}
