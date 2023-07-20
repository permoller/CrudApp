using CrudApp.SuperHeroes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudApp.Database;

[ApiController]
public class DbController : ControllerBase
{
    private readonly CrudAppDbContext _dbContext;

    public DbController(CrudAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("/db/ensure")]
    public async Task EnsureCreatedAsync()
    {
        await _dbContext.EnsureCreatedAsync();
    }

    [HttpGet("/db/superheros")]
    public async Task<IEnumerable<SuperHero>> GetSuperHeroes()
    {
        var heroes = await _dbContext.Set<SuperHero>().ToListAsync();
        return heroes;
    }
}
