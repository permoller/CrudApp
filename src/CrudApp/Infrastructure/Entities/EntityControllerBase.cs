using CrudApp.Infrastructure.Query;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace CrudApp.Infrastructure.Entities;


public abstract class EntityControllerBase<T> : QueryControllerBase<T> where T : EntityBase
{
    protected override IQueryable<T> GetQueryable(bool includeSoftDeleted) => DbContext.Authorized<T>(includeSoftDeleted);

    [HttpGet("{id}")]
    public async Task<T> Get([FromRoute] EntityId id, CancellationToken cancellationToken)
    {
        var entity = await DbContext.GetByIdAuthorized<T>(id, asNoTracking: true, cancellationToken);
        return entity;
    }

    [HttpPost]
    [ProducesResponseType(HttpStatus.Created)]
    public async Task<ActionResult<EntityId>> Post([FromBody] T entity, CancellationToken cancellationToken)
    {
        DbContext.Add(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
        // We do not return the created entity.
        // We return a response with a location header where the entity can be fetched.
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity.Id);
    }

    /// <summary>
    /// Updates the given entity.
    /// If the entity has navigation-properties that contains EntityBase objects they will also be updated.
    /// Adding and removing objects via collection-navigation-properties is not supported, but updating existing entities is.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="ApiResponseException"></exception>
    [HttpPut("{id}")]
    public async Task Put([FromRoute] EntityId id, [FromBody] T entity, CancellationToken cancellationToken)
    {
        if (entity.Id != id)
        {
            throw new ApiResponseException(HttpStatus.BadRequest, "Id in body and in URL must be the same.");
        }

        EntityBase existingEntity;
        try
        {
            existingEntity = await DbContext.GetByIdAuthorized(entity.GetType(), entity.Id, asNoTracking: false, cancellationToken);
        }
        catch (ApiResponseException ex) when (ex.HttpStatus == HttpStatus.NotFound)
        {
            throw new ApiResponseException(HttpStatus.Conflict, $"Entity {entity.Id} does not exist in the database. It may have been deleted. Adding new entities is not supported by the update endpoint.");
        }

        if (existingEntity.Version != entity.Version)
            throw new ApiResponseException(HttpStatus.Conflict, $"Entity {entity.Id} has a version conflict. Version in request: {entity.Version}. Version in database: {existingEntity.Version}.");


        var propertiesToUpdate = existingEntity
            .GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanRead && p.CanWrite);
        foreach (var propInfo in propertiesToUpdate)
        {
            var newValue = propInfo.GetValue(entity);
            propInfo.SetValue(existingEntity, newValue);
        }

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    [HttpDelete("{id}")]
    public async Task Delete([FromRoute] EntityId id, CancellationToken cancellationToken)
    {
        var entity = await DbContext.GetByIdAuthorized<T>(id, asNoTracking: false, cancellationToken);
        DbContext.Remove(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
