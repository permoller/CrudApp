using CrudApp.Infrastructure.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudApp.Infrastructure.Entities;

public abstract class EntityControllerBase<T> : QueryControllerBase<T> where T : EntityBase
{
    protected override IQueryable<T> GetQueryable(bool includeSoftDeleted) => DbContext.Authorized<T>(includeSoftDeleted);

    [HttpGet("{id}")]
    public async Task<T> Get([FromRoute] EntityId id, bool includeSoftDeleted = false, CancellationToken cancellationToken = default)
    {
        var entity = await DbContext.GetByIdAuthorized<T>(id, asNoTracking: true, cancellationToken);
        if (!includeSoftDeleted)
            entity.AssertNotDeleted();
        return entity;
    }

    [HttpPost]
    [ProducesResponseType(HttpStatus.Created)]
    public async Task<ActionResult<EntityId>> Post([FromBody] T entity, CancellationToken cancellationToken = default)
    {
        if (entity.IsSoftDeleted)
            throw new ApiResponseException(HttpStatus.BadRequest, $"{nameof(entity.IsSoftDeleted)} can not be set to true when creating an entity. That can only be done using the DELETE endpoint.");

        if (entity.Id != default)
        {
            var existingEntity = await DbContext.All<T>(includeSoftDeleted: true).FirstOrDefaultAsync(e => e.Id == entity.Id, cancellationToken);
            if (existingEntity is not null)
                throw new ApiResponseException(HttpStatus.BadRequest, $"{existingEntity.DisplayName} already exists with the same id {existingEntity.Id}.");
        }

        DbContext.Add(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
        // We do not return the created entity.
        // We return a response with a location header where the entity can be fetched.
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity.Id);
    }

    /// <summary>
    /// Updates the given entity.
    /// If the entity has navigation-properties with owned entities they will also be updated.
    /// </summary>
    [HttpPut("{id}")]
    public async Task Put([FromRoute] EntityId id, [FromBody] T entity, CancellationToken cancellationToken = default)
    {
        if (entity.Id != id)
            throw new ApiResponseException(HttpStatus.BadRequest, $"Id in body {entity.Id} and in URL {id} must be the same.");

        if (entity.IsSoftDeleted)
            throw new ApiResponseException(HttpStatus.BadRequest, $"{nameof(entity.IsSoftDeleted)} can not be set to true when updating and entity. That can only be done using the DELETE endpoint.");

        var existingEntity = await DbContext.GetByIdAuthorized(entity.GetType(), entity.Id, asNoTracking: false, cancellationToken);
        existingEntity.AssertNotDeleted();

        if (existingEntity.Version != entity.Version)
            throw new ApiResponseException(HttpStatus.Conflict, $"{entity.DisplayName} has a version conflict. Version in request: {entity.Version}. Version in database: {existingEntity.Version}.");

        DbContext.UpdateExistingEntity(existingEntity, entity);

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    [HttpDelete("{id}")]
    public async Task Delete([FromRoute] EntityId id, CancellationToken cancellationToken = default)
    {
        var entity = await DbContext.GetByIdAuthorized<T>(id, asNoTracking: false, cancellationToken);
        entity.AssertNotDeleted();
        entity.IsSoftDeleted = true;
        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
