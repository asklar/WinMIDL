using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json.Linq;
using Task = System.Threading.Tasks.Task;

namespace Midl.VsExtension
{
    /// <summary>
    /// Carries the data needed to render a class diagram for a source MIDL document.
    /// </summary>
    internal sealed class ClassDiagramRequestContext
    {
        /// <summary>
        /// Gets or sets the client used to send the custom LSP request.
        /// </summary>
        public MidlLanguageClient Client { get; set; }

        /// <summary>
        /// Gets or sets the document URI the server tracks for the source MIDL document.
        /// </summary>
        public string Uri { get; set; }
    }

    /// <summary>
    /// Editor pane that renders a MIDL class diagram in a WebView2 control hosted in the document well.
    /// </summary>
    /// <remarks>
    /// The pane is both the document view and the document data for the custom class-diagram editor. Its backing
    /// "file" is a placeholder whose only purpose is to give the shell a real moniker to open; the render context
    /// (client and source URI) is looked up from <see cref="Contexts"/> by that moniker.
    /// </remarks>
    internal sealed class ClassDiagramEditorPane : WindowPane, IVsPersistDocData, IPersistFileFormat
    {
        private const string RequestMethod = "midl/classDiagram";

        private static readonly ConcurrentDictionary<string, ClassDiagramRequestContext> Contexts =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly WebView2 webView;

        private string moniker;
        private MidlLanguageClient client;
        private string documentUri;
        private bool showReferences;
        private bool webViewInitialized;

        /// <summary>
        /// Gets the most recently loaded diagram pane, used to re-render the shared document for a new source file.
        /// </summary>
        internal static ClassDiagramEditorPane Current { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassDiagramEditorPane"/> class.
        /// </summary>
        public ClassDiagramEditorPane()
            : base(null)
        {
            this.webView = new WebView2();
            this.Content = this.webView;
        }

        /// <summary>
        /// Associates a diagram document moniker with the context needed to render it.
        /// </summary>
        /// <param name="documentMoniker">The full path of the placeholder diagram document.</param>
        /// <param name="languageClient">The client used to send the custom LSP request.</param>
        /// <param name="uri">The document URI the server tracks for the source document.</param>
        public static void RegisterContext(string documentMoniker, MidlLanguageClient languageClient, string uri)
        {
            Contexts[Normalize(documentMoniker)] = new ClassDiagramRequestContext
            {
                Client = languageClient,
                Uri = uri,
            };
        }

        private static string Normalize(string path) => Path.GetFullPath(path);

        private void BeginLoad(string documentMoniker)
        {
            this.moniker = documentMoniker;
            Current = this;

#pragma warning disable VSTHRD110 // Observe result of async calls
            ThreadHelper.JoinableTaskFactory.RunAsync(() => this.ReloadAsync());
#pragma warning restore VSTHRD110
        }

        /// <summary>
        /// Re-renders the diagram for the current context, used when the shared document is reused for a new file.
        /// </summary>
        /// <returns>A task that completes when the render finishes.</returns>
        internal async Task ReloadAsync()
        {
            try
            {
                await this.LoadCoreAsync(this.moniker);
            }
            catch (Exception ex)
            {
                await this.ShowErrorAsync(ex);
            }
        }

        private async Task LoadCoreAsync(string documentMoniker)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (!Contexts.TryGetValue(Normalize(documentMoniker), out ClassDiagramRequestContext context))
            {
                this.NavigateToMessage("MIDL: No diagram context is available for this document.");

                return;
            }

            this.client = context.Client;
            this.documentUri = context.Uri;

            await this.EnsureWebViewAsync();
            await this.RefreshAsync(CancellationToken.None);
        }

        private async Task EnsureWebViewAsync()
        {
            if (this.webViewInitialized)
            {
                return;
            }

            // WebView2 defaults its user-data folder next to the host process (devenv.exe under Program Files),
            // which isn't writable and fails with E_ACCESSDENIED. Use a writable per-user folder instead.
            string userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Midl.VsExtension",
                "WebView2");
            CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
            await this.webView.EnsureCoreWebView2Async(environment);
            this.webView.CoreWebView2.WebMessageReceived += this.OnWebMessageReceived;
            this.webViewInitialized = true;
        }

        private async Task RefreshAsync(CancellationToken cancellationToken)
        {
            string dot = await this.RequestDotAsync(cancellationToken);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            if (string.IsNullOrEmpty(dot))
            {
                this.NavigateToMessage("MIDL: Server returned no diagram.");

                return;
            }

            this.webView.CoreWebView2.NavigateToString(ClassDiagramHtml.Build(dot, this.showReferences));
        }

        private async Task<string> RequestDotAsync(CancellationToken cancellationToken)
        {
            JToken result = await this.client.RequestAsync(RequestMethod, new Dictionary<string, object>
            {
                ["uri"] = this.documentUri,
                ["showReferences"] = this.showReferences,
            }, cancellationToken);

            if (result is not JObject response || response["dot"]?.Type != JTokenType.String)
            {
                throw new InvalidOperationException("MIDL language server returned an invalid class diagram response.");
            }

            return response.Value<string>("dot");
        }

        private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            JObject message = JObject.Parse(e.WebMessageAsJson);
            if ((string)message["command"] != "toggleReferences")
            {
                return;
            }

            this.showReferences = (bool?)message["value"] ?? false;

#pragma warning disable VSTHRD110 // Observe result of async calls
            ThreadHelper.JoinableTaskFactory.RunAsync(() => this.RefreshAsync(CancellationToken.None));
#pragma warning restore VSTHRD110
        }

        private void NavigateToMessage(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            this.webView.CoreWebView2.NavigateToString(
                $"<html><body style='font-family:Segoe UI;color:#ccc;background:#1e1e1e'>{HtmlEncode(message)}</body></html>");
        }

        private async Task ShowErrorAsync(Exception exception)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            string details = FormatException(exception);

            if (this.webViewInitialized)
            {
                this.webView.CoreWebView2.NavigateToString(
                    $"<html><body style='font-family:Consolas;color:#f48771;background:#1e1e1e;white-space:pre-wrap'>{HtmlEncode(details)}</body></html>");
            }
            else
            {
                VsShellUtilities.ShowMessageBox(
                    ServiceProvider.GlobalProvider,
                    details,
                    "MIDL",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

        private static string FormatException(Exception exception)
        {
            System.Text.StringBuilder builder = new();
            builder.AppendLine("MIDL: Failed to show the class diagram.");
            builder.AppendLine();

            for (Exception current = exception; current != null; current = current.InnerException)
            {
                builder.AppendLine($"{current.GetType().FullName}: {current.Message}");
            }

            builder.AppendLine();
            builder.AppendLine(exception.StackTrace);

            return builder.ToString();
        }

        private static string HtmlEncode(string value) =>
            value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Current == this)
                {
                    Current = null;
                }

                if (this.moniker != null)
                {
                    Contexts.TryRemove(Normalize(this.moniker), out _);
                }

                this.webView.Dispose();
            }

            base.Dispose(disposing);
        }

        int IPersist.GetClassID(out Guid pClassID)
        {
            pClassID = ClassDiagramEditorFactory.EditorFactoryGuid;

            return VSConstants.S_OK;
        }

        int IPersistFileFormat.GetClassID(out Guid pClassID)
        {
            pClassID = ClassDiagramEditorFactory.EditorFactoryGuid;

            return VSConstants.S_OK;
        }

        int IPersistFileFormat.IsDirty(out int pfIsDirty)
        {
            pfIsDirty = 0;

            return VSConstants.S_OK;
        }

        int IPersistFileFormat.InitNew(uint nFormatIndex) => VSConstants.S_OK;

        int IPersistFileFormat.Load(string pszFilename, uint grfMode, int fReadOnly)
        {
            if (!string.IsNullOrEmpty(pszFilename))
            {
                this.BeginLoad(pszFilename);
            }

            return VSConstants.S_OK;
        }

        int IPersistFileFormat.Save(string pszFilename, int fRemember, uint nFormatIndex) => VSConstants.S_OK;

        int IPersistFileFormat.SaveCompleted(string pszFilename) => VSConstants.S_OK;

        int IPersistFileFormat.GetCurFile(out string ppszFilename, out uint pnFormatIndex)
        {
            ppszFilename = this.moniker;
            pnFormatIndex = 0;

            return VSConstants.S_OK;
        }

        int IPersistFileFormat.GetFormatList(out string ppszFormatList)
        {
            ppszFormatList = "MIDL Class Diagram (*.midlclassdiagram)\n*.midlclassdiagram\n\n";

            return VSConstants.S_OK;
        }

        int IVsPersistDocData.GetGuidEditorType(out Guid pClassID)
        {
            pClassID = ClassDiagramEditorFactory.EditorFactoryGuid;

            return VSConstants.S_OK;
        }

        int IVsPersistDocData.IsDocDataDirty(out int pfDirty)
        {
            pfDirty = 0;

            return VSConstants.S_OK;
        }

        int IVsPersistDocData.SetUntitledDocPath(string pszDocDataPath) => VSConstants.S_OK;

        int IVsPersistDocData.LoadDocData(string pszMkDocument)
        {
            this.BeginLoad(pszMkDocument);

            return VSConstants.S_OK;
        }

        int IVsPersistDocData.SaveDocData(VSSAVEFLAGS dwSave, out string pbstrMkDocumentNew, out int pfSaveCanceled)
        {
            pbstrMkDocumentNew = null;
            pfSaveCanceled = 0;

            return VSConstants.S_OK;
        }

        int IVsPersistDocData.Close() => VSConstants.S_OK;

        int IVsPersistDocData.OnRegisterDocData(uint docCookie, IVsHierarchy pHierNew, uint itemidNew) => VSConstants.S_OK;

        int IVsPersistDocData.RenameDocData(uint grfAttribs, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew) => VSConstants.S_OK;

        int IVsPersistDocData.IsDocDataReloadable(out int pfReloadable)
        {
            pfReloadable = 0;

            return VSConstants.S_OK;
        }

        int IVsPersistDocData.ReloadDocData(uint grfFlags) => VSConstants.S_OK;
    }
}
