using CrudApp.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace CrudApp.Controllers;

public abstract class EntityControllerBase<T> : QueryControllerBase<T> where T : EntityBase
{
    protected override IQueryable<T> GetQueryable() => DbContext.Authorized<T>();

    [HttpGet("{id}")]
    [ProducesResponseType(Status200OK)]
    [ProducesResponseType(Status403Forbidden)]
    [ProducesResponseType(Status404NotFound)]
    public async Task<ActionResult<T>> Get([FromRoute] EntityId id)
    {
        var entity = await DbContext.Authorized<T>().FirstAsync(e => e.Id == id);
        if (entity is null)
        {
            var exists = await DbContext.Set<T>().AnyAsync(e => e.Id == id);
            if (exists)
                return Forbid();
            return NotFound();
        }
        return entity;
    }

    [HttpPost]
    [ProducesResponseType(Status201Created)]
    public async Task<ActionResult<T>> Post([FromBody] T entity)
    {
        DbContext.Set<T>().Add(entity);
        await DbContext.SaveChangesAsync();
        // We do not return the created entity.
        // We return a response with a location header where the entity can be fetched.
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, null);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(Status204NoContent)]
    [ProducesResponseType(Status403Forbidden)]
    [ProducesResponseType(Status404NotFound)]
    public async Task<ActionResult> Put([FromRoute] EntityId id, [FromBody] T entity)
    {
        if (entity.Id != id)
        {
            ModelState.AddModelError(nameof(entity.Id), "Id in body and in URL must be the same.");
            return ValidationProblem(ModelState);
        }
        var existing = await DbContext.Authorized<T>().FirstOrDefaultAsync(e => e.Id == id);
        if (existing is null)
        {
            var exists = await DbContext.Set<T>().AnyAsync(e => e.Id == id);
            if (exists)
                return Forbid();
            return NotFound();
        }
        DbContext.Set<T>().Update(entity);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(Status204NoContent)]
    [ProducesResponseType(Status403Forbidden)]
    [ProducesResponseType(Status404NotFound)]
    public async Task<ActionResult> Delete([FromRoute] EntityId id)
    {
        var entity = await DbContext.Authorized<T>().FirstOrDefaultAsync(e => e.Id == id);
        if (entity is null)
        {
            var exists = await DbContext.Set<T>().AnyAsync(e => e.Id == id);
            if (exists)
                return Forbid();
            return NotFound();
        }
        DbContext.Set<T>().Remove(entity);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }
}
