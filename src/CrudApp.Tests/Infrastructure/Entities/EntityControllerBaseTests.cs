using CrudApp.Infrastructure.Testing;
using CrudApp.Infrastructure.UtilityCode;
using CrudApp.Tests.Infrastructure.WebApi;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit.Abstractions;
using static CrudApp.Infrastructure.Primitives.Error;

namespace CrudApp.Tests.Infrastructure.Entities;

// TODO: Test adding, updating and removing entities from non-owned collection and non-owned properties. These updates are intentionally ignored.
public class EntityControllerBaseTests(ITestOutputHelper testOutputHelper, WebAppFixture fixture) :
    IntegrationTestsBase(testOutputHelper, fixture), IClassFixture<WebAppFixture>
{
    HttpClient _client = null!;
    InfrastructureTestEntity _entity = null!;
    Func<Task> _onDispose = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _client = Fixture.CreateHttpClient(Fixture.RootUserId);
        _entity = await CreateEntity();
        var entityId = _entity.Id;
        _onDispose = () => DeleteEntity(entityId);
    }

    public override async Task DisposeAsync()
    {
        await _onDispose();
        await base.DisposeAsync();
    }

    private async Task<InfrastructureTestEntity> CreateEntity()
    {
        var nonNullableOwned = new InfrastructureTestOwnedEntity() { OwnedTestProp = "original OwnedTestProp" };
        var entity = new InfrastructureTestEntity(nonNullableOwned) { TestProp = "original TestProp" };

        var id = await _client.CreateEntityAsync(entity);

        Assert.NotEqual(default, id);
        var actual = await _client.GetEntityAsync<InfrastructureTestEntity>(id);
        entity.Id = id;
        entity.Version = 1;
        AssertEqual(entity, actual);

        return actual;
    }

    private async Task DeleteEntity(EntityId entityId)
    {
        try
        {
            await _client.DeleteEntityAsync<InfrastructureTestEntity>(entityId, null);
        }
        catch(ProblemDetailsApiException e) when (e.ProblemDetails?.GetErrorTypeName() == nameof(EntityAlreadyDeleted))
        {
            // ignore
        }
    }


    [Fact]
    public async Task CreateEntityWithVersionShouldFail()
    {
        var entity = new InfrastructureTestEntity(new()) { Version = 1 };
        await AssertError<VersionCannotBeSetDirectly>(() => _client.CreateEntityAsync(entity));
    }

    [Fact]
    public async Task RecreateEntityWithSameIdShouldFail()
    {
        _entity.Version = default;
        await AssertError<CannotCreateEntityWithSameIdAsExistingEntity>(() => _client.CreateEntityAsync(_entity));
    }

    [Fact]
    public async Task UpdateWithoutChanges()
    {
        var actual = await _client.PutAndGetEntity(_entity);
        AssertEqual(_entity, actual);
    }

    [Fact]
    public async Task UpdateSimpleValue()
    {
        _entity.TestProp = "updated simple prop";
        var actual = await _client.PutAndGetEntity(_entity);
        _entity.Version++;
        AssertEqual(_entity, actual);
    }

    [Fact]
    public async Task UpdateShouldFailIfVersionDoesNotMatchDb()
    {
        _entity.TestProp = "updated old entity";
        _entity.Version--;
        await AssertError<EntityVersionInRequestDoesNotMatchVersionInDatabase>(() => _client.UpdateEntityAsync(_entity));
    }

    [Fact]
    public async Task UpdateNonNullableOwnedEntity()
    {
        _entity.NonNullableOwnedEntity.OwnedTestProp = "updated OwnedTestProp";
        var actual = await _client.PutAndGetEntity(_entity);
        _entity.Version++;
        AssertEqual(_entity, actual);

        _entity.NonNullableOwnedEntity = null!;
        var problemDetails = await AssertError<ValidationFailed>(() => _client.UpdateEntityAsync(_entity));
        Assert.True(problemDetails.TryGetErrors(out var errors));
        Assert.Equal($"The {nameof(_entity.NonNullableOwnedEntity)} field is required.", errors[nameof(_entity.NonNullableOwnedEntity)][0]);
    }

    [Fact]
    public async Task UpdateNullableOwnedEntity()
    {
        // Add owned entity
        _entity.NullableOwnedEntity = new InfrastructureTestOwnedEntity() { OwnedTestProp = "created" };
        var actual = await _client.PutAndGetEntity(_entity);
        _entity.Version++;
        AssertEqual(_entity, actual);

        // Update owned entity
        _entity.NullableOwnedEntity.OwnedTestProp = "updated";
        actual = await _client.PutAndGetEntity(_entity);
        _entity.Version++;
        AssertEqual(_entity, actual);

        // Remove owned entity
        _entity.NullableOwnedEntity = null;
        actual = await _client.PutAndGetEntity(_entity);
        _entity.Version++;
        AssertEqual(_entity, actual);
    }

    [Fact]
    public async Task UpdateOwnedEntityCollection()
    {
        // Add owned entity in collection
        var entityInCollection = new InfrastructureTestChildEntity() { TestProp = "added" };
        _entity.CollectionOfOwnedEntities.Add(entityInCollection);
        var actual = await _client.PutAndGetEntity(_entity);
        _entity.Version++;
        entityInCollection.Id = actual.CollectionOfOwnedEntities.Single().Id;
        AssertEqual(_entity, actual);

        // Update owned enity in collection
        _entity.CollectionOfOwnedEntities.Single().TestProp = "updated";
        actual = await _client.PutAndGetEntity(_entity);
        _entity.Version++;
        AssertEqual(_entity, actual);

        // Remove owned entity from collection
        _entity.CollectionOfOwnedEntities.Remove(entityInCollection);
        actual = await _client.PutAndGetEntity(_entity);
        _entity.Version++;
        AssertEqual(_entity, actual);

        // Add two owned entities in collection
        _entity.CollectionOfOwnedEntities.Add(new() { TestProp = "first" });
        _entity.CollectionOfOwnedEntities.Add(new() { TestProp = "second" });
        actual = await _client.PutAndGetEntity(_entity);
        _entity.Version++;
        _entity.CollectionOfOwnedEntities.Single(e => e.TestProp == "first").Id = actual.CollectionOfOwnedEntities.First(e => e.TestProp == "first").Id;
        _entity.CollectionOfOwnedEntities.Single(e => e.TestProp == "second").Id = actual.CollectionOfOwnedEntities.First(e => e.TestProp == "second").Id;
        AssertEqual(_entity, actual);
    }

    
    [Fact]
    public async Task SoftDelete()
    {
        // Delete
        await _client.DeleteEntityAsync<InfrastructureTestEntity>(_entity.Id, _entity.Version);
        _entity.Version++;
        _entity.IsSoftDeleted = true;
        var actual = await _client.GetEntityAsync<InfrastructureTestEntity>(_entity.Id, includeSoftDeleted: true);
        AssertEqual(_entity, actual);

        await AssertError<CannotGetDeletedEntity>(() => _client.GetEntityAsync<InfrastructureTestEntity>(_entity.Id));
        await AssertError<CannotUpdateDeletedEntity>(() => _client.UpdateEntityAsync(_entity));
        await AssertError<EntityAlreadyDeleted>(() => _client.DeleteEntityAsync<InfrastructureTestEntity>(_entity.Id, version: null));
    }

    [Fact]
    public async Task SoftDeleteWithWrongVersionShouldFail()
    {
        await AssertError<EntityVersionInRequestDoesNotMatchVersionInDatabase>(() =>
        _client.DeleteEntityAsync<InfrastructureTestEntity>(_entity.Id, _entity.Version + 1));
    }

    [Fact]
    public async Task SoftDeleteIgnoringVersion()
    {
        await _client.DeleteEntityAsync<InfrastructureTestEntity>(_entity.Id, version: null);
        _entity.Version++;
        _entity.IsSoftDeleted = true;
        var actual = await _client.GetEntityAsync<InfrastructureTestEntity>(_entity.Id, includeSoftDeleted: true);
        AssertEqual(_entity, actual);
    }


    [Fact]
    public async Task SoftDeleteDuringCreateShoudFail()
    {
        // Delete during create
        var newEntity = new InfrastructureTestEntity(new()) { TestProp = "Marking as deleted during create", IsSoftDeleted = true };
        await AssertError<SoftDeleteCannotBeSetDirectly>(() => _client.CreateEntityAsync(newEntity));

    }

    [Fact]
    public async Task SoftDeleteDuringUpdateShouldFail()
    {
        _entity.TestProp = "Marking as deleted during update";
        _entity.IsSoftDeleted = true;
        await AssertError<SoftDeleteCannotBeSetDirectly>(() => _client.UpdateEntityAsync(_entity));
        _entity.IsSoftDeleted = false;
    }

    [Fact]
    public async Task CreateShouldReturnLocationHeader()
    {
        var client = Fixture.CreateHttpClient(Fixture.RootUserId);

        var entity = new InfrastructureTestEntity(new InfrastructureTestOwnedEntity()) { TestProp = "test location header" };
        var response = await client.ApiPostAsJsonAsync("/api/infrastructuretest", entity);
        await response.ApiEnsureSuccessAsync(HttpStatus.Created);
        var location = response.Headers.Location;
        var id = await response.ApiReadContentAsync<EntityId>();
        try
        {
            Assert.NotNull(location);
            entity = await client.GetFromJsonAsync<InfrastructureTestEntity>(location, JsonUtils.ApiJsonSerializerOptions);
            Assert.NotNull(entity);
            Assert.Equal(id, entity.Id);
            Assert.Equal("test location header", entity.TestProp);
        }
        finally
        {
            await _client.DeleteEntityAsync<InfrastructureTestEntity>(id, version: null);
        }
    }

    private static async Task<ProblemDetails> AssertError<T>(Func<Task> action) where T : Error
    {
        var ex = await Assert.ThrowsAsync<ProblemDetailsApiException>(action);
        Assert.Equal(typeof(T).Name, ex.ProblemDetails.GetErrorTypeName());
        return ex.ProblemDetails;
    }

    private static void AssertEqual(InfrastructureTestEntity expected, InfrastructureTestEntity actual)
    {
        Assert.Equal(expected is null, actual is null);
        if (expected is not null && actual is not null)
        {
            // We test all properties individually to make debugging easier.
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Version, actual.Version);
            Assert.Equal(expected.IsSoftDeleted, actual.IsSoftDeleted);
            Assert.Equal(expected.TestProp, actual.TestProp);
            Assert.Equal(expected.NonNullableOwnedEntity is null, actual.NonNullableOwnedEntity is null);
            if (expected.NonNullableOwnedEntity is not null && actual.NonNullableOwnedEntity is not null)
            {
                Assert.Equal(expected.NonNullableOwnedEntity.RequiredProp, actual.NonNullableOwnedEntity.RequiredProp);
                Assert.Equal(expected.NonNullableOwnedEntity.OwnedTestProp, actual.NonNullableOwnedEntity.OwnedTestProp);
            }
            Assert.Equal(expected.NullableOwnedEntity is null, actual.NullableOwnedEntity is null);
            if (expected.NullableOwnedEntity is not null && actual.NullableOwnedEntity is not null)
            {
                Assert.Equal(expected.NullableOwnedEntity.RequiredProp, actual.NullableOwnedEntity.RequiredProp);
                Assert.Equal(expected.NullableOwnedEntity.OwnedTestProp, actual.NullableOwnedEntity.OwnedTestProp);
            }
            Assert.Equal(expected.CollectionOfOwnedEntities is null, actual.CollectionOfOwnedEntities is null);
            if (expected.CollectionOfOwnedEntities is not null && actual.CollectionOfOwnedEntities is not null)
            {
                var expectedChildrenOwned = expected.CollectionOfOwnedEntities.OrderBy(c => c.Id).ToList();
                var actualChildrenOwned = actual.CollectionOfOwnedEntities.OrderBy(c => c.Id).ToList();
                Assert.Equal(expectedChildrenOwned.Count, actualChildrenOwned.Count);
                for (var i = 0; i < expectedChildrenOwned.Count; i++)
                {
                    var expectedChild = expectedChildrenOwned[i];
                    var actualChild = actualChildrenOwned[i];
                    Assert.Equal(expectedChild is null, actualChild is null);
                    if (expectedChild is not null && actualChild is not null)
                    {
                        Assert.Equal(expectedChild.Id, actualChild.Id);
                        Assert.Equal(expectedChild.TestProp, actualChild.TestProp);
                    }
                }
            }

            // In case new properties gets added and we forget to test them individually, we also compare the serialized objects.
            Assert.Equal(JsonSerializer.Serialize(expected), JsonSerializer.Serialize(actual));
        }
    }
}
