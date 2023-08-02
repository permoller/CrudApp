using CrudApp.Infrastructure.Http;

namespace CrudApp.Tests.Infrastructure.Http;


public class StatusCodeTests : IClassFixture<WebAppFixture>
{
    private readonly WebAppFixture _fixture;

    public StatusCodeTests(WebAppFixture fixture)
    {
        _fixture =  fixture;
    }

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
    public async Task TestStatusCode(string httpMethod, string url, HttpStatus expectedHttpStatus)
    {
        var client = _fixture.WebAppFactory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod(httpMethod), url);
        var response = await client.SendAsync(request);
        Assert.Equal((int)expectedHttpStatus, (int)response.StatusCode);
    }
}
