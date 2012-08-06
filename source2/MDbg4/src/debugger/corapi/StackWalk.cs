//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
//using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Microsoft.Samples.Debugging.CorDebug.NativeApi;
using Microsoft.Samples.Debugging.Native;

namespace Microsoft.Samples.Debugging.CorDebug
{ 
    // This class is a thin managed wrapper over the V3 stackwalking API (ICorDebugStackWalk).
    // It does not expose ICorDebugInternalFrame.  Use the derived class CorStackWalkEx if you want 
    // ICorDebugInternalFrame.
    public class CorStackWalk : WrapperBase
    {
        internal CorStackWalk (ICorDebugStackWalk stackwalk, CorThread thread)
            :base(stackwalk)
        {
            m_th = thread;
            m_sw = stackwalk;
        }

        internal ICorDebugStackWalk GetInterface ()
        {
            return m_sw;
        }

        [CLSCompliant(false)]
        public ICorDebugStackWalk Raw
        {
            get 
            { 
                return m_sw;
            }
        }

        //
        // IEnumerator interface
        //
        public virtual bool MoveNext()
        {
            int hr;
            if (m_init)
            {
                hr = m_sw.Next();
            }
            else
            {
                m_init = true;
                hr = (int)HResult.S_OK;
            }

            if (HRUtils.IsFailingHR(hr))
            {
                Marshal.ThrowExceptionForHR(hr);
            }
            if (hr == (int)HResult.S_OK)
            {
                ICorDebugFrame frame;
                m_sw.GetFrame(out frame);

                if (frame == null)
                {
                    m_frame = null;
                    return true;
                }

                m_frame = new CorFrame(frame);
                return true;
            }
            else
            {
                Debug.Assert(hr == (int)HResult.CORDBG_S_AT_END_OF_STACK);
                return false;
            }
        }

        public virtual void Reset()
        {
            // There is no way to reset.  Just create a new one.
            CorStackWalk s = m_th.CreateStackWalk(CorStackWalkType.PureV3StackWalk);
            m_sw = s.GetInterface();

            m_frame = null;
        }

        public CorFrame Current
        {
            get
            {
                return m_frame;
            }
        }

        #region Get/Set Context

        public void GetThreadContext(ContextFlags flags, int contextBufferSize, out int contextSize, IntPtr contextBuffer)
        {
            uint uContextSize = 0;
            m_sw.GetContext((uint)flags, (uint)contextBufferSize, out uContextSize, contextBuffer);
                contextSize = (int)uContextSize;
            }

        public INativeContext GetContext()
        {
            INativeContext context = ContextAllocator.GenerateContext();
            this.GetContext(context);
        
            return context;
        }

        // ClsComplaint version of GetThreadContext.
        // Caller must ensure that the context is valid, and for the right architecture.
        public void GetContext(INativeContext context)
        {
            using (IContextDirectAccessor w = context.OpenForDirectAccess())
            { // context buffer is now locked        

                // We initialize to a HUGE number so that we make sure GetThreadContext is updating the size variable.  If it doesn't,
                // then we will hit the assert statement below.
                int size = Int32.MaxValue;
                this.GetThreadContext((ContextFlags)context.Flags,w.Size, out size, w.RawBuffer);

                // We should only assert when the buffer is insufficient.  Since the runtime only keeps track of CONTEXT_CONTROL and CONTEXT_INTEGER
                // we will expect to create a larger buffer than absolutely necessary.  The context buffer will be the size of a FULL machine
                // context.
                Debug.Assert(size <= w.Size, String.Format("Insufficient Buffer:  Expected {0} bytes, but received {1}",w.Size, size));
            }
        }

        [CLSCompliant(false)]
        public void SetThreadContext ( CorDebugSetContextFlag flag, int contextSize, IntPtr contextBuffer)
        {
            m_sw.SetContext(flag, (uint)contextSize, contextBuffer);

            // update the current frame
            ICorDebugFrame frame;
            m_sw.GetFrame(out frame);

            if (frame == null)
            {
                m_frame = null;
            }
            else
            {
                m_frame = new CorFrame(frame);
            }
        }

        // ClsComplaint version of SetThreadContext.
        // Caller must ensure that the context is valid, and for the right architecture.
        public void SetContext(CorDebugSetContextFlag flag, INativeContext context)
        {
            using (IContextDirectAccessor w = context.OpenForDirectAccess())
            { // context buffer is now locked
                SetThreadContext(flag, w.Size, w.RawBuffer);
            }
        }

        #endregion // Get/Set Context

        protected ICorDebugStackWalk m_sw;
        protected CorThread m_th;
        protected CorFrame m_frame;

        // This is an artificat of managed enumerator semantics.  We must call MoveNext() once before the enumerator becomes "valid".
        // Once this occurs, m_init becomes true, but it is false at creation time
        protected bool m_init; // = false (at creation time)      
    } /* class CorStackWalk */

    // Unlike CorStackWalk, this class exposes ICorDebugInternalFrame.  It interleaves ICorDebugInternalFrames
    // with real stack frames strictly according to the frame address and the SP of the stack frames.
    public sealed class CorStackWalkEx : CorStackWalk
    {
        internal CorStackWalkEx (ICorDebugStackWalk stackwalk, CorThread thread)
            :base(stackwalk, thread)
        {
            m_internalFrameIndex = 0;
            m_internalFrames = thread.GetActiveInternalFrames();
            m_bEndOfStackFrame = false;
        }

        //
        // IEnumerator interface
        //
        public override bool MoveNext()
        {
            // This variable is used to store the child frame we are currently skipping for.  It's also
            // used as a flag to indicate whether this method should return or continue to the next frame.
            CorFrame childFrame = null;

            bool bMoreToWalk = false;
            while (true)
            {
                // Check to see if the frame we have just given back is a child frame.
                if (m_init)
                {
                    CorFrame prevFrame = this.Current;
                    if ((prevFrame != null) && prevFrame.IsChild)
                    {
                        childFrame = prevFrame;
                    }
                }
                
                bMoreToWalk = MoveNextWorker();
                if (!bMoreToWalk)
                {
                    // Unless there is a catastrophic failure, we should always find the parent frame for any
                    // child frame.
                    Debug.Assert(childFrame == null);
                    break;
                }

                if (childFrame != null)
                {
                    // We are currently skipping frames due to a child frame.

                    // Check whether the current frame is the parent frame.
                    CorFrame currentFrame = this.Current;
                    if ((currentFrame != null) && childFrame.IsMatchingParentFrame(currentFrame))
                    {
                        // We have reached the parent frame.  We should skip the parent frame as well, so clear 
                        // childFrame and unwind to the caller of the parent frame.  We will break out of the
                        // loop on the next iteration.
                        childFrame = null;
                    }
                    continue;
                }
                break;
            }
            return bMoreToWalk;
        }

        // This method handles internal frames but no child frames.
        private bool MoveNextWorker()
        {
            if (m_bEndOfStackFrame)
            {
                return false;
            }

            bool fIsLeafFrame = false;
            bool fIsFuncEvalFrame = false;

            int hr;
            if (m_init)
            {
                // this the 2nd or more call to MoveNext() and MoveNextWorker()

                if ((m_frame != null) && (m_frame.FrameType == CorFrameType.InternalFrame))
                {
                    // We have just handed out an internal frame, so we need to start looking at the next
                    // internal frame AND we don't call m_sw.Next() to preserve the previous managed frame
                    if (m_internalFrameIndex < m_internalFrames.Length)
                    {
                        m_internalFrameIndex++;
                    }

                    fIsFuncEvalFrame = (m_frame.InternalFrameType == CorDebugInternalFrameType.STUBFRAME_FUNC_EVAL);
                    
                    hr = (int)HResult.S_OK;
                }
                else
                {
                    // We just handed out a managed or native frame.
                    // In any case, use the managed unwinder to unwind.
                    hr = m_sw.Next();

                    // Check for end-of-stack condition.
                    if (hr == (int)HResult.CORDBG_S_AT_END_OF_STACK)
                    {
                        m_bEndOfStackFrame = true;
                    }
                }
            }
            else
            {
                // this is the initial call to MoveNext() and MoveNextWorker() that validates the enumeration
                // after we return from MoveNext(), .Current will point to the leaf-most frame
                m_init = true;
                fIsLeafFrame = true;
                hr = (int)HResult.S_OK;
            }

            // Check for errors.
            if (HRUtils.IsFailingHR(hr))
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            if (!m_bEndOfStackFrame)
            {
                // Now we need to do a comparison between the current stack frame and the internal frame (if any)
                // to figure out which one to give back first.
                ICorDebugFrame frame;
                m_sw.GetFrame(out frame);

                if (frame == null)
                {
                    // this represents native frame(s) to managed code, we return true because there may be more
                    // managed frames beneath
                    m_frame = null;
                    return true;
                }

                // we compare the current managed frame with the internal frame
                CorFrame currentFrame = new CorFrame(frame);
                for (; m_internalFrameIndex < m_internalFrames.Length; m_internalFrameIndex++)
                {
                    CorFrame internalFrame = m_internalFrames[m_internalFrameIndex];

                    // Check for internal frame types which are not exposed in V2.
                    if (IsV3InternalFrameType(internalFrame))
                    {
                        continue;
                    }
                    if (internalFrame.IsCloserToLeaf(currentFrame))
                    {
                        currentFrame = internalFrame;
                    }
                    else if (internalFrame.InternalFrameType == CorDebugInternalFrameType.STUBFRAME_M2U)
                    {
                        // we need to look at the caller stack frame's SP

                        INativeContext ctx = this.GetContext();
                        CorStackWalk tStack = m_th.CreateStackWalk(CorStackWalkType.PureV3StackWalk);
                        CorDebugSetContextFlag flag = ((fIsFuncEvalFrame || fIsLeafFrame) ? 
                            CorDebugSetContextFlag.SET_CONTEXT_FLAG_ACTIVE_FRAME :
                            CorDebugSetContextFlag.SET_CONTEXT_FLAG_UNWIND_FRAME);
                        tStack.SetContext(flag, ctx);

                        //tStack now points to the "previous" managed frame, not the managed frame we're looking at
                        //the first MoveNext call moves the temporary stackwalker to the "current" frame and the next
                        //MoveNext call moves the temporary stackwalker to the "caller" frame
                        Int64 current = 0, caller = 0;
                        if (tStack.MoveNext())
                        {
                            if (tStack.Current != null)
                            {
                                current = tStack.GetContext().StackPointer.ToInt64();
                            }
                        }
                        if (tStack.MoveNext())
                        {
                            if (tStack.Current != null)
                            {
                                caller = tStack.GetContext().StackPointer.ToInt64();
                            }
                        }
                        if (current == 0 || caller == 0)
                        {
                            //we've hit a native frame somewhere, we shouldn't be doing anything with this
                            break;
                        }
                        if (current < caller && internalFrame.IsCloserToLeaf(tStack.Current))
                        {
                            // if there is no caller frame or the current managed frame is closer to the leaf frame
                            // than the next managed frame on the stack (the caller frame), then we must be in the case
                            // where:
                            //          [IL frame without metadata]
                            //          [Internal frame, 'M --> U']
                            //          Caller frame (managed)
                            // We need to flip the internal and IL frames, so we hand back the internal frame first
                            currentFrame = internalFrame;
                        }
                    }
                    
                    break;
                }

                m_frame = currentFrame;
                return true;


               
            }
            else
            {
                // We have reached the end of the managed stack.
                // Check to see if we have any internal frames left.

                for (; m_internalFrameIndex < m_internalFrames.Length; m_internalFrameIndex++)
                {
                    CorFrame internalFrame = m_internalFrames[m_internalFrameIndex];
                    if (IsV3InternalFrameType(internalFrame))
                    {
                        continue;
                    }
                    m_frame = internalFrame;
                    return true;
                }

                return false;
            }
        }

        public override void Reset()
        {
            // There is no way to reset.  Just create a new one.
            CorStackWalk s = m_th.CreateStackWalk(CorStackWalkType.ExtendedV3StackWalk);
            m_sw = s.GetInterface();

            m_frame = null;
        }

        private static bool IsV3InternalFrameType(CorFrame internalFrame)
        {
            // CorStackWalkEx wants to expose V2 behaviour.  The following frame types are new in V3.
            // This function checks whether the specified internal frame is a V3 frame type and returns
            // true if it is.  CorStackWalkEx uses this function to decide whether it should expose
            // an internal frame.
            CorDebugInternalFrameType type = internalFrame.InternalFrameType;
            if ((type == CorDebugInternalFrameType.STUBFRAME_INTERNALCALL) ||
                (type == CorDebugInternalFrameType.STUBFRAME_CLASS_INIT) ||
                (type == CorDebugInternalFrameType.STUBFRAME_EXCEPTION) ||
                (type == CorDebugInternalFrameType.STUBFRAME_SECURITY) ||
                (type == CorDebugInternalFrameType.STUBFRAME_JIT_COMPILATION))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private CorFrame[] m_internalFrames;
        private uint m_internalFrameIndex;
        private bool m_bEndOfStackFrame;
    } /* class StackWalk Thread */

} /* namespace */
