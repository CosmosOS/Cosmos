//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Collections;

using Microsoft.Samples.Debugging.CorDebug.NativeApi;

namespace Microsoft.Samples.Debugging.CorDebug
{
    /**
     * Represents a scope at which program execution can be controlled.
     */
    public class CorController : WrapperBase
    {
        internal CorController (ICorDebugController controller)
            :base(controller)
        {
            m_controller = controller;
        }

        /**
         * Cooperative stop on all threads running managed code in the process.
         */
        public virtual void Stop (int timeout)
        {
            m_controller.Stop ((uint)timeout);
        }

        /**
         * Continue processes after a call to Stop.
         *
         * outOfBand is true if continuing from an unmanaged event that
         * was sent with the outOfBand flag in the unmanaged callback;
         * false if continueing from a managed event or normal unmanaged event.
         */
        public virtual void Continue (bool outOfBand)
        {
            m_controller.Continue (outOfBand ? 1 : 0);
        }

        /**
         * Are the threads in the process running freely?
         */
        public bool IsRunning ()
        {
            int running = 0;
            m_controller.IsRunning (out running);
            return !(running == 0);
        }

        /**
         * Are there managed callbacks queued up for the requested thread?
         */
        public bool HasQueuedCallbacks (CorThread managedThread)
        {
            int queued = 0;
            m_controller.HasQueuedCallbacks( (managedThread==null)?null:managedThread.GetInterface(),
                                             out queued
                                             );
            return !(queued == 0);
        }

        /** Enumerate over all threads in active in the process. */
        public IEnumerable Threads
        {
            get 
            {
                ICorDebugThreadEnum ethreads = null;
                m_controller.EnumerateThreads (out ethreads);
                return new CorThreadEnumerator (ethreads);
            }
        }

        /**
         * Set the current debug state of each thread.
         */
        [CLSCompliant(false)]
        public void SetAllThreadsDebugState (CorDebugThreadState state, CorThread exceptThis)
        {
            m_controller.SetAllThreadsDebugState (state, exceptThis != null ? exceptThis.GetInterface() : null);
        }

        /** Detach the debugger from the process/appdomain. */
        public void Detach ()
        {
            m_controller.Detach ();
        }
    
        /** Terminate the current process. */
        public void Terminate (int exitCode)
        {
            m_controller.Terminate ((uint)exitCode);
        }

        /* Can the delta PEs be applied to the running process? */
        /*
        public IEnumerable CanCommitChanges (uint number, EditAndContinueSnapshot[] snapshots)
        {
            ICorDebugErrorInfoEnum error = null;
            m_controller.CanCommitChanges (number, snapshots, out error);
            if (error == null)
                return null;
            return new ErrorInfoEnumerator (error);
        }
        */

        /* Apply the delta PEs to the running process. */
        /*
        public IEnumerable CommitChanges (uint number, EditAndContinueSnapshot[] snapshots)
        {
            ICorDebugErrorInfoEnum error = null;
            m_controller.CommitChanges (number, snapshots, out error);
            if (error == null)
                return null;
            return new ErrorInfoEnumerator (error);
        }
        */
        [CLSCompliant(false)]
        protected ICorDebugController GetController ()
        {
            return m_controller;
        }
        
        private ICorDebugController m_controller;
    }
}
