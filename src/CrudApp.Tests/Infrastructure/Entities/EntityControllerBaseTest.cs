using CrudApp.Infrastructure.Http;
using CrudApp.Infrastructure.Testing;
using System.Net.Http.Json;

namespace CrudApp.Tests.Infrastructure.Entities;

public class EntityControllerBaseTest
{
    [Fact]
    public async Task TestCrudActions()
    {
        var fixture = await WebAppFixture.CreateAsync();
        var client = fixture.CreateHttpClient(fixture.InitialUserId);

        // Create
        var entity = new InfrastructureTestEntity(new InfrastructureTestRefEntity());
        var response = await client.PostAsJsonAsync("/api/infrastructuretest", entity);
        Assert.Equal((int)HttpStatus.Created, (int)response.StatusCode);
        var location = response.Headers.Location;
        Assert.NotNull(location);
        var id = await response.Content.ReadFromJsonAsync<EntityId>();
        Assert.NotEqual(default, id);

        // Read created
        response = await client.GetAsync(location);
        Assert.Equal((int)HttpStatus.Ok, (int)response.StatusCode);
        entity = await response.Content.ReadFromJsonAsync<InfrastructureTestEntity>();
        Assert.NotNull(entity);
        Assert.Equal(id, entity.Id);
        Assert.Equal(1, entity.Version);
        Assert.False(entity.IsSoftDeleted);
        var childEntityId = entity.NonNullableRefId;
        Assert.Equal(childEntityId, entity.NonNullableRef.Id);

        // Update
        entity.TestProp = "updated entity";
        entity.NonNullableRef.TestProp = "updated child entity";
        response = await client.PutAsJsonAsync(location, entity);
        Assert.Equal((int)HttpStatus.NoContent, (int)response.StatusCode);

        // Read updated
        response = await client.GetAsync(location);
        Assert.Equal((int)HttpStatus.Ok, (int)response.StatusCode);
        entity = await response.Content.ReadFromJsonAsync<InfrastructureTestEntity>();
        Assert.NotNull(entity);
        Assert.Equal(id, entity.Id);
        Assert.Equal(2, entity.Version);
        Assert.False(entity.IsSoftDeleted);
        Assert.Equal("updated entity", entity.TestProp);
        Assert.Equal(2, entity.NonNullableRef.Version);
        Assert.Equal("updated child entity", entity.NonNullableRef.TestProp);

        // Soft delete
        entity.IsSoftDeleted = true;
        response = await client.PutAsJsonAsync(location, entity);
        Assert.Equal((int)HttpStatus.NoContent, (int)response.StatusCode);

        // Read soft deleted
        response = await client.GetAsync(location);
        Assert.Equal((int)HttpStatus.Ok, (int)response.StatusCode);
        entity = await response.Content.ReadFromJsonAsync<InfrastructureTestEntity>();
        Assert.NotNull(entity);
        Assert.Equal(id, entity.Id);
        Assert.Equal(3, entity.Version);
        Assert.True(entity.IsSoftDeleted);
        Assert.Equal(2, entity.NonNullableRef.Version);

        // Delete
        response = await client.DeleteAsync(location);
        Assert.Equal((int)HttpStatus.NoContent, (int)response.StatusCode);

        // Read deleted
        response = await client.GetAsync(location);
        Assert.Equal((int)HttpStatus.NotFound, (int)response.StatusCode);
    }
}
