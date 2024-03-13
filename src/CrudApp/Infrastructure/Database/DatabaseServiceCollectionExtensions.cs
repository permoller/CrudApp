using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CrudApp.Infrastructure.Database;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddCrudAppDbContext(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(nameof(DatabaseOptions)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContext<CrudAppDbContext>((serviceProvider, dbContextOptionsBuilder) =>
        {
            var dbOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            dbContextOptionsBuilder.UseSqlite(dbOptions.ConnectionString);
            dbContextOptionsBuilder.EnableDetailedErrors(dbOptions.EnableDetailedErrors);
            dbContextOptionsBuilder.EnableSensitiveDataLogging(dbOptions.EnableSensitiveDataLogging);
        });
        return services;
    }
}
