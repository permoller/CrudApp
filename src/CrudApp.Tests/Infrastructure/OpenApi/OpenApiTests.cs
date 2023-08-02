using CrudApp.Infrastructure.Http;

namespace CrudApp.Tests.Infrastructure.OpenApi;

[UsesVerify]
public class OpenApiTests
{
    [Fact]
    public async Task GetOpenApiDocumentation()
    {
        var fixture = await WebAppFixture.CreateAsync();
        var client = fixture.CreateHttpClient(fixture.InitialUserId);
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal((int)HttpStatus.Ok, (int)response.StatusCode);
        var openApiDoc = await response.Content.ReadAsStringAsync();
        await Verify(openApiDoc);
    }
}
