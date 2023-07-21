using CrudApp.ChangeTracking;
using CrudApp.Database;
using CrudApp.SuperHeroes;
using Microsoft.Extensions.DependencyInjection;

namespace CrudApp.Tests;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        var fixture = await WebAppFixture.CreateAsync();

        using var scope = fixture.WebAppFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CrudAppDbContext>();


        Assert.Empty(db.Set<EntityChangeEvent>());
        var batman = new SuperHero { SuperHeroName = "Batman" };
        db.Set<SuperHero>().Add(batman);
        await db.SaveChangesAsync();

        Assert.Collection(
            db.Set<EntityChangeEvent>(),
            auditEvent =>
            {
                Assert.Equal(ChangeType.EntityCreated, auditEvent.ChangeType);
                Assert.Equal(batman.Id, auditEvent.EntityId);
            });

        batman.SuperHeroName = "Bruce Wayne";
        await db.SaveChangesAsync();

        //Assert.Collection(
        //    db.Set<EntityChangeEvent>(),
        //    auditEvent =>
        //    {
        //        Assert.Equal(ChangeType.EntityCreated, auditEvent.ChangeType);
        //        Assert.Equal(batman.Id, auditEvent.EntityId);
        //        Assert.Empty(auditEvent.Properties);
        //    },
        //    auditEvent =>
        //    {
        //        Assert.Equal(ChangeType.EntityUpdated, auditEvent.ChangeType);
        //        Assert.Equal(batman.Id, auditEvent.EntityId);
        //        Assert.Collection(
        //            auditEvent.Properties,
        //            p =>
        //            {
        //                Assert.Equal(nameof(EntityBase.DisplayName), p.Name);
        //                Assert.Equal("Batman", p.OldValue);
        //                Assert.Equal("Bruce Wayne", p.NewValue);
        //            });
        //    });
    }

    
}
