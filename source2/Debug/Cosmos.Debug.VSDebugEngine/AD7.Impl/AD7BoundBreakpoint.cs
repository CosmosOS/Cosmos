using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Diagnostics;
using Microsoft.VisualStudio;

namespace Cosmos.Debug.VSDebugEngine
{
    // This class represents a breakpoint that has been bound to a location in the debuggee. It is a child of the pending breakpoint
    // that creates it. Unless the pending breakpoint only has one bound breakpoint, each bound breakpoint is displayed as a child of the
    // pending breakpoint in the breakpoints window. Otherwise, only one is displayed.
    class AD7BoundBreakpoint : IDebugBoundBreakpoint2
    {
        private AD7PendingBreakpoint m_pendingBreakpoint;
        private AD7BreakpointResolution m_breakpointResolution;
        private AD7Engine m_engine;
        internal uint m_address;                                                                                               

        private bool m_enabled;
        private bool m_deleted;

        public AD7BoundBreakpoint(AD7Engine engine, uint address, AD7PendingBreakpoint pendingBreakpoint, AD7BreakpointResolution breakpointResolution)
        {
            m_engine = engine;
            m_address = address;
            m_pendingBreakpoint = pendingBreakpoint;
            m_breakpointResolution = breakpointResolution;
            m_enabled = true;
            m_deleted = false;
            m_engine.mProcess.SetBreakpointAddress(address);
        }

        #region IDebugBoundBreakpoint2 Members

        // Called when the breakpoint is being deleted by the user.
        int IDebugBoundBreakpoint2.Delete()
        {
            if (!m_deleted)
            {
                m_deleted = true;
                m_pendingBreakpoint.OnBoundBreakpointDeleted(this);
            }

            return VSConstants.S_OK;
        }

        // Called by the debugger UI when the user is enabling or disabling a breakpoint.
        int IDebugBoundBreakpoint2.Enable(int fEnable)
        {
            bool enabled = fEnable == 0 ? false : true;
            if (m_enabled != enabled)
            {
                // A production debug engine would remove or add the underlying int3 here. The sample engine does not support true disabling
                // of breakpionts.
            }
            m_enabled = fEnable == 0 ? false : true;
            return VSConstants.S_OK;
        }

        // Return the breakpoint resolution which describes how the breakpoint bound in the debuggee.
        int IDebugBoundBreakpoint2.GetBreakpointResolution(out IDebugBreakpointResolution2 ppBPResolution)
        {
            ppBPResolution = m_breakpointResolution;
            return VSConstants.S_OK;
        }

        // Return the pending breakpoint for this bound breakpoint.
        int IDebugBoundBreakpoint2.GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBreakpoint)
        {
            ppPendingBreakpoint = m_pendingBreakpoint;
            return VSConstants.S_OK;
        }

        // 
        int IDebugBoundBreakpoint2.GetState(out uint pState)
        {
            pState = 0;

            if (m_deleted)
            {
                pState = (uint)enum_BP_STATE.BPS_DELETED;
            }
            else if (m_enabled)
            {
                pState = (uint)enum_BP_STATE.BPS_ENABLED;
            }
            else if (!m_enabled)
            {
                pState = (uint)enum_BP_STATE.BPS_DISABLED;
            }

            return VSConstants.S_OK;
        }

        // The sample engine does not support hit counts on breakpoints. A real-world debugger will want to keep track 
        // of how many times a particular bound breakpoint has been hit and return it here.
        int IDebugBoundBreakpoint2.GetHitCount(out uint pdwHitCount)
        {
            throw new NotImplementedException();
        }

        // The sample engine does not support conditions on breakpoints.
        // A real-world debugger will use this to specify when a breakpoint will be hit
        // and when it should be ignored.
        int IDebugBoundBreakpoint2.SetCondition(BP_CONDITION bpCondition)
        {
            throw new NotImplementedException();
        }

        // The sample engine does not support hit counts on breakpoints. A real-world debugger will want to keep track 
        // of how many times a particular bound breakpoint has been hit. The debugger calls SetHitCount when the user 
        // resets a breakpoint's hit count.
        int IDebugBoundBreakpoint2.SetHitCount(uint dwHitCount)
        {
            throw new NotImplementedException();
        }

        // The sample engine does not support pass counts on breakpoints.
        // This is used to specify the breakpoint hit count condition.
        int IDebugBoundBreakpoint2.SetPassCount(BP_PASSCOUNT bpPassCount)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
