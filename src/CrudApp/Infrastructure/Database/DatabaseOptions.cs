using System.Diagnostics.CodeAnalysis;

namespace CrudApp.Infrastructure.Database;

public class DatabaseOptions
{
    [NotNull]
    public string ConnectionString { get; set; } = default!;
    public bool EnableDetailedErrors { get; set; }
    public bool EnableSensitiveDataLogging { get; set; }
}
