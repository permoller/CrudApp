using CrudApp.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CrudApp.Tests;

internal class WebAppFixture : IDisposable
{
    public WebApplicationFactory<Program> WebAppFactory { get; }
    public WebAppFixture(WebApplicationFactory<Program> webAppFactory)
    {
        WebAppFactory = webAppFactory;
    }

    public static async Task<WebAppFixture> CreateAsync()
    {
        var dbName = Guid.NewGuid().ToString();

        var webAppFactory = new WebApplicationFactory<Program>()
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

        var scope = webAppFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CrudAppDbContext>();
        await db.EnsureCreatedAsync();
        
        return new(webAppFactory);
    }

    public void Dispose()
    {
        WebAppFactory.Dispose();
    }
}