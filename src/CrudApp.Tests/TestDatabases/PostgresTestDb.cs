using CrudApp.Infrastructure.Database;
using Testcontainers.PostgreSql;

namespace CrudApp.Tests.TestDatabases;
internal class PostgresTestDb : ITestDb
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private static PostgreSqlContainer? _container;

    private readonly string _dbName;

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
        
        var port = _container.GetMappedPublicPort(PostgreSqlBuilder.PostgreSqlPort);
        var host = _container.Hostname;
        var usr = PostgreSqlBuilder.DefaultUsername;
        var pwd = PostgreSqlBuilder.DefaultPassword;
        ConnectionString = $"Host={host};Port={port};Database={_dbName};Username={usr};Password={pwd};Include Error Detail=True";
        
        // When connecting to Postgres you must connect to an existing database.
        // You would normally connect to an "administration" database like the default database named postgress to create another database.
        // When using DBContext.Database.EnsureCreated() it does not connect to an "administration" database.
        // Instead EnsureCreated() tries to connect to the database that it should create which fails because it does not exists.
        // For that reason we create an empty database here and let EnsureCreated() create the tables.
        var result = await _container.ExecScriptAsync($"CREATE DATABASE {_dbName}");
        Assert.True(0 == result.ExitCode, result.Stderr + Environment.NewLine + result.Stdout);
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            var result = await _container.ExecScriptAsync($"""
                SELECT pg_terminate_backend(pg_stat_activity.pid)
                FROM pg_stat_activity
                WHERE pg_stat_activity.datname = '{_dbName}';
                DROP DATABASE IF EXISTS {_dbName};
                """);
            Assert.True(0 == result.ExitCode, result.Stderr + Environment.NewLine + result.Stdout);
        }
    }
}
