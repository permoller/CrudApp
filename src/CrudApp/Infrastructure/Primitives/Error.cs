using System.Diagnostics;

namespace CrudApp.Infrastructure.Primitives;


/// <summary>
/// When an error is returned from a controller action, it gets translated to a ProblemDetails response.
/// </summary>
public readonly struct Error
{
    private readonly Dictionary<string, object?> _data = new();

    private Error(Exception? exception, ErrorHttpStatusCode errorStatus, string? type, string? title, string? details)
    {
        ErrorStatus = errorStatus;
        Type = type;
        Title = title;
        Detail = details;
        Exception = exception;
        _data.Add("activity-span-id", Activity.Current?.SpanId.ToHexString());
        _data.Add("actitivy-trace-id", Activity.Current?.TraceId.ToHexString());
    }

    public ErrorHttpStatusCode ErrorStatus { get; } // Translated to HTTP status code
    public string? Type { get; } // Short url-frindly name of the error type. Should be a static string. Defaults to the name of the HTTP status code
    public string? Title { get; } // Human readable one-liner describing the type. Should be a static string. It may be localized. Defaults to the reason phrase of the HTTP status code
    public string? Detail { get; } // Human readable text with information about the specific error instance. May be dynamically created with instance specific data. May be localized. May be null if the title says it all.
    public string? Instance { get; } = EntityBase.NewEntityId().ToString(); // Identification of the error instance.
    public Exception? Exception { get; } // If an exception caused the error it can be included in the error for debugging purposes.
    public IReadOnlyDictionary<string, object?> Data => _data; // Context specific data that may be extracted/used/displayed by the client. The data values should be small simple types. It should not be big objects.

    public static Error BadRequest(string type, string title, string? details) => new(null, ErrorHttpStatusCode.BadRequest, type, title, details);
    public static Error BadRequest(Exception? exception, string type, string title, string details) => new(exception, ErrorHttpStatusCode.BadRequest, type, title, details);
    public static Error Unauthorized(string? details) => new(null, ErrorHttpStatusCode.Unauthorized, null, null, details);
    public static Error Unauthorized(Exception? exception, string? details) => new(exception, ErrorHttpStatusCode.Unauthorized, null, null, details);
    public static Error Forbidden(string? details) => new(null, ErrorHttpStatusCode.Forbidden, null, null, details);
    public static Error Forbidden(Exception? exception, string? details) => new(exception, ErrorHttpStatusCode.Forbidden, null, null, details);
    public static Error NotFound(string? details) => new(null, ErrorHttpStatusCode.NotFound, null, null, details);
    public static Error NotFound(Exception? exception, string? details) => new(exception, ErrorHttpStatusCode.NotFound, null, null, details);
    public static Error Conflict(string? details) => new(null, ErrorHttpStatusCode.Conflict, null, null, details);
    public static Error Conflict(Exception? exception, string? details) => new(exception, ErrorHttpStatusCode.Conflict, null, null, details);
    public static Error InternalServerError(Exception? exception) => new(exception, ErrorHttpStatusCode.InternalServerError, null, null, null);
    public static Error InternalServerError(string? details) => new(null, ErrorHttpStatusCode.InternalServerError, null, null, details);
    public static Error InternalServerError(Exception? exception, string? details) => new(exception, ErrorHttpStatusCode.InternalServerError, null, null, details);

    public Error WithData(string key, object? value)
    {
        _data[key] = value;
        return this;
    }
}
