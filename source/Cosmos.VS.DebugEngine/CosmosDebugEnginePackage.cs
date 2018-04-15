using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

using Cosmos.VS.DebugEngine.Commands;

[assembly: ProvideBindingRedirection(
    AssemblyName = "SQLitePCLRaw.batteries_green",
    NewVersion = "1.1.10.86",
    OldVersionLowerBound = "1.0.0.0",
    OldVersionUpperBound = "1.1.10.86")]

[assembly: ProvideBindingRedirection(
    AssemblyName = "SQLitePCLRaw.batteries_v2",
    NewVersion = "1.1.10.86",
    OldVersionLowerBound = "1.0.0.0",
    OldVersionUpperBound = "1.1.10.86")]

[assembly: ProvideBindingRedirection(
    AssemblyName = "SQLitePCLRaw.core",
    NewVersion = "1.1.10.86",
    OldVersionLowerBound = "1.0.0.0",
    OldVersionUpperBound = "1.1.10.86")]

[assembly: ProvideBindingRedirection(
    AssemblyName = "SQLitePCLRaw.provider.e_sqlite3",
    NewVersion = "1.1.10.86",
    OldVersionLowerBound = "1.0.0.0",
    OldVersionUpperBound = "1.1.10.86")]

namespace Cosmos.VS.DebugEngine
{
    [Guid(Guids.guidPackageString)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    internal sealed class CosmosDebugEnginePackage : Package, IOleCommandTarget
    {
        private IOleCommandTarget packageCommandTarget;
        private DebugCommandHandler packageCommandHandler;

        protected override void Initialize()
        {
            base.Initialize();

            packageCommandTarget = GetService(typeof(IOleCommandTarget)) as IOleCommandTarget;
            packageCommandHandler = new DebugCommandHandler(this);
        }

        int IOleCommandTarget.Exec(ref Guid cmdGroup, uint nCmdID, uint nCmdExecOpt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (cmdGroup == Guids.DebugEngineCmdSetGuid)
            {
                return packageCommandHandler.Execute(nCmdID, nCmdExecOpt, pvaIn, pvaOut);
            }

            return packageCommandTarget.Exec(ref cmdGroup, nCmdID, nCmdExecOpt, pvaIn, pvaOut);
        }

        int IOleCommandTarget.QueryStatus(ref Guid cmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (cmdGroup == Guids.DebugEngineCmdSetGuid)
            {
                return packageCommandHandler.Query(cCmds, prgCmds, pCmdText);
            }

            return packageCommandTarget.QueryStatus(ref cmdGroup, cCmds, prgCmds, pCmdText);
        }
    }
}
