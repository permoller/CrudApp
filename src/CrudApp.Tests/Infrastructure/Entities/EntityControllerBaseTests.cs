using CrudApp.Infrastructure.Http;
using CrudApp.Infrastructure.Testing;
using CrudApp.Infrastructure.UtilityCode;
using CrudApp.Tests.Infrastructure.Http;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace CrudApp.Tests.Infrastructure.Entities;

public class EntityControllerBaseTests : IntegrationTestsBase, IClassFixture<WebAppFixture>
{
    public EntityControllerBaseTests(ITestOutputHelper testOutputHelper, WebAppFixture fixture) : base(testOutputHelper, fixture) { }

    [Fact]
    public async Task TestCrudActions()
    {
        long expectedVersion = 0;
        var client = Fixture.CreateHttpClient(Fixture.InitialUserId);

        // Create
        var entity = new InfrastructureTestEntity(new InfrastructureTestOwnedEntity() { OwnedTestProp = "original owned entity" }) { TestProp = "original entity" };
        var id = await client.PostEntityAsync(entity);
        expectedVersion++;
        Assert.NotEqual(default, id);

        // TODO: Test creating entity with same id again

        // Read created
        entity = await client.GetEntityAsync<InfrastructureTestEntity>(id);
        Assert.NotNull(entity);
        Assert.Equal(id, entity.Id);
        Assert.Equal(expectedVersion, entity.Version);
        Assert.False(entity.IsSoftDeleted);
        Assert.Equal("original entity", entity.TestProp);
        Assert.NotNull(entity.NonNullableOwned);
        Assert.Equal("original owned entity", entity.NonNullableOwned.OwnedTestProp);
        Assert.Null(entity.NullableOwned);
        //Assert.Empty(entity.Children);

        // Update
        entity.TestProp = "updated entity";
        await client.PutEntityAsync(entity);
        expectedVersion++;

        // Read updated
        entity = await client.GetEntityAsync<InfrastructureTestEntity>(id);
        Assert.NotNull(entity);
        Assert.Equal(id, entity.Id);
        Assert.Equal(expectedVersion, entity.Version);
        Assert.False(entity.IsSoftDeleted);
        Assert.Equal("updated entity", entity.TestProp);
        Assert.NotNull(entity.NonNullableOwned);
        Assert.Equal("original owned entity", entity.NonNullableOwned.OwnedTestProp);
        Assert.Null(entity.NullableOwned);

        // Update NonNullableOwned
        entity.NonNullableOwned.OwnedTestProp = "updated owned entity";
        await client.PutEntityAsync(entity);
        expectedVersion++;

        // Read updated NonNullableOwned
        entity = await client.GetEntityAsync<InfrastructureTestEntity>(id);
        Assert.NotNull(entity);
        Assert.Equal(id, entity.Id);
        Assert.Equal(expectedVersion, entity.Version);
        Assert.False(entity.IsSoftDeleted);
        Assert.Equal("updated entity", entity.TestProp);
        Assert.NotNull(entity.NonNullableOwned);
        Assert.Equal("updated owned entity", entity.NonNullableOwned.OwnedTestProp);
        Assert.Null(entity.NullableOwned);

        // Add NullableOwned
        entity.NullableOwned = new InfrastructureTestOwnedEntity() { OwnedTestProp = "created NullableOwned" };
        await client.PutEntityAsync(entity);
        expectedVersion++;

        // Read added NullableOwned
        entity = await client.GetEntityAsync<InfrastructureTestEntity>(id);
        Assert.NotNull(entity);
        Assert.Equal(id, entity.Id);
        Assert.Equal(expectedVersion, entity.Version);
        Assert.False(entity.IsSoftDeleted);
        Assert.Equal("updated entity", entity.TestProp);
        Assert.NotNull(entity.NonNullableOwned);
        Assert.Equal("updated owned entity", entity.NonNullableOwned.OwnedTestProp);
        Assert.NotNull(entity.NullableOwned);
        Assert.Equal("created NullableOwned", entity.NullableOwned.OwnedTestProp);

        // Update NullableOwned
        entity.NullableOwned.OwnedTestProp = "updated NullableOwned";
        await client.PutEntityAsync(entity);
        expectedVersion++;

        // Read updated NullableOwned
        entity = await client.GetEntityAsync<InfrastructureTestEntity>(id);
        Assert.NotNull(entity);
        Assert.Equal(id, entity.Id);
        Assert.Equal(expectedVersion, entity.Version);
        Assert.False(entity.IsSoftDeleted);
        Assert.Equal("updated entity", entity.TestProp);
        Assert.NotNull(entity.NonNullableOwned);
        Assert.Equal("updated owned entity", entity.NonNullableOwned.OwnedTestProp);
        Assert.NotNull(entity.NullableOwned);
        Assert.Equal("updated NullableOwned", entity.NullableOwned.OwnedTestProp);

        // Remove NullableOwned
        entity.NullableOwned = null;
        await client.PutEntityAsync(entity);
        expectedVersion++;

        // Read removed NullableOwned
        entity = await client.GetEntityAsync<InfrastructureTestEntity>(id);
        Assert.NotNull(entity);
        Assert.Equal(id, entity.Id);
        Assert.Equal(expectedVersion, entity.Version);
        Assert.False(entity.IsSoftDeleted);
        Assert.Equal("updated entity", entity.TestProp);
        Assert.NotNull(entity.NonNullableOwned);
        Assert.Equal("updated owned entity", entity.NonNullableOwned.OwnedTestProp);
        Assert.Null(entity.NullableOwned);

        // Soft delete
        entity.IsSoftDeleted = true;
        await client.PutEntityAsync(entity);
        expectedVersion++;

        // Read soft deleted
        entity = await client.GetEntityAsync<InfrastructureTestEntity>(id);
        Assert.NotNull(entity);
        Assert.Equal(id, entity.Id);
        Assert.Equal(expectedVersion, entity.Version);
        Assert.True(entity.IsSoftDeleted);
        Assert.Equal("updated entity", entity.TestProp);
        Assert.NotNull(entity.NonNullableOwned);
        Assert.Equal("updated owned entity", entity.NonNullableOwned.OwnedTestProp);
        Assert.Null(entity.NullableOwned);

        // Delete
        await client.DeleteEntityAsync<InfrastructureTestEntity>(id);

        // Read deleted
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => client.GetEntityAsync<InfrastructureTestEntity>(id));
        Assert.Equal(HttpStatus.NotFound, (int?)ex.StatusCode);
    }

    [Fact]
    public async Task CreateShouldReturnLocationHeader()
    {
        var client = Fixture.CreateHttpClient(Fixture.InitialUserId);

        var entity = new InfrastructureTestEntity(new InfrastructureTestOwnedEntity()) { TestProp = "test location header" };
        var response = await client.PostAsJsonAsync("/api/infrastructuretest", entity, JsonUtils.ApiJsonSerializerOptions);
        await response.EnsureSuccessAsync(HttpStatus.Created);
        var location = response.Headers.Location;
        Assert.NotNull(location);
        var id = await response.ReadContentAsync<EntityId>();

        entity = await client.GetFromJsonAsync<InfrastructureTestEntity>(location, JsonUtils.ApiJsonSerializerOptions);
        Assert.NotNull(entity);
        Assert.Equal(id, entity.Id);
        Assert.Equal("test location header", entity.TestProp);
    }

    
}
