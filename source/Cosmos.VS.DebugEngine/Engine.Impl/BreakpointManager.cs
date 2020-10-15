using System;
using System.Collections.Generic;
using Cosmos.VS.DebugEngine.AD7.Impl;
using Cosmos.Debug.DebugConnectors;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Cosmos.VS.DebugEngine.Engine.Impl {
    // This class manages breakpoints for the engine.
    public class BreakpointManager {
        public const int MaxBP = 256;
        protected AD7Engine mEngine;
        protected DebugConnector mDbgConnector;
        public List<AD7PendingBreakpoint> mPendingBPs = new List<AD7PendingBreakpoint>();
        public AD7BoundBreakpoint[] mActiveBPs = new AD7BoundBreakpoint[MaxBP];

        public BreakpointManager(AD7Engine aEngine) {
            mEngine = aEngine;
        }

        public void SetDebugConnector(DebugConnector aConnector) {
            mDbgConnector = aConnector;
        }

        // A helper method used to construct a new pending breakpoint.
        public void CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, out IDebugPendingBreakpoint2 ppPendingBP) {
            var pendingBreakpoint = new AD7PendingBreakpoint(pBPRequest, mEngine, this);
            ppPendingBP = pendingBreakpoint;
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
                    var label = mEngine.mProcess.mDebugInfoDb.GetLabels(aBBP.mAddress)[0];
                    mEngine.mProcess.INT3sSet.Add(new KeyValuePair<uint, string>(aBBP.mAddress, label));
                    mDbgConnector.SetBreakpoint(xID, aBBP.mAddress);
                    return xID;
                }
            }
            throw new Exception("Maximum number of active breakpoints exceeded (" + MaxBP + ").");
        }

        public void RemoteDisable(AD7BoundBreakpoint aBBP) {
            mActiveBPs[aBBP.RemoteID] = null;

            int index = mEngine.mProcess.INT3sSet.FindIndex(x => x.Key == aBBP.mAddress);
            mEngine.mProcess.INT3sSet.RemoveAt(index);
            mDbgConnector.DeleteBreakpoint(aBBP.RemoteID);
        }
    }
}
