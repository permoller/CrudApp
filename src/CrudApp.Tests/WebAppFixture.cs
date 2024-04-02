﻿using CrudApp.Infrastructure.Authentication;
using CrudApp.Infrastructure.Database;
using CrudApp.Infrastructure.ErrorHandling;
using CrudApp.Infrastructure.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using Xunit.Abstractions;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using CrudApp.Tests.Infrastructure.Database;
using CrudApp.Tests.Infrastructure.Logging;
using CrudApp.Infrastructure.Logging;

namespace CrudApp.Tests;

public class WebAppFixture : IAsyncLifetime
{
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

    private TestDb? _testDb;



    public virtual async Task InitializeAsync()
    {
        var unittestSettingsFile = "appsettings.Unittest.json";
        var swTotal = Stopwatch.StartNew();
        var unittestConfig = new ConfigurationBuilder().AddJsonFile(unittestSettingsFile).Build();
        _testOutputLoggerProvider = new TestOutputLogger.Provider(Log);
        var dbType = Enum.Parse<DatabaseType>(unittestConfig[$"{nameof(DatabaseOptions)}:{nameof(DatabaseOptions.DbType)}"]!);
        _testDb = await TestDb.CreateAsync(dbType);

        WebAppFactory = new WebApplicationFactory<CrudAppApiControllerBase>()
            .WithWebHostBuilder(WithWebHostBuilder);
        
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

    protected virtual void WithWebHostBuilder(IWebHostBuilder builder)
    {
        // Make sure the application loads appsettings.Unittest.json
        builder.UseEnvironment("Unittest");

        // Configure DB connection string
        builder.ConfigureAppConfiguration(configBuilder =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?> {
                { $"{nameof(DatabaseOptions)}:{nameof(DatabaseOptions.ConnectionString)}", _testDb?.ConnectionString }
            });
        });

        // Capture log output
        builder.ConfigureLogging(loggingBuilder => loggingBuilder.AddProvider(_testOutputLoggerProvider));
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
}