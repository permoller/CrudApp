using CrudApp.Infrastructure.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudApp.Infrastructure.Entity;


public abstract class EntityControllerBase<T> : QueryControllerBase<T> where T : EntityBase
{
    protected override IQueryable<T> GetQueryable(bool includeSoftDeleted) => DbContext.Authorized<T>(includeSoftDeleted);

    [HttpGet("{id}")]
    public async Task<T> Get([FromRoute] EntityId id)
    {
        var entity = await DbContext.GetAuthorized<T>(id, asNoTracking: true);
        return entity;
    }

    [HttpPost]
    [ProducesResponseType((int)HttpStatus.Created)]
    public async Task<ActionResult<EntityId>> Post([FromBody] T entity)
    {
        DbContext.Add(entity);
        await DbContext.SaveChangesAsync();
        // We do not return the created entity.
        // We return a response with a location header where the entity can be fetched.
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity.Id);
    }

    [HttpPut("{id}")]
    [ProducesResponseType((int)HttpStatus.NoContent)]
    public async Task<ActionResult> Put([FromRoute] EntityId id, [FromBody] T entity)
    {
        if (entity.Id != id)
        {
            throw new ApiResponseException(HttpStatus.BadRequest, "Id in body and in URL must be the same.");
        }
        var existingEntity = await DbContext.GetAuthorized<T>(id, asNoTracking: false);
        DbContext.Entry(existingEntity).CurrentValues.SetValues(entity);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatus.NoContent)]
    public async Task<ActionResult> Delete([FromRoute] EntityId id)
    {
        var entity = await DbContext.GetAuthorized<T>(id,asNoTracking: false);
        DbContext.Remove(entity);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }
}
