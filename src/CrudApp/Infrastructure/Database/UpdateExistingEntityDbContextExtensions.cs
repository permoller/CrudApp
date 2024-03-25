using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CrudApp.Infrastructure.Database;

public static class UpdateExistingEntityDbContextExtensions
{
    /// <summary>
    /// Updates <paramref name="existingEntity"/> with the values from <paramref name="newEntity"/>.
    /// It also recursivly updates the loaded navigation properties to owned entities. They must be loaded.
    /// Non-owned navigation properties are not updated and are not required to be loaded.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbContext"></param>
    /// <param name="existingEntity">The entity to update. This must already be tracked by <paramref name="dbContext"/> (loaded from the database using <paramref name="dbContext"/>).</param>
    /// <param name="newEntity">The entity with the new values. This should not be trackked by <paramref name="dbContext"/> (should not have been loaded from the database using <paramref name="dbContext"/>).</param>
    public static bool UpdateExistingEntity<T>(this DbContext dbContext, T existingEntity, T newEntity) where T : class
    {
        var modified = SetValuesRecursively(dbContext, existingEntity, newEntity, new());
        if (modified)
        {
            var entry = dbContext.Entry(existingEntity);
            // Not all changes (like removing an entity from a collection) requires Entity Framework to update the owner entity.
            // But we want it to be marked as changed so we can detect it later and update the version-property.
            if (entry.State == EntityState.Unchanged)
                entry.State = EntityState.Modified;
        }
        return modified;
    }

    private static bool SetValuesRecursively(DbContext dbContext, object existingEntity, object newEntity, HashSet<EntityEntry> visited)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(existingEntity);
        ArgumentNullException.ThrowIfNull(newEntity);
        ArgumentNullException.ThrowIfNull(visited);

        var modified = false;
        var entry = dbContext.Entry(existingEntity);
        if (!visited.Add(entry))
            return modified;

        foreach (var memberEntry in entry.Members)
        {
            try
            {
                var propInfo = memberEntry.Metadata.PropertyInfo;
                if (propInfo is null || !propInfo.CanRead || !propInfo.CanWrite)
                {
                    continue;
                }
                var existingValue = propInfo.GetValue(existingEntity);
                var newValue = propInfo.GetValue(newEntity);
                if (Equals(existingValue, newValue))
                {
                    continue;
                }

                // Non-navigation property (points to a non-entity type)
                if (memberEntry is PropertyEntry propertyEntry)
                {
                    propertyEntry.CurrentValue = newValue;
                    modified = true;
                }
                // Reference-navigation property (points to an entity)
                else if (memberEntry is ReferenceEntry referenceEntry)
                {
                    if (!IsNavigationTargetOwnedByEntity(referenceEntry, entry))
                        continue;

                    if (!referenceEntry.IsLoaded)
                        throw new ArgumentException($"Property {propInfo.DeclaringType?.Name}.{propInfo.Name} is not loaded.");

                    if (existingValue is null || newValue is null)
                    {
                        referenceEntry.CurrentValue = newValue;
                        modified = true;
                    }
                    else
                    {
                        modified |= SetValuesRecursively(dbContext, existingValue, newValue, visited);
                    }
                }
                // Collection-navigation property (points to a collection of entities)
                else if (memberEntry is CollectionEntry collectionEntry)
                {
                    if (!IsNavigationTargetOwnedByEntity(collectionEntry, entry))
                        continue;

                    if (!collectionEntry.IsLoaded)
                        throw new ArgumentException($"Property {propInfo.DeclaringType?.Name}.{propInfo.Name} is not loaded.");

                    var existingChildCollection = (IEnumerable<object>)existingValue!;
                    var newChildCollection = (IEnumerable<object>?)newValue ?? Array.Empty<object>();
                    var primaryKeyComparer = new EntityPrimayKeyComparer(collectionEntry.Metadata.TargetEntityType);

                    // Handle added and updated items
                    foreach (var newItem in newChildCollection)
                    {
                        var existingItem = primaryKeyComparer.HasDefaultKey(newItem)
                            ? null
                            : existingChildCollection.FirstOrDefault(e => primaryKeyComparer.Equals(e, newItem));
                        if (existingItem is null)
                        {
                            dbContext.Add(newItem);
                            existingChildCollection.GetType().InvokeMember("Add", BindingFlags.InvokeMethod, null, existingChildCollection, new[] { newItem });
                            modified = true;
                        }
                        else
                        {
                            modified |= SetValuesRecursively(dbContext, existingItem, newItem, visited);
                        }
                    }

                    // Handle removed items
                    foreach (var existingItem in existingChildCollection.ToList())
                    {
                        ArgumentNullException.ThrowIfNull(existingItem);
                        if (!newChildCollection.Contains(existingItem, primaryKeyComparer))
                        {
                            existingChildCollection.GetType().InvokeMember("Remove", BindingFlags.InvokeMethod, null, existingChildCollection, new[] { existingItem });
                            dbContext.Remove(existingItem);
                            modified = true;
                        }
                    }
                }
                else
                {
                    throw new NotSupportedException($"Entry type '{memberEntry.GetType().Name}' not supported.");
                }
            }
            catch (Exception ex)
            {
                ex.Data["MemberEntry"] = $"{entry.Metadata.ShortName()}.{memberEntry.Metadata.Name}";
                throw;
            }
        }
        return modified;
    }

    private static bool IsNavigationTargetOwnedByEntity(NavigationEntry navigationEntry, EntityEntry entityEntry) =>
        navigationEntry.Metadata.TargetEntityType.IsInOwnershipPath(entityEntry.Metadata);

    private sealed class EntityPrimayKeyComparer : IEqualityComparer<object?>
    {
        private readonly List<PropertyInfo> _primaryKeyProperties = new();

        private readonly object?[] _defaultKey;

        public EntityPrimayKeyComparer(IEntityType entityType)
        {
            var primaryKey = entityType.FindPrimaryKey();
            ArgumentNullException.ThrowIfNull(primaryKey);

            foreach (var metadataProp in primaryKey.Properties)
            {
                var clrProp = metadataProp.PropertyInfo;
                if (clrProp is null)
                    throw new NotSupportedException($"PropertyInfo is null for property {metadataProp.Name} on entity {entityType.Name}. Shadow properties are not supported.");

                _primaryKeyProperties.Add(clrProp);
            }

            _defaultKey = _primaryKeyProperties.Select(p => p.PropertyType.GetDefault()).ToArray();
        }

        public object?[] GetKey(object? o)
        {
            return _primaryKeyProperties.Select(p => o is null ? null : p.GetValue(o)).ToArray();
        }

        public bool HasDefaultKey(object? o)
        {
            return _defaultKey.SequenceEqual(GetKey(o));
        }

        public new bool Equals(object? x, object? y)
        {
            return GetKey(x).SequenceEqual(GetKey(y));
        }

        public int GetHashCode(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
