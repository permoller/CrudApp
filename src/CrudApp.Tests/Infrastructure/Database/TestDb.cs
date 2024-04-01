using CrudApp.Infrastructure.Database;
using Microsoft.Extensions.Configuration;

namespace CrudApp.Tests.Infrastructure.Database;
public abstract class TestDb : IAsyncDisposable
{
    private static int _dbCounter = 0;

    public static async Task<TestDb> CreateAsync(IConfiguration config)
    {
        var dbName = "test_db_" + Interlocked.Increment(ref _dbCounter);
        var dbType = Enum.Parse<DatabaseType>(config[$"{nameof(DatabaseOptions)}:{nameof(DatabaseOptions.DbType)}"]!);
        TestDb testDb = dbType switch
        {
            DatabaseType.Sqlite => await SqliteTestDb.CreateAsync(dbName),
            DatabaseType.MySql => await MySqlTestDb.CreateAsync(dbName),
            DatabaseType.MsSql => await MsSqlTestDb.CreateAsync(dbName),
            DatabaseType.Postgres => await PostgresTestDb.CreateAsync(dbName),
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
