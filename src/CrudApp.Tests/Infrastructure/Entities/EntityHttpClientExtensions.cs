using CrudApp.Infrastructure.Entities;
using CrudApp.Infrastructure.UtilityCode;
using CrudApp.Tests.Infrastructure.Http;
using System.Net.Http.Json;

namespace CrudApp.Tests.Infrastructure.Entities;
internal static class EntityHttpClientBase
{

    private static readonly Dictionary<Type, string> _entityTypeToControllerName = new Dictionary<Type, string>();

    static EntityHttpClientBase()
    {
        var entityControllerTypes = typeof(EntityControllerBase<>).GetSubclasses();
        foreach(var entityControllerType in entityControllerTypes)
        {
            var type = entityControllerType.GetGenericTypeArgumentsFor(typeof(EntityControllerBase<>))[0];
            var controllerName = entityControllerType.Name.Replace("Controller", "");
            _entityTypeToControllerName.Add(type, controllerName);
        }
    }

    private static string GetPath<T>(EntityId? id = default) where T : EntityBase
    {
        if (_entityTypeToControllerName.TryGetValue(typeof(T), out var controllerName))
        {
            if (id != default)
                return $"/api/{controllerName}/{id}";
            return $"/api/{controllerName}";
        }

        throw new ArgumentException($"No entity controller exists for entity type {typeof(T).Name}.");
    }

    public static async Task<T?> GetEntityAsync<T>(this HttpClient httpClient, EntityId id) where T : EntityBase
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync(GetPath<T>(id));
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException("Error making GET request.", ex, ex.StatusCode);
        }
        await response.EnsureSuccessAsync();
        return await response.ReadContentAsync<T>();
    }

    public static async Task<EntityId> PostEntityAsync<T>(this HttpClient httpClient, T entity) where T : EntityBase
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync(GetPath<T>(), entity);
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException("Error making POST request.", ex, ex.StatusCode);
        }
        await response.EnsureSuccessAsync();
        return await response.ReadContentAsync<EntityId>();
    }

    public static async Task PutEntityAsync<T>(this HttpClient httpClient, T entity) where T : EntityBase
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PutAsJsonAsync(GetPath<T>(entity.Id), entity);            
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException("Error making PUT request.", ex, ex.StatusCode);
        }
        await response.EnsureSuccessAsync();
    }

    public static async Task DeleteEntityAsync<T>(this HttpClient httpClient, EntityId id) where T : EntityBase
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.DeleteAsync(GetPath<T>(id));
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException("Error making DELETE request.", ex, ex.StatusCode);
        }
        await response.EnsureSuccessAsync();
    }
}
