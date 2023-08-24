using CrudApp.Infrastructure.Query;

namespace CrudApp.Infrastructure.ChangeTracking;


public sealed class EntityChangeDto
{
    public EntityChange EntityChange { get; set; } = null!;
    public List<PropertyChange> PropertyChanges { get; set; } = null!;
}

public class ChangeTrackingController : QueryControllerBase<EntityChangeDto>
{
    protected override IQueryable<EntityChangeDto> GetQueryable(bool includeSoftDeleted)
    {
        var query = from e in DbContext.Authorized<EntityChange>()
                    select new EntityChangeDto
                    {
                        EntityChange = e,
                        PropertyChanges = DbContext.Authorized<PropertyChange>(includeSoftDeleted).Where(p => p.EntityChangeId == e.Id).ToList()
                    };
        return query;
    }
}
