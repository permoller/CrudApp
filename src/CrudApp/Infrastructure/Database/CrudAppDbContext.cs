using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using CrudApp.Infrastructure.ChangeTracking;

namespace CrudApp.Infrastructure.Database;

public class CrudAppDbContext : DbContext
{
    public CrudAppDbContext(DbContextOptions<CrudAppDbContext> options) : base(options)
    {
        SavingChanges += OnSavingChanges;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Add entity types
        foreach (var entityType in GetEntityBaseTypes())
        {
            modelBuilder
                .Entity(entityType)
                .HasMany(nameof(EntityBase.EntityChangeEvents))
                .WithOne()
                .HasForeignKey(nameof(EntityChange.EntityId));
        }
        // Add converters
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var entityTypeBuilder = modelBuilder.Entity(entityType.Name);
            foreach (var propertyInfo in entityType.ClrType.GetProperties())
            {


                if (propertyInfo.HasAttribute<JsonValueConverterAttribute>())
                    entityTypeBuilder.Property(propertyInfo.Name).HasConversion(JsonValueConverterAttribute.GetConverter(propertyInfo.PropertyType));


                if (propertyInfo.HasAttribute<EnumValueConverterAttribute>())
                    entityTypeBuilder.Property(propertyInfo.Name).HasConversion(EnumValueConverterAttribute.GetConverter(propertyInfo.PropertyType));


                // SQLite can not compare/order by DateTimeOffset.
                // https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
                // Use a converter to save the DateTimeOffset as a long.
                // Note that we loose some precision and comparing times with different offsets may not work as expected.
                if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite" &&
                    (propertyInfo.PropertyType == typeof(DateTimeOffset) || propertyInfo.PropertyType == typeof(DateTimeOffset?)))
                    entityTypeBuilder.Property(propertyInfo.Name).HasConversion(new DateTimeOffsetToBinaryConverter());
            }
        }
    }

    private static IEnumerable<Type> GetEntityBaseTypes() =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
        a.GetTypes().Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(EntityBase))));

    private static void OnSavingChanges(object? sender, SavingChangesEventArgs _)
    {
        ArgumentNullException.ThrowIfNull(sender);
        var dbContext = (CrudAppDbContext)sender;

        // update the version number of all modified entities
        foreach (var entry in dbContext.ChangeTracker.Entries<EntityBase>().Where(e => e.State == EntityState.Modified))
            entry.Entity.Version += 1;

        // Add change tracking events that will also be saved to the database.
        ChangeDetector.AddChangeRecords(dbContext);

    }

    public IQueryable<T> All<T>(bool includeSoftDeleted = false) where T : EntityBase
    {
        IQueryable<T> query = Set<T>();
        if (!includeSoftDeleted)
            query = query.Where(t => !t.IsSoftDeleted);
        return query;
    }

    public async Task EnsureCreatedAsync()
    {
        await Database.OpenConnectionAsync();
        await Database.EnsureCreatedAsync();
    }
}
