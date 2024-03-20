using CrudApp.Infrastructure.Authentication;
using CrudApp.Infrastructure.Database;
using CrudApp.Infrastructure.ErrorHandling;
using CrudApp.Infrastructure.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using Xunit.Abstractions;
using CrudApp.Tests.TestDatabases;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace CrudApp.Tests;

public class WebAppFixture : IAsyncLifetime
{
    static int _dbCounter = 0;

    static WebAppFixture()
    {
        ProblemDetailsHelper.IncludeExceptionInProblemDetails = true;
    }


    public WebApplicationFactory<CrudAppApiControllerBase> WebAppFactory { get; private set; } = null!;
    
    public EntityId RootUserId { get; private set; }

    
    public virtual Task StartTestAsync(ITestOutputHelper? testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        return Task.CompletedTask;
    }

    public virtual Task StopTestAsync()
    {
        return Task.CompletedTask;
    }

    public HttpClient CreateHttpClient(EntityId? userId = null)
    {
        var client = WebAppFactory.CreateDefaultClient();
        if (userId != default)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(UserIdAuthenticationHandler.HttpAuthenticationScheme, userId.ToString());
        return client;
    }

    private List<string>? _logOutputFromBetweenTests;

    private TestOutputLogger.Provider _testOutputLoggerProvider = null!;

    private ITestOutputHelper? _testOutputHelper;

    private ITestDb? _testDb;

    

    public virtual async Task InitializeAsync()
    {
        var swTotal = Stopwatch.StartNew();
        _testOutputLoggerProvider = new TestOutputLogger.Provider(Log);

        WebAppFactory = new WebApplicationFactory<CrudAppApiControllerBase>()
            .WithWebHostBuilder(builder =>
            {
                // Make sure we load appsettings.Unittest.json
                builder.UseEnvironment("Unittest");

                // Create test DB and configure connection string
                builder.ConfigureAppConfiguration(configBuilder =>
                {
                    _testDb = StartTestDbAsync(configBuilder.Build()).GetAwaiter().GetResult();
                    configBuilder.AddInMemoryCollection(new Dictionary<string, string?> {
                        { $"{nameof(DatabaseOptions)}:{nameof(DatabaseOptions.ConnectionString)}", _testDb.ConnectionString }
                    });
                });

                // Capture log output
                builder.ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders().AddProvider(_testOutputLoggerProvider));
            });

        // Create tables and root user
        using var scope = WebAppFactory.Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<CrudAppDbContext>();
        var swEnsureDb = Stopwatch.StartNew();
        var rootUserId = await db.EnsureDatabaseCreatedAsync(CancellationToken.None);
        swEnsureDb.Stop();
        swTotal.Stop();
        Log("EnsureDatabaseCreated in seconds: " + swEnsureDb.Elapsed.TotalSeconds);
        Log("Total test fixture setup in seconds: " + swTotal.Elapsed.TotalSeconds);
        ArgumentNullException.ThrowIfNull(rootUserId); // We just created the database, so a new user should have been inserted.
        RootUserId = rootUserId.Value;
    }

    public virtual async Task DisposeAsync()
    {
        if (_testDb is not null)
        {
            await _testDb.DisposeAsync();
            _testDb = null;
        }
    }

    private void Log(string message)
    {
        if (_testOutputHelper is null)
        {
            // No test is currently running... capture logs and try to write them later
            if (_logOutputFromBetweenTests is null)
                _logOutputFromBetweenTests = new();
            _logOutputFromBetweenTests.Add(message);
        }
        else
        {
            // Write captured logs
            if (_logOutputFromBetweenTests is not null && _logOutputFromBetweenTests.Count > 0)
            {
                var lines = _logOutputFromBetweenTests;
                _logOutputFromBetweenTests = null;
                foreach (var line in lines)
                    _testOutputHelper.WriteLine(line);
            }

            _testOutputHelper.WriteLine(message);
        }
    }


    private static async Task<ITestDb> StartTestDbAsync(IConfiguration config)
    {
        var dbName = "test_db_" + Interlocked.Increment(ref _dbCounter);
        var dbType = Enum.Parse<DatabaseType>(config[$"{nameof(DatabaseOptions)}:{nameof(DatabaseOptions.DbType)}"]!);
        ITestDb testDb = dbType switch
        {
            DatabaseType.Sqlite => new SqliteTestDb(dbName),
            DatabaseType.MySql => new MySqlTestDb(dbName),
            DatabaseType.MsSql => new MsSqlTestDb(dbName),
            DatabaseType.Postgres => new PostgresTestDb(dbName),
            _ => throw new NotSupportedException($"{nameof(DatabaseType)} {dbType} is not supported.")
        };

        await testDb.InitializeAsync();

        return testDb;
    }

}