using CrudApp.Infrastructure.Database;
using Testcontainers.MySql;

namespace CrudApp.Tests.TestDatabases;
internal class MySqlTestDb : ITestDb
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private static MySqlContainer? _container;

    private readonly string _dbName;

    public MySqlTestDb(string dbName)
    {
        _dbName = dbName;
    }

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
                    // The default user can not create new databases, so we run as root.
                    _container = new MySqlBuilder().WithUsername("root").Build();
                    await _container.StartAsync();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        var port = _container.GetMappedPublicPort(MySqlBuilder.MySqlPort);
        var host = _container.Hostname;
        var usr = "root";
        var pwd = MySqlBuilder.DefaultPassword;
        ConnectionString = $@"Server={host};Port={port};Database={_dbName};Uid={usr};Pwd={pwd}";
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            var result = await _container.ExecScriptAsync($"DROP DATABASE IF EXISTS {_dbName}");
            Assert.True(0 == result.ExitCode, result.Stderr + Environment.NewLine + result.Stdout);
        }
    }
}
