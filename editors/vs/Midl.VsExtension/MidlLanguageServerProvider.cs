namespace Midl.VsExtension;

using System.Diagnostics;
using System.IO.Pipelines;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.Extensibility.LanguageServer;
using Microsoft.VisualStudio.RpcContracts.LanguageServerProvider;
using Nerdbank.Streams;

[VisualStudioContribution]
public class MidlLanguageServerProvider : LanguageServerProvider
{
    [VisualStudioContribution]
    public static DocumentTypeConfiguration MidlDocumentType => new("midl")
    {
        FileExtensions = [".idl", ".idlrt"],
    };

    private readonly TraceSource traceSource;
    private Process? serverProcess;

    public MidlLanguageServerProvider(
        ExtensionCore container,
        VisualStudioExtensibility extensibility,
        TraceSource traceSource)
        : base(container, extensibility)
    {
        this.traceSource = traceSource;
    }

    public override LanguageServerProviderConfiguration LanguageServerProviderConfiguration =>
        new("%MidlLanguageServerProvider.DisplayName%",
            [DocumentFilter.FromDocumentType(MidlDocumentType)]);

    public override async Task<IDuplexPipe?> CreateServerConnectionAsync(
        CancellationToken cancellationToken)
    {
        string serverPath = ServerDiscovery.FindServerPath()
            ?? throw new InvalidOperationException(
                "Could not find Midl.Cli.exe. Ensure the MIDL CLI is built or bundled with the extension.");

        this.traceSource.TraceInformation("Starting MIDL language server: {0}", serverPath);

        ProcessStartInfo startInfo = new()
        {
            FileName = serverPath,
            Arguments = "--lsp",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        this.serverProcess = Process.Start(startInfo)
            ?? throw new InvalidOperationException(
                $"Failed to start MIDL language server process: {serverPath}");

        // Forward stderr to trace output for diagnostics
        this.serverProcess.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null)
            {
                this.traceSource.TraceEvent(TraceEventType.Warning, 0, "Server stderr: {0}", e.Data);
            }
        };
        this.serverProcess.BeginErrorReadLine();

        // Splice stdout/stdin into a full-duplex stream, then convert to IDuplexPipe
        Stream duplexStream = FullDuplexStream.Splice(
            this.serverProcess.StandardOutput.BaseStream,
            this.serverProcess.StandardInput.BaseStream);

        return duplexStream.UsePipe();
    }

    public override Task OnServerInitializationResultAsync(
        ServerInitializationResult startState,
        LanguageServerInitializationFailureInfo? initializationFailureInfo,
        CancellationToken cancellationToken)
    {
        if (startState == ServerInitializationResult.Failed)
        {
            this.traceSource.TraceEvent(
                TraceEventType.Error, 0,
                "MIDL language server failed to initialize: {0}",
                initializationFailureInfo?.StatusMessage ?? "unknown error");
        }
        else
        {
            this.traceSource.TraceInformation("MIDL language server initialized successfully.");
        }

        return Task.CompletedTask;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && this.serverProcess is { HasExited: false } process)
        {
            try
            {
                process.Kill();
                process.Dispose();
            }
            catch (InvalidOperationException)
            {
                // Process already exited
            }

            this.serverProcess = null;
        }

        base.Dispose(disposing);
    }
}
