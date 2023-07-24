using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudApp.SuperHeroes;

public class SuperHeroesController : EntityControllerBase<SuperHero>
{
    [HttpGet("/api/[controller]/top-ten")]
    public async Task<IEnumerable<SuperHero>> GetTopTen()
    {
        return await DbContext.Authorized<SuperHero>().AsNoTracking().ToListAsync();
    }
}
