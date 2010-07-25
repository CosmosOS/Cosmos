using System;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Diagnostics;

namespace Cosmos.Debug.VSDebugEngine {
    // This class manages breakpoints for the engine. 
    class BreakpointManager {
        private AD7Engine mEngine;
        internal System.Collections.Generic.List<AD7PendingBreakpoint> mPendingBPs;

        public BreakpointManager(AD7Engine aEngine) {
            mEngine = aEngine;
            mPendingBPs = new System.Collections.Generic.List<AD7PendingBreakpoint>();
        }
      
        // A helper method used to construct a new pending breakpoint.
        public void CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, out IDebugPendingBreakpoint2 ppPendingBP) {
            AD7PendingBreakpoint pendingBreakpoint = new AD7PendingBreakpoint(pBPRequest, mEngine, this);
            ppPendingBP = (IDebugPendingBreakpoint2)pendingBreakpoint;
            mPendingBPs.Add(pendingBreakpoint);
        }

        // Called from the engine's detach method to remove the debugger's breakpoint instructions.
        public void ClearBoundBreakpoints() {
            foreach (AD7PendingBreakpoint pendingBreakpoint in mPendingBPs) {
                pendingBreakpoint.ClearBoundBreakpoints();
            }
        }
    }
}
