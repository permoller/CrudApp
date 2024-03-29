﻿
using System.Text.RegularExpressions;

namespace CrudApp.Infrastructure.Primitives;


public partial class Error
{
    //
    // 400 Bad request
    //
    public sealed class ValidationFailed(Dictionary<string, string[]> errors) : Error(HttpStatus.BadRequest, errors: errors);
    public sealed class SoftDeleteCannotBeSetDirectly(Type entityType, EntityId entityId, Exception? exception = default) : Error(HttpStatus.BadRequest, exception, new DataDictionary().Add(entityType).Add(entityId));
    public sealed class VersionCannotBeSetDirectly(Type entityType, EntityId entityId, long version, Exception? exception = default) : Error(HttpStatus.BadRequest, exception, new DataDictionary().Add(entityType).Add(entityId).Add(version));
    public sealed class CannotCreateEntityWithSameIdAsExistingEntity(Type entityType, EntityId entityId, Exception? exception = default) : Error(HttpStatus.BadRequest, exception, new DataDictionary().Add(entityType).Add(entityId));
    public sealed class InconsistentEntityIdInRequest(Type entityType, EntityId entityIdInPath, EntityId entityIdInBody, Exception? exception = default) : Error(HttpStatus.BadRequest, exception, new DataDictionary().Add(entityType).Add(entityIdInPath).Add(entityIdInBody));
    public sealed class CannotGetDeletedEntity(Type entityType, EntityId entityId, Exception? exception = default) : Error(HttpStatus.BadRequest, exception, new DataDictionary().Add(entityType).Add(entityId));
    public sealed class CannotUpdateDeletedEntity(Type entityType, EntityId entityId, Exception? exception = default) : Error(HttpStatus.BadRequest, exception, new DataDictionary().Add(entityType).Add(entityId));
    public sealed class EntityAlreadyDeleted(Type entityType, EntityId entityId, Exception? exception = default) : Error(HttpStatus.BadRequest, exception, new DataDictionary().Add(entityType).Add(entityId));
    public sealed class InvalidFilterFormat(string filter, Exception? exception = default) : Error(HttpStatus.BadRequest, exception, new DataDictionary().Add(filter));
    public sealed class CannotConvertValueInFilterToTheExpectedType(string value, Type expectedType, Exception? exception = default) : Error(HttpStatus.BadRequest, exception, new DataDictionary().Add(value).Add(expectedType));
    public sealed class InvalidOperatorInFilter(string filterOperator, Exception? exception = default) : Error(HttpStatus.BadRequest, exception, new DataDictionary().Add(filterOperator));
    public sealed class OperatorCannotBeUsedOnTheValueType(string filterOperator, Type valueType, Exception? exception = default) : Error(HttpStatus.BadRequest, exception, new DataDictionary().Add(filterOperator).Add(valueType));
    public sealed class OrderByIsRequiredWhenUsingSkipAndTake(Exception? exception = default) : Error(HttpStatus.BadRequest, exception);
    public sealed class InvalidOrderByFormat(string orderBy, Exception? exception = default) : Error(HttpStatus.BadRequest, exception, new DataDictionary().Add(orderBy));
    public sealed class PropertyNotFoundOnType(string property, Type type, Exception? exception = default) : Error(HttpStatus.BadRequest, exception, new DataDictionary().Add(property).Add(type));
    
    //
    // 401 Unauthorized
    //
    public sealed class Unauthorized(Exception? exception = default) : Error(HttpStatus.Unauthorized, exception);

    //
    // 403 Forbidden
    //
    public sealed class AccessDeniedToEntity(Type entityType, EntityId entityId, Exception? exception = default) : Error(HttpStatus.Forbidden, exception, new DataDictionary().Add(entityType).Add(entityId));

    //
    // 404 Not found
    //
    public sealed class NotFound(Exception? exception = default) : Error(HttpStatus.NotFound, exception);
    public sealed class EntityNotFound(Type entityType, EntityId entityId, Exception? exception = default) : Error(HttpStatus.NotFound, exception, new DataDictionary().Add(entityType).Add(entityId));
    
    //
    // 409 Conflict
    //
    public sealed class EntityVersionInRequestDoesNotMatchVersionInDatabase(Type entityType, long versionInRequest, long versionInDatabase, Exception? exception = default) : Error(HttpStatus.Conflict, exception, new DataDictionary().Add(entityType).Add(versionInRequest).Add(versionInDatabase));
    
}
