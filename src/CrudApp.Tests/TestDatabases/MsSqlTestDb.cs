using CrudApp.Infrastructure.Database;
using Testcontainers.MsSql;

namespace CrudApp.Tests.TestDatabases;
internal sealed class MsSqlTestDb : ITestDb
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private static MsSqlContainer? _container;

    private readonly string _dbName;

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

        var port = _container.GetMappedPublicPort(MsSqlBuilder.MsSqlPort);
        var host = _container.Hostname;
        var usr = MsSqlBuilder.DefaultUsername;
        var pwd = MsSqlBuilder.DefaultPassword;
        ConnectionString = $"Server={host},{port};Database={_dbName};User Id={usr};Password={pwd};TrustServerCertificate=True";
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            var result = await _container.ExecScriptAsync($"""
                ALTER DATABASE {_dbName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE IF EXISTS {_dbName};
                """);
            Assert.True(0 == result.ExitCode, result.Stderr + Environment.NewLine + result.Stdout);
        }
    }
}
