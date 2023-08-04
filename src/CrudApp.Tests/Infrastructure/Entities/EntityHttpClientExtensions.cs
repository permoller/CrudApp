using CrudApp.Infrastructure.Entities;
using CrudApp.Infrastructure.UtilityCode;
using CrudApp.Tests.Infrastructure.Http;
using System;
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

    public static async Task<T?> GetEntityAsync<T>(this HttpClient httpClient, EntityId id) where T : EntityBase
    {
        var path = GetPath<T>(id);
        try
        {
            var response = await httpClient.GetAsync(path);
            await response.EnsureSuccessAsync();
            return await response.ReadContentAsync<T>();
        }
        catch (HttpRequestException ex)
        {
            throw ex.WrapWithRequestDetails("GET", path);
        }
    }

    public static async Task<EntityId> PostEntityAsync<T>(this HttpClient httpClient, T entity) where T : EntityBase
    {
        var path = GetPath<T>();
        try
        {
            var response = await httpClient.PostAsJsonAsync(path, entity, JsonUtils.ApiJsonSerializerOptions);
            await response.EnsureSuccessAsync();
            return await response.ReadContentAsync<EntityId>();
        }
        catch (HttpRequestException ex)
        {
            throw ex.WrapWithRequestDetails("POST", path);
        }
    }

    public static async Task PutEntityAsync<T>(this HttpClient httpClient, T entity) where T : EntityBase
    {
        var path = GetPath<T>(entity.Id);
        try
        {
            var response = await httpClient.PutAsJsonAsync(path, entity, JsonUtils.ApiJsonSerializerOptions);            
            await response.EnsureSuccessAsync();
        }
        catch (HttpRequestException ex)
        {
            throw ex.WrapWithRequestDetails("PUT", path);
        }
    }

    public static async Task DeleteEntityAsync<T>(this HttpClient httpClient, EntityId id) where T : EntityBase
    {
        var path = GetPath<T>(id);
        try
        {
            var response = await httpClient.DeleteAsync(path);
            await response.EnsureSuccessAsync();
        }
        catch (HttpRequestException ex)
        {
            throw ex.WrapWithRequestDetails("DELETE", path);
        }
    }
}
