using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Cosmos.Debug.VSDebugEngine {
    // This class manages breakpoints for the engine. 
    // Breakpoint types: http://msdn.microsoft.com/en-us/library/bb161312%28VS.80%29.aspx\
    // Binding breakpoints: http://msdn.microsoft.com/en-us/library/bb146593%28v=VS.80%29.aspx
    class BreakpointManager {
        private AD7Engine mEngine;
        internal List<AD7PendingBreakpoint> mPendingBreakpoints = new List<AD7PendingBreakpoint>();

        public BreakpointManager(AD7Engine aEngine) {
            mEngine = aEngine;
        }
      
        // A helper method used to construct a new pending breakpoint.
        public void CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, out IDebugPendingBreakpoint2 ppPendingBP) {
            AD7PendingBreakpoint pendingBreakpoint = new AD7PendingBreakpoint(pBPRequest, mEngine, this);
            ppPendingBP = (IDebugPendingBreakpoint2)pendingBreakpoint;
            mPendingBreakpoints.Add(pendingBreakpoint);
        }

        // Called from the engine's detach method to remove the debugger's breakpoint instructions.
        public void ClearBoundBreakpoints() {
            foreach (var xBP in mPendingBreakpoints) {
                xBP.ClearBoundBreakpoints();
            }
        }
    }
}
