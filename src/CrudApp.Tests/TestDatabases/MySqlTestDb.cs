using CrudApp.Infrastructure.Database;
using MySqlConnector;
using Testcontainers.MySql;

namespace CrudApp.Tests.TestDatabases;
internal class MySqlTestDb : ITestDb
{
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private static MySqlContainer? _container;

    private readonly string _dbName;
    private string _adminConnectionString;

    public MySqlTestDb(string dbName)
    {
        _dbName = dbName;
    }

    public DatabaseType DbType => DatabaseType.MySql;

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
                    _container = new MySqlBuilder().Build();
                    await _container.StartAsync();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        _adminConnectionString = _container.GetConnectionString();
        Assert.Contains(";Database=test;", _adminConnectionString);
        ConnectionString = _adminConnectionString.Replace(";Database=test;", $";Database={_dbName};");
        await _container.ExecScriptAsync($"CREATE DATABASE {_dbName}");
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            using var connection = new MySqlConnection(_adminConnectionString);
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = $"DROP DATABASE \"{_dbName}\"";
            await command.ExecuteNonQueryAsync();
        }
    }
}
