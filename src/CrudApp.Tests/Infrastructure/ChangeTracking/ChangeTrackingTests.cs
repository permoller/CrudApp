﻿using CrudApp.Infrastructure.ChangeTracking;
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
        var client = Fixture.CreateHttpClient();

        var entity = new InfrastructureTestEntity(new InfrastructureTestOwnedEntity() { OwnedTestProp = "original ref entity" }) { TestProp = "original entity" };
        var id = await client.PostEntityAsync(entity);
        var changeFilter = new FilteringParams() { Filter = $"{nameof(EntityChangeDto.EntityChange)}.{nameof(EntityChange.EntityId)} EQ {id}" };
        var changes = await client.Query<EntityChangeDto>(changeFilter);


        entity = await client.GetEntityAsync<InfrastructureTestEntity>(id);
        entity.TestProp = "updated entity";
        await client.PutEntityAsync(entity);
        changes = await client.Query<EntityChangeDto>(changeFilter);


        entity = await client.GetEntityAsync<InfrastructureTestEntity>(id);
        entity.NonNullableOwned.OwnedTestProp = "updated ref entity";
        await client.PutEntityAsync(entity);
        changes = await client.Query<EntityChangeDto>(changeFilter);


    }
}
