using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace Midl.VsExtension
{
    [Guid("B8A3C4D5-E6F7-4890-ABCD-123456789ABC")]
    public class MidlOptionsPage : DialogPage
    {
        private bool strictMode = true;
        private bool codeQualityHints = true;
        private bool useWindowsSdk = true;
        private string windowsSdkVersion = "latest";
        private bool flatSdkTypeCompletions = false;
        private string serverPath = "";

        [Category("MIDL Language Server")]
        [DisplayName("Server Path")]
        [Description("Path to the MIDL CLI executable. Leave empty for auto-detection (recommended).")]
        [DefaultValue("")]
        public string ServerPath
        {
            get { return serverPath; }
            set { serverPath = value; }
        }

        [Category("MIDL Language Server")]
        [DisplayName("Strict MIDL3 Mode")]
        [Description("When enabled, only MIDL3 syntax is valid (no pointers, no classic MIDL2 constructs). Disable for compatibility mode.")]
        [DefaultValue(true)]
        public bool StrictMode
        {
            get { return strictMode; }
            set { strictMode = value; }
        }

        [Category("MIDL Language Server")]
        [DisplayName("Code Quality Hints")]
        [Description("Show code quality diagnostics like naming conventions, property shorthand suggestions, and async suffix checks.")]
        [DefaultValue(true)]
        public bool CodeQualityHints
        {
            get { return codeQualityHints; }
            set { codeQualityHints = value; }
        }

        [Category("Windows SDK")]
        [DisplayName("Use Windows SDK")]
        [Description("Automatically reference the Windows SDK for type completion and go-to-definition.")]
        [DefaultValue(true)]
        public bool UseWindowsSdk
        {
            get { return useWindowsSdk; }
            set { useWindowsSdk = value; }
        }

        [Category("Windows SDK")]
        [DisplayName("Windows SDK Version")]
        [Description("Which Windows SDK version to use. Set to 'latest' for auto-detection, or a specific version like '10.0.22621.0'.")]
        [DefaultValue("latest")]
        public string WindowsSdkVersion
        {
            get { return windowsSdkVersion; }
            set { windowsSdkVersion = value; }
        }

        [Category("IntelliSense")]
        [DisplayName("Flat SDK Type Completions")]
        [Description("Show all SDK types in a flat list instead of requiring namespace drill-down (Windows. → Foundation. → ...).")]
        [DefaultValue(false)]
        public bool FlatSdkTypeCompletions
        {
            get { return flatSdkTypeCompletions; }
            set { flatSdkTypeCompletions = value; }
        }

        private bool enableArraySyntax = false;

        [Category("MIDL Language Server")]
        [DisplayName("Enable Array Syntax")]
        [Description("Enable array parameter syntax (T[]) in MIDL3. When enabled, parameters and return types can use T[] for WinRT array passing conventions.")]
        [DefaultValue(false)]
        public bool EnableArraySyntax
        {
            get { return enableArraySyntax; }
            set { enableArraySyntax = value; }
        }

        private string defines = "";

        [Category("MIDL Language Server")]
        [DisplayName("Preprocessor Defines")]
        [Description("Comma-separated preprocessor defines for IDL compilation (e.g. DEBUG,WITH_ARRAYS). Content inside #ifdef blocks for undefined symbols will be grayed out.")]
        [DefaultValue("")]
        public string Defines
        {
            get { return defines; }
            set { defines = value; }
        }

        /// <summary>
        /// Called when the user clicks OK/Apply in Tools > Options.
        /// Notifies the running LSP client to push updated settings to the server.
        /// </summary>
        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);
            if (e.ApplyBehavior == ApplyKind.Apply)
            {
                MidlLanguageClient.NotifySettingsChanged();
            }
        }
    }
}
