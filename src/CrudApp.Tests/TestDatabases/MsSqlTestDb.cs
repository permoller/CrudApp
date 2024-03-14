using CrudApp.Infrastructure.Database;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace CrudApp.Tests.TestDatabases;
internal sealed class MsSqlTestDb : ITestDb
{
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private static MsSqlContainer? _container;

    private readonly string _dbName;
    private string _adminConnectionString;

    public MsSqlTestDb(string dbName)
    {
        _dbName = dbName;
    }

    public DatabaseType DbType => DatabaseType.MsSql;

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
                    _container = new MsSqlBuilder().Build();
                    await _container.StartAsync();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        _adminConnectionString = _container.GetConnectionString();
        Assert.Contains(";Database=master;", _adminConnectionString);
        ConnectionString = _adminConnectionString.Replace(";Database=master;", $";Database={_dbName};");
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            using var connection = new SqlConnection(_adminConnectionString);
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = $"""
                ALTER DATABASE [{_dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{_dbName}];
                """;
            await command.ExecuteNonQueryAsync();
        }
    }
}
