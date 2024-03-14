using CrudApp.Infrastructure.Database;
using Microsoft.Data.Sqlite;

namespace CrudApp.Tests.TestDatabases;

internal class SqliteTestDb : ITestDb
{
    private readonly string _dbName;
    private SqliteConnection? _sqliteDbConnection = null;

    public SqliteTestDb(string dbName)
    {        
        _dbName = dbName;
    }

    public DatabaseType DbType => DatabaseType.Sqlite;
    public string ConnectionString { get; private set; }

    public async Task InitializeAsync()
    {
        var connectionString = $"DataSource={_dbName};Mode=Memory;Cache=Shared";
        // To make sure the in-memory database is not deleted we need to keep at least one connection open.
        _sqliteDbConnection = new SqliteConnection(connectionString);
        await _sqliteDbConnection.OpenAsync();
        ConnectionString = connectionString;
    }

    public async Task DisposeAsync()
    {
        if (_sqliteDbConnection is not null)
        {
            await _sqliteDbConnection.CloseAsync();
            _sqliteDbConnection.Dispose();
            _sqliteDbConnection = null;
        }
    }
}
