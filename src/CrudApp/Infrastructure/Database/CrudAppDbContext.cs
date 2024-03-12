using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using CrudApp.Infrastructure.Users;
using CrudApp.Infrastructure.ChangeTracking;
using CrudApp.Infrastructure.UtilityCode;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CrudApp.Infrastructure.Database;

public class CrudAppDbContext : DbContext
{
    public CrudAppDbContext(DbContextOptions<CrudAppDbContext> options) : base(options)
    {
        SavingChanges += OnSavingChanges;
    }

    private void OnSavingChanges(object? sender, SavingChangesEventArgs e)
    {
        EntityVersionUpdater.UpdateVersionOfModifiedEntities(this);
        ChangeTracking.ChangeTracker.AddChangeEntities(this);
        AuthorizationCleanup.DeleteRelationsToDeletedEntities(this);
        ChangeTrackingCleanup.DeleteChangeEntitiesForDeletedEntities(this);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Add entity types
        foreach (var entityType in typeof(EntityBase).GetSubclasses())
        {
            var entityTypeBuilder = modelBuilder.Entity(entityType);

            EntityBase.ConfigureEntityModel(entityTypeBuilder);
        }

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Add converters
            foreach (var propertyInfo in entityType.ClrType.GetProperties())
            {
                if (propertyInfo.HasAttribute<JsonValueConverterAttribute>())
                    entityType.GetProperty(propertyInfo.Name).SetValueConverter(JsonValueConverterAttribute.GetConverter(propertyInfo.PropertyType));

                if (propertyInfo.HasAttribute<JsonValueConverterAttribute>())
                    entityType.GetProperty(propertyInfo.Name).SetValueConverter(EnumValueConverterAttribute.GetConverter(propertyInfo.PropertyType));

                // SQLite can not compare/order by DateTimeOffset.
                // https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
                // Use a converter to save the DateTimeOffset as a long.
                // Note that we loose some precision and comparing times with different offsets may not work as expected.
                if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite" &&
                    (propertyInfo.PropertyType == typeof(DateTimeOffset) || propertyInfo.PropertyType == typeof(DateTimeOffset?)))
                    entityType.GetProperty(propertyInfo.Name).SetValueConverter(new DateTimeOffsetToBinaryConverter());    
            }

            // Auto-include non-nullable navigation properties
            foreach (var nav in entityType.GetNavigations())
            {
                if (nav.PropertyInfo is null)
                    continue;

                if (nav.PropertyInfo.MayPropertyBeNull() == false)
                    nav.SetIsEagerLoaded(true);
            }

        }
    }



    public IQueryable<T> All<T>(bool includeSoftDeleted = false) where T : EntityBase
    {
        IQueryable<T> query = Set<T>();
        if (!includeSoftDeleted)
            query = query.Where(t => !t.IsSoftDeleted);
        return query;
    }

    public IQueryable<EntityBase> All(Type entityType, bool includeSoftDeleted = false)
    {
        if (!entityType.IsSubclassOf(typeof(EntityBase)))
            throw new ArgumentException($"Entity type must be a subclass of {typeof(EntityBase)}.");

        var methodInfo = Array.Find(GetType().GetMethods(), m => m.Name == nameof(All) && m.IsGenericMethodDefinition);

        if (methodInfo is null)
            throw new InvalidOperationException($"Generic method {nameof(All)} not found on {GetType()}");

        var query = methodInfo.MakeGenericMethod(entityType).Invoke(this, new object[] { includeSoftDeleted });

        if (query is null)
            throw new InvalidOperationException($"Invoking method {methodInfo} returned null.");

        return (IQueryable<EntityBase>)query;
    }

    public async Task<EntityId?> EnsureCreatedAsync(CancellationToken cancellationToken)
    {
        await Database.OpenConnectionAsync(cancellationToken);
        var dbCreated = await Database.EnsureCreatedAsync(cancellationToken);
        if (dbCreated)
        {
            var user = new User();
            Add(user);
            await SaveChangesAsync(cancellationToken);
            return user.Id;
        }
        return default;

    }

    /// <summary>
    /// Updates <paramref name="existingEntity"/> with the values from <paramref name="newEntity"/>.
    /// It also recursivly updates the loaded navigation properties.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="existingEntity">The entity to update. This should be loaded from the database.</param>
    /// <param name="newEntity">The entity with the new values. This should not have been loaded from the database.</param>
    public void UpdateExisting<T>(T existingEntity, T newEntity) where T : EntityBase
    {
        var modified = SetValuesRecursively(existingEntity, newEntity, new());
        if (modified)
        {
            var entry = Entry(existingEntity);
            entry.DetectChanges();
            // Not all changes (like removing an entity from a collection) requires Entity Framework to update the owner entity.
            // But we want it to be marked as changed so we can detect it later and update the version-property.
            if (entry.State == EntityState.Unchanged)
                entry.State = EntityState.Modified;
        }
    }

    private bool SetValuesRecursively(object existingEntity, object newEntity, HashSet<EntityEntry> visited)
    {
        var modified = false;
        var entry = Entry(existingEntity);
        if (!visited.Add(entry))
            return modified;
        
        foreach(var memberEntry in entry.Members)
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
                if (memberEntry is PropertyEntry)
                {
                    propInfo.SetValue(existingEntity, newValue);
                    modified = true;
                }
                // Navigation property (points to an entity)
                else if (memberEntry is ReferenceEntry referenceEntry)
                {
                    if (!referenceEntry.IsLoaded || !IsNavigationTargetOwnedByEntity(referenceEntry, entry))
                        continue;

                    if (existingValue is null || newValue is null)
                    {
                        propInfo.SetValue(existingEntity, newValue);
                        modified = true;
                    }
                    else
                    {
                        modified |= SetValuesRecursively(existingValue, newValue, visited);
                    }
                }
                // Collection-navigation property (points to a collection of entities)
                else if (memberEntry is CollectionEntry collectionEntry)
                {
                    if (!collectionEntry.IsLoaded || !IsNavigationTargetOwnedByEntity(collectionEntry, entry))
                        continue;

                    var existingChildCollection = (IEnumerable<object>)existingValue!;
                    var newChildCollection = (IEnumerable<object>?)newValue ?? Array.Empty<object>();
                    var primaryKeyComparer = new EntityPrimayKeyComparer(collectionEntry.Metadata.TargetEntityType);

                    // Handle added and updated items
                    foreach (var newItem in newChildCollection)
                    {
                        var existingItem = existingChildCollection.FirstOrDefault(e => primaryKeyComparer.Equals(e, newItem));
                        if (existingItem is null)
                        {
                            Add(newItem);
                            existingChildCollection.GetType().InvokeMember("Add", BindingFlags.InvokeMethod, null, existingChildCollection, new[] { newItem });
                            modified = true;
                        }
                        else
                        {
                            modified |= SetValuesRecursively(existingItem, newItem, visited);
                        }
                    }

                    // Handle removed items
                    foreach (var existingItem in existingChildCollection.ToList())
                    {
                        ArgumentNullException.ThrowIfNull(existingItem);
                        if (!newChildCollection.Contains(existingItem, primaryKeyComparer))
                        {
                            
                            existingChildCollection.GetType().InvokeMember("Remove", BindingFlags.InvokeMethod, null, existingChildCollection, new[] { existingItem });
                            modified = true;
                            Remove(existingItem);
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
                ex.Data["NavigationEntry"] = $"{entry.Metadata.ShortName()}.{memberEntry.Metadata.Name}";
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

        public EntityPrimayKeyComparer(IEntityType entityType)
        {
            var primaryKey = entityType.FindPrimaryKey();
            ArgumentNullException.ThrowIfNull(primaryKey);

            foreach (var metadataProp in primaryKey.Properties)
            {
                var clrProp = metadataProp.PropertyInfo;
                if (clrProp is null)
                    throw new ErrorException($"PropertyInfo is null for property {metadataProp.Name} on entity {entityType.Name}. Shadow properties are not supported.");

                _primaryKeyProperties.Add(clrProp);
            }
        }

        private object?[] GetKey(object? o)
        {
            return _primaryKeyProperties.Select(p => o is null ? null : p.GetValue(o)).ToArray();
        }

        public new bool Equals(object? x, object? y)
        {
            return GetKey(x).SequenceEqual(GetKey(y));
        }

        public int GetHashCode(object obj)
        {
            var keyArray = GetKey(obj);
            int hash = keyArray.Length;
            foreach (object? keyItem in keyArray)
            {
                var itemHash = keyItem?.GetHashCode() ?? 0;
                hash = unchecked(hash * 31 + itemHash);
            }
            return hash;
        }
    }
}