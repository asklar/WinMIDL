// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Syntax;

namespace Midl.Integration.Tests;

/// <summary>
/// Starting the language server must not depend on being able to open the trace log.
///
/// Every server instance logs to a fixed path (<c>%TEMP%\midl-lsp.log</c> unless
/// <c>MIDL_DEBUG_LOG</c> overrides it), and the file was opened without sharing. A second
/// editor window, a server left running by a previous session, or a test run alongside a
/// live server would therefore fail to start with an IOException — over a diagnostic aid
/// that nothing depends on.
/// </summary>
[Collection("MidlTrace")]
public class TraceLogSharingTests : IDisposable
{
    private readonly bool _wasEnabled = MidlTrace.IsEnabled;

    public void Dispose()
    {
        if (!_wasEnabled) MidlTrace.Disable();
    }

    [Fact]
    public void EnablingTracing_SucceedsWhenAnotherProcessHoldsTheLog()
    {
        var path = Path.Combine(Path.GetTempPath(), "midl-trace-lock-" + Guid.NewGuid().ToString("N") + ".log");
        try
        {
            // Deny sharing outright — the strictest thing another holder could do.
            using var exclusive = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);

            var ex = Record.Exception(() => MidlTrace.Enable(path, alsoWriteToStdErr: false));

            Assert.Null(ex);
        }
        finally
        {
            MidlTrace.Disable();
            try { File.Delete(path); } catch { }
        }
    }

    /// <summary>
    /// Two writers sharing the log is the normal case for a second editor window, and the
    /// second one must actually write — not silently fall back and lose its output.
    /// </summary>
    [Fact]
    public void TwoConcurrentWritersCanShareTheLog()
    {
        var path = Path.Combine(Path.GetTempPath(), "midl-trace-share-" + Guid.NewGuid().ToString("N") + ".log");
        var marker = "marker-" + Guid.NewGuid().ToString("N");
        try
        {
            using (var other = new FileStream(path, FileMode.Append, FileAccess.Write,
                       FileShare.ReadWrite | FileShare.Delete))
            {
                MidlTrace.Enable(path, alsoWriteToStdErr: false);
                MidlTrace.Write("TEST", marker);
            }

            MidlTrace.Disable();

            var written = File.ReadAllText(path);
            Assert.Contains(marker, written);
        }
        finally
        {
            MidlTrace.Disable();
            try { File.Delete(path); } catch { }
        }
    }
}
