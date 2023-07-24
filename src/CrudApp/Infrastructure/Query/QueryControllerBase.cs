using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudApp.Infrastructure.Query;

public abstract class QueryControllerBase<T> : CrudAppControllerBase where T : class
{
    protected abstract IQueryable<T> GetQueryable(bool includeSoftDeleted);

    [HttpGet]
    [ProducesResponseType((int)HttpStatus.Ok)]
    public async Task<ActionResult<IEnumerable<T>>> Query([FromQuery] FilteringParams filteringParams, [FromQuery] OrderingParams orderingParams, bool includeSoftDeleted = false)
    {
        var queryable =
            GetQueryable(includeSoftDeleted)
            .ApplyFiltering(filteringParams)
            .ApplyOrdering(orderingParams);
        var result = await queryable.ToListAsync();
        return result;
    }

    [HttpGet("count")]
    [ProducesResponseType((int)HttpStatus.Ok)]
    public async Task<ActionResult<long>> Count([FromQuery] FilteringParams filteringParams, bool includeSoftDeleted = false)
    {
        var query =
            GetQueryable(includeSoftDeleted)
            .ApplyFiltering(filteringParams);
        var result = await query.LongCountAsync();
        return result;
    }
}
