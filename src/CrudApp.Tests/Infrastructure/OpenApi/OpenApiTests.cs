using CrudApp.Infrastructure.Http;
using Xunit.Abstractions;

namespace CrudApp.Tests.Infrastructure.OpenApi;

public class OpenApiTests : IntegrationTestsBase, IClassFixture<WebAppFixture>
{
    public OpenApiTests(ITestOutputHelper testOutputHelper, WebAppFixture fixture) : base(testOutputHelper, fixture) { }

    [Fact]
    public async Task GetOpenApiDocumentation()
    {
        var client = Fixture.CreateHttpClient(Fixture.InitialUserId);
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatus.Ok, (int)response.StatusCode);
        var openApiDoc = await response.Content.ReadAsStringAsync();
        await Verify(openApiDoc);
    }
}
