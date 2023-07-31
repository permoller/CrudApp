using CrudApp.Infrastructure.Query;
using System.ComponentModel.DataAnnotations;

namespace CrudApp.Infrastructure.ChangeTracking;

public class EntityChangeEventDto
{
    [Key]
    public EntityId EntityChangeEventId { get; set; }
    public EntityChange EntityChangeEvent { get; set; }
    public List<PropertyChange> PropertyChangeEvents { get; set; }
}

public class ChangeTrackingController : QueryControllerBase<EntityChangeEventDto>
{
    protected override IQueryable<EntityChangeEventDto> GetQueryable(bool includeSoftDeleted)
    {
        var query = from e in DbContext.Authorized<EntityChange>()
                    select new EntityChangeEventDto
                    {
                        EntityChangeEventId = e.Id,
                        EntityChangeEvent = e,
                        PropertyChangeEvents = DbContext.Authorized<PropertyChange>(includeSoftDeleted).Where(p => p.EntityChangeId == e.Id).ToList()
                    };
        return query;
    }
}
