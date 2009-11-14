using System;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Cosmos.Debug.VSDebugEngine
{
    // This class manages breakpoints for the engine. 
    class BreakpointManager
    {
        private AD7Engine m_engine;
        internal System.Collections.Generic.List<AD7PendingBreakpoint> m_pendingBreakpoints;

        public BreakpointManager(AD7Engine engine)
        {
            m_engine = engine;
            m_pendingBreakpoints = new System.Collections.Generic.List<AD7PendingBreakpoint>();
        }
      
        // A helper method used to construct a new pending breakpoint.
        public void CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, out IDebugPendingBreakpoint2 ppPendingBP)
        {
            AD7PendingBreakpoint pendingBreakpoint = new AD7PendingBreakpoint(pBPRequest, m_engine, this);
            ppPendingBP = (IDebugPendingBreakpoint2)pendingBreakpoint;
            m_pendingBreakpoints.Add(pendingBreakpoint);
        }

        // Called from the engine's detach method to remove the debugger's breakpoint instructions.
        public void ClearBoundBreakpoints()
        {
            foreach (AD7PendingBreakpoint pendingBreakpoint in m_pendingBreakpoints)
            {
                pendingBreakpoint.ClearBoundBreakpoints();
            }
        }
    }
}
