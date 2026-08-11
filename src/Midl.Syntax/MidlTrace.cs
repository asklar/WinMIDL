// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
    private static volatile bool _enabled;
    private static readonly object _lock = new();

    /// <summary>Is tracing enabled?</summary>
    public static bool IsEnabled => _enabled;

    /// <summary>Enable tracing. Call once at startup.</summary>
    /// <param name="logFilePath">Path to log file. If null, writes to stderr.</param>
    /// <param name="alsoWriteToStdErr">When true and logFilePath is set, also tee to stderr. Default true.</param>
    public static void Enable(string? logFilePath = null, bool alsoWriteToStdErr = true)
    {
        lock (_lock)
        {
            _enabled = true;
            if (logFilePath != null)
            {
                // Another process may already hold the log — a second editor window, or a
                // language server still running from a previous session. Tracing is a
                // diagnostic aid, so share the file where possible and fall back to stderr
                // rather than failing to start over it.
                TextWriter fileWriter;
                try
                {
                    var stream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write,
                        FileShare.ReadWrite | FileShare.Delete);
                    fileWriter = new StreamWriter(stream) { AutoFlush = true };
                }
                catch (IOException)
                {
                    _writer = alsoWriteToStdErr ? Console.Error : TextWriter.Null;
                    return;
                }
                catch (UnauthorizedAccessException)
                {
                    _writer = alsoWriteToStdErr ? Console.Error : TextWriter.Null;
                    return;
                }

                _writer = alsoWriteToStdErr
                    ? new TeeWriter(fileWriter, Console.Error)
                    : fileWriter;
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

    /// <summary>Writes to two TextWriters simultaneously.</summary>
    private sealed class TeeWriter : TextWriter
    {
        private readonly TextWriter _a;
        private readonly TextWriter _b;

        public TeeWriter(TextWriter a, TextWriter b) { _a = a; _b = b; }

        public override System.Text.Encoding Encoding => _a.Encoding;

        public override void WriteLine(string? value)
        {
            _a.WriteLine(value);
            try { _b.WriteLine(value); } catch { /* stderr may be closed */ }
        }

        public override void Flush()
        {
            _a.Flush();
            try { _b.Flush(); } catch { }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _a.Dispose();
                // Don't dispose _b (Console.Error)
            }
            base.Dispose(disposing);
        }
    }
}
