using CrudApp.Infrastructure.Database;

namespace CrudApp.Tests.TestDatabases;
public interface ITestDb
{
    DatabaseType DbType { get; }
    string ConnectionString { get; }
    Task InitializeAsync();
    Task DisposeAsync();
}
