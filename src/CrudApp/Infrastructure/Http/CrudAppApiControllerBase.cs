using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrudApp.Infrastructure.Http;

// Add ApiController-attribute to enable automatic model validation.
[ApiController]
[Route("/api/[controller]")]
[Authorize]
public class CrudAppApiControllerBase : ControllerBase
{
    private readonly Lazy<CrudAppDbContext> _lazyDbContext;
    protected CrudAppDbContext DbContext => _lazyDbContext.Value;

    public CrudAppApiControllerBase()
    {
        _lazyDbContext = new Lazy<CrudAppDbContext>(() => HttpContext.RequestServices.GetRequiredService<CrudAppDbContext>());
    }
}
