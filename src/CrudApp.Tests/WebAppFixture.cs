using CrudApp.Infrastructure.Authentication;
using CrudApp.Infrastructure.Database;
using CrudApp.Infrastructure.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace CrudApp.Tests;

public sealed class WebAppFixture : IDisposable, IAsyncLifetime
{
    public WebApplicationFactory<CrudAppApiControllerBase> WebAppFactory { get; }

    public EntityId InitialUserId { get; private set; }

    public WebAppFixture()
    {
        var dbName = Guid.NewGuid().ToString();

        WebAppFactory = new WebApplicationFactory<CrudAppApiControllerBase>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.Remove(services.First(s => s.ServiceType == typeof(DbContextOptions<CrudAppDbContext>)));
                    services.AddDbContext<CrudAppDbContext>(dbContextOptionsBuilder =>
                    {
                        dbContextOptionsBuilder.UseSqlite(new SqliteConnection($"DataSource={dbName};Mode=Memory;Cache=Shared"));
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

    public async Task InitializeAsync()
    {
        var scope = WebAppFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CrudAppDbContext>();
        InitialUserId = (await db.EnsureCreatedAsync()).Value;
    }

    public Task DisposeAsync()
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
}