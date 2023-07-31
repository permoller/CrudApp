using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Diagnostics;
using System.Reflection;

namespace CrudApp.Infrastructure.ChangeTracking;

/// <summary>
/// Adds change tracking entities that will also be saved to the database.
/// </summary>
public static class ChangeTracker
{
    public static void AddChangeEntities(CrudAppDbContext dbContext)
    {
        foreach (var entry in dbContext.ChangeTracker.Entries<EntityBase>().Where(IsChangeTrackingEnabled).ToList())
        {
            // get the change type
            ChangeType? changeType = entry.State switch
            {
                EntityState.Added => ChangeType.EntityCreated,
                EntityState.Modified => ChangeType.EntityUpdated,
                EntityState.Deleted => ChangeType.EntityDeleted,
                EntityState.Detached => null,
                EntityState.Unchanged => null,
                _ => throw new NotSupportedException($"Entity state '{entry.State}' not supported.")
            };

            // skip unchanged entities
            if (changeType is null)
                continue;

            // create entity change event
            var time = DateTimeOffset.UtcNow;
            var activityId = Activity.Current?.Id;
            var entityChange = new EntityChange
            {
                Time = time,
                ActivityId = activityId,
                ChangeType = changeType.Value,
                EntityType = entry.Entity.GetType().Name,
                EntityId = entry.Entity.Id,
                UserId = AuthenticationContext.Current?.User.Id
            };
            entityChange.PropertyChanges = new List<PropertyChange>();
            dbContext.Add(entityChange);
            
            // create property change event for each changed property
            foreach (var prop in entry.Properties.Where(p => IsChangeTrackingEnabled(p) && (p.IsModified || changeType != ChangeType.EntityUpdated)))
            {
                var propertyChange = new PropertyChange
                {
                    EntityChangeId = entityChange.Id,
                    EntityChange = entityChange,
                    PropertyName = prop.Metadata.Name,
                    OldPropertyValue = changeType == ChangeType.EntityCreated ? null : prop.OriginalValue,
                    NewPropertyValue = changeType == ChangeType.EntityDeleted ? null : prop.CurrentValue
                };
                entityChange.PropertyChanges.Add(propertyChange);
                dbContext.Add(propertyChange);
            }
        }
    }


    private static bool IsChangeTrackingEnabled(PropertyEntry p) =>
        p.Metadata.PropertyInfo != null && p.Metadata.PropertyInfo.GetCustomAttribute<SkipChangeTrackingAttribute>() is null;

    private static bool IsChangeTrackingEnabled(EntityEntry e) =>
    e.Metadata.ClrType is not null && e.Metadata.ClrType.GetCustomAttribute<SkipChangeTrackingAttribute>() is null;
}
