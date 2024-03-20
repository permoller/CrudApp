using CrudApp.Infrastructure.Testing;
using CrudApp.Infrastructure.UtilityCode;
using CrudApp.Tests.Infrastructure.Http;
using System.Net.Http.Json;
using Xunit.Abstractions;
using CrudApp.Infrastructure.Primitives;

namespace CrudApp.Tests.Infrastructure.Entities;

// TODO: Test adding, updating and removing entities from non-owned collection and non-owned properties. These updates are intentionally ignored.
public class EntityControllerBaseTests : IntegrationTestsBase, IClassFixture<WebAppFixture>
{
    HttpClient _client;
    InfrastructureTestEntity _entity;

    public EntityControllerBaseTests(ITestOutputHelper testOutputHelper, WebAppFixture fixture) : base(testOutputHelper, fixture)
    {
        // Non-nullable members are set in InitializeAsync()
        _client = null!;
        _entity = null!;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _client = Fixture.CreateHttpClient(Fixture.RootUserId);
        _entity = await CreateEntity();
    }

    public override async Task DisposeAsync()
    {
        await DeleteEntity();
        await base.DisposeAsync();
    }

    private async Task<InfrastructureTestEntity> CreateEntity()
    {
        var nonNullableOwned = new InfrastructureTestOwnedEntity() { OwnedTestProp = "original OwnedTestProp" };
        var entity = new InfrastructureTestEntity(nonNullableOwned) { TestProp = "original TestProp" };
        
        var id = await _client.PostEntityAsync(entity);

        Assert.NotEqual(default, id);
        var actual = await _client.GetEntityAsync<InfrastructureTestEntity>(id);
        entity.Id = id;
        entity.Version = 1;
        AssertEqual(entity, actual);

        return entity;
    }

    private async Task DeleteEntity()
    {
        if (!_entity.IsSoftDeleted)
            await _client.DeleteEntityAsync<InfrastructureTestEntity>(_entity.Id);
    }

    [Fact]
    public async Task RecreateEntityWithSameIdShouldFail()
    {
        var ex = await Assert.ThrowsAsync<ProblemDetailsApiException>(async () => await _client.PostEntityAsync(_entity));
        Assert.Equal(HttpStatus.BadRequest, (int?)ex.StatusCode);
        Assert.Contains($"{_entity.DisplayName} already exists with the same id {_entity.Id}.", ex.Message);
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
        var dbVersion = _entity.Version;
        _entity.Version--;
        await UpdateEntityAndAssertFailure(HttpStatus.Conflict, $"Version in request: {_entity.Version}. Version in database: {dbVersion}.");
    }

    [Fact]
    public async Task UpdateNonNullableOwnedEntity()
    {
        _entity.NonNullableOwnedEntity.OwnedTestProp = "updated OwnedTestProp";
        var actual = await _client.PutAndGetEntity(_entity);
        _entity.Version++;
        AssertEqual(_entity, actual);

        _entity.NonNullableOwnedEntity = null!;
        await UpdateEntityAndAssertFailure(HttpStatus.BadRequest, $"The {nameof(_entity.NonNullableOwnedEntity)} field is required.");
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
        await _client.DeleteEntityAsync<InfrastructureTestEntity>(_entity.Id);

        var ex = await Assert.ThrowsAsync<ProblemDetailsApiException>(async () => await _client.GetEntityAsync<InfrastructureTestEntity>(_entity.Id));
        Assert.Equal(HttpStatus.NotFound, (int?)ex.StatusCode);
        Assert.Contains($"{_entity.DisplayName} has been deleted.", ex.Message);

        ex = await Assert.ThrowsAsync<ProblemDetailsApiException>(async () => await _client.PutEntityAsync(_entity));
        Assert.Equal(HttpStatus.NotFound, (int?)ex.StatusCode);
        Assert.Contains($"{_entity.DisplayName} has been deleted.", ex.Message);

        ex = await Assert.ThrowsAsync<ProblemDetailsApiException>(async () => await _client.DeleteEntityAsync<InfrastructureTestEntity>(_entity.Id));
        Assert.Equal(HttpStatus.NotFound, (int?)ex.StatusCode);
        Assert.Contains($"{_entity.DisplayName} has been deleted.", ex.Message);

        var actual = await _client.GetEntityAsync<InfrastructureTestEntity>(_entity.Id, includeSoftDeleted: true);
        _entity.Version++;
        _entity.IsSoftDeleted = true;
        AssertEqual(_entity, actual);
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
            await _client.DeleteEntityAsync<InfrastructureTestEntity>(id);
        }
    }


    private async Task<ProblemDetailsApiException> UpdateEntityAndAssertFailure(int expectedHttpStatus, string expectedMessage)
    {
        var ex = await Assert.ThrowsAsync<ProblemDetailsApiException>(() => _client.PutEntityAsync(_entity));
        Assert.Equal(expectedHttpStatus, (int?)ex.StatusCode);
        Assert.Contains(expectedMessage, ex.Message);
        return ex;
    }

    private static void AssertEqual(InfrastructureTestEntity expected, InfrastructureTestEntity actual)
    {
        Assert.Equal(expected is null, actual is null);
        if (expected is not null && actual is not null)
        {
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
        }
    }
}
