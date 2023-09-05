using System.Text.Json.Serialization;

namespace CrudApp.Infrastructure.Logging;

/// <summary>
/// Based on a subset of the Elastic Common Schema (https://www.elastic.co/guide/en/ecs/current/ecs-field-reference.html)
/// </summary>
public sealed class LogEntry
{
    [JsonPropertyName("@timestamp")]
    public DateTimeOffset? Timestamp { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("labels")]
    public Dictionary<string, string?>? Labels { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("log")]
    public Log? Log { get; set; }

    [JsonPropertyName("trace")]
    public Trace? Trace { get; set; }

    [JsonPropertyName("span")]
    public Span? Span { get; set; }

    [JsonPropertyName("error")]
    public Error? Error { get; set; }

    /// <summary>
    /// Not part of ECS
    /// </summary>
    [JsonPropertyName("scopes")]
    public List<Scope>? Scopes { get; set; }
}

public class Log
{
    [JsonPropertyName("level")]
    public string? Level { get; set; }

    [JsonPropertyName("logger")]
    public string? Logger { get; set; }

    /// <summary>
    /// Not part of ECS
    /// </summary>
    [JsonPropertyName("state")]
    public Dictionary<string, string?>? State { get; set; }
}

public class Trace
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public class Span
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public class Error
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("stack_trace")]
    public string? StackTrace { get; set; }
}

/// <summary>
/// Not part of ECS
/// </summary>
public class Scope
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("state")]
    public Dictionary<string, string?>? State { get; set; }
}