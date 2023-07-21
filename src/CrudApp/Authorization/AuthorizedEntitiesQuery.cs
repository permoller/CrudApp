using CrudApp.Users;
using Microsoft.EntityFrameworkCore;

namespace CrudApp.Authorization;

public static class AuthorizedEntitiesQuery
{
    public static IQueryable<T> Authorized<T>(this CrudAppDbContext dbContext, bool includeSoftDeleted = false) where T : EntityBase
    {
        var authUserId = AuthorizationContext.Current?.User.Id ?? Guid.NewGuid();
        var authorizedEntityIds = dbContext.All<User>(includeSoftDeleted)
            .Where(u => u.Id == authUserId)
            .SelectMany(u => u.AuthorizationGroupMemberships
            .SelectMany(m => m.AuthorizationGroup.AuthorizationGroupEntities
            .Select(e => e.Id)));

        return dbContext.All<T>(includeSoftDeleted).Where(t => authorizedEntityIds.Contains(t.Id) && !t.IsSoftDeleted);
    }
}
