using CrudApp.Infrastructure.Authentication;
using CrudApp.Infrastructure.Database;
using CrudApp.Infrastructure.ErrorHandling;
using CrudApp.Infrastructure.Http;
using CrudApp.Infrastructure.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using Xunit.Abstractions;
using CrudApp.Tests.TestDatabases;


namespace CrudApp.Tests;

public class WebAppFixture : IAsyncLifetime
{
    static int _dbCounter = 0;

    static WebAppFixture()
    {
        ApiExceptionHandler.IsExceptionDetailsInResponseEnabled = true;
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
        _testOutputLoggerProvider = new TestOutputLogger.Provider(Log);

        var dbName = "test_db_" + Interlocked.Increment(ref _dbCounter);
        _testDb = await CreateTestDbAsync(DatabaseType.MySql, dbName);
        WebAppFactory = InitializeWebAppFactory(_testDb);
        RootUserId = await InitializeRootUserAsync();
    }

    public virtual async Task DisposeAsync()
    {
        if (_testDb is not null)
        {
            await _testDb.DisposeAsync();
            _testDb = null;
        }
    }


    protected virtual WebApplicationFactory<CrudAppApiControllerBase> InitializeWebAppFactory(ITestDb testDb)
    {
        return new WebApplicationFactory<CrudAppApiControllerBase>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders().AddProvider(_testOutputLoggerProvider));
                builder.ConfigureServices(services =>
                {
                    services.Configure<DatabaseOptions>(o =>
                    {
                        o.DbType = testDb.DbType;
                        o.ConnectionString = testDb.ConnectionString;
                        o.EnableDetailedErrors = true;
                        o.EnableSensitiveDataLogging = true;
                    });
                });
            });
    }

    protected virtual async Task<EntityId> InitializeRootUserAsync()
    {
        using var scope = WebAppFactory.Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<CrudAppDbContext>();
        var user = new User();
        db.Add(user);
        await db.SaveChangesAsync();
        return user.Id;
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


    private static async Task<ITestDb> CreateTestDbAsync(DatabaseType dbType, string dbName)
    {
        ITestDb testDb = dbType switch
        {
            DatabaseType.Sqlite => new SqliteTestDb(dbName),
            DatabaseType.MySql => new MySqlTestDb(dbName),
            DatabaseType.MsSql => new MsSqlTestDb(dbName),
            DatabaseType.Postgres => new PostgresTestDb(dbName),
            _ => throw new NotSupportedException($"{nameof(DatabaseType)} {dbType} is not supported.")
        };

        await testDb.InitializeAsync();

        var dbOptions = new DatabaseOptions
        {
            DbType = dbType,
            ConnectionString = testDb.ConnectionString,
            EnableDetailedErrors = true,
            EnableSensitiveDataLogging = true,
        };

        using var dbContext = new CrudAppDbContext(Options.Create(dbOptions));
        await dbContext.Database.EnsureCreatedAsync();
        return testDb;
    }

}