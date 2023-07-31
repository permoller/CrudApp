using Microsoft.EntityFrameworkCore;

namespace CrudApp.Infrastructure.Entity;

public static class EntityVersionUpdater
{
    public static void UpdateVersionOfModifiedEntities(CrudAppDbContext dbContext)
    {
        // update the version number of all modified entities
        foreach (var entry in dbContext.ChangeTracker.Entries<EntityBase>().Where(e => e.State == EntityState.Modified))
            entry.Entity.Version += 1;
    }
}
