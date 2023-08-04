using CrudApp.Infrastructure.Authentication;
using CrudApp.Infrastructure.Database;
using CrudApp.Infrastructure.ErrorHandling;
using CrudApp.Infrastructure.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace CrudApp.Tests;

public class WebAppFixture : IDisposable, IAsyncLifetime
{
    static WebAppFixture()
    {
        ApiExceptionHandler.IsExceptionDetailsInResponseEnabled = true;
    }

    public WebApplicationFactory<CrudAppApiControllerBase> WebAppFactory { get; }

    public EntityId InitialUserId { get; private set; }

    public WebAppFixture()
    {
        var dbName = Guid.NewGuid().ToString();
        // Evenry instance of WebAppFixture will get its own in-memory database.
        _dbName = Guid.NewGuid().ToString();

        // To make sure the in-memory database is not deleted we need to keep at least one connection open.
        _dbConnection = CreateDbConnection(_dbName);
        _dbConnection.Open();

        WebAppFactory = new WebApplicationFactory<CrudAppApiControllerBase>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.Remove(services.First(s => s.ServiceType == typeof(DbContextOptions<CrudAppDbContext>)));
                    services.AddDbContext<CrudAppDbContext>(dbContextOptionsBuilder =>
                    {
                        dbContextOptionsBuilder.UseSqlite(new SqliteConnection($"DataSource={dbName};Mode=Memory;Cache=Shared"));
                        dbContextOptionsBuilder.UseSqlite(CreateDbConnection(_dbName));
                        dbContextOptionsBuilder.EnableSensitiveDataLogging(true);
                    });
                });
            });
    }

    public static async Task<WebAppFixture> CreateAsync()
    {
        var webAppFixture = new WebAppFixture();
        await webAppFixture.InitializeAsync();
        return webAppFixture;
    }

    public void Dispose()
    {
        WebAppFactory.Dispose();
    }

    public virtual async Task InitializeAsync()
    {
        var scope = WebAppFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CrudAppDbContext>();
        using var scope = WebAppFactory.Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<CrudAppDbContext>();
        InitialUserId = (await db.EnsureCreatedAsync()).Value;
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public HttpClient CreateHttpClient(EntityId? userId = null)
    {
        var client = WebAppFactory.CreateClient();
        if(userId != default)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(UserIdAuthenticationHandler.HttpAuthenticationScheme, userId.ToString());
        return client;
    }

    private readonly string _dbName;

    private readonly SqliteConnection _dbConnection;

    private static SqliteConnection CreateDbConnection(string dbName)
        => new SqliteConnection($"DataSource={dbName};Mode=Memory;Cache=Shared");
}