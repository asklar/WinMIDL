using System.Diagnostics;

namespace Midl.Syntax;

/// <summary>
/// Structured debug tracing for the MIDL compiler.
/// Enable via MIDL_DEBUG=1 environment variable, /debug CLI switch, or programmatically.
/// Output goes to a log file (MIDL_DEBUG_LOG env var) or stderr.
/// </summary>
public static class MidlTrace
{
    private static TextWriter? _writer;
    private static bool _enabled;
    private static readonly object _lock = new();

    /// <summary>Is tracing enabled?</summary>
    public static bool IsEnabled => _enabled;

    /// <summary>Enable tracing. Call once at startup.</summary>
    public static void Enable(string? logFilePath = null)
    {
        lock (_lock)
        {
            _enabled = true;
            if (logFilePath != null)
            {
                _writer = new StreamWriter(logFilePath, append: true) { AutoFlush = true };
            }
            else
            {
                _writer = Console.Error;
            }
            Write("TRACE", "Tracing enabled");
        }
    }

    /// <summary>Initialize from environment. Call at startup.</summary>
    public static void InitFromEnvironment()
    {
        var debugEnv = Environment.GetEnvironmentVariable("MIDL_DEBUG");
        if (debugEnv is "1" or "true" or "yes")
        {
            var logPath = Environment.GetEnvironmentVariable("MIDL_DEBUG_LOG");
            Enable(logPath);
        }
    }

    /// <summary>Disable tracing and flush.</summary>
    public static void Disable()
    {
        lock (_lock)
        {
            if (_writer != null && _writer != Console.Error)
            {
                _writer.Flush();
                _writer.Dispose();
            }
            _writer = null;
            _enabled = false;
        }
    }

    /// <summary>Write a trace message with category.</summary>
    public static void Write(string category, string message)
    {
        if (!_enabled) return;
        lock (_lock)
        {
            _writer?.WriteLine($"[MIDL:{category}] {message}");
        }
    }

    /// <summary>Write a trace with format args (avoids allocation when disabled).</summary>
    public static void Write(string category, string format, params object[] args)
    {
        if (!_enabled) return;
        Write(category, string.Format(format, args));
    }

    // Convenience methods for each pipeline stage
    public static void Lexer(string message) => Write("LEX", message);
    public static void Parser(string message) => Write("PARSE", message);
    public static void Binder(string message) => Write("BIND", message);
    public static void CodeGen(string message) => Write("CODEGEN", message);
    public static void Lsp(string message) => Write("LSP", message);
    public static void Cli(string message) => Write("CLI", message);
}
