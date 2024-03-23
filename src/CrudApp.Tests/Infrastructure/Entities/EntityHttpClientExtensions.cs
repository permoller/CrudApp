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
        var entityControllerTypes = typeof(EntityControllerBase<>).GetSubclassesInApplication();
        foreach(var entityControllerType in entityControllerTypes)
        {
            var type = entityControllerType.GetGenericArgumentsForGenericTypeDefinition(typeof(EntityControllerBase<>))[0];
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

    public static async Task<T> GetEntityAsync<T>(this HttpClient httpClient, EntityId id, bool includeSoftDeleted = false) where T : EntityBase
    {
        var path = GetPath<T>(id);
        if(includeSoftDeleted)
            path += $"?{nameof(includeSoftDeleted)}={includeSoftDeleted}";

        var response = await httpClient.ApiGetAsync(path);
        await response.ApiEnsureSuccessAsync();
        var receivedEntity = await response.ApiReadContentAsync<T>();
        ArgumentNullException.ThrowIfNull(receivedEntity);
        return receivedEntity;
    }

    public static async Task<EntityId> PostEntityAsync<T>(this HttpClient httpClient, T entity) where T : EntityBase
    {
        var path = GetPath<T>();
        var response = await httpClient.ApiPostAsJsonAsync(path, entity);
        await response.ApiEnsureSuccessAsync();
        return await response.ApiReadContentAsync<EntityId>();
    }

    public static async Task PutEntityAsync<T>(this HttpClient httpClient, T entity) where T : EntityBase
    {
        var path = GetPath<T>(entity.Id);
        var response = await httpClient.PutAsJsonAsync(path, entity);
        await response.ApiEnsureSuccessAsync();
    }

    public static async Task<T> PutAndGetEntity<T>(this HttpClient httpClient, T entity) where T : EntityBase
    {
        await httpClient.PutEntityAsync(entity);
        var receivedEntity = await httpClient.GetEntityAsync<T>(entity.Id);
        ArgumentNullException.ThrowIfNull(receivedEntity);
        return receivedEntity;
    }

    public static async Task DeleteEntityAsync<T>(this HttpClient httpClient, EntityId id, long? version) where T : EntityBase
    {
        var path = GetPath<T>(id);
        if(version.HasValue)
            path += "?version=" + version.Value;
        var response = await httpClient.ApiDeleteAsync(path);
        await response.ApiEnsureSuccessAsync();
    }
}
