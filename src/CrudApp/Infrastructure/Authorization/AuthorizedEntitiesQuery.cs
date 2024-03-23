using Microsoft.EntityFrameworkCore;

namespace CrudApp.Infrastructure.Authorization;

public static class AuthorizedEntitiesQuery
{
    public static IQueryable<T> Authorized<T>(this CrudAppDbContext dbContext, bool includeSoftDeleted = false) where T : EntityBase
    {
        return dbContext.All<T>(includeSoftDeleted);
        // TODO: Include this when authorization data is actually created... currently no one has access to enything
        //var userId = AuthorizationContext.Current?.UserId ?? AuthenticationContext.Current?.UserId ?? throw new NotAuthenticatedException();

        //var authorizedEntityIds =
        //    from m in dbContext.All<AuthorizationGroupUserRelation>().Where(m => m.UserId == userId)
        //    join e in dbContext.All<AuthorizationGroupEntityRelation>() on m.AuthorizationGroupId equals e.AuthorizationGroupId
        //    select e.EntityId;

        //return dbContext.All<T>(includeSoftDeleted).Where(t => authorizedEntityIds.Contains(t.Id));
    }
    public static IQueryable<EntityBase> Authorized(this CrudAppDbContext dbContext, Type entityType, bool includeSoftDeleted = false)
    {
        // TODO: Implement authorization
        return dbContext.All(entityType, includeSoftDeleted);
    }

    public static async Task<Result<T>> GetByIdAuthorized<T>(this CrudAppDbContext dbContext, EntityId id, bool asNoTracking, CancellationToken cancellationToken) where T : EntityBase
    {
        var queryable = dbContext.Authorized<T>(includeSoftDeleted: true);
        if (asNoTracking)
            queryable = queryable.AsNoTracking();
        var entity = await queryable.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null)
        {
            var exists = await dbContext.All<T>(includeSoftDeleted: true).AnyAsync(e => e.Id == id, cancellationToken);
            if (exists)
                return new Error.AccessDeniedToEntity(typeof(T), id);
            return new Error.EntityNotFound(typeof(T), id);
        }
        return entity;
    }

    public static async Task<Result<EntityBase>> GetByIdAuthorized(this CrudAppDbContext dbContext, Type entityType, EntityId id, bool asNoTracking, CancellationToken cancellationToken)
    {
        var queryable = dbContext.Authorized(entityType, includeSoftDeleted: true);
        if (asNoTracking)
            queryable = queryable.AsNoTracking();
        var entity = await queryable.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null)
        {
            var exists = await dbContext.All(entityType, includeSoftDeleted: true).AnyAsync(e => e.Id == id, cancellationToken);
            if (exists)
                return new Error.AccessDeniedToEntity(entityType, id);
            return new Error.EntityNotFound(entityType, id);
        }
        return entity;
    }
}
