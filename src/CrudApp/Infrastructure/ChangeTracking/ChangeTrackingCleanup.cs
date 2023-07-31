using Microsoft.EntityFrameworkCore;

namespace CrudApp.Infrastructure.ChangeTracking;

public static class ChangeTrackingCleanup
{
    public static void DeleteChangeEntitiesForDeletedEntities(CrudAppDbContext dbContext)
    {
        foreach (var entry in dbContext.ChangeTracker.Entries<EntityBase>().Where(e => e.State == EntityState.Deleted).ToList())
        {
            var entityChanges = dbContext.All<EntityChange>().Where(m => m.EntityId == entry.Entity.Id).ToList();
            dbContext.RemoveRange(entityChanges);
        }
    }
}
