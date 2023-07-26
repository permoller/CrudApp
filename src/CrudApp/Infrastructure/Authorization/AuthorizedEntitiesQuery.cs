﻿using CrudApp.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;

namespace CrudApp.Infrastructure.Authorization;

public static class AuthorizedEntitiesQuery
{
    public static IQueryable<T> Authorized<T>(this CrudAppDbContext dbContext, bool includeSoftDeleted = false) where T : EntityBase
    {
        var authUserId = AuthorizationContext.Current?.User.Id ?? throw new ProblemDetailsException(HttpStatus.Unauthorized);
        var authorizedEntityIds = dbContext.All<User>(includeSoftDeleted)
            .Where(u => u.Id == authUserId)
            .SelectMany(u => u.AuthorizationGroupMemberships
            .SelectMany(m => m.AuthorizationGroup.AuthorizationGroupEntities
            .Select(e => e.Id)));

        return dbContext.All<T>(includeSoftDeleted).Where(t => authorizedEntityIds.Contains(t.Id) && !t.IsSoftDeleted);
    }

    public static async Task<T> GetAuthorized<T>(this CrudAppDbContext dbContext, EntityId id, bool asNoTracking) where T : EntityBase
    {
        var queryable = dbContext.Authorized<T>();
        if (asNoTracking)
            queryable = queryable.AsNoTracking();
        var entity = queryable.FirstOrDefault(e => e.Id == id);
        if (entity is null)
        {
            var exists = await dbContext.All<T>().AnyAsync(e => e.Id == id);
            if (exists)
                throw new ProblemDetailsException(HttpStatus.Forbidden);
            throw new ProblemDetailsException(HttpStatus.NotFound);
        }
        return entity;
    }
}