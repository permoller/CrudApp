using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudApp.Infrastructure.Query;

public abstract class QueryControllerBase<T> : CrudAppApiControllerBase where T : class
{
    protected abstract IQueryable<T> GetQueryable(bool includeSoftDeleted);

    [HttpGet("query")]
    public async Task<IEnumerable<T>> Query([FromQuery] FilteringParams filteringParams, [FromQuery] OrderingParams orderingParams, bool includeSoftDeleted = false, CancellationToken cancellationToken = default)
    {
        var queryable =
            GetQueryable(includeSoftDeleted)
            .ApplyFiltering(filteringParams)
            .ApplyOrdering(orderingParams);
        var result = await queryable.AsNoTracking().ToListAsync(cancellationToken);
        return result;
    }

    [HttpGet("count")]
    public async Task<long> Count([FromQuery] FilteringParams filteringParams, bool includeSoftDeleted = false, CancellationToken cancellationToken = default)
    {
        var query =
            GetQueryable(includeSoftDeleted)
            .ApplyFiltering(filteringParams);
        var result = await query.LongCountAsync(cancellationToken);
        return result;
    }
}
