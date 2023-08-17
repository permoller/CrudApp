using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CrudApp.Infrastructure.Entities;

public static class EntityVersionUpdater
{
    public static void UpdateVersionOfModifiedEntities(CrudAppDbContext dbContext)
    {
        // update the version number of all modified entities
        foreach (var entry in dbContext.ChangeTracker.Entries<EntityBase>().Where(e => IsChangedRecursive(dbContext, e)))
            entry.Entity.Version += 1;
    }

    private static bool IsChangedRecursive(CrudAppDbContext dbContext, EntityEntry entry, List<EntityEntry>? visited = null)
    {
        if (entry is null)
            return false;

        if (visited is null)
            visited = new List<EntityEntry>();

        if (visited.Contains(entry))
            return false;

        visited.Add(entry);

        if (entry.State == EntityState.Added || entry.State == EntityState.Deleted || entry.State == EntityState.Modified)
            return true;

        foreach (var nav in entry.Navigations)
        {
            var isOwned = nav.Metadata.TargetEntityType.IsInOwnershipPath(entry.Metadata);
            if (!isOwned)
                continue;

            if (nav.CurrentValue is null)
                continue;

            var ownedEntityEntry = dbContext.Entry(nav.CurrentValue);

            if (IsChangedRecursive(dbContext, ownedEntityEntry, visited))
                return true;
        }

        return false;
    }
}
