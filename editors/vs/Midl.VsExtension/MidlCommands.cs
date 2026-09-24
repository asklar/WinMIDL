// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Newtonsoft.Json.Linq;

namespace Midl.VsExtension
{
    /// <summary>
    /// Registers and handles the MIDL menu commands.
    /// </summary>
    internal static class MidlCommands
    {
        public static readonly Guid CommandSet = new Guid("45dcbad2-91e4-4b26-b972-dc1ef49fbd27");

        public const int ToggleStrictModeCommandId = 0x0100;
        public const int RestartServerCommandId = 0x0200;
        public const int ShowServerLogCommandId = 0x0300;
        public const int SelectSdkVersionCommandId = 0x0400;
        public const int ShowClassDiagramCommandId = 0x0500;
        public const int ReportIssueCommandId = 0x0600;

        private static AsyncPackage package;
        private static bool strictMode = true;

        private const string SdkRoot = @"C:\Program Files (x86)\Windows Kits\10\UnionMetadata";

        private const string ReportRequestMethod = "midl/diagnosticReport";

        public static async System.Threading.Tasks.Task InitializeAsync(AsyncPackage pkg)
        {
            package = pkg;

            await pkg.JoinableTaskFactory.SwitchToMainThreadAsync();
            var commandService = await pkg.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService == null) return;

            commandService.AddCommand(new MenuCommand(OnToggleStrictMode,
                new CommandID(CommandSet, ToggleStrictModeCommandId)));
            commandService.AddCommand(new MenuCommand(OnRestartServer,
                new CommandID(CommandSet, RestartServerCommandId)));
            commandService.AddCommand(new MenuCommand(OnShowServerLog,
                new CommandID(CommandSet, ShowServerLogCommandId)));
            commandService.AddCommand(new MenuCommand(OnSelectSdkVersion,
                new CommandID(CommandSet, SelectSdkVersionCommandId)));
            commandService.AddCommand(new MenuCommand(OnShowClassDiagram,
                new CommandID(CommandSet, ShowClassDiagramCommandId)));
            commandService.AddCommand(new MenuCommand(OnReportIssue,
                new CommandID(CommandSet, ReportIssueCommandId)));
        }

        private static void OnToggleStrictMode(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var options = MidlPackage.Instance?.GetOptions();
            if (options != null)
            {
                options.StrictMode = !options.StrictMode;
                options.SaveSettingsToStorage();
                MidlLanguageClient.NotifySettingsChanged();
                strictMode = options.StrictMode;
            }
            else
            {
                strictMode = !strictMode;
            }

            string mode = strictMode ? "Strict MIDL3" : "Compatibility";
            VsShellUtilities.ShowMessageBox(
                package,
                $"MIDL: Switched to {mode} mode",
                "MIDL",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private static void OnRestartServer(object sender, EventArgs e)
        {
#pragma warning disable VSTHRD110 // Observe result of async calls
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
#pragma warning restore VSTHRD110
            {
                var componentModel = (IComponentModel)await package.GetServiceAsync(typeof(SComponentModel));
                if (componentModel != null)
                {
                    var client = componentModel.DefaultExportProvider
                        .GetExportedValueOrDefault<ILanguageClient>() as MidlLanguageClient;
                    if (client != null)
                    {
                        await client.RestartAsync();
                        return;
                    }
                }

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                VsShellUtilities.ShowMessageBox(
                    package,
                    "MIDL: Language server restart requested.",
                    "MIDL",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            });
        }

        private static void OnShowServerLog(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                var outputWindow = (IVsOutputWindow)ServiceProvider.GlobalProvider.GetService(typeof(SVsOutputWindow));
                if (outputWindow != null)
                {
                    var paneGuid = MidlLanguageClient.OutputPaneGuid;
                    outputWindow.GetPane(ref paneGuid, out var pane);
                    pane?.Activate();

                    // Show the Output tool window
                    var shell = (IVsUIShell)ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShell));
                    if (shell != null)
                    {
                        var outputWindowGuid = new Guid("{34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3}"); // Output window GUID
                        shell.FindToolWindow(0, ref outputWindowGuid, out var frame);
                        frame?.Show();
                    }
                }
            }
            catch
            {
                // Fallback to message box if Output window activation fails
                VsShellUtilities.ShowMessageBox(
                    package,
                    "Check the Output window (View \u2192 Output) and select 'MIDL Language Server' from the dropdown.",
                    "MIDL",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

        private static void OnReportIssue(object sender, EventArgs e)
        {
#pragma warning disable VSTHRD110 // Observe result of async calls
            ThreadHelper.JoinableTaskFactory.RunAsync(ReportIssueAsync);
#pragma warning restore VSTHRD110
        }

        private static async System.Threading.Tasks.Task ReportIssueAsync()
        {
            await package.JoinableTaskFactory.SwitchToMainThreadAsync();

            string ideVersion = GetVisualStudioVersion();
            string extensionVersion = typeof(MidlCommands).Assembly.GetName().Version?.ToString() ?? "unknown";

            JObject? report;
            try
            {
                report = await TryGetDiagnosticReportAsync();
            }
            catch (OperationCanceledException) when (package.DisposalToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                await package.JoinableTaskFactory.SwitchToMainThreadAsync();
                VsShellUtilities.ShowMessageBox(
                    package,
                    $"MIDL: Could not retrieve the diagnostic report: {ex.Message}",
                    "MIDL: Report Issue",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }

            string body;
            if (report != null)
            {
                bool? includeIdl = PromptIncludeIdlFiles();
                if (includeIdl == null)
                {
                    return;
                }

                body = DiagnosticReportFormatter.BuildIssueBody(report, includeIdl.Value, ideVersion, extensionVersion);
            }
            else
            {
                body = DiagnosticReportFormatter.BuildFallbackBody(ReadServerLogTail(), ideVersion, extensionVersion);
            }

            OpenGitHubIssue(body);
        }

        private static async System.Threading.Tasks.Task<JObject?> TryGetDiagnosticReportAsync()
        {
            MidlLanguageClient? client = MidlLanguageClient.ActiveInstance;
            if (client == null)
            {
                return null;
            }

            JToken response = await client.RequestAsync(
                ReportRequestMethod,
                new Dictionary<string, object>(),
                package.DisposalToken);
            return response as JObject
                ?? throw new InvalidOperationException("MIDL language server returned an invalid diagnostic report.");
        }

        private static bool? PromptIncludeIdlFiles()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            int result = VsShellUtilities.ShowMessageBox(
                package,
                "Include IDL file contents in the report to help diagnosis?\n\nYes: attach IDL file contents.\nNo: include statistics only.",
                "MIDL: Report Issue",
                OLEMSGICON.OLEMSGICON_QUERY,
                OLEMSGBUTTON.OLEMSGBUTTON_YESNOCANCEL,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

            return result switch
            {
                6 => true,   // IDYES
                7 => false,  // IDNO
                _ => null,   // IDCANCEL / dismissed
            };
        }

        private static string ReadServerLogTail()
        {
            try
            {
                string home = Environment.GetEnvironmentVariable("USERPROFILE")
                    ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string logFile = Path.Combine(home, ".midl", "server.log");
                if (!File.Exists(logFile))
                {
                    return null;
                }

                string[] lines = File.ReadAllLines(logFile);
                int skip = Math.Max(0, lines.Length - 100);

                return string.Join("\n", lines.Skip(skip));
            }
            catch (IOException)
            {
                return null;
            }
        }

        private static void OpenGitHubIssue(string body)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            VsShellUtilities.OpenSystemBrowser(DiagnosticReportFormatter.BuildIssueUrl(body));
        }

        private static string GetVisualStudioVersion()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var shell = ServiceProvider.GlobalProvider.GetService(typeof(SVsShell)) as IVsShell;
            if (shell != null
                && ErrorHandler.Succeeded(shell.GetProperty((int)__VSSPROPID5.VSSPROPID_ReleaseVersion, out object version))
                && version is string versionString)
            {
                return versionString;
            }

            return "unknown";
        }

        private static void OnShowClassDiagram(object sender, EventArgs e)
        {
#pragma warning disable VSTHRD110 // Observe result of async calls
            ThreadHelper.JoinableTaskFactory.RunAsync(ShowClassDiagramAsync);
#pragma warning restore VSTHRD110
        }

        private static async System.Threading.Tasks.Task ShowClassDiagramAsync()
        {
            await package.JoinableTaskFactory.SwitchToMainThreadAsync();

            var componentModel = (IComponentModel)await package.GetServiceAsync(typeof(SComponentModel));
            var textManager = (IVsTextManager)await package.GetServiceAsync(typeof(SVsTextManager));
            if (componentModel == null || textManager == null)
            {
                ShowClassDiagramWarning("MIDL: Required editor services are unavailable.");

                return;
            }

            MidlLanguageClient? client = MidlLanguageClient.ActiveInstance;
            var adapters = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            var documentFactory = componentModel.GetService<ITextDocumentFactoryService>();
            if (client == null || adapters == null || documentFactory == null)
            {
                ShowClassDiagramWarning("MIDL: Required editor services are unavailable.");

                return;
            }

            textManager.GetActiveView(1, null, out IVsTextView activeView);
            IWpfTextView wpfView = activeView != null ? adapters.GetWpfTextView(activeView) : null;
            ITextBuffer buffer = wpfView?.TextBuffer;
            if (buffer == null || !buffer.ContentType.IsOfType("midl"))
            {
                ShowClassDiagramWarning("MIDL: Open a MIDL (.idl) file to show its class diagram.");

                return;
            }

            if (!documentFactory.TryGetTextDocument(buffer, out ITextDocument document) || string.IsNullOrEmpty(document.FilePath))
            {
                ShowClassDiagramWarning("MIDL: The active document must be saved to a file first.");

                return;
            }

            string uri = new Uri(document.FilePath).AbsoluteUri;

            try
            {
                await OpenClassDiagramDocumentAsync(client, uri, document.FilePath);
            }
            catch (Exception ex)
            {
                await package.JoinableTaskFactory.SwitchToMainThreadAsync();
                ShowClassDiagramError(ex);
            }
        }

        private static async System.Threading.Tasks.Task OpenClassDiagramDocumentAsync(
            MidlLanguageClient client, string uri, string idlPath)
        {
            await package.JoinableTaskFactory.SwitchToMainThreadAsync();

            string diagramPath = GetDiagramDocumentPath();
            Directory.CreateDirectory(Path.GetDirectoryName(diagramPath));
            File.WriteAllText(diagramPath, idlPath);

            ClassDiagramEditorPane.RegisterContext(diagramPath, client, uri);

            string caption = Path.GetFileName(idlPath) + " - Class Diagram";

            // A single shared diagram document is reused across files (parity with VS Code): reveal it if already
            // open, otherwise create it. A fresh open renders through the editor's load path; an already-open
            // document won't re-run that path, so it's re-rendered explicitly below.
            bool alreadyOpen = VsShellUtilities.IsDocumentOpen(
                package, diagramPath, Guid.Empty, out _, out _, out IVsWindowFrame frame) && frame != null;

            if (!alreadyOpen)
            {
                VsShellUtilities.OpenDocumentWithSpecificEditor(
                    package,
                    diagramPath,
                    ClassDiagramEditorFactory.EditorFactoryGuid,
                    VSConstants.LOGVIEWID_Primary,
                    out _,
                    out _,
                    out frame);
            }

            frame?.SetProperty((int)__VSFPROPID.VSFPROPID_OwnerCaption, caption);
            frame?.SetProperty((int)__VSFPROPID.VSFPROPID_EditorCaption, string.Empty);

            // The diagram is a computed, session-scoped document backed by an in-memory render context. Prevent VS
            // from reopening it on the next launch (where that context no longer exists), which otherwise leaves the
            // restored tab stuck on the shell's async-load "Loading document..." placeholder.
            frame?.SetProperty((int)__VSFPROPID5.VSFPROPID_DontAutoOpen, true);

            frame?.Show();

            if (alreadyOpen)
            {
                ClassDiagramEditorPane current = ClassDiagramEditorPane.Current;
                if (current != null)
                {
                    await current.ReloadAsync();
                }
            }
            else
            {
                await MoveToVerticalTabGroupAsync();
            }
        }

        private static async System.Threading.Tasks.Task MoveToVerticalTabGroupAsync()
        {
            await package.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                EnvDTE.DTE dte = await package.GetServiceAsync(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
                dte?.ExecuteCommand("Window.NewVerticalTabGroup");
            }
            catch (Exception)
            {
                // Side-by-side layout is best-effort; the diagram still opens in the active document group.
            }
        }

        private static string GetDiagramDocumentPath()
        {
            string directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Midl.VsExtension",
                "Diagrams");

            // A single fixed moniker means one shared diagram document that is reused across source files.
            return Path.Combine(directory, "MidlClassDiagram.midlclassdiagram");
        }

        private static void ShowClassDiagramError(Exception exception)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            StringBuilder builder = new();
            builder.AppendLine("MIDL: Failed to show the class diagram.");
            builder.AppendLine();

            for (Exception current = exception; current != null; current = current.InnerException)
            {
                builder.AppendLine($"{current.GetType().FullName}: {current.Message}");
            }

            builder.AppendLine();
            builder.AppendLine(exception.StackTrace);

            VsShellUtilities.ShowMessageBox(
                package,
                builder.ToString(),
                "MIDL",
                OLEMSGICON.OLEMSGICON_CRITICAL,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private static void ShowClassDiagramWarning(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            VsShellUtilities.ShowMessageBox(
                package,
                message,
                "MIDL",
                OLEMSGICON.OLEMSGICON_WARNING,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private static void OnSelectSdkVersion(object sender, EventArgs e)
        {
            List<string> versions = DiscoverSdkVersions();

            if (versions.Count == 0)
            {
                VsShellUtilities.ShowMessageBox(
                    package,
                    "No Windows SDK installations found.",
                    "MIDL",
                    OLEMSGICON.OLEMSGICON_WARNING,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }

            string versionList = string.Join("\n", versions.Select((v, i) => $"  {i + 1}. {v}"));
            string message = $"Available Windows SDK versions:\n{versionList}\n\n" +
                             $"Current newest: {versions[0]}\n\n" +
                             "To change, update the 'windowsSdkVersion' setting in Tools \u2192 Options \u2192 MIDL.";

            VsShellUtilities.ShowMessageBox(
                package,
                message,
                "MIDL: Select Windows SDK Version",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        internal static List<string> DiscoverSdkVersions()
        {
            var versions = new List<string>();

            try
            {
                if (!Directory.Exists(SdkRoot))
                    return versions;

                foreach (string dir in Directory.GetDirectories(SdkRoot))
                {
                    string dirName = Path.GetFileName(dir);
                    if (string.Equals(dirName, "Facade", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (File.Exists(Path.Combine(dir, "Windows.winmd")))
                        versions.Add(dirName);
                }

                versions.Sort();
                versions.Reverse();
            }
            catch
            {
                // SDK scan failures are non-fatal
            }

            return versions;
        }
    }
}
