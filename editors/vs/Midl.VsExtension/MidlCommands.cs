using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Midl.VsExtension
{
    /// <summary>
    /// Registers and handles the four MIDL menu commands.
    /// </summary>
    internal static class MidlCommands
    {
        public static readonly Guid CommandSet = new Guid("45dcbad2-91e4-4b26-b972-dc1ef49fbd27");

        public const int ToggleStrictModeCommandId = 0x0100;
        public const int RestartServerCommandId = 0x0200;
        public const int ShowServerLogCommandId = 0x0300;
        public const int SelectSdkVersionCommandId = 0x0400;

        private static AsyncPackage package;
        private static bool strictMode = true;

        private const string SdkRoot = @"C:\Program Files (x86)\Windows Kits\10\UnionMetadata";

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
