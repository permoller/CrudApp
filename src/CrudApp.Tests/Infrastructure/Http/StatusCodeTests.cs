using CrudApp.Infrastructure.Primitives;
using Xunit.Abstractions;

namespace CrudApp.Tests.Infrastructure.Http;


public class StatusCodeTests : IntegrationTestsBase, IClassFixture<WebAppFixture>
{
    public StatusCodeTests(ITestOutputHelper testOutputHelper, WebAppFixture fixture) : base(testOutputHelper, fixture) { }

    [Theory]
    [InlineData("GET", "/api/infrastructuretest/void", HttpStatus.NoContent)]
    [InlineData("POST", "/api/infrastructuretest/void", HttpStatus.NoContent)]
    [InlineData("PUT", "/api/infrastructuretest/void", HttpStatus.NoContent)]
    [InlineData("DELETE", "/api/infrastructuretest/void", HttpStatus.NoContent)]
    [InlineData("GET", "/api/infrastructuretest/null-ref", HttpStatus.NoContent)]
    [InlineData("POST", "/api/infrastructuretest/null-ref", HttpStatus.NoContent)]
    [InlineData("PUT", "/api/infrastructuretest/null-ref", HttpStatus.NoContent)]
    [InlineData("DELETE", "/api/infrastructuretest/null-ref", HttpStatus.NoContent)]
    [InlineData("GET", "/api/infrastructuretest/not-null-ref", HttpStatus.Ok)]
    [InlineData("POST", "/api/infrastructuretest/not-null-ref", HttpStatus.Ok)]
    [InlineData("PUT", "/api/infrastructuretest/not-null-ref", HttpStatus.Ok)]
    [InlineData("DELETE", "/api/infrastructuretest/not-null-ref", HttpStatus.Ok)]
    [InlineData("GET", "/api/infrastructuretest/null-int", HttpStatus.NoContent)]
    [InlineData("POST", "/api/infrastructuretest/null-int", HttpStatus.NoContent)]
    [InlineData("PUT", "/api/infrastructuretest/null-int", HttpStatus.NoContent)]
    [InlineData("DELETE", "/api/infrastructuretest/null-int", HttpStatus.NoContent)]
    [InlineData("GET", "/api/infrastructuretest/not-null-int", HttpStatus.Ok)]
    [InlineData("POST", "/api/infrastructuretest/not-null-int", HttpStatus.Ok)]
    [InlineData("PUT", "/api/infrastructuretest/not-null-int", HttpStatus.Ok)]
    [InlineData("DELETE", "/api/infrastructuretest/not-null-int", HttpStatus.Ok)]
    public async Task TestStatusCode(string httpMethod, string url, int expectedHttpStatus)
    {
        var client = Fixture.CreateHttpClient();
        var request = new HttpRequestMessage(new HttpMethod(httpMethod), url);
        var response = await client.SendAsync(request);
        Assert.Equal(expectedHttpStatus, (int)response.StatusCode);
    }
}
