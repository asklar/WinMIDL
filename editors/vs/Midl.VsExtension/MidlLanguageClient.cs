// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using StreamJsonRpc;

namespace Midl.VsExtension
{
    [ContentType("midl")]
    [Export(typeof(ILanguageClient))]
    public class MidlLanguageClient : ILanguageClient, ILanguageClientCustomMessage2
    {
        private readonly HoverDocsMiddleLayer middleLayer = new();

        public object MiddleLayer => middleLayer;

        public object? CustomMessageTarget => null;

        public string Name => "MIDL Language Server";

        public IEnumerable<string> ConfigurationSections => null;

        public object InitializationOptions
        {
            get
            {
                var options = GetOptionsPage();
                if (options == null) return null;
                return new Dictionary<string, object>
                {
                    ["midl.strictMode"] = options.StrictMode,
                    ["midl.codeQualityHints"] = options.CodeQualityHints,
                    ["midl.useWindowsSdk"] = options.UseWindowsSdk,
                    ["midl.windowsSdkVersion"] = options.WindowsSdkVersion,
                    ["midl.flatSdkTypeCompletions"] = options.FlatSdkTypeCompletions,
                    ["midl.arrays"] = options.EnableArraySyntax,
                    ["midl.defines"] = new Newtonsoft.Json.Linq.JArray(
                        string.IsNullOrWhiteSpace(options.Defines) ? Array.Empty<string>() :
                        options.Defines.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray()),
                };
            }
        }

        public IEnumerable<string> FilesToWatch=> null;

        public bool ShowNotificationOnInitializeFailed => true;

        public event AsyncEventHandler<EventArgs> StartAsync;
        public event AsyncEventHandler<EventArgs> StopAsync;

        private Process serverProcess;
        private IVsOutputWindowPane outputPane;
        private static MidlLanguageClient _instance;

        internal static Guid OutputPaneGuid = new Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");

        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            _instance = this;
            await Task.Yield();

            // Create output window pane for server logging
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
            var outputWindow = (IVsOutputWindow)ServiceProvider.GlobalProvider.GetService(typeof(SVsOutputWindow));
            var paneGuid = OutputPaneGuid;
            outputWindow.CreatePane(ref paneGuid, "MIDL Language Server", 1, 1);
            outputWindow.GetPane(ref paneGuid, out outputPane);
            outputPane?.OutputStringThreadSafe("MIDL Language Server starting...\n");
            await TaskScheduler.Default;

            string serverPath = null;

            // Check user-configured server path first (parity with VS Code's midl.serverPath)
            var options = GetOptionsPage();
            if (options != null && !string.IsNullOrWhiteSpace(options.ServerPath))
            {
                if (File.Exists(options.ServerPath))
                {
                    serverPath = options.ServerPath;
                    OutputLog($"Using user-configured server path: {serverPath}");
                }
                else
                {
                    OutputLog($"WARNING: Configured server path does not exist: {options.ServerPath}");
                }
            }

            if (serverPath == null)
            {
                serverPath = ServerDiscovery.FindServerPath();
            }

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

        public Task AttachForCustomMessageAsync(JsonRpc rpc) => Task.CompletedTask;

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

        internal static void LogWarning(string message)
        {
            if (_instance != null)
                _instance.OutputLog($"WARNING: {message}");
            else
                Trace.TraceWarning(message);
        }

        private static MidlOptionsPage GetOptionsPage()
        {
            return MidlPackage.Instance?.GetOptions();
        }

        /// <summary>
        /// Sends workspace/didChangeConfiguration to the running server with current settings.
        /// Called from MidlOptionsPage.OnApply when the user clicks OK/Apply.
        /// </summary>
        internal static void NotifySettingsChanged()
        {
            var instance = _instance;
            if (instance?.serverProcess == null || instance.serverProcess.HasExited) return;

            var options = GetOptionsPage();
            if (options == null) return;

            try
            {
                var settingsObj = new Newtonsoft.Json.Linq.JObject
                {
                    ["midl.strictMode"] = options.StrictMode,
                    ["midl.codeQualityHints"] = options.CodeQualityHints,
                    ["midl.useWindowsSdk"] = options.UseWindowsSdk,
                    ["midl.windowsSdkVersion"] = options.WindowsSdkVersion,
                    ["midl.flatSdkTypeCompletions"] = options.FlatSdkTypeCompletions,
                    ["midl.arrays"] = options.EnableArraySyntax,
                    ["midl.defines"] = new Newtonsoft.Json.Linq.JArray(
                        string.IsNullOrWhiteSpace(options.Defines) ? Array.Empty<string>() :
                        options.Defines.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray()),
                };

                var msg = new Newtonsoft.Json.Linq.JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["method"] = "workspace/didChangeConfiguration",
                    ["params"] = new Newtonsoft.Json.Linq.JObject { ["settings"] = settingsObj }
                };

                var body = msg.ToString(Newtonsoft.Json.Formatting.None);
                var header = $"Content-Length: {System.Text.Encoding.UTF8.GetByteCount(body)}\r\n\r\n";
                var stream = instance.serverProcess.StandardInput;
                stream.Write(header);
                stream.Write(body);
                stream.Flush();

                instance.OutputLog("Sent workspace/didChangeConfiguration with updated settings");
            }
            catch (Exception ex)
            {
                instance.OutputLog($"Failed to send settings: {ex.Message}");
            }
        }
    }
}
