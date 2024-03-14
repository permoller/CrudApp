namespace CrudApp.Infrastructure.Database;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddCrudAppDbContext(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(nameof(DatabaseOptions)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContext<CrudAppDbContext>();
        return services;
    }
}
