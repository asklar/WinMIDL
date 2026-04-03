using Midl.Lsp.Protocol;

namespace Midl.Lsp;

/// <summary>
/// Fixed-size ring buffer that records timestamped diagnostic events.
/// Acts as a flight recorder for the LSP server — always running, near-zero overhead.
/// Also writes events to a persistent log file so the VS Code extension can read them
/// even after the server crashes or is killed.
/// </summary>
public sealed class DiagnosticEventLog : IDisposable
{
    private readonly Entry[] _buffer;
    private int _head;
    private int _count;
    private StreamWriter? _fileWriter;
    private readonly object _lock = new();

    /// <summary>Path to the persistent log file, or null if file logging is disabled.</summary>
    public string? LogFilePath { get; }

    private const long MaxLogFileSize = 1024 * 1024; // 1 MB — rotate after this

    public DiagnosticEventLog(int capacity = 500, string? logDirectory = null)
    {
        _buffer = new Entry[capacity];

        if (logDirectory != null)
        {
            try
            {
                Directory.CreateDirectory(logDirectory);
                LogFilePath = Path.Combine(logDirectory, "server.log");

                // Rotate if over size limit
                if (File.Exists(LogFilePath) && new FileInfo(LogFilePath).Length > MaxLogFileSize)
                {
                    var prev = LogFilePath + ".prev";
                    try { File.Delete(prev); } catch { }
                    try { File.Move(LogFilePath, prev); } catch { }
                }

                _fileWriter = new StreamWriter(LogFilePath, append: true) { AutoFlush = true };
            }
            catch { /* can't write logs — continue without file */ }
        }
    }

    /// <summary>Record a diagnostic event.</summary>
    public void Log(string category, string message, Dictionary<string, string>? metadata = null)
    {
        var entry = new Entry(DateTime.UtcNow, category, message, metadata);
        lock (_lock)
        {
            _buffer[_head] = entry;
            _head = (_head + 1) % _buffer.Length;
            if (_count < _buffer.Length) _count++;

            // Write to file
            if (_fileWriter != null)
            {
                try
                {
                    var meta = metadata is { Count: > 0 }
                        ? " " + string.Join(", ", metadata.Select(kv => $"{kv.Key}={kv.Value}"))
                        : "";
                    _fileWriter.WriteLine($"[{entry.Timestamp:o}] [{category}] {message}{meta}");
                }
                catch { /* don't let file I/O failures break the server */ }
            }
        }
    }

    /// <summary>Returns all events in chronological order.</summary>
    public IReadOnlyList<Entry> GetEvents()
    {
        lock (_lock)
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

    /// <summary>
    /// Read the persistent log file. Used by the extension when the server is not running.
    /// Returns the last N lines.
    /// </summary>
    public static string[] ReadLogFile(string logDirectory, int tailLines = 200)
    {
        var path = Path.Combine(logDirectory, "server.log");
        if (!File.Exists(path)) return [];
        try
        {
            var lines = File.ReadAllLines(path);
            return lines.Length > tailLines ? lines[^tailLines..] : lines;
        }
        catch { return []; }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _fileWriter?.Flush();
            _fileWriter?.Dispose();
            _fileWriter = null;
        }
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
