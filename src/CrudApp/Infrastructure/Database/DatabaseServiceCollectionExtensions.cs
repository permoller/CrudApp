using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CrudApp.Infrastructure.Database;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddCrudAppDbContext(this IServiceCollection services)
    {
        services.AddDbContext<CrudAppDbContext>(dbContextOptionsBuilder =>
        {
            dbContextOptionsBuilder.UseSqlite(new SqliteConnection("DataSource=CrudApp.db"));
#if DEBUG
            dbContextOptionsBuilder.EnableSensitiveDataLogging(true);
#endif
        });
        return services;
    }
}
