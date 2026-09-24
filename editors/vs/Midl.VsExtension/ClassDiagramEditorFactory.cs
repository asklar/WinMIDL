using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Midl.VsExtension
{
    /// <summary>
    /// Editor factory that creates <see cref="ClassDiagramEditorPane"/> instances hosted in the document well.
    /// </summary>
    [Guid(EditorFactoryGuidString)]
    internal sealed class ClassDiagramEditorFactory : IVsEditorFactory, IDisposable
    {
        /// <summary>
        /// The editor factory GUID, registered with the shell and used to open diagram documents.
        /// </summary>
        public const string EditorFactoryGuidString = "d7e9a1b2-3c4d-4e5f-8a9b-0c1d2e3f4a5b";

        /// <summary>
        /// The editor factory GUID as a <see cref="Guid"/>.
        /// </summary>
        public static readonly Guid EditorFactoryGuid = new(EditorFactoryGuidString);

        private Microsoft.VisualStudio.Shell.ServiceProvider serviceProvider;

        /// <inheritdoc/>
        public int SetSite(IOleServiceProvider psp)
        {
            this.serviceProvider = new Microsoft.VisualStudio.Shell.ServiceProvider(psp);

            return VSConstants.S_OK;
        }

        /// <inheritdoc/>
        public int Close() => VSConstants.S_OK;

        /// <inheritdoc/>
        public int MapLogicalView(ref Guid rguidLogicalView, out string pbstrPhysicalView)
        {
            pbstrPhysicalView = null;

            if (rguidLogicalView == VSConstants.LOGVIEWID_Primary || rguidLogicalView == VSConstants.LOGVIEWID_Designer)
            {
                return VSConstants.S_OK;
            }

            return VSConstants.E_NOTIMPL;
        }

        /// <inheritdoc/>
        public int CreateEditorInstance(
            uint grfCreateDoc,
            string pszMkDocument,
            string pszPhysicalView,
            IVsHierarchy pvHier,
            uint itemid,
            IntPtr punkDocDataExisting,
            out IntPtr ppunkDocView,
            out IntPtr ppunkDocData,
            out string pbstrEditorCaption,
            out Guid pguidCmdUI,
            out int pgrfCDW)
        {
            ppunkDocView = IntPtr.Zero;
            ppunkDocData = IntPtr.Zero;
            pbstrEditorCaption = string.Empty;
            pguidCmdUI = EditorFactoryGuid;
            pgrfCDW = 0;

            // A non-zero, non-sentinel existing doc data means another editor already owns the document; we don't share.
            if (punkDocDataExisting != IntPtr.Zero && punkDocDataExisting != new IntPtr(-1))
            {
                return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
            }

            ClassDiagramEditorPane pane = new();
            ppunkDocView = Marshal.GetIUnknownForObject(pane);
            ppunkDocData = Marshal.GetIUnknownForObject(pane);

            return VSConstants.S_OK;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.serviceProvider?.Dispose();
        }
    }
}
