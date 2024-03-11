using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CrudApp.Infrastructure.Entities;

public static class EntityVersionUpdater
{
    public static void UpdateVersionOfModifiedEntities(CrudAppDbContext dbContext)
    {
        // update the version number of all modified entities
        foreach (var entry in dbContext.ChangeTracker.Entries<EntityBase>().Where(e => IsChangedRecursive(dbContext, e.Entity, new())))
            entry.Entity.Version = entry.Property(e => e.Version).OriginalValue + 1;
    }

    private static bool IsChangedRecursive(CrudAppDbContext dbContext, object? entity, HashSet<EntityEntry> visited)
    {
        if (entity is null)
            return false;

        var entityEntry = dbContext.Entry(entity);

        if (!visited.Add(entityEntry))
            return false;

        if (entityEntry.State == EntityState.Added || entityEntry.State == EntityState.Deleted || entityEntry.State == EntityState.Modified)
            return true;

        foreach (var nav in entityEntry.Navigations)
        {
            if (!nav.IsLoaded)
                continue;

            var isReferencedEntitiesOwned = nav.Metadata.TargetEntityType.IsInOwnershipPath(entityEntry.Metadata);

            if (nav is CollectionEntry collectionEntry)
            {
                var currentCollection = collectionEntry.CurrentValue;

                // TODO: Figure out how to detect if an entity was removed

                //var originalCollection = ???

                //if (originalCollection.Except(currentCollection).Any())
                //    return true;

                //if (currentCollection.Except(originalCollection).Any())
                //    return true;

                if (currentCollection is not null && isReferencedEntitiesOwned)
                {
                    foreach (var currentEntity in currentCollection)
                    {
                        if (IsChangedRecursive(dbContext, currentEntity, visited))
                            return true;
                    }
                }
            }
            else if (nav is ReferenceEntry)
            {
                var currentEntity = nav.CurrentValue;

                // TODO: Figure out how to detect if an entity was removed

                //var originalEntity = ???
                //if (originalEntity is null && currentEntity is null)
                //    continue;

                //if (originalEntity is null || currentEntity is null)
                //    return true;

                //if (originalEntity != currentEntity)
                //    return true;

                if (isReferencedEntitiesOwned && IsChangedRecursive(dbContext, currentEntity, visited))
                    return true;
            }
        }

        return false;
    }
}
