using CrudApp.Infrastructure.Authentication;
using CrudApp.Infrastructure.Database;
using CrudApp.Infrastructure.ErrorHandling;
using CrudApp.Infrastructure.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using Xunit.Abstractions;

namespace CrudApp.Tests;

public class WebAppFixture
{
    static WebAppFixture()
    {
        ApiExceptionHandler.IsExceptionDetailsInResponseEnabled = true;
    }

    public WebApplicationFactory<CrudAppApiControllerBase> WebAppFactory { get; private set; } = null!;
    
    public EntityId InitialUserId { get; private set; }

    

    public virtual async Task StartTestAsync(ITestOutputHelper? testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        if (!_isInitialized)
        {
            await InitializeFixtureAsync();
            _isInitialized = true;
        }
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

    private bool _isInitialized;

    private List<string>? _logOutputFromBetweenTests;

    private TestOutputLogger.Provider _testOutputLoggerProvider = null!;

    private SqliteConnection _dbConnection = null!;
    
    private ITestOutputHelper? _testOutputHelper;

    private static SqliteConnection CreateDbConnection(string dbName)
        => new($"DataSource={dbName};Mode=Memory;Cache=Shared");

    protected virtual async Task InitializeFixtureAsync()
    {
        _testOutputLoggerProvider = new TestOutputLogger.Provider(Log);

        // Every instance of WebAppFixture will get its own in-memory database.
        var dbName = Guid.NewGuid().ToString();

        // To make sure the in-memory database is not deleted we need to keep at least one connection open.
        _dbConnection = CreateDbConnection(dbName);
        _dbConnection.Open();

        WebAppFactory = new WebApplicationFactory<CrudAppApiControllerBase>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders().AddProvider(_testOutputLoggerProvider));
                builder.ConfigureServices(services =>
                {
                    services.PostConfigure<DatabaseOptions>(options =>
                    {
                        options.ConnectionString = $"DataSource={dbName};Mode=Memory;Cache=Shared";
                        options.EnableSensitiveDataLogging = true;
                        options.EnableDetailedErrors = true;
                    });
                });
            });

        using var scope = WebAppFactory.Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<CrudAppDbContext>();
        InitialUserId = (await db.EnsureDatabaseCreatedAsync(CancellationToken.None)).Value;
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
}