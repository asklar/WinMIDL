using Midl.Lsp.Protocol;

namespace Midl.Lsp;

/// <summary>
/// Fixed-size ring buffer that records timestamped diagnostic events.
/// Acts as a flight recorder for the LSP server — always running, near-zero overhead.
/// </summary>
public sealed class DiagnosticEventLog
{
    private readonly Entry[] _buffer;
    private int _head;
    private int _count;

    public DiagnosticEventLog(int capacity = 500)
    {
        _buffer = new Entry[capacity];
    }

    /// <summary>Record a diagnostic event.</summary>
    public void Log(string category, string message, Dictionary<string, string>? metadata = null)
    {
        lock (_buffer)
        {
            _buffer[_head] = new Entry(DateTime.UtcNow, category, message, metadata);
            _head = (_head + 1) % _buffer.Length;
            if (_count < _buffer.Length) _count++;
        }
    }

    /// <summary>Returns all events in chronological order.</summary>
    public IReadOnlyList<Entry> GetEvents()
    {
        lock (_buffer)
        {
            var result = new Entry[_count];
            if (_count < _buffer.Length)
            {
                Array.Copy(_buffer, 0, result, 0, _count);
            }
            else
            {
                // Ring wrapped — oldest is at _head
                var tail = _buffer.Length - _head;
                Array.Copy(_buffer, _head, result, 0, tail);
                Array.Copy(_buffer, 0, result, tail, _head);
            }
            return result;
        }
    }

    /// <summary>Convert to source-gen-compatible DiagnosticEvent array.</summary>
    public DiagnosticEvent[] ToArray()
    {
        var events = GetEvents();
        var result = new DiagnosticEvent[events.Count];
        for (int i = 0; i < events.Count; i++)
        {
            var e = events[i];
            result[i] = new DiagnosticEvent(
                e.Timestamp.ToString("o"),
                e.Category,
                e.Message,
                e.Metadata is { Count: > 0 } ? e.Metadata : null);
        }
        return result;
    }

    public readonly record struct Entry(
        DateTime Timestamp,
        string Category,
        string Message,
        Dictionary<string, string>? Metadata);

    // Well-known categories
    public static class Categories
    {
        public const string Init = "init";
        public const string Scan = "scan";
        public const string Compile = "compile";
        public const string Memory = "memory";
        public const string Watcher = "watcher";
        public const string Error = "error";
    }
}
