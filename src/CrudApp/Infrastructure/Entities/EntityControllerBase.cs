using CrudApp.Infrastructure.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudApp.Infrastructure.Entities;

public abstract class EntityControllerBase<T> : QueryControllerBase<T> where T : EntityBase
{
    protected override IQueryable<T> GetQueryable(bool includeSoftDeleted) => DbContext.Authorized<T>(includeSoftDeleted);

    [HttpGet("{id}")]
    public async Task<Result<T>> Get([FromRoute] EntityId id, bool includeSoftDeleted = false, CancellationToken cancellationToken = default)
    {
        var result = await Result.From(id)
            .Select(id => DbContext.GetByIdAuthorized<T>(id, asNoTracking: true, cancellationToken))
            .Validate(entity => !includeSoftDeleted && entity.IsSoftDeleted ? new Error.CannotGetDeletedEntity(typeof(T), id) : null);

        return result;
    }

    [HttpPost]
    [ProducesResponseType(HttpStatus.Created)]
    public async Task<ActionResult<EntityId>> Post([FromBody] T entity, CancellationToken cancellationToken = default)
    {
        if (entity.IsSoftDeleted)
            return MapErrorToActionResult(new Error.SoftDeleteCannotBeSetDirectly(typeof(T), entity.Id));

        if (entity.Version != default)
            return MapErrorToActionResult(new Error.VersionCannotBeSetDirectly(typeof(T), entity.Id, entity.Version));

        if (entity.Id != default)
        {
            var existingEntity = await DbContext.All<T>(includeSoftDeleted: true).FirstOrDefaultAsync(e => e.Id == entity.Id, cancellationToken);
            if (existingEntity is not null)
                return MapErrorToActionResult(new Error.CannotCreateEntityWithSameIdAsExistingEntity(typeof(T), entity.Id));
        }

        DbContext.Add(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
               
        // We do not return the created entity.
        // We return a response with a location header where the entity can be fetched and the id in the body.
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity.Id);
    }

    /// <summary>
    /// Updates the given entity.
    /// If the entity has navigation-properties with owned entities they will also be updated.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<Result<Nothing>> Put([FromRoute] EntityId id, [FromBody] T entity, CancellationToken cancellationToken = default)
    {
        var result = await Result.From(entity)
            .Validate(entity => entity.Id != id ? new Error.InconsistentEntityIdInRequest(typeof(T), entityIdInPath: id, entityIdInBody: entity.Id) : null)
            .Select(entity => DbContext.GetByIdAuthorized<T>(entity.Id, asNoTracking: false, cancellationToken))
            .Validate(dbEntity => dbEntity.Version != entity.Version ? new Error.EntityVersionInRequestDoesNotMatchVersionInDatabase(typeof(T), versionInRequest: entity.Version, versionInDatabase: dbEntity.Version) : null)
            .Validate(dbEntity => dbEntity.IsSoftDeleted ? new Error.CannotUpdateDeletedEntity(typeof(T), dbEntity.Id) : null)
            .Validate(dbEntity => entity.IsSoftDeleted ? new Error.SoftDeleteCannotBeSetDirectly(typeof(T), entity.Id) : null)
            .Use(dbEntity => DbContext.UpdateExistingEntity(dbEntity, entity))
            .Use(dbEntity => DbContext.SaveChangesAsync(cancellationToken));

        return result;
    }

    //[HttpPut("{id}")]
    //public async Task<Result<Nothing>> Put4([FromRoute] EntityId id, [FromBody] T entity, CancellationToken cancellationToken = default)
    //{
    //    if (entity.Id != id)
    //        return new Error.InconsistentEntityIdInRequest(typeof(T), entityIdInPath: id, entityIdInBody: entity.Id);

    //    var result =
    //        from requestEntity in Result.From(entity)
    //        .Validate(e => e.Id != id ? new Error.InconsistentEntityIdInRequest(typeof(T), entityIdInPath: id, entityIdInBody: e.Id) : null)
    //        from dbEntity in DbContext.GetByIdAuthorized<T>(requestEntity.Id, asNoTracking: false, cancellationToken)
    //        .Validate(dbEntity => dbEntity.Version != entity.Version ? new Error.EntityVersionInRequestDoesNotMatchVersionInDatabase(typeof(T), versionInRequest: entity.Version, versionInDatabase: dbEntity.Version) : null)
    //        .Validate(dbEntity => dbEntity.IsSoftDeleted ? new Error.CannotUpdateDeletedEntity(typeof(T), dbEntity.Id) : null)
    //        .Validate(dbEntity => entity.IsSoftDeleted ? new Error.SoftDeleteCannotBeSetDirectly(typeof(T), entity.Id) : null)
    //        .Use(dbEntity => DbContext.UpdateExistingEntity(dbEntity, entity))
    //        select DbContext.SaveChangesAsync(cancellationToken);

    //    return await result;
    //}


    //[HttpPut("{id}")]
    //public async Task<Result<Nothing>> Put2([FromRoute] EntityId id, [FromBody] T entity, CancellationToken cancellationToken = default)
    //{
    //    if (entity.Id != id) return new Error.InconsistentEntityIdInRequest(typeof(T), entityIdInPath: id, entityIdInBody: entity.Id);
    //    if (entity.IsSoftDeleted) return new Error.CannotDeleteEntityWhileUpdating(typeof(T), entity.Id);
    //    var dbEntityResult = await DbContext.GetByIdAuthorized<T>(entity.Id, asNoTracking: false, cancellationToken);
    //    if (dbEntityResult.TryGetError(out var error, out var dbEntity)) return error;
    //    if (dbEntity.Version != entity.Version) return new Error.EntityVersionInRequestDoesNotMatchVersionInDatabase(typeof(T), versionInRequest: entity.Version, versionInDatabase: dbEntity.Version);
    //    if (dbEntity.IsSoftDeleted) return new Error.CannotUpdateEntityThatHasBeenDeleted(typeof(T), dbEntity.Id);
    //    DbContext.UpdateExistingEntity(dbEntity, entity);
    //    await DbContext.SaveChangesAsync(cancellationToken);
    //    return Result.FromNothing();
    //}

    //[HttpPut("{id}")]
    //public async Task<Result<Nothing>> Put3([FromRoute] EntityId id, [FromBody] T entity, CancellationToken cancellationToken = default)
    //{
    //    if (entity.Id != id)
    //        return new Error.InconsistentEntityIdInRequest(typeof(T), entityIdInPath: id, entityIdInBody: entity.Id);

    //    if (entity.IsSoftDeleted)
    //        return new Error.CannotDeleteEntityWhileUpdating(typeof(T), entity.Id);

    //    var dbEntityResult = await DbContext.GetByIdAuthorized<T>(entity.Id, asNoTracking: false, cancellationToken);
    //    if (dbEntityResult.TryGetError(out var error, out var dbEntity))
    //        return error;

    //    if (dbEntity.Version != entity.Version)
    //        return new Error.EntityVersionInRequestDoesNotMatchVersionInDatabase(typeof(T), versionInRequest: entity.Version, versionInDatabase: dbEntity.Version);

    //    if (dbEntity.IsSoftDeleted)
    //        return new Error.CannotUpdateEntityThatHasBeenDeleted(typeof(T), dbEntity.Id);

    //    DbContext.UpdateExistingEntity(dbEntity, entity);
    //    await DbContext.SaveChangesAsync(cancellationToken);
    //    return Result.FromNothing();
    //}

    /// <summary>
    /// Marks an entity as deleted.
    /// If <paramref name="version"/> is provided and it does not match the version in the database, the operation fails.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<Result<Nothing>> Delete([FromRoute] EntityId id, long? version, CancellationToken cancellationToken = default)
    {
        var result = await Result.From(id)
            .Select(id => DbContext.GetByIdAuthorized<T>(id, asNoTracking: false, cancellationToken))
            .Validate(dbEntity => version is not null && dbEntity.Version != version.Value ? new Error.EntityVersionInRequestDoesNotMatchVersionInDatabase(typeof(T), versionInRequest: version.Value, versionInDatabase: dbEntity.Version) : null)
            .Validate(dbEntity => dbEntity.IsSoftDeleted ? new Error.EntityAlreadyDeleted(typeof(T), dbEntity.Id) : null)
            .Use(dbEntity => dbEntity.IsSoftDeleted = true)
            .Use(dbEntity => DbContext.SaveChangesAsync(cancellationToken));

        return result;
    }
}
