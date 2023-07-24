using CrudApp.Infrastructure.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CrudApp.Infrastructure.ChangeTracking;

public class EntityChangeEventDto
{
    [Key]
    public EntityId EntityChangeEventId { get; set; }
    public EntityChangeEvent EntityChangeEvent { get; set; }
    public List<PropertyChangeEvent> PropertyChangeEvents { get; set; }
}

[ApiController]
public class ChangeTrackingController : QueryControllerBase<EntityChangeEventDto>
{
    protected override IQueryable<EntityChangeEventDto> GetQueryable(bool includeSoftDeleted)
    {
        var query = from e in DbContext.Authorized<EntityChangeEvent>()
                    select new EntityChangeEventDto
                    {
                        EntityChangeEventId = e.Id,
                        EntityChangeEvent = e,
                        PropertyChangeEvents = DbContext.Authorized<PropertyChangeEvent>(includeSoftDeleted).Where(p => p.EntityChangeEventId == e.Id).ToList()
                    };
        return query.AsNoTracking();
    }

}
