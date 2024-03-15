using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using CrudApp.Infrastructure.Users;
using CrudApp.Infrastructure.ChangeTracking;
using Microsoft.Extensions.Options;

namespace CrudApp.Infrastructure.Database;

public class CrudAppDbContext : DbContext
{
    private readonly DatabaseOptions _dbOptions;

    public CrudAppDbContext(IOptions<DatabaseOptions> options)
    {
        SavingChanges += OnSavingChanges;
        _dbOptions = options.Value;
    }

    private void OnSavingChanges(object? sender, SavingChangesEventArgs e)
    {
        EntityVersionUpdater.UpdateVersionOfModifiedEntities(this);
        ChangeTracking.ChangeTracker.AddChangeEntities(this);
        AuthorizationCleanup.DeleteRelationsToDeletedEntities(this);
        ChangeTrackingCleanup.DeleteChangeEntitiesForDeletedEntities(this);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        switch (_dbOptions.DbType)
        {
            case DatabaseType.Sqlite:
                optionsBuilder.UseSqlite(_dbOptions.ConnectionString);
                break;
            case DatabaseType.Postgres:
                optionsBuilder.UseNpgsql(_dbOptions.ConnectionString);
                break;
            case DatabaseType.MsSql:
                optionsBuilder.UseSqlServer(_dbOptions.ConnectionString);
                break;
            case DatabaseType.MySql:
                optionsBuilder.UseMySql(_dbOptions.ConnectionString, ServerVersion.AutoDetect(_dbOptions.ConnectionString));
                break;
            default:
                throw new NotSupportedException($"DB Provider {_dbOptions.DbType} not supported.");
        }

        optionsBuilder.EnableDetailedErrors(_dbOptions.EnableDetailedErrors);
        optionsBuilder.EnableSensitiveDataLogging(_dbOptions.EnableSensitiveDataLogging);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Add entity types
        foreach (var entityType in typeof(EntityBase).GetSubclassesInApplication())
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

                if (propertyInfo.HasAttribute<EnumValueConverterAttribute>())
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

    public async Task<EntityId?> EnsureDatabaseCreatedAsync(CancellationToken cancellationToken)
    {
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