using Microsoft.Data.Sqlite;

namespace CrudApp.Tests.Infrastructure.Database;

internal class SqliteTestDb : TestDb
{
    private readonly SqliteConnection _sqliteConnection;

    private SqliteTestDb(SqliteConnection sqliteConnection, string connectionString) : base(connectionString)
    {
        _sqliteConnection = sqliteConnection;
    }


    public static async Task<SqliteTestDb> CreateAsync(string dbName)
    {
        var connectionString = $"DataSource={dbName};Mode=Memory;Cache=Shared";

        // To make sure the in-memory database is not deleted once created we need to keep at least one connection open.
        var sqliteConnection = new SqliteConnection(connectionString);
        await sqliteConnection.OpenAsync();
        return new SqliteTestDb(sqliteConnection, connectionString);
    }

    public override async ValueTask DisposeAsync()
    {
        await _sqliteConnection.CloseAsync();
        _sqliteConnection.Dispose();
    }
}
