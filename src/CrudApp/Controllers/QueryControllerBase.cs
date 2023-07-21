using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace CrudApp.Controllers;

// Add ApiController-attribute to enable automatic model validation and return ProblemDetails on exceptions.
[ApiController]
// Add OpenAPI metadata for status 500 and 4xx
[ProducesResponseType(Status500InternalServerError, Type = typeof(ProblemDetails))]
[ProducesErrorResponseType(typeof(ProblemDetails))]
[ProducesResponseType(Status400BadRequest, Type = typeof(ValidationProblemDetails))]
[Route("/api/[controller]")]
public abstract class QueryControllerBase<T> : ControllerBase where T : class
{
    private readonly Lazy<CrudAppDbContext> _lazyDbContext;
    protected CrudAppDbContext DbContext => _lazyDbContext.Value;

    protected QueryControllerBase()
    {
        _lazyDbContext = new Lazy<CrudAppDbContext>(() => HttpContext.RequestServices.GetRequiredService<CrudAppDbContext>());
    }

    protected abstract IQueryable<T> GetQueryable(bool includeSoftDeleted);

    [HttpGet]
    [ProducesResponseType(Status200OK)]
    public async Task<ActionResult<IEnumerable<T>>> Query(string? filter = null, string? orderBy = null, int? skip = null, int? take = null, bool includeSoftDeleted = false)
    {
        // TODO: Move query functionality to DbContext so it can also be used directly in other places without calling the controller.
        var query = GetQueryable(includeSoftDeleted);
        
        if (!Filter.TryApply(ref query, filter, out var error))
            return ValidationProblem(error);

        if(!OrderBy.TryApply(ref query, orderBy, out error))
            return ValidationProblem(error);

        if (skip.HasValue)
            query = query.Skip(skip.Value);

        if (take.HasValue)
            query = query.Take(take.Value);

        var result = await query.ToListAsync();
        return result;
    }

    [HttpGet("count")]
    [ProducesResponseType(Status200OK)]
    public async Task<ActionResult<long>> Count(string? filter = null, bool includeSoftDeleted = false)
    {
        var query = GetQueryable(includeSoftDeleted);

        if (!Filter.TryApply(ref query, filter, out var error))
            return ValidationProblem(error);

        var result = await query.LongCountAsync();
        return result;
    }
}
