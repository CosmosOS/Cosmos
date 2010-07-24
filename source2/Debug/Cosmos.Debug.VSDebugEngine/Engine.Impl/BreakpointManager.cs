using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Cosmos.Debug.VSDebugEngine {
    // This class manages breakpoints for the engine. 
    class BreakpointManager {
        private AD7Engine mEngine;
        internal List<AD7PendingBreakpoint> m_pendingBreakpoints = new List<AD7PendingBreakpoint>();

        public BreakpointManager(AD7Engine aEngine) {
            mEngine = aEngine;
        }
      
        // A helper method used to construct a new pending breakpoint.
        public void CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, out IDebugPendingBreakpoint2 ppPendingBP) {
            AD7PendingBreakpoint pendingBreakpoint = new AD7PendingBreakpoint(pBPRequest, mEngine, this);
            ppPendingBP = (IDebugPendingBreakpoint2)pendingBreakpoint;
            m_pendingBreakpoints.Add(pendingBreakpoint);
        }

        // Called from the engine's detach method to remove the debugger's breakpoint instructions.
        public void ClearBoundBreakpoints() {
            foreach (AD7PendingBreakpoint pendingBreakpoint in m_pendingBreakpoints) {
                pendingBreakpoint.ClearBoundBreakpoints();
            }
        }
    }
}
