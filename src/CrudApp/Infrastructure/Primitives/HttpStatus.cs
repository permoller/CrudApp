using System.Net;

namespace CrudApp.Infrastructure.Primitives;

/// <summary>
/// These are the HTTP status codes that are returned by the application.
/// Note that the ASP.NET pipeline itself and external gateways/proxies may return other status codes.
/// </summary>
public static class HttpStatus
{
    public static readonly int[] UsedReturnStatusCodes = { Ok, Created, NoContent, BadRequest, Unauthorized, Forbidden, NotFound, Conflict, InternalServerError };

    public const int Ok = (int)HttpStatusCode.OK;

    public const int Created = (int)HttpStatusCode.Created;

    public const int NoContent = (int)HttpStatusCode.NoContent;

    /// <summary>
    /// Indicates there is a problem with the request, like incorretly formatted json or validation problems.
    /// </summary>
    public const int BadRequest = (int)HttpStatusCode.BadRequest;

    /// <summary>
    /// Indicates that the client is not authenticated, but it is required by the operation.
    /// </summary>
    public const int Unauthorized = (int)HttpStatusCode.Unauthorized;

    /// <summary>
    /// Indicates that the client is authenticated, but not autorized to perform the operation.
    /// </summary>
    public const int Forbidden = (int)HttpStatusCode.Forbidden;

    /// <summary>
    /// Indicates the client is trying to access a resource that does not exists (or has been soft-deleted).
    /// </summary>
    public const int NotFound = (int)HttpStatusCode.NotFound;

    /// <summary>
    /// Indicates that an entity being updated has been changed since it was read.
    /// Like if an entity has been changed since the client read it and is trying to update it.
    /// </summary>
    public const int Conflict = (int)HttpStatusCode.Conflict;

    /// <summary>
    /// Indicates an unexpected error happend at the server.
    /// </summary>
    public const int InternalServerError = (int)HttpStatusCode.InternalServerError;
}
