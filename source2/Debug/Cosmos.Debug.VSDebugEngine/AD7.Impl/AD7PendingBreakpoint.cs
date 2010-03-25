using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Diagnostics;

namespace Cosmos.Debug.VSDebugEngine
{
    // This class represents a pending breakpoint which is an abstract representation of a breakpoint before it is bound.
    // When a user creates a new breakpoint, the pending breakpoint is created and is later bound. The bound breakpoints
    // become children of the pending breakpoint.
    class AD7PendingBreakpoint : IDebugPendingBreakpoint2
    {       
        // The breakpoint request that resulted in this pending breakpoint being created.
        private IDebugBreakpointRequest2 m_pBPRequest;
        private BP_REQUEST_INFO m_bpRequestInfo; 
        private AD7Engine m_engine;
        private BreakpointManager m_bpManager;

        internal System.Collections.Generic.List<AD7BoundBreakpoint> m_boundBreakpoints;

        private bool m_enabled;
        private bool m_deleted;       

        public AD7PendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, AD7Engine engine, BreakpointManager bpManager)
        {
            m_pBPRequest = pBPRequest;
            BP_REQUEST_INFO[] requestInfo = new BP_REQUEST_INFO[1];
            EngineUtils.CheckOk(m_pBPRequest.GetRequestInfo((uint)enum_BPREQI_FIELDS.BPREQI_BPLOCATION, requestInfo));
            m_bpRequestInfo = requestInfo[0];
            EngineUtils.CheckOk(m_pBPRequest.GetRequestInfo((uint)enum_BPREQI_FIELDS.BPREQI_THREAD, requestInfo));

            m_engine = engine;
            m_bpManager = bpManager;
            m_boundBreakpoints = new System.Collections.Generic.List<AD7BoundBreakpoint>();
            
            m_enabled = true;
            m_deleted = false;
        }

        private bool CanBind()
        {
            // The sample engine only supports breakpoints on a file and line number. No other types of breakpoints are supported.
            if (this.m_deleted || m_bpRequestInfo.bpLocation.bpLocationType != (uint)enum_BP_LOCATION_TYPE.BPLT_CODE_FILE_LINE)
            {
                return false;
            }

            if (m_engine.mProcess == null)
            {
                return false;
            }

            return true;
        }

        // Get the document context for this pending breakpoint. A document context is a abstract representation of a source file 
        // location.
        public AD7DocumentContext GetDocumentContext(uint address)
        {
            IDebugDocumentPosition2 docPosition = (IDebugDocumentPosition2)(Marshal.GetObjectForIUnknown(m_bpRequestInfo.bpLocation.unionmember2));
            string documentName;
            EngineUtils.CheckOk(docPosition.GetFileName(out documentName));

            // Get the location in the document that the breakpoint is in.
            TEXT_POSITION[] startPosition = new TEXT_POSITION[1];
            TEXT_POSITION[] endPosition = new TEXT_POSITION[1];
            EngineUtils.CheckOk(docPosition.GetRange(startPosition, endPosition));           

            AD7MemoryAddress codeContext = new AD7MemoryAddress(m_engine, address);
            
            return new AD7DocumentContext(documentName, startPosition[0], startPosition[0], codeContext);
        }

        // Remove all of the bound breakpoints for this pending breakpoint
        public void ClearBoundBreakpoints()
        {
            lock (m_boundBreakpoints)
            {
                for (int i = m_boundBreakpoints.Count - 1; i >= 0; i--)
                {
                    ((IDebugBoundBreakpoint2)m_boundBreakpoints[i]).Delete();
                }
            }
        }

        // Called by bound breakpoints when they are being deleted.
        public void OnBoundBreakpointDeleted(AD7BoundBreakpoint boundBreakpoint)
        {
            lock (m_boundBreakpoints)
            {
                m_boundBreakpoints.Remove(boundBreakpoint);
            }
        }

        #region IDebugPendingBreakpoint2 Members

        // Binds this pending breakpoint to one or more code locations.
        int IDebugPendingBreakpoint2.Bind()
        {
            try
            {
                Trace.WriteLine("Try to bind breakpoint");
                if (CanBind())
                {
                    Trace.WriteLine("Succeeded");
                    IDebugDocumentPosition2 docPosition = (IDebugDocumentPosition2)(Marshal.GetObjectForIUnknown(m_bpRequestInfo.bpLocation.unionmember2));

                    // Get the name of the document that the breakpoint was put in
                    string documentName;
                    EngineUtils.CheckOk(docPosition.GetFileName(out documentName));

                    // Get the location in the document that the breakpoint is in.
                    TEXT_POSITION[] startPosition = new TEXT_POSITION[1];
                    TEXT_POSITION[] endPosition = new TEXT_POSITION[1];
                    EngineUtils.CheckOk(docPosition.GetRange(startPosition, endPosition));

                    uint xAddress = 0;
                    if( m_engine.mProcess.mReverseSourceMappings.FindAddressForSourceLocation(documentName, startPosition[0].dwLine + 1, startPosition[0].dwColumn, out xAddress)){
                        var xBPR = new AD7BreakpointResolution(m_engine, xAddress, GetDocumentContext(xAddress));
                        var xBBP = new AD7BoundBreakpoint(m_engine, xAddress, this, xBPR);
                        m_boundBreakpoints.Add(xBBP);
                        m_engine.mProcess.SetBreakpointAddress(xAddress);
                    }

                    // Ask the symbol engine to find all addresses in all modules with symbols that match this source and line number.
                    //uint[] addresses = m_engine.DebuggedProcess.GetAddressesForSourceLocation(null, 
                    //                                                                           documentName, 
                    //                                                                           startPosition[0].dwLine + 1, 
                    //                                                                           startPosition[0].dwColumn);
                    lock (m_boundBreakpoints)
                    {
                        //foreach (uint addr in addresses)
                        //{
                        //    AD7BreakpointResolution breakpointResolution = new AD7BreakpointResolution(m_engine, addr, GetDocumentContext(addr));
                        //    AD7BoundBreakpoint boundBreakpoint = new AD7BoundBreakpoint(m_engine, addr, this, breakpointResolution);
                        //    m_boundBreakpoints.Add(boundBreakpoint);
                        //    m_engine.DebuggedProcess.SetBreakpoint(addr, boundBreakpoint);
                        //}
                    }
                    
                    return VSConstants.S_OK;
                }
                else
                {
                    Trace.WriteLine("Not succesful!");
                    // The breakpoint could not be bound. This may occur for many reasons such as an invalid location, an invalid expression, etc...
                    // The sample engine does not support this, but a real world engine will want to send an instance of IDebugBreakpointErrorEvent2 to the
                    // UI and return a valid instance of IDebugErrorBreakpoint2 from IDebugPendingBreakpoint2::EnumErrorBreakpoints. The debugger will then
                    // display information about why the breakpoint did not bind to the user.
                    return VSConstants.S_FALSE;
                }
            }
            //catch (ComponentException e)
            //{
            //    return e.HResult;
            //}
            catch (Exception e)
            {
                return EngineUtils.UnexpectedException(e);
            }
        }

        // Determines whether this pending breakpoint can bind to a code location.
        int IDebugPendingBreakpoint2.CanBind(out IEnumDebugErrorBreakpoints2 ppErrorEnum)
        {
            ppErrorEnum = null;

            if (!CanBind())
            {
                // Called to determine if a pending breakpoint can be bound. 
                // The breakpoint may not be bound for many reasons such as an invalid location, an invalid expression, etc...
                // The sample engine does not support this, but a real world engine will want to return a valid enumeration of IDebugErrorBreakpoint2.
                // The debugger will then display information about why the breakpoint did not bind to the user.
                ppErrorEnum = null;
                return VSConstants.S_FALSE;
            }

            return VSConstants.S_OK;
        }

        // Deletes this pending breakpoint and all breakpoints bound from it.
        int IDebugPendingBreakpoint2.Delete()
        {
            lock (m_boundBreakpoints)
            {
                for (int i = m_boundBreakpoints.Count - 1; i >= 0; i--)
                {
                    ((IDebugBoundBreakpoint2)m_boundBreakpoints[i]).Delete();
                }
            }

            return VSConstants.S_OK;
        }

        // Toggles the enabled state of this pending breakpoint.
        int IDebugPendingBreakpoint2.Enable(int fEnable)
        {
            lock (m_boundBreakpoints)
            {
                m_enabled = fEnable == 0 ? false : true;

                foreach (AD7BoundBreakpoint bp in m_boundBreakpoints)
                {
                    ((IDebugBoundBreakpoint2)m_boundBreakpoints).Enable(fEnable);
                }
            }

            return VSConstants.S_OK;
        }

        // Enumerates all breakpoints bound from this pending breakpoint
        int IDebugPendingBreakpoint2.EnumBoundBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
        {
            lock (m_boundBreakpoints)
            {
                IDebugBoundBreakpoint2[] boundBreakpoints = m_boundBreakpoints.ToArray();
                ppEnum = new AD7BoundBreakpointsEnum(boundBreakpoints);
            }
            return VSConstants.S_OK;
        }

        // Enumerates all error breakpoints that resulted from this pending breakpoint.
        int IDebugPendingBreakpoint2.EnumErrorBreakpoints(uint bpErrorType, out IEnumDebugErrorBreakpoints2 ppEnum)
        {
            // Called when a pending breakpoint could not be bound. This may occur for many reasons such as an invalid location, an invalid expression, etc...
            // The sample engine does not support this, but a real world engine will want to send an instance of IDebugBreakpointErrorEvent2 to the
            // UI and return a valid enumeration of IDebugErrorBreakpoint2 from IDebugPendingBreakpoint2::EnumErrorBreakpoints. The debugger will then
            // display information about why the breakpoint did not bind to the user.
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        // Gets the breakpoint request that was used to create this pending breakpoint
        int IDebugPendingBreakpoint2.GetBreakpointRequest(out IDebugBreakpointRequest2 ppBPRequest)
        {
            ppBPRequest = this.m_pBPRequest;
            return VSConstants.S_OK;
        }

        // Gets the state of this pending breakpoint.
        int IDebugPendingBreakpoint2.GetState(PENDING_BP_STATE_INFO[] pState)
        {
            if (m_deleted)
            {
                pState[0].state = (uint)enum_BP_STATE.BPS_DELETED;
            }
            else if (m_enabled)
            {
                pState[0].state = (uint)enum_BP_STATE.BPS_ENABLED;
            }
            else if (!m_enabled)
            {
                pState[0].state = (uint)enum_BP_STATE.BPS_DISABLED;
            }

            return VSConstants.S_OK;
        }

        // The sample engine does not support conditions on breakpoints.
        int IDebugPendingBreakpoint2.SetCondition(BP_CONDITION bpCondition)
        {
            throw new NotImplementedException();
        }

        // The sample engine does not support pass counts on breakpoints.
        int IDebugPendingBreakpoint2.SetPassCount(BP_PASSCOUNT bpPassCount)
        {
            throw new NotImplementedException();
        }

        // Toggles the virtualized state of this pending breakpoint. When a pending breakpoint is virtualized, 
        // the debug engine will attempt to bind it every time new code loads into the program.
        // The sample engine will does not support this.
        int IDebugPendingBreakpoint2.Virtualize(int fVirtualize)
        {
            return VSConstants.S_OK;
        }

        #endregion
    }
}
