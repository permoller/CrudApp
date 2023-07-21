using CrudApp.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Diagnostics;
using System.Reflection;

namespace CrudApp.ChangeTracking;

public static class ChangeEventTracker
{
    public static void AddChangeEvents(CrudAppDbContext db)
    {
        var entityChangeEvents = new List<EntityChangeEvent>();

        foreach (var entry in db.ChangeTracker.Entries<EntityBase>().Where(IsChangeTrackingEnabled))
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
            var activityId = Activity.Current?.Id ?? "NO_ACTIVITY:" + Guid.NewGuid().ToString();
            var entityChangeEvent = new EntityChangeEvent
            {
                Time = time,
                ActivityId = activityId,
                ChangeType = changeType.Value,
                EntityType = entry.Entity.GetType().Name,
                EntityId = entry.Entity.Id,
                AuthPrincipalId = AuthorizationContext.Current?.User.Id
            };
            entityChangeEvents.Add(entityChangeEvent);

            // create property change event for each changed property
            foreach (var prop in entry.Properties.Where(p => IsChangeTrackingEnabled(p) && (p.IsModified || changeType != ChangeType.EntityUpdated)))
            {
                var propertyChangeEvent = new PropertyChangeEvent
                {
                    EntityChangeEventId = entityChangeEvent.Id,
                    PropertyName = prop.Metadata.Name,
                    OldPropertyValue = changeType == ChangeType.EntityCreated ? null : prop.OriginalValue,
                    NewPropertyValue = changeType == ChangeType.EntityDeleted ? null : prop.CurrentValue
                };
                entityChangeEvent.PropertyChangeEvents.Add(propertyChangeEvent);
            }
        }
        db.AddRange(entityChangeEvents);
    }

    private static bool IsChangeTrackingEnabled(PropertyEntry p) =>
        p.Metadata.PropertyInfo != null && p.Metadata.PropertyInfo.GetCustomAttribute<SkipChangeTrackingAttribute>() is null;
    private static bool IsChangeTrackingEnabled(EntityEntry e) =>
        e.Metadata.ClrType is not null && e.Metadata.ClrType.GetCustomAttribute<SkipChangeTrackingAttribute>() is null;
}
