//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorMetadata;
using Microsoft.Samples.Debugging.Native;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;


namespace Microsoft.Samples.Debugging.MdbgEngine
{
    // Abstract interfaces that allow plugable implementation of stack-walking into mdbg.

    /// <summary>
    /// Interface used for creating a new stackwalkers after a process is synchronized.
    /// </summary>
    public interface IMDbgFrameFactory
    {
        /// <summary>
        /// Creates a new StackWalker as an enumeration of frames on a given thread.
        /// </summary>
        /// <param name="thread">a thread object associated with the stackwalker</param>
        /// <returns>enumeration of Frames</returns>
        IEnumerable<MDbgFrame> EnumerateFrames(MDbgThread thread);
    }

    /// <summary>
    /// A utility class to cache stack frames. 
    /// This provides an array view (random access) on top of the enumerator (sequential) view.
    /// </summary>
    /// <remarks>Thread safety - this class supports free threaded access
    /// and will synchronize all access to the underlying enumerator</remarks>
    public class FrameCache
    {
        /// <summary>
        /// Creates a frame cache for the given thread
        /// </summary>
        /// <param name="frames">Enumeration of frames</param>
        /// <param name="thread">Thread associated the the stack walker</param>
        public FrameCache(IEnumerable<MDbgFrame> frames, MDbgThread thread)
        {
            m_frameEnum = frames.GetEnumerator();
            m_thread = thread;
        }

        /// <summary>
        /// Enumerate the frames in the cache, and builds the cache if neeed.
        /// </summary>
        /// <returns>An enumeration of cached frames</returns>
        /// <remarks>This is useful when it's important to return the same instance of MdbgFrame objects.</remarks>
        public IEnumerable<MDbgFrame> EnumerateCachedFrames()
        {
            lock (m_cacheLock)
            {
                CheckUsability();

                MDbgFrame frame;
                int idx = 0;
                do
                {
                    // Defer to GetFrame since that's the one place to grow the cache.
                    frame = GetFrame(idx);
                    idx++; ;
                    if (frame != null)  yield return frame;

                } while (frame != null);
            }
        }

        /// <summary>
        /// Function returns mdbg frame for the thred it was created for. 
        /// </summary>
        /// <param name="index">index of the frame. The leaf-most frame is indexed with 0.</param>
        /// <returns>the object representing frame</returns>
        /// <remarks>
        /// When the stack has 10 frames, it returns the frame for indexes 0-9. It returns null
        /// for index 10 and throws otherwise.
        /// </remarks>
        public MDbgFrame GetFrame(int index)
        {
            lock (m_cacheLock)
            {
                CheckUsability();

                // Need to check for out-of-range 
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException("index");
                }

                // Grow the cache until it is large enough to contain the index.
                // This should be the only place that calls IterateNextFrame().
                while (index >= InternalFrameCache.Count)
                {
                    MDbgFrame frame = IterateNextFrame();

                    InternalFrameCache.Add(frame);

                    // Check if the index is asking for more frames than the callstack has.
                    if (frame == null)
                    {
                        if (index > InternalFrameCache.Count)
                        {
                            throw new ArgumentOutOfRangeException("index", index, "Callstack only has " + InternalFrameCache.Count + " frame(s).");
                        }
                    }
                    else
                    {
                        // Ensure that that the same frame is not added multiple times. 
                        // This can cause infinite loops in the stackwalker because each frame 
                        // must have a unique index.
                        Debug.Assert(GetFrameIndex(frame) == index, "FrameFactory " + m_frameEnum.ToString() + " added same frame (" + frame.ToString() + ") multiple times.");
                    }
                }
                return InternalFrameCache[index];
            }
        }

        /// <summary>
        /// The function returns the index of the frame in the stack.
        /// </summary>
        /// <param name="frame">A frame returned with call to GetFrame.</param>
        /// <returns>an index of the frame</returns>
        /// <remarks>
        /// If the frame passed in was not created with this StackWalker object the function
        /// throws an exception.
        /// </remarks>
        public int GetFrameIndex(MDbgFrame frame)
        {
            lock (m_leafLock)
            {
                CheckUsability();

                // We do not need to grow frame cache. If it's not already in the cache, it can't be valid.
                if (m_frameCache != null)
                {
                    for (int i = 0; i < m_frameCache.Count; ++i)
                    {
                        if (m_frameCache[i] == frame)
                        {
                            return i;
                        }
                    }
                }

                throw new ArgumentException("Invalid frame");
            }
        }


        /// <summary>
        /// Wrapper to access enumerator and get next frame.
        /// </summary>
        /// <returns>Next frame in the enumeration. Null to end the enumeration</returns>
        /// <remarks>The enumeration starts with the leaf (active) of the callstack and then returns 
        /// callers working its way up to main(). This will not be called after it returns null.</remarks>
        MDbgFrame IterateNextFrame()
        {
            if (!m_frameEnum.MoveNext())
            {
                return null;
            }
            return m_frameEnum.Current;
        }
        IEnumerator<MDbgFrame> m_frameEnum;

        /// <summary>
        /// Thread associated with the stack-walker
        /// </summary>
        protected MDbgThread Thread
        {
            get
            {
                return m_thread;
            }
        }

        /// <summary>
        /// cache of frames created by the stack walker
        /// </summary>
        protected IList<MDbgFrame> InternalFrameCache
        {
            get
            {
                lock (m_leafLock)
                {
                    if (m_frameCache == null)
                    {
                        m_frameCache = new List<MDbgFrame>();
                    }
                    return m_frameCache;
                }
            }
        }

        private MDbgThread m_thread;
        private List<MDbgFrame> m_frameCache;
        private bool m_disposed;
        // Synchronizes multiple threads from growing the cache simultaneously. This lock can be
        // held while calling to the enumerator.
        private object m_cacheLock = new object();
        // A leaf lock used to protect mutable data members m_disposed and m_frameCache.
        // This lock is not allowed to be held when we call out to the enumerator
        private object m_leafLock = new object();


        /// <summary>
        /// The function throws if the FrameFactory invalidated the Stackwalker.
        /// </summary>
        protected void CheckUsability()
        {
            lock (m_leafLock)
            {
                if (m_disposed)
                {
                    throw new InvalidOperationException("Reading old stack frames");
                }
            }
        }


        /// <summary>
        /// Mark that the cache is invalidated. Caller should discard this object and create a new instance.
        /// Marking invalid prevents clients from using stale entries.
        /// </summary>
        /// <remarks>This is not IDisposable because it does not hold unmanaged resources.</remarks>
        public void Invalidate()
        {
            lock (m_leafLock)
            {
                m_disposed = true;
                m_frameCache = null;
            }
        }



    }

    // V2 implementation of plugable stack-walking API

    /// <summary>
    /// Implementation of FrameFactory that creates a StackWalker that uses debugger V2 StackWalking API 
    /// </summary>
    public class MDbgV2FrameFactory : IMDbgFrameFactory
    {

        /// <summary>
        /// Creates a new V2 StackWalker.
        /// </summary>
        /// <param name="thread">a thread object associated with the stackwalker</param>
        /// <returns>object implementing MDbgStackWalker interface</returns>
        public IEnumerable<MDbgFrame> EnumerateFrames(MDbgThread thread)
        {
            // To do stackwalking using V2 ICorDebug, we enumerate through the chains, 
            // and then enumerate each frame in every chain

            CorChain chain = null;
            try
            {
                chain = thread.CorThread.ActiveChain;
            }
            catch (System.Runtime.InteropServices.COMException ce)
            {
                // Sometimes we cannot get the callstack.  For example, the thread
                // may not be scheduled yet (CORDBG_E_THREAD_NOT_SCHEDULED),
                // or the debuggee may be corrupt (CORDBG_E_BAD_THREAD_STATE).
                // In either case, we'll ignore the problem and return an empty callstack.
                Debug.Assert(ce.ErrorCode == (int)HResult.CORDBG_E_BAD_THREAD_STATE ||
                    ce.ErrorCode == (int)HResult.CORDBG_E_THREAD_NOT_SCHEDULED);
            }

            while (chain != null)
            {
                if (chain.IsManaged)
                {
                    // Enumerate managed frames
                    // A chain may have 0 managed frames.
                    CorFrame f = chain.ActiveFrame;
                    while (f != null)
                    {
                        MDbgFrame frame = new MDbgILFrame(thread, f);
                        f = f.Caller;
                        yield return frame;
                    }
                }
                else
                {
                    // ICorDebug doesn't unwind unmanaged frames. Need to let a native-debug component handle that.
                    foreach (MDbgFrame frame in UnwindNativeFrames(thread, chain))
                    {
                        yield return frame;
                    }
                }

                // Move to next chain
                chain = chain.Caller;
            }

        }

        /// <summary>
        /// Unwind unmanaged frames within an native chain.
        /// </summary>
        /// <param name="thread">thread containing chain</param>
        /// <param name="nativeChain">a native CorChain</param>
        /// <returns>enumeration of MDbgFrames for the native frames</returns>
        /// <remarks>ICorDebug stackwalking only unwinds managed chains. 
        /// A derived class can override this function to provide native stacktracing.</remarks>
        protected virtual IEnumerable<MDbgFrame> UnwindNativeFrames(MDbgThread thread, CorChain nativeChain)
        {
            Debug.Assert(!nativeChain.IsManaged);

            // Base class can't unwind unmanaged chains. 
            // A derived class can override and provide native stack unwinding.
            // Use:
            // 1) chain.RegisterSet to get the starting context.
            // 2) chain.GetStackRange(out start, out end);   to get the stackrange to unwind to
            // 3) a native unwinder (such as DbgHelp.dll) to actually do the native stack unwinding.
            yield break;
        }
    } // end class MDbgV2StackWalker

    

    // V3 implementation of plugable stack-walking API

    /// <summary>
    /// Implementation of FrameFactory that creates a StackWalker that uses debugger V3 StackWalking API 
    /// </summary>
    public class MDbgV3FrameFactory : IMDbgFrameFactory 
    {
        public IEnumerable<MDbgFrame> EnumerateFrames(MDbgThread thread)
        {
            MDbgFrame frameToReturn = null;
            long pEndVal = Int64.MinValue;
            CorStackWalk m_v3StackWalk = thread.CorThread.CreateStackWalk(CorStackWalkType.ExtendedV3StackWalk);
            INativeContext ctxUnmanagedChain = null, currentCtx = null;
            IEnumerable<MDbgFrame> nFrames;
            List<CorFrame> iFrameCache = new List<CorFrame>(); //storage cache for internal frames

            // we need to call MoveNext() to make the enumerator valid
            while ((m_v3StackWalk != null) && (m_v3StackWalk.MoveNext()))
            {
                CorFrame frame = m_v3StackWalk.Current;

                if (frame != null)
                {
                    // If we get a RuntimeUnwindableFrame, then the stackwalker is also stopped at a native
                    // stack frame, but it's a native stack frame which requires special unwinding help from
                    // the runtime. When a debugger gets a RuntimeUnwindableFrame, it should use the runtime
                    // to unwind, but it has to do inspection on its own. It can call
                    // ICorDebugStackWalk::GetContext() to retrieve the context of the native stack frame.
                    if (frame.FrameType != CorFrameType.RuntimeUnwindableFrame)
                    {

                        // check for an internal frame...if the internal frame happens to come after the last
                        // managed frame, any call to GetContext() will assert
                        if (frame.FrameType != CorFrameType.InternalFrame)
                        {
                            // we need to store the CONTEXT when we're at a managed frame, if there's an internal frame
                            // after this, then we'll need this CONTEXT
                            currentCtx = m_v3StackWalk.GetContext();
                        }
                        else if ((frame.FrameType == CorFrameType.InternalFrame) && (ctxUnmanagedChain != null))
                        {
                            // we need to check to see if this internal frame could have been sandwiched between
                            // native frames, this will be the case if ctxUnmanagedChain is not null

                            // we need to store ALL internal frames until we hit the next managed frame
                            iFrameCache.Add(frame);
                            continue;
                        }
                        // else we'll use the 'stored' currentCtx if we're at an InternalFrame

                        pEndVal = Int64.MaxValue;
                        if (currentCtx != null)
                        {
                            pEndVal = currentCtx.StackPointer.ToInt64();
                        }

                        //check to see if we have native frames to unwind
                        nFrames = UnwindNativeFrames(thread, ctxUnmanagedChain, pEndVal);

                        foreach (MDbgFrame fr in StitchInternalFrames(thread,iFrameCache.GetEnumerator(),nFrames))
                        {
                            yield return fr;
                        }

                        //clear out the CONTEXT and native frame cache
                        ctxUnmanagedChain = null;
                        nFrames = null;
                        iFrameCache.Clear();

                        //return the managed frame
                        frameToReturn = thread.LookupFrame(frame);
                        yield return frameToReturn;
                    }
                    else
                    {
                        continue;
                    }

                }
                // If we don't get a frame, then the stackwalker is stopped at a native stack frame.
                else
                {
                    // we've hit a native frame, we need to store the CONTEXT
                    ctxUnmanagedChain = m_v3StackWalk.GetContext();

                    //we need to invalidate the currentCtx since it won't be valid on the next loop iteration
                    currentCtx = null;
                }

            } //end while

            // we may have native frames at the end of the stack
            nFrames = UnwindNativeFrames(thread, ctxUnmanagedChain, Int64.MaxValue);

           
            foreach (MDbgFrame frame in StitchInternalFrames(thread, iFrameCache.GetEnumerator(), nFrames))
            {
                yield return frame;
            }

            nFrames = null;
            ctxUnmanagedChain = null;

            // End of stackwalk. Return null as sentinel.
            yield return null;
        }

        /// <summary>
        /// We want to be able to interleave native and internal frames together and also minimize the exposure of internal frames.
        /// This method takes the internal frame cache and interleaves them with the native frames returned from UnwindNativeFrames.  In this
        /// case, this function will merely return all of the internal frames since the interop extension isn't loaded.
        /// <param name="thread">current thread that we're creating the stackwalker on</param>
        /// <param name="internalFrameCache">the internal frame cache</param>
        /// <param name="nativeFrames">native frames returned from UnwindNativeFrames</param>
        /// <returns>enumeration of the interleaved internal and native MDbgFrames</returns>
        /// </summary>
        protected virtual IEnumerable<MDbgFrame> StitchInternalFrames(MDbgThread thread, IEnumerator<CorFrame> internalFrameCache, IEnumerable<MDbgFrame> nativeFrames)
        {
            if (internalFrameCache == null)
            {
                yield break;
            }

            MDbgFrame mf = null;

            // iterate through the internal frames
            internalFrameCache.Reset();
            while (internalFrameCache.MoveNext())
            {
                mf = thread.LookupFrame(internalFrameCache.Current);
                yield return mf;
            }
        }
        
        /// <summary>
        /// Unwind unmanaged frames within an native chain.
        /// </summary>
        /// <param name="thread">thread containing an unmanged frame</param>
        /// <param name="context">context containing the current context of the stack frame</param>
        /// <param name="endValue">address of the next managed frame</param>
        /// <returns>enumeration of MDbgFrames for the native frames</returns>
        /// <remarks>ICorDebug stackwalking only unwinds managed chains. 
        /// A derived class can override this function to provide native stacktracing.</remarks>
        protected virtual IEnumerable<MDbgFrame> UnwindNativeFrames(MDbgThread thread, INativeContext context, long endValue)
        {
            // Base class can't unwind unmanaged chains. 
            // A derived class can override and provide native stack unwinding.
            // Use:
            // 1) chain.RegisterSet to get the starting context.
            // 2) chain.GetStackRange(out start, out end);   to get the stackrange to unwind to
            // 3) a native unwinder (such as DbgHelp.dll) to actually do the native stack unwinding.

            yield break;
        }
    }

    /// <summary>
    /// This FrameFactory creates either a V2 or a V3 StackWalker depending on whether the specified thread
    /// supports the ICorDebugThread3 API.
    /// </summary>
    public class MDbgLatestFrameFactory : IMDbgFrameFactory 
    {
        public IEnumerable<MDbgFrame> EnumerateFrames(MDbgThread thread)
        {
            IMDbgFrameFactory factory;
            if (thread.CorThread.IsV3)
            {
                factory = new MDbgV3FrameFactory();
                return factory.EnumerateFrames(thread);
            }
            else
            {
                factory = new MDbgV2FrameFactory();
                return factory.EnumerateFrames(thread);
            }
        }
    }
}
