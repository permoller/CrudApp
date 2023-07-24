﻿using System.Runtime.Serialization;

namespace CrudApp.ErrorHandling;

/// <summary>
/// Indicates the request did not succeed.
/// Contains that HTTP status code and optionally a message with details to be returned to the client.
/// </summary>
[Serializable]
public sealed class HttpStatusException : Exception
{
    public HttpStatusException(HttpStatus clientError) : base()
    {
        HttpStatus = clientError;
    }

    public HttpStatusException(HttpStatus clientError, string? message) : base(message)
    {
        HttpStatus = clientError;
    }

    public HttpStatusException(HttpStatus error, string? message, Exception? innerException) : base(message, innerException)
    {
        HttpStatus = error;
    }

    private HttpStatusException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        HttpStatus = (HttpStatus)info.GetValue(nameof(HttpStatus), typeof(HttpStatus))!;
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(HttpStatus), HttpStatus);
        base.GetObjectData(info, context);
    }

    public HttpStatus HttpStatus { get; }
}
