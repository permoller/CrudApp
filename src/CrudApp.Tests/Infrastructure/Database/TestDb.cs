using CrudApp.Infrastructure.Database;

namespace CrudApp.Tests.Infrastructure.Database;
public abstract class TestDb : IAsyncDisposable
{
    private static int _dbCounter = 0;

    public static async Task<TestDb> CreateAsync(DatabaseType dbType)
    {
        var dbName = "test_db_" + Interlocked.Increment(ref _dbCounter);
        TestDb testDb = dbType switch
        {
            DatabaseType.Sqlite => await SqliteTestDb.CreateAsync(dbName),
            DatabaseType.MySql => await MySqlTestDb.CreateAsync(dbName),
            DatabaseType.MsSql => await MsSqlTestDb.CreateAsync(dbName),
            DatabaseType.PostgreSql => await PostgreSqlTestDb.CreateAsync(dbName),
            _ => throw new NotSupportedException($"{nameof(DatabaseType)} {dbType} is not supported.")
        };

        return testDb;
    }

    protected TestDb(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; }
    public abstract ValueTask DisposeAsync();
}
