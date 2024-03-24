using CrudApp.Infrastructure.Testing;
using CrudApp.Infrastructure.UtilityCode;
using CrudApp.Tests.Infrastructure.Entities;
using CrudApp.Tests.Infrastructure.WebApi;
using System.Text.Json;
using Xunit.Abstractions;
using static CrudApp.Infrastructure.Primitives.Error;

namespace CrudApp.Tests.Infrastructure.ErrorHandling;
public class ProblemDetailsHelperTests(ITestOutputHelper testOutputHelper, WebAppFixture fixture) :
    IntegrationTestsBase(testOutputHelper, fixture), IClassFixture<WebAppFixture>
{
    [Fact]
    public async Task TestProblemDetailsWithData()
    {
        var client = Fixture.CreateHttpClient();
        var ex = await Assert.ThrowsAsync<ProblemDetailsApiException>(() => client.GetEntityAsync<InfrastructureTestEntity>(-1));
        var actual = ex.ProblemDetails;
        Assert.Equal(HttpStatus.NotFound, actual.Status);
        Assert.Equal(nameof(EntityNotFound), actual.GetErrorTypeName());
        Assert.Equal("Entity not found.", actual.Title);
        Assert.Null(actual.Detail);
        Assert.True(actual.TryGetData(out var data));
        Assert.True(data.TryGetValue("entityType", out var entityTypeJsonElement));
        Assert.Equal(nameof(InfrastructureTestEntity), entityTypeJsonElement?.GetString());
        Assert.True(data.TryGetValue("entityId", out var entityIdJsonElement));
        Assert.Equal(-1, entityIdJsonElement?.GetInt64());
        actual.TryGetExtension("xxx", out JsonElement element);
    }

    [Fact]
    public async Task TestProblemDetailsWithErrors()
    {
        var client = Fixture.CreateHttpClient();
        var invalidEntity = new InfrastructureTestEntity(null!);
        var ex = await Assert.ThrowsAsync<ProblemDetailsApiException>(() => client.CreateEntityAsync(invalidEntity));
        var actual = ex.ProblemDetails;
        Assert.Equal(HttpStatus.BadRequest, actual.Status);

        Assert.Equal(nameof(ValidationFailed), actual.GetErrorTypeName());
        Assert.Equal("Validation failed.", actual.Title);
        Assert.Null(actual.Detail);
        Assert.True(actual.TryGetErrors(out var errors));

        Assert.Equal("The NonNullableOwnedEntity field is required.", Assert.Contains("NonNullableOwnedEntity", errors)[0]);
    }
}
