using CrudApp.Infrastructure.Http;
using CrudApp.Infrastructure.Testing;
using CrudApp.Infrastructure.UtilityCode;
using CrudApp.Tests.Infrastructure.Http;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace CrudApp.Tests.Infrastructure.Entities;

public class EntityControllerBaseTests : IntegrationTestsBase
{
    public EntityControllerBaseTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper, new WebAppFixture()) { }

    [Fact]
    public async Task TestCrudActions()
    {
        
        var client = Fixture.CreateHttpClient(Fixture.InitialUserId);

        // Create
        var entity = new InfrastructureTestEntity(new InfrastructureTestRefEntity() { TestProp = "original ref entity" }) { TestProp = "original entity" };
        var id = await client.PostEntityAsync(entity);
        Assert.NotEqual(default, id);

        // TODO: Test creating entity with same id again

        // Read created
        entity = await client.GetEntityAsync<InfrastructureTestEntity>(id);
        Assert.NotNull(entity);
        Assert.Equal(id, entity.Id);
        Assert.Equal(1, entity.Version);
        Assert.False(entity.IsSoftDeleted);
        Assert.Equal("original entity", entity.TestProp);
        Assert.Equal(entity.NonNullableRefId, entity.NonNullableRef.Id);
        Assert.Equal("original ref entity", entity.NonNullableRef.TestProp);

        // Update
        entity.TestProp = "updated entity";
        entity.NonNullableRef.TestProp = "updated ref entity";
        await client.PutEntityAsync(entity);

        // Read updated
        entity = await client.GetEntityAsync<InfrastructureTestEntity>(id);
        Assert.NotNull(entity);
        Assert.Equal(id, entity.Id);
        Assert.Equal(2, entity.Version);
        Assert.False(entity.IsSoftDeleted);
        Assert.Equal("updated entity", entity.TestProp);
        Assert.Equal(2, entity.NonNullableRef.Version);
        Assert.Equal("updated ref entity", entity.NonNullableRef.TestProp);

        // Soft delete
        entity.IsSoftDeleted = true;
        await client.PutEntityAsync(entity);

        // Read soft deleted
        entity = await client.GetEntityAsync<InfrastructureTestEntity>(id);
        Assert.NotNull(entity);
        Assert.Equal(id, entity.Id);
        Assert.Equal(3, entity.Version);
        Assert.True(entity.IsSoftDeleted);
        Assert.Equal(2, entity.NonNullableRef.Version);

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

        var entity = new InfrastructureTestEntity(new InfrastructureTestRefEntity()) { TestProp = "test location header" };
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
