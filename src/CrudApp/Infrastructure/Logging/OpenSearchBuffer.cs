using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace CrudApp.Infrastructure.Logging;

/// <summary>
/// A buffer for the request to the bulk-endpoint in OpenSearch.
/// </summary>
public sealed class OpenSearchBuffer : ILogSink
{
    private MemoryStream _buffer = new();
    private readonly object _lock = new object();
    private readonly byte[] _newLineBytes = Encoding.UTF8.GetBytes(Environment.NewLine);
    private readonly byte[] _createDocumentBytes = Encoding.UTF8.GetBytes("{ \"create\": { } }" + Environment.NewLine);

    public void Write(LogEntry logEntry)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(logEntry);
        lock (_lock)
        {
            _buffer.Write(_createDocumentBytes);
            _buffer.Write(bytes);
            _buffer.Write(_newLineBytes);
        }
    }

    public bool TryGetStream([NotNullWhen(true)] out MemoryStream? stream)
    {
        if (_buffer.Length > 0)
        {
            stream = _buffer;
            lock (_lock)
                _buffer = new();

            stream.Position = 0;
            return true;
        }
        stream = null;
        return false;
    }
}
