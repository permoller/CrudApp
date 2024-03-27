using Microsoft.Data.Sqlite;

namespace CrudApp.Tests.TestDatabases;

internal class SqliteTestDb : ITestDb
{
    private readonly string _dbName;
    private SqliteConnection? _sqliteDbConnection = null;

    public SqliteTestDb(string dbName)
    {        
        _dbName = dbName;
        ConnectionString = null!; // Set in InitializeAsync
    }

    public string ConnectionString { get; private set; }

    public async Task InitializeAsync()
    {
        ConnectionString = $"DataSource={_dbName};Mode=Memory;Cache=Shared";

        // To make sure the in-memory database is not deleted once created we need to keep at least one connection open.
        _sqliteDbConnection = new SqliteConnection(ConnectionString);
        await _sqliteDbConnection.OpenAsync();
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
