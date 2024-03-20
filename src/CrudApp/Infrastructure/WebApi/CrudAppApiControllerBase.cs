using Microsoft.AspNetCore.Mvc;

namespace CrudApp.Infrastructure.WebApi;

// Add ApiController-attribute to enable automatic model validation.
[ApiController]
[Route("/api/[controller]")]
public class CrudAppApiControllerBase : ControllerBase
{
    private readonly Lazy<CrudAppDbContext> _lazyDbContext;
    protected CrudAppDbContext DbContext => _lazyDbContext.Value;

    public CrudAppApiControllerBase()
    {
        _lazyDbContext = new Lazy<CrudAppDbContext>(() => HttpContext.RequestServices.GetRequiredService<CrudAppDbContext>());
    }
}
