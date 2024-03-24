using CrudApp.Infrastructure.ChangeTracking;
using CrudApp.Infrastructure.Query;
using CrudApp.Infrastructure.Testing;
using CrudApp.Tests.Infrastructure.Entities;
using CrudApp.Tests.Infrastructure.Query;
using Xunit.Abstractions;

namespace CrudApp.Tests.Infrastructure.ChangeTracking;
public class ChangeTrackingTests : IntegrationTestsBase, IClassFixture<WebAppFixture>
{
    public ChangeTrackingTests(ITestOutputHelper testOutputHelper, WebAppFixture webAppFixture) : base(testOutputHelper, webAppFixture) { }

    [Fact]
    public async Task Test()
    {
        // TODO: Test ChangeTracking... it is probably not working.
        var client = Fixture.CreateHttpClient();

        var entity = new InfrastructureTestEntity(new InfrastructureTestOwnedEntity() { OwnedTestProp = "original ref entity" }) { TestProp = "original entity" };
        var id = await client.CreateEntityAsync(entity);
        var changeFilter = new FilteringParams() { Filter = $"{nameof(EntityChangeDto.EntityChange)}.{nameof(EntityChange.EntityId)} EQ {id}" };
        var changes = await client.Query<EntityChangeDto>(changeFilter);


        entity = await client.GetEntityAsync<InfrastructureTestEntity>(id);
        entity.TestProp = "updated entity";
        await client.UpdateEntityAsync(entity);
        changes = await client.Query<EntityChangeDto>(changeFilter);


        entity = await client.GetEntityAsync<InfrastructureTestEntity>(id);
        entity.NonNullableOwnedEntity.OwnedTestProp = "updated ref entity";
        await client.UpdateEntityAsync(entity);
        changes = await client.Query<EntityChangeDto>(changeFilter);


    }
}
