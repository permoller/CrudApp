using CrudApp.Infrastructure.Http;
using Xunit.Abstractions;

namespace CrudApp.Tests.Infrastructure.OpenApi;

[UsesVerify]
public class OpenApiTests : IntegrationTestsBase
{
    public OpenApiTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper, new WebAppFixture()) { }

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
