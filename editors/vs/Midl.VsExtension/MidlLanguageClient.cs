using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;

namespace Midl.VsExtension
{
    [ContentType("midl")]
    [Export(typeof(ILanguageClient))]
    public class MidlLanguageClient : ILanguageClient
    {
        public string Name => "MIDL Language Server";

        public IEnumerable<string> ConfigurationSections => null;

        public object InitializationOptions => null;

        public IEnumerable<string> FilesToWatch => null;

        public bool ShowNotificationOnInitializeFailed => true;

        public event AsyncEventHandler<EventArgs> StartAsync;
        public event AsyncEventHandler<EventArgs> StopAsync;

        private Process serverProcess;

        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            await Task.Yield();

            string serverPath = ServerDiscovery.FindServerPath();
            if (serverPath == null)
            {
                throw new InvalidOperationException(
                    "Could not find Midl.Cli.exe. Ensure the MIDL CLI is built or bundled with the extension.");
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = serverPath,
                Arguments = "--lsp",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            serverProcess = Process.Start(startInfo);
            if (serverProcess == null)
            {
                throw new InvalidOperationException(
                    $"Failed to start MIDL language server process: {serverPath}");
            }

            return new Connection(
                serverProcess.StandardOutput.BaseStream,
                serverProcess.StandardInput.BaseStream);
        }

        public async Task OnLoadedAsync()
        {
            if (StartAsync != null)
            {
                await StartAsync.InvokeAsync(this, EventArgs.Empty);
            }
        }

        public Task<InitializationFailureContext> OnServerInitializeFailedAsync(ILanguageClientInitializationInfo initializationInfo)
        {
            return Task.FromResult<InitializationFailureContext>(null);
        }

        public Task OnServerInitializedAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Restarts the language server by firing stop/start events.
        /// </summary>
        internal async Task RestartAsync()
        {
            if (StopAsync != null)
            {
                await StopAsync.InvokeAsync(this, EventArgs.Empty);
            }

            if (serverProcess != null && !serverProcess.HasExited)
            {
                try
                {
                    serverProcess.Kill();
                    serverProcess.Dispose();
                }
                catch (InvalidOperationException)
                {
                    // Process already exited
                }

                serverProcess = null;
            }

            if (StartAsync != null)
            {
                await StartAsync.InvokeAsync(this, EventArgs.Empty);
            }
        }
    }
}
