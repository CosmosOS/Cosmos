using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Cosmos.VS.DebugEngine.Engine.Impl;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Cosmos.VS.DebugEngine.AD7.Impl
{
    // This class represents a pending breakpoint which is an abstract representation of a breakpoint before it is bound.
    // When a user creates a new breakpoint, the pending breakpoint is created and is later bound. The bound breakpoints
    // become children of the pending breakpoint.
    public class AD7PendingBreakpoint : IDebugPendingBreakpoint2
    {
        // The breakpoint request that resulted in this pending breakpoint being created.
        private IDebugBreakpointRequest2 m_pBPRequest;
        private BP_REQUEST_INFO mBpRequestInfo;
        private AD7Engine mEngine;
        private BreakpointManager mBPMgr;

        internal List<AD7BoundBreakpoint> mBoundBPs = new List<AD7BoundBreakpoint>();

        private bool mEnabled = true;
        private bool mDeleted = false;

        public AD7PendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, AD7Engine engine, BreakpointManager bpManager)
        {
            m_pBPRequest = pBPRequest;
            BP_REQUEST_INFO[] requestInfo = new BP_REQUEST_INFO[1];
            EngineUtils.CheckOk(m_pBPRequest.GetRequestInfo(enum_BPREQI_FIELDS.BPREQI_BPLOCATION, requestInfo));
            mBpRequestInfo = requestInfo[0];
            EngineUtils.CheckOk(m_pBPRequest.GetRequestInfo(enum_BPREQI_FIELDS.BPREQI_THREAD, requestInfo));

            mEngine = engine;
            mBPMgr = bpManager;
        }

        private bool CanBind()
        {
            // The sample engine only supports breakpoints on a file and line number. No other types of breakpoints are supported.
            if (mDeleted || mBpRequestInfo.bpLocation.bpLocationType != (uint)enum_BP_LOCATION_TYPE.BPLT_CODE_FILE_LINE)
            {
                return false;
            }
            else if (mEngine.mProcess == null)
            {
                return false;
            }

            return true;
        }

        // Get the document context for this pending breakpoint. A document context is a abstract representation of a source file
        // location.
        public AD7DocumentContext GetDocumentContext(uint address)
        {
            IDebugDocumentPosition2 docPosition = (IDebugDocumentPosition2)(Marshal.GetObjectForIUnknown(mBpRequestInfo.bpLocation.unionmember2));
            string documentName;
            EngineUtils.CheckOk(docPosition.GetFileName(out documentName));

            // Get the location in the document that the breakpoint is in.
            TEXT_POSITION[] startPosition = new TEXT_POSITION[1];
            TEXT_POSITION[] endPosition = new TEXT_POSITION[1];
            EngineUtils.CheckOk(docPosition.GetRange(startPosition, endPosition));

            AD7MemoryAddress codeContext = new AD7MemoryAddress(mEngine, address);

            return new AD7DocumentContext(documentName, startPosition[0], startPosition[0], codeContext);
        }

        // Remove all of the bound breakpoints for this pending breakpoint
        public void ClearBoundBreakpoints()
        {
            lock (mBoundBPs)
            {
                for (int i = mBoundBPs.Count - 1; i >= 0; i--)
                {
                    ((IDebugBoundBreakpoint2)mBoundBPs[i]).Delete();
                }
            }
        }

        // Called by bound breakpoints when they are being deleted.
        public void OnBoundBreakpointDeleted(AD7BoundBreakpoint boundBreakpoint)
        {
            lock (mBoundBPs)
            {
                mBoundBPs.Remove(boundBreakpoint);
            }
        }

        // Binds this pending breakpoint to one or more code locations.
        int IDebugPendingBreakpoint2.Bind()
        {
            try
            {
                if (CanBind())
                {
                    var xDocPos = (IDebugDocumentPosition2)(Marshal.GetObjectForIUnknown(mBpRequestInfo.bpLocation.unionmember2));

                    // Get the name of the document that the breakpoint was put in
                    string xDocName;
                    EngineUtils.CheckOk(xDocPos.GetFileName(out xDocName));
                    xDocName = xDocName.ToLower(); //Bug: Some filenames were returned with the drive letter as lower case but in DocumentGUIDs it was captialised so file-not-found!

                    // Get the location in the document that the breakpoint is in.
                    var xStartPos = new TEXT_POSITION[1];
                    var xEndPos = new TEXT_POSITION[1];
                    EngineUtils.CheckOk(xDocPos.GetRange(xStartPos, xEndPos));

                    UInt32 xAddress = 0;
                    var xDebugInfo = mEngine.mProcess.mDebugInfoDb;

                    // We must check for DocID. This is important because in a solution that contains many projects,
                    // VS will send us BPs from other Cosmos projects (and possibly non Cosmos ones, didnt look that deep)
                    // but we wont have them in our doc list because it contains only ones from the currently project
                    // to run.
                    long xDocID;
                    if (xDebugInfo.DocumentGUIDs.TryGetValue(xDocName, out xDocID))
                    {
                        // Find which Method the Doc, Line, Col are in.
                        // Must add +1 for both Line and Col. They are 0 based, while SP ones are 1 based.
                        // () around << are VERY important.. + has precedence over <<
                        Int64 xPos = (((Int64)xStartPos[0].dwLine + 1) << 32) + xStartPos[0].dwColumn + 1;

                        try
                        {
                            var xMethod = xDebugInfo.GetMethodByDocumentIDAndLinePosition(xDocID, xPos, xPos);
                            var asm = xDebugInfo.GetAssemblyFileById(xMethod.AssemblyFileID);

                            // We have the method. Now find out what Sequence Point it belongs to.
                            var xSPs = xDebugInfo.GetSequencePoints(asm.Pathname, xMethod.MethodToken);
                            var xSP = xSPs.Single(q => q.LineColStart <= xPos && q.LineColEnd >= xPos);

                            // We have the Sequence Point, find the MethodILOp
                            var xOp = xDebugInfo.GetFirstMethodIlOpByMethodIdAndILOffset(xMethod.ID, xSP.Offset);

                            // Get the address of the Label
                            xAddress = xDebugInfo.GetAddressOfLabel(xOp.LabelName);


                            if (xAddress > 0)
                            {
                                var xBPR = new AD7BreakpointResolution(mEngine, xAddress, GetDocumentContext(xAddress));
                                var xBBP = new AD7BoundBreakpoint(mEngine, xAddress, this, xBPR);
                                mBoundBPs.Add(xBBP);
                            }

                            // Ask the symbol engine to find all addresses in all modules with symbols that match this source and line number.
                            //uint[] addresses = mEngine.DebuggedProcess.GetAddressesForSourceLocation(null, documentName, startPosition[0].dwLine + 1, startPosition[0].dwColumn);
                            lock (mBoundBPs)
                            {
                                //foreach (uint addr in addresses) {
                                //    AD7BreakpointResolution breakpointResolution = new AD7BreakpointResolution(mEngine, addr, GetDocumentContext(addr));
                                //    AD7BoundBreakpoint boundBreakpoint = new AD7BoundBreakpoint(mEngine, addr, this, breakpointResolution);
                                //    m_boundBreakpoints.Add(boundBreakpoint);
                                //    mEngine.DebuggedProcess.SetBreakpoint(addr, boundBreakpoint);
                                //}
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            //No elements in potXMethods sequence!
                            return VSConstants.S_FALSE;
                        }
                    }
                    return VSConstants.S_OK;
                }
                else
                {
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
            lock (mBoundBPs)
            {
                for (int i = mBoundBPs.Count - 1; i >= 0; i--)
                {
                    ((IDebugBoundBreakpoint2)mBoundBPs[i]).Delete();
                }
            }

            return VSConstants.S_OK;
        }

        // Toggles the enabled state of this pending breakpoint.
        int IDebugPendingBreakpoint2.Enable(int fEnable)
        {
            lock (mBoundBPs)
            {
                mEnabled = fEnable != 0;

                foreach (AD7BoundBreakpoint bp in mBoundBPs)
                {
                    ((IDebugBoundBreakpoint2)mBoundBPs).Enable(fEnable);
                }
            }

            return VSConstants.S_OK;
        }

        // Enumerates all breakpoints bound from this pending breakpoint
        int IDebugPendingBreakpoint2.EnumBoundBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
        {
            lock (mBoundBPs)
            {
                IDebugBoundBreakpoint2[] boundBreakpoints = mBoundBPs.ToArray();
                ppEnum = new AD7BoundBreakpointsEnum(boundBreakpoints);
            }
            return VSConstants.S_OK;
        }

        // Enumerates all error breakpoints that resulted from this pending breakpoint.
        int IDebugPendingBreakpoint2.EnumErrorBreakpoints(enum_BP_ERROR_TYPE bpErrorType, out IEnumDebugErrorBreakpoints2 ppEnum)
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
            pState[0].state = enum_PENDING_BP_STATE.PBPS_DISABLED;
            if (mDeleted)
            {
                pState[0].state = enum_PENDING_BP_STATE.PBPS_DELETED;
            }
            else if (mEnabled)
            {
                pState[0].state = enum_PENDING_BP_STATE.PBPS_ENABLED;
            }
            else
            {
                pState[0].state = enum_PENDING_BP_STATE.PBPS_DISABLED;
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

    }
}
