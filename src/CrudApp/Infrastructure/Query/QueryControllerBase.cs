using CrudApp.Infrastructure.WebApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudApp.Infrastructure.Query;

public abstract class QueryControllerBase<T> : CrudAppApiControllerBase where T : class
{
    protected abstract IQueryable<T> GetQueryable(bool includeSoftDeleted);

    [HttpGet("query")]
    public Task<Result<List<T>>> Query([FromQuery] FilteringParams filteringParams, [FromQuery] OrderingParams orderingParams, bool includeSoftDeleted = false, CancellationToken cancellationToken = default)
    {
        return
            GetQueryable(includeSoftDeleted)
            .ApplyFiltering(filteringParams)
            .Select(query => query.ApplyOrdering(orderingParams))
            .Select(query => query.AsNoTracking().ToListAsync(cancellationToken));
    }

    [HttpGet("count")]
    public Task<Result<long>> Count([FromQuery] FilteringParams filteringParams, bool includeSoftDeleted = false, CancellationToken cancellationToken = default)
    {
        return GetQueryable(includeSoftDeleted)
            .ApplyFiltering(filteringParams)
            .Select(query => query.LongCountAsync(cancellationToken));
    }
}
