using CrudApp.Infrastructure.Database;
using Npgsql;
using Testcontainers.PostgreSql;

namespace CrudApp.Tests.TestDatabases;
internal class PostgresTestDb : ITestDb
{
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private static PostgreSqlContainer? _container;

    private readonly string _dbName;
    private string _adminConnectionString;

    public PostgresTestDb(string dbName)
    {
        _dbName = dbName;
    }

    public DatabaseType DbType => DatabaseType.Postgres;

    public string ConnectionString { get; private set; }

    public async Task InitializeAsync()
    {
        if (_container is null)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_container is null)
                {
                    _container = new PostgreSqlBuilder().Build();
                    await _container.StartAsync();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // When connecting to a Postgres DB we must always to connect to an existing database.
        // This means DBContext can not create it for us. We must create an empty DB and then DBContext will be able to connect and create the tables.
        _adminConnectionString = _container.GetConnectionString() + ";Include Error Detail=True";
        Assert.Contains(";Database=postgres;", _adminConnectionString);
        using var connection = new NpgsqlConnection(_adminConnectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE \"{_dbName}\"";
        await command.ExecuteNonQueryAsync();

        // Return connection string pointing to the empty database
        ConnectionString = _adminConnectionString.Replace(";Database=postgres;", $";Database={_dbName};");
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            using var connection = new NpgsqlConnection(_adminConnectionString);
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = $"""
                SELECT pg_terminate_backend(pg_stat_activity.pid)
                FROM pg_stat_activity
                WHERE pg_stat_activity.datname = '{_dbName}';
                DROP DATABASE "{_dbName}";
                """;
            await command.ExecuteNonQueryAsync();
        }
    }
}
