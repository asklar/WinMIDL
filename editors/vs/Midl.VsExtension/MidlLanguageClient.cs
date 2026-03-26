using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
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
        private IVsOutputWindowPane outputPane;

        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            await Task.Yield();

            // Create output window pane for server logging
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
            var outputWindow = (IVsOutputWindow)ServiceProvider.GlobalProvider.GetService(typeof(SVsOutputWindow));
            var paneGuid = new Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");
            outputWindow.CreatePane(ref paneGuid, "MIDL Language Server", 1, 1);
            outputWindow.GetPane(ref paneGuid, out outputPane);
            outputPane?.OutputStringThreadSafe("MIDL Language Server starting...\n");
            await TaskScheduler.Default;

            string serverPath = ServerDiscovery.FindServerPath();
            if (serverPath == null)
            {
                OutputLog("ERROR: Could not find Midl.Cli.exe");
                throw new InvalidOperationException(
                    "Could not find Midl.Cli.exe. Ensure the MIDL CLI is built or bundled with the extension.");
            }

            OutputLog($"Server path: {serverPath}");

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

            OutputLog($"Server started (PID {serverProcess.Id})");

            // Forward stderr to Output window
            serverProcess.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    OutputLog(e.Data);
            };
            serverProcess.BeginErrorReadLine();

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
            OutputLog($"Server initialization failed: {initializationInfo?.StatusMessage}");
            return Task.FromResult<InitializationFailureContext>(null);
        }

        public Task OnServerInitializedAsync()
        {
            OutputLog("Server initialized successfully");
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

        private void OutputLog(string message)
        {
            outputPane?.OutputStringThreadSafe($"[MIDL] {message}\n");
        }
    }
}
