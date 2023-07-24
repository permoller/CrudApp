using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudApp.Infrastructure.Query;

public abstract class QueryControllerBase<T> : CrudAppControllerBase where T : class
{
    protected abstract IQueryable<T> GetQueryable(bool includeSoftDeleted);

    [HttpGet]
    public async Task<IEnumerable<T>> Query([FromQuery] FilteringParams filteringParams, [FromQuery] OrderingParams orderingParams, bool includeSoftDeleted = false)
    {
        var queryable =
            GetQueryable(includeSoftDeleted)
            .ApplyFiltering(filteringParams)
            .ApplyOrdering(orderingParams);
        var result = await queryable.AsNoTracking().ToListAsync();
        return result;
    }

    [HttpGet("count")]
    public async Task<long> Count([FromQuery] FilteringParams filteringParams, bool includeSoftDeleted = false)
    {
        var query =
            GetQueryable(includeSoftDeleted)
            .ApplyFiltering(filteringParams);
        var result = await query.LongCountAsync();
        return result;
    }
}
