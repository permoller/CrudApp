namespace CrudApp.Infrastructure.Controllers;

/// <summary>
/// These are the HTTP status codes that are returned by the application.
/// Note that the ASP.NET pipeline itself and external gateways/proxies may return other status codes.
/// </summary>
public enum HttpStatus
{
    Ok = 200,

    Created = 201,

    NoContent = 204,

    /// <summary>
    /// Indicates there is a problem with the request, like incorretly formatted json.
    /// </summary>
    BadRequest = 400,

    /// <summary>
    /// Indicates that the client is not authenticated, but it is required by the operation.
    /// </summary>
    Unauthorized = 401,

    /// <summary>
    /// Indicates that the client is authenticated, but not autorized to perform the operation.
    /// </summary>
    Forbidden = 403,

    /// <summary>
    /// Indicates the client is trying to access a resource that does not exists (or has been soft-deleted).
    /// </summary>
    NotFound = 404,

    /// <summary>
    /// Indicates that an entity being updated has been changed since it was read.
    /// Like if an entity has been changed since the client read it and is trying to update it.
    /// </summary>
    Conflict = 409,

    /// <summary>
    /// Indicates an unexpected error happend at the server.
    /// </summary>
    InternalServerError = 500
}
