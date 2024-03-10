using CrudApp.Infrastructure.Query;
using CrudApp.Infrastructure.UtilityCode;
using CrudApp.Tests.Infrastructure.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace CrudApp.Tests.Infrastructure.Query;
internal static class QueryHttpClientExtensions
{
    private static readonly Dictionary<Type, string> _queryTypeToControllerName = new Dictionary<Type, string>();

    static QueryHttpClientExtensions()
    {
        var queryControllerTypes = typeof(QueryControllerBase<>).GetSubclasses();
        foreach (var queryControllerType in queryControllerTypes)
        {
            var type = queryControllerType.GetGenericArgumentsForGenericTypeDefinition(typeof(QueryControllerBase<>))[0];
            var controllerName = queryControllerType.Name.Replace("Controller", "");
            _queryTypeToControllerName.Add(type, controllerName);
        }
    }

    private static string GetPath<T>(string action, QueryBuilder query)
    {
        if (_queryTypeToControllerName.TryGetValue(typeof(T), out var controllerName))
        {
            return $"/api/{controllerName}/{action}{query}";
        }

        throw new ArgumentException($"No query controller exists for type {typeof(T).Name}.");
    }

    public static async Task<List<T>> Query<T>(this HttpClient httpClient, FilteringParams? filteringParams = null, OrderingParams? orderingParams = null, bool includeSoftDeleted = false)
    {
        var queryBuilder = new QueryBuilder();
            
        if(filteringParams?.Filter != default)
            queryBuilder.Add(nameof(filteringParams.Filter), filteringParams.Filter);

        if (orderingParams?.OrderBy != default)
            queryBuilder.Add(nameof(orderingParams.OrderBy), orderingParams.OrderBy);

        if (orderingParams?.Skip.HasValue == true)
            queryBuilder.Add(nameof(orderingParams.Skip), orderingParams.Skip.Value.ToString());

        if (orderingParams?.Take.HasValue == true)
            queryBuilder.Add(nameof(orderingParams.Take), orderingParams.Take.Value.ToString());

        if (includeSoftDeleted)
            queryBuilder.Add(nameof(includeSoftDeleted), includeSoftDeleted.ToString());

        var path = GetPath<T>("query", queryBuilder);
        
        var response = await httpClient.ApiGetAsync(path);
        await response.ApiEnsureSuccessAsync();
        return (await response.ApiReadContentAsync<List<T>>()) ?? new List<T>();
    }

    public static async Task<long> Count<T>(this HttpClient httpClient, FilteringParams? filteringParams = null, bool includeSoftDeleted = false)
    {
        var queryBuilder = new QueryBuilder();

        if (filteringParams?.Filter != default)
            queryBuilder.Add(nameof(filteringParams.Filter), filteringParams.Filter);

        if (includeSoftDeleted)
            queryBuilder.Add(nameof(includeSoftDeleted), includeSoftDeleted.ToString());

        var path = GetPath<T>("count", queryBuilder);
        var response = await httpClient.ApiGetAsync(path);
        await response.ApiEnsureSuccessAsync();
        return await response.ApiReadContentAsync<long>();
    }
}
