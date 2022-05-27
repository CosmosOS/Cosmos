using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

namespace Cosmos.VS.DebugEngine.Commands
{
    public class DebugCommandHandlerBase
    {

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