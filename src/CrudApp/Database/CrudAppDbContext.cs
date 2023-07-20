using CrudApp.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CrudApp.Database;

public class CrudAppDbContext : DbContext
{
    public CrudAppDbContext(DbContextOptions<CrudAppDbContext> options) : base(options)
    {
        SavingChanges += OnSavingChanges;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Add entity types
        foreach (var entityType in GetEntityTypes())
            modelBuilder.Entity(entityType);

        // Add converters
        foreach(var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var entityTypeBuilder = modelBuilder.Entity(entityType.Name);

            foreach (var propertyInfo in entityType.ClrType.GetProperties())
            {
                var propertyBuilder = entityTypeBuilder.Property(propertyInfo.Name);

                
                if (propertyInfo.HasAttribute<JsonValueConverterAttribute>())
                    propertyBuilder.HasConversion(JsonValueConverterAttribute.GetConverter(propertyInfo.PropertyType));


                if (propertyInfo.HasAttribute<EnumValueConverterAttribute>())
                    propertyBuilder.HasConversion(EnumValueConverterAttribute.GetConverter(propertyInfo.PropertyType));


                // SQLite can not compare/order by DateTimeOffset.
                // https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
                // Use a converter to save the DateTimeOffset as a long.
                // Note that we loose some precision and comparing times with different offsets may not work as expected.
                if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite" &&
                    (propertyInfo.PropertyType == typeof(DateTimeOffset) || propertyInfo.PropertyType == typeof(DateTimeOffset?)))
                    propertyBuilder.HasConversion(new DateTimeOffsetToBinaryConverter());
            }
        }
    }

    private static IEnumerable<Type> GetEntityTypes() => 
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
        ChangeEventTracker.AddChangeEvents(dbContext);

    }

    public async Task EnsureCreatedAsync()
    {
        await Database.OpenConnectionAsync();
        await Database.EnsureCreatedAsync();
    }
}
