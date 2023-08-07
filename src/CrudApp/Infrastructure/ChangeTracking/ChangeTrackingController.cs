using CrudApp.Infrastructure.Query;
using Microsoft.EntityFrameworkCore;

namespace CrudApp.Infrastructure.ChangeTracking;


public class ChangeTrackingController : QueryControllerBase<EntityChange>
{
    protected override IQueryable<EntityChange> GetQueryable(bool includeSoftDeleted)
    {
        return DbContext.Authorized<EntityChange>().Include(e => e.PropertyChanges).AsNoTracking().Select(e => e);
    }
}
