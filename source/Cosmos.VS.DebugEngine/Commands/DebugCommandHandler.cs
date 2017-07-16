using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using IServiceProvider = System.IServiceProvider;

namespace Cosmos.VS.DebugEngine.Commands
{
    public delegate int DebugCommandExecute(uint nCmdExecOpt, IntPtr pvaIn, IntPtr pvaOut);

    public class DebugCommandHandler
    {
        public DebugCommandHandler(IServiceProvider serviceProvider)
        {
            var launchCommand = new DebugLaunchCommand(serviceProvider);
            OnDebugLaunch += launchCommand.Execute;

            var logCommand = new DebugLogCommand(serviceProvider);
            OnDebugLog += logCommand.Execute;

            var execCommand = new DebugExecCommand(serviceProvider);
            OnDebugExec += execCommand.Execute;
        }

        public event DebugCommandExecute OnDebugLaunch;
        public event DebugCommandExecute OnDebugExec;
        public event DebugCommandExecute OnDebugLog;

        public int Execute(uint nCmdID, uint nCmdExecOpt, IntPtr pvaIn, IntPtr pvaOut)
        {
            switch (nCmdID)
            {
                case CmdIDList.DebugLaunchCmdID:
                    if (OnDebugLaunch != null)
                    {
                        return OnDebugLaunch(nCmdExecOpt, pvaIn, pvaOut);
                    }
                    break;
                case CmdIDList.DebugExecCmdID:
                    if (OnDebugExec != null)
                    {
                        return OnDebugExec(nCmdExecOpt, pvaIn, pvaOut);
                    }
                    break;
                case CmdIDList.DebugLogCmdID:
                    if (OnDebugLog != null)
                    {
                        return OnDebugLog(nCmdExecOpt, pvaIn, pvaOut);
                    }
                    break;
            }
            global::System.Diagnostics.Debug.Fail("Unknown command id");
            return VSConstants.E_NOTIMPL;
        }

        public int Query(uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            switch (prgCmds[0].cmdID)
            {
                case CmdIDList.DebugLaunchCmdID:
                case CmdIDList.DebugExecCmdID:
                case CmdIDList.DebugLogCmdID:
                    prgCmds[0].cmdf |= (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_INVISIBLE);
                    return VSConstants.S_OK;
            }

            global::System.Diagnostics.Debug.Fail("Unknown command id");
            return VSConstants.E_NOTIMPL;
        }
    }
}
