using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cosmos.Debug.Common.CDebugger;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Cosmos.Debug.VSDebugEngine {
    // This class manages breakpoints for the engine. 
    public class BreakpointManager {
        public const int MaxBP = 256;
        protected AD7Engine mEngine;
        protected DebugConnector mDbgConnector;
        public List<AD7PendingBreakpoint> mPendingBPs = new List<AD7PendingBreakpoint>();
        public AD7BoundBreakpoint[] mActiveBPs = new AD7BoundBreakpoint[MaxBP];

        public BreakpointManager(AD7Engine aEngine) {
            mEngine = aEngine;
            mDbgConnector = aEngine.mProcess.mDbgConnector;
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

        // Creates an entry and remotely enables the breakpoint in the debug stub
        public int RemoteEnable(AD7BoundBreakpoint aBBP) {
            for (int xID = 0; xID < MaxBP; xID++) {
                if (mActiveBPs[xID] == null) {
                    mActiveBPs[xID] = aBBP;
                    mDbgConnector.SetBreakpointAddress(xID, aBBP.mAddress);
                    return xID;
                }
            }
            throw new Exception("Maximum number of active breakpoints exceeded (" + MaxBP + ").");
        }
    }
}
