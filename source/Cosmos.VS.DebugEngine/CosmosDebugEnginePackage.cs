using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Cosmos.VS.DebugEngine.Commands;
using Microsoft;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

//[assembly: ProvideBindingRedirection(
//    AssemblyName = "SQLitePCLRaw.batteries_v2",
//    NewVersion = "2.0.6.1341",
//    OldVersionLowerBound = "1.0.0.0",
//    OldVersionUpperBound = "2.0.6.1341")]

//[assembly: ProvideBindingRedirection(
//    AssemblyName = "SQLitePCLRaw.core",
//    NewVersion = "2.0.6.1341",
//    OldVersionLowerBound = "1.0.0.0",
//    OldVersionUpperBound = "2.0.6.1341")]

//[assembly: ProvideBindingRedirection(
//    AssemblyName = "SQLitePCLRaw.provider.e_sqlite3",
//    NewVersion = "2.0.6.1341",
//    OldVersionLowerBound = "1.0.0.0",
//    OldVersionUpperBound = "2.0.6.1341")]

namespace Cosmos.VS.DebugEngine
{
    [Guid(Guids.guidPackageString)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    internal sealed class CosmosDebugEnginePackage : AsyncPackage, IOleCommandTarget
    {
        private IOleCommandTarget packageCommandTarget;
        private DebugCommandHandler packageCommandHandler;

        protected override async Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            var xDir = IntPtr.Size == 4 ? "x86" : "x64";
            Environment.SetEnvironmentVariable("PATH", // add path so that it finds SQLitePCLRaw.nativelibrary
                String.Join(";", Environment.GetEnvironmentVariable("PATH"),
                    Path.Combine(Path.GetDirectoryName(typeof(CosmosDebugEnginePackage).Assembly.Location), xDir)));
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            packageCommandTarget = await GetServiceAsync(typeof(IOleCommandTarget)).ConfigureAwait(true) as IOleCommandTarget;
            Assumes.Present(packageCommandTarget);
            packageCommandHandler = new DebugCommandHandler(this);
        }

        int IOleCommandTarget.Exec(ref Guid cmdGroup, uint nCmdID, uint nCmdExecOpt, IntPtr pvaIn, IntPtr pvaOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (cmdGroup == Guids.DebugEngineCmdSetGuid)
            {
                return packageCommandHandler.Execute(nCmdID, nCmdExecOpt, pvaIn, pvaOut);
            }

            return packageCommandTarget.Exec(ref cmdGroup, nCmdID, nCmdExecOpt, pvaIn, pvaOut);
        }

        int IOleCommandTarget.QueryStatus(ref Guid cmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (cmdGroup == Guids.DebugEngineCmdSetGuid)
            {
                return packageCommandHandler.Query(cCmds, prgCmds, pCmdText);
            }

            return packageCommandTarget.QueryStatus(ref cmdGroup, cCmds, prgCmds, pCmdText);
        }
    }
}
