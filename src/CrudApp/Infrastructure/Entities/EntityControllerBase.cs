using CrudApp.Infrastructure.Query;
using CrudApp.Infrastructure.UtilityCode;
using Microsoft.AspNetCore.Mvc;
using System.Collections;

namespace CrudApp.Infrastructure.Entities;


public abstract class EntityControllerBase<T> : QueryControllerBase<T> where T : EntityBase
{
    protected override IQueryable<T> GetQueryable(bool includeSoftDeleted) => DbContext.Authorized<T>(includeSoftDeleted);

    [HttpGet("{id}")]
    public async Task<T> Get([FromRoute] EntityId id)
    {
        var entity = await DbContext.GetByIdAuthorized<T>(id, asNoTracking: true);
        return entity;
    }

    [HttpPost]
    [ProducesResponseType(HttpStatus.Created)]
    public async Task<ActionResult<EntityId>> Post([FromBody] T entity)
    {
        DbContext.Add(entity);
        await DbContext.SaveChangesAsync();
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
    public async Task Put([FromRoute] EntityId id, [FromBody] T entity)
    {
        if (entity.Id != id)
        {
            throw new ApiResponseException(HttpStatus.BadRequest, "Id in body and in URL must be the same.");
        }

        await UpdateRecursively(entity, visitedEntities: new HashSet<EntityId>());

        await DbContext.SaveChangesAsync();
    }

    [HttpDelete("{id}")]
    public async Task Delete([FromRoute] EntityId id)
    {
        var entity = await DbContext.GetByIdAuthorized<T>(id, asNoTracking: false);
        DbContext.Remove(entity);
        await DbContext.SaveChangesAsync();
    }

    private async Task UpdateRecursively(EntityBase entity, ISet<EntityId> visitedEntities)
    {
        // Prevent looping arround circular references.
        if (!visitedEntities.Add(entity.Id))
            return;

        EntityBase existingEntity;
        try
        {
            existingEntity = await DbContext.GetByIdAuthorized(entity.GetType(), entity.Id, asNoTracking: false);
        }
        catch (ApiResponseException ex) when(ex.HttpStatus == HttpStatus.NotFound)
        {
            throw new ApiResponseException(HttpStatus.Conflict, $"Entity {entity.Id} does not exist in the database. It may have been deleted. Adding new entities is not supported by the update endpoint.");
        }

        if (existingEntity.Version != entity.Version)
            throw new ApiResponseException(HttpStatus.Conflict, $"Entity {entity.Id} has a version conflict. Version in request: {entity.Version}. Version in database: {existingEntity.Version}.");

        // Update child entities that can be reach via navigation properties
        foreach (var navProp in DbContext.Entry(existingEntity).Navigations.Select(nav => nav.Metadata.PropertyInfo))
        {
            if (navProp is null)
                continue;

            
            var childEntity = navProp.GetValue(entity);

            // Only update children that are supplied in the request
            if (childEntity is null)
                continue;

            // If child is a collection of EntityBase we update them individually
            if (childEntity is IEnumerable collection)
            {
                var collectionTypeArguments = collection.GetType().FindGenericArgumentsForGenericTypeDefinition(typeof(ICollection<>));
                if (collectionTypeArguments is not null && collectionTypeArguments[0].IsAssignableTo(typeof(EntityBase)))
                {
                    foreach(var item in collection)
                    {
                        if (item is EntityBase itemEntityBase)
                        {
                            await UpdateRecursively(itemEntityBase, visitedEntities);
                        }
                    }
                    continue;
                }
            }

            // Only uodate children that are of type EntityBase
            if (childEntity is not EntityBase childEntityBase)
                continue;

            await UpdateRecursively(childEntityBase, visitedEntities);
            
        }

        // Update the current entity values
        DbContext.Entry(existingEntity).CurrentValues.SetValues(entity);
    }
}
