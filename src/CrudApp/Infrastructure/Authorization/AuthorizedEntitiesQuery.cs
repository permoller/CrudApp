using Microsoft.EntityFrameworkCore;

namespace CrudApp.Infrastructure.Authorization;

public static class AuthorizedEntitiesQuery
{
    public static IQueryable<T> Authorized<T>(this CrudAppDbContext dbContext, bool includeSoftDeleted = false) where T : EntityBase
    {
        var userId = AuthorizationContext.Current?.UserId ?? AuthenticationContext.Current?.UserId ?? throw new NotAuthenticatedException();

        var authorizedEntityIds = dbContext.All<AuthorizationGroupUserRelation>().Where(m => m.UserId == userId)
            .SelectMany(m => m.AuthorizationGroup!.AuthorizationGroupEntityRelations!.Select(e => e.Id));

        return dbContext.All<T>(includeSoftDeleted).Where(t => authorizedEntityIds.Contains(t.Id));
    }

    public static async Task<T> GetByIdAuthorized<T>(this CrudAppDbContext dbContext, EntityId id, bool asNoTracking) where T : EntityBase
    {
        var queryable = dbContext.Authorized<T>();
        if (asNoTracking)
            queryable = queryable.AsNoTracking();
        var entity = queryable.FirstOrDefault(e => e.Id == id);
        if (entity is null)
        {
            var exists = await dbContext.All<T>().AnyAsync(e => e.Id == id);
            if (exists)
                throw new NotAuthorizedException();
            throw new ApiResponseException(HttpStatus.NotFound);
        }
        return entity;
    }
}
