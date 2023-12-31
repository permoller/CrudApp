﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using CrudApp.Infrastructure.Users;
using CrudApp.Infrastructure.ChangeTracking;
using CrudApp.Infrastructure.UtilityCode;

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
}
