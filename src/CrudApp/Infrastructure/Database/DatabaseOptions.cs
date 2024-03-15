using System.Diagnostics.CodeAnalysis;

namespace CrudApp.Infrastructure.Database;

public enum DatabaseType { NotSet, Sqlite, Postgres, MsSql, MySql }

public class DatabaseOptions
{
    [NotNull]
    public DatabaseType DbType { get; set; }

    [NotNull]
    public string ConnectionString { get; set; } = default!;
    public bool EnableDetailedErrors { get; set; }
    public bool EnableSensitiveDataLogging { get; set; }
}
