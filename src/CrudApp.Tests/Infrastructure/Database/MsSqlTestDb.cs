using Testcontainers.MsSql;

namespace CrudApp.Tests.Infrastructure.Database;
internal sealed class MsSqlTestDb : TestDb
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private static MsSqlContainer? _container;

    private readonly string _dbName;

    private MsSqlTestDb(string dbName, string connectionString) : base(connectionString)
    {
        _dbName = dbName;
    }

    public static async Task<MsSqlTestDb> CreateAsync(string dbName)
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
        var connectionString = $"Server={host},{port};Database={dbName};User Id={usr};Password={pwd};TrustServerCertificate=True";
        return new MsSqlTestDb(dbName, connectionString);
    }

    public override async ValueTask DisposeAsync()
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
