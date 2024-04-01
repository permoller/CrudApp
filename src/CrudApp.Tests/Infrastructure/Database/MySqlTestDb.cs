using Testcontainers.MySql;

namespace CrudApp.Tests.Infrastructure.Database;
internal class MySqlTestDb : TestDb
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private static MySqlContainer? _container;

    private readonly string _dbName;

    private MySqlTestDb(string dbName, string connectionString) : base(connectionString)
    {
        _dbName = dbName;
    }

    public static async Task<MySqlTestDb> CreateAsync(string dbName)
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
        var connectionString = $@"Server={host};Port={port};Database={dbName};Uid={usr};Pwd={pwd}";
        return new MySqlTestDb(dbName, connectionString);
    }

    public override async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            var result = await _container.ExecScriptAsync($"DROP DATABASE IF EXISTS {_dbName}");
            Assert.True(0 == result.ExitCode, result.Stderr + Environment.NewLine + result.Stdout);
        }
    }
}
