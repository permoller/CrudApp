using Microsoft.AspNetCore.Mvc;

namespace CrudApp.Infrastructure.Http;

// Add ApiController-attribute to enable automatic model validation.
[ApiController]
[Route("/api/[controller]")]
public class CrudAppControllerBase : ControllerBase
{
    private readonly Lazy<CrudAppDbContext> _lazyDbContext;
    protected CrudAppDbContext DbContext => _lazyDbContext.Value;

    public CrudAppControllerBase()
    {
        _lazyDbContext = new Lazy<CrudAppDbContext>(() => HttpContext.RequestServices.GetRequiredService<CrudAppDbContext>());
    }
}
