using CrudApp.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;

namespace CrudApp.Infrastructure.Authorization;

public static class AuthorizationCleanup
{
    public static void DeleteRelationsToDeletedEntities(CrudAppDbContext dbContext)
    {
        foreach(var entry in dbContext.ChangeTracker.Entries<User>().Where(e => e.State == EntityState.Deleted).ToList())
        {
            var userRelations = dbContext.All<AuthorizationGroupUserRelation>().Where(m => m.UserId == entry.Entity.Id).ToList();
            dbContext.RemoveRange(userRelations);
        }

        foreach (var entry in dbContext.ChangeTracker.Entries<EntityBase>().Where(e => e.State == EntityState.Deleted).ToList())
        {
            var entityRelations = dbContext.All<AuthorizationGroupEntityRelation>().Where(m => m.EntityId == entry.Entity.Id).ToList();
            dbContext.RemoveRange(entityRelations);
        }
    }
}
