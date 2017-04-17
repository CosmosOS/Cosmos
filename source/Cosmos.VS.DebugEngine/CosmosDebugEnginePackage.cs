using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.IO;
using Cosmos.VS.DebugEngine.Commands;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace Cosmos.VS.DebugEngine
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(Guids.guidPackageString)]
    public sealed class CosmosDebugEnginePackage : Package, IOleCommandTarget
    {
        private IOleCommandTarget packageCommandTarget;
        private DebugCommandHandler packageCommandHandler;

        public CosmosDebugEnginePackage()
        {
        }

        #region Package Members

        protected override void Initialize()
        {
            base.Initialize();

            packageCommandTarget = GetService(typeof(IOleCommandTarget)) as IOleCommandTarget;
            packageCommandHandler = new DebugCommandHandler(this);
        }

        #endregion

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