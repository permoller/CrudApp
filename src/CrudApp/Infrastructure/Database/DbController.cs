using Microsoft.AspNetCore.Mvc;

namespace CrudApp.Infrastructure.Database;

[ApiController]
public class DbController : ControllerBase
{
    private readonly CrudAppDbContext _dbContext;

    public DbController(CrudAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("/db/ensure")]
    public async Task<EntityId?> EnsureCreatedAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.EnsureDatabaseCreatedAsync(cancellationToken);
    }
}
