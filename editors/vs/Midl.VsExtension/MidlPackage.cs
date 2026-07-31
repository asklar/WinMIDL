// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Midl.VsExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(MidlOptionsPage), "MIDL", "General", 0, 0, true)]
    [Guid(PackageGuidString)]
    public sealed class MidlPackage : AsyncPackage
    {
        public const string PackageGuidString = "b86d8fe3-55d1-4b94-a626-2a76b4d86567";

        internal static MidlPackage Instance { get; private set; }

        protected override async Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
        {
            Instance = this;
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await MidlCommands.InitializeAsync(this);
        }

        internal MidlOptionsPage GetOptions() =>
            GetDialogPage(typeof(MidlOptionsPage)) as MidlOptionsPage;
    }
}
