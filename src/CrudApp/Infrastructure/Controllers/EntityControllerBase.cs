using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudApp.Infrastructure.Controllers;

public abstract class EntityControllerBase<T> : QueryControllerBase<T> where T : EntityBase
{
    protected override IQueryable<T> GetQueryable(bool includeSoftDeleted) => DbContext.Authorized<T>(includeSoftDeleted);

    [HttpGet("{id}")]
    [ProducesResponseType((int)HttpStatus.Ok)]
    [ProducesResponseType((int)HttpStatus.Forbidden)]
    [ProducesResponseType((int)HttpStatus.NotFound)]
    public async Task<ActionResult<T>> Get([FromRoute] EntityId id)
    {
        var entity = await DbContext.Authorized<T>().FirstAsync(e => e.Id == id);
        if (entity is null)
        {
            var exists = await DbContext.All<T>().AnyAsync(e => e.Id == id);
            if (exists)
                return Forbid();
            return NotFound();
        }
        return entity;
    }

    [HttpPost]
    [ProducesResponseType((int)HttpStatus.Created)]
    public async Task<ActionResult<T>> Post([FromBody] T entity)
    {
        DbContext.Add(entity);
        await DbContext.SaveChangesAsync();
        // We do not return the created entity.
        // We return a response with a location header where the entity can be fetched.
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, null);
    }

    [HttpPut("{id}")]
    [ProducesResponseType((int)HttpStatus.NoContent)]
    [ProducesResponseType((int)HttpStatus.Forbidden)]
    [ProducesResponseType((int)HttpStatus.NotFound)]
    public async Task<ActionResult> Put([FromRoute] EntityId id, [FromBody] T entity)
    {
        if (entity.Id != id)
        {
            return BadRequest("Id in body and in URL must be the same.");
        }
        var existing = await DbContext.Authorized<T>().FirstOrDefaultAsync(e => e.Id == id);
        if (existing is null)
        {
            var exists = await DbContext.All<T>().AnyAsync(e => e.Id == id);
            if (exists)
                return Forbid();
            return NotFound();
        }
        DbContext.Update(entity);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatus.NoContent)]
    [ProducesResponseType((int)HttpStatus.Forbidden)]
    [ProducesResponseType((int)HttpStatus.NotFound)]
    public async Task<ActionResult> Delete([FromRoute] EntityId id)
    {
        var entity = await DbContext.Authorized<T>().FirstOrDefaultAsync(e => e.Id == id);
        if (entity is null)
        {
            var exists = await DbContext.All<T>().AnyAsync(e => e.Id == id);
            if (exists)
                return Forbid();
            return NotFound();
        }
        DbContext.Remove(entity);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }
}
