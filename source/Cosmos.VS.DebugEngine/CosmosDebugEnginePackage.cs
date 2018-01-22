using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

using Cosmos.VS.DebugEngine.Commands;

namespace Cosmos.VS.DebugEngine
{
    [Guid(Guids.guidPackageString)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.ShellInitialized_string)]
    public sealed class CosmosDebugEnginePackage : Package, IOleCommandTarget
    {
        private IOleCommandTarget packageCommandTarget;
        private DebugCommandHandler packageCommandHandler;

        protected override void Initialize()
        {
            base.Initialize();

            // TODO: remove this, as well as ProvideAutoLoad, if and when https://github.com/ericsink/SQLitePCL.raw/issues/181 is resolved.
            var xDir = IntPtr.Size == 4 ? "x86" : "x64";
            Environment.SetEnvironmentVariable("PATH",
                String.Join(";", Environment.GetEnvironmentVariable("PATH"),
                    Path.Combine(Path.GetDirectoryName(typeof(CosmosDebugEnginePackage).Assembly.Location), xDir)));

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
