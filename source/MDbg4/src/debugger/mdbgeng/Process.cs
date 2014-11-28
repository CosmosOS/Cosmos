//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------

using System;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Globalization;

using Microsoft.Win32.SafeHandles;

using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;
using Microsoft.Samples.Debugging.CorMetadata;

using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using Microsoft.Samples.Debugging.Native;
using Microsoft.Samples.Debugging.CorDebug.Utility;

namespace Microsoft.Samples.Debugging.MdbgEngine
{
    /// <summary>
    /// ProcessCollectionChangedEventArgs class.
    /// </summary>
    public class ProcessCollectionChangedEventArgs : EventArgs
    {
        internal ProcessCollectionChangedEventArgs(MDbgProcess p)
        {
            m_process = p;
        }

        /// <value>
        ///     Process that has been newly created.
        /// </value>
        public MDbgProcess Process
        {
            get { return m_process; }
        }

        private MDbgProcess m_process;
    }


    /// <summary>
    /// Delegate for notification of engine about starting new processes.
    /// </summary>
    /// <param name="sender">Object that sent the event.</param>
    /// <param name="e">ProcessCollectionChangedEventArgs for the event.</param>
    public delegate void ProcessCollectionChangedEventHandler(Object sender, ProcessCollectionChangedEventArgs e);

    /// <summary>
    /// MDbg Process Collection class for grouping Processes.
    /// </summary>
    public sealed class MDbgProcessCollection : MarshalByRefObject, IEnumerable
    {
        internal MDbgProcessCollection(MDbgEngine engine)
        {
            m_engine = engine;
        }

        /// <summary> Event fired when a process is added to this collection 
        /// and has an underlying valid CorProcess object. </summary>
        /// <remarks> Extensions can subscribe to this so that they have a chance to 
        /// inspect a process when it's first created and subscribe to more specific state.
        /// There is no ProcessRemoved because extensions can use the Proecss Exited callback.
        ///</remarks>
        public event ProcessCollectionChangedEventHandler ProcessAdded;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_items.GetEnumerator();
        }

        /// <summary>
        /// Lookup an MDbgProcss from a CorProcess.
        /// </summary>
        /// <param name="process">The CorProcess.</param>
        /// <returns>The MDbgProcess.</returns>
        public MDbgProcess Lookup(CorProcess process)
        {
            foreach (MDbgProcess p in m_items)
            {
                if (p.CorProcess == process)
                {
                    return p;
                }
            }
            return null;
        }

        /// <summary>
        /// How many Processes are in the Collection.
        /// </summary>
        /// <value>How many Processes.</value>
        public int Count
        {
            get
            {
                return m_items.Count;
            }
        }

        /// <summary>
        /// Gets or Sets which MDbgProcess is active.
        /// </summary>
        /// <value>The Active Process.</value>
        public MDbgProcess Active
        {
            get
            {
                if (m_active == null)
                    throw new MDbgNoActiveInstanceException("No active process");

                return m_active;
            }
            set
            {
                Debug.Assert(value != null);
                Debug.Assert(m_items.Contains(value));
                if (value == null || !m_items.Contains(value))
                {
                    throw new ArgumentException();
                }
                m_active = value;
            }
        }

        /// <summary>
        /// Gets if there is an Active process in the Collection.
        /// </summary>
        /// <value>true if it has an Active Process, else false.</value>
        public bool HaveActive
        {
            get
            {
                return m_active != null;
            }
        }

        /// <summary>
        /// OBSOLETE - Use CreateLocalProcess(CorDebugger)
        /// Creates a Local Process and adds it to the Collection.
        /// </summary>
        /// <remarks>
        /// This method was obsoleted because it accepted null version strings and 
        /// then used a very arbitrary versioning policy. Versioning policy should be
        /// decided by the caller.
        /// </remarks>
        /// <returns>The Process that got created.</returns>
        [Obsolete("Use CreateLocalProcess(CorDebugger)")]
        public MDbgProcess CreateLocalProcess()
        {
            return CreateLocalProcess((string)null);
        }


        /// <summary>
        /// OBSOLETE - Use CreateLocalProcess(CorDebugger)
        /// Creates a local process object that will be able to debug specified program.
        /// </summary>
        /// <remarks>The created process object will be empty -- you still have to call
        /// CreateProcess method on it to start debugging
        /// This method was obsoleted because it accepted null version strings and
        /// then used a very arbitrary versioning policy. Versioning policy should be
        /// decided by the caller.
        /// </remarks>
        /// <param name="version">version of CLR to use for the process</param>
        /// <returns>The Process that got created.</returns>
        [Obsolete("Use CreateLocalProcess(CorDebugger)")]
        public MDbgProcess CreateLocalProcess(string version)
        {
            if (version == null)
            {
                return CreateLocalProcess(new CorDebugger(CorDebugger.GetDefaultDebuggerVersion()));
            }
            else
            {
                return CreateLocalProcess(new CorDebugger(version));
            }
        }

        /// <summary>
        /// Creates a new process which can be debugged using the given ICorDebug instance
        /// </summary>
        /// <param name="debugger">CorDebugger to use for the process</param>
        /// <returns>The Process that got created.</returns>
        public MDbgProcess CreateLocalProcess(CorDebugger debugger)
        {
            Debug.Assert(debugger != null);

            // This is called on the Main thread so it's safe to flush
            FreeStaleUnmanagedResources();
            RegisterDebuggerForCleanup(debugger);
            return new MDbgProcess(m_engine, debugger);
        }

        /// <summary>
        /// Creates a process which can use the new native pipeline
        /// debugging architecture
        /// </summary>
        /// <returns>The Process that got created.</returns>
        public MDbgProcess CreateProcess()
        {
            return new MDbgProcess(m_engine);
        }

        /// <summary>
        /// Adds the specified CorDebugger object to the cleanup list
        /// </summary>
        /// <param name="debugger"></param>
        public void RegisterDebuggerForCleanup(CorDebugger debugger)
        {
            if (null == debugger)
                throw new ArgumentNullException();
            lock (m_CleanupList)
            {
                m_CleanupList.Add(debugger, false);
            }
        }

        /// <summary>
        /// Explicitly free any stale unmanaged resources associated with this process collection.
        /// This mainly means calling ICorDebug::Terminate on all orphaned CorDebugger objects. </summary>
        /// <remarks>
        /// ICorDebug::Terminate can not safely be called on the finalizer thread or debugger callback thread.
        /// Thus we maintain a queue of objects to delete, and then flush that queue once we know we can safely 
        /// delete them.
        /// </remarks>
        public void FreeStaleUnmanagedResources()
        {
            lock (m_CleanupList)
            {

                Dictionary<CorDebugger, bool>.Enumerator myEnumerator = m_CleanupList.GetEnumerator();
                while (myEnumerator.MoveNext())
                {
                    if (myEnumerator.Current.Value == true)
                    {
                        CorDebugger d = myEnumerator.Current.Key;
                        d.Terminate();
                        m_CleanupList.Remove(d);

                        // Since we can't use the enumerator once we change the collection, we need to reset it.
                        // This makes the algorithm potentially n^2, but since n is the number of processes, we expect 
                        // it to be very small (usually 1).
                        myEnumerator = m_CleanupList.GetEnumerator();
                    }
                }
            }
        }

        // (key, value)
        // Key is the CorDebugger object that we'll eventually call ICorDebug::Terminate on.
        // value is the state of the CorDebugger object:
        //  false - that means object is live
        //  true  - means that object is now dead and can be cleaned up.
        Dictionary<CorDebugger, bool> m_CleanupList = new Dictionary<CorDebugger, bool>();

        /// <summary>
        /// Called by MDbgProcess constructor to register a process into process collection
        /// </summary>
        /// <param name="process">process to register</param>
        /// <returns>Logical process number that should be assigned to the registered process.</returns>
        /// <remarks>This will update the Active process to the current process.</remarks>
        internal int RegisterProcess(MDbgProcess process)
        {
            lock (m_items)
            {
                m_items.Add(process);
                m_active = process;

                // We don't fire the ProcessAdded event yet because we don't yet have a valid CorProcess object.
                // We'll fire the event when we initialize the callbacks.
                return m_freeProcessNumber++;
            }
        }

        // Fired once process has an underlying CorProcess object.
        internal void OnProcessResolved(MDbgProcess p)
        {
            if (ProcessAdded != null)
            {
                Debug.Assert(p != null);
                Debug.Assert(p.CorProcess != null);
                ProcessAdded(this, new ProcessCollectionChangedEventArgs(p));
            }
        }

        /// <summary>
        /// Removes process from the process collection.
        /// </summary>
        /// <param name="process">process to unregister from collection</param>
        /// <remarks>
        /// If the process was active, the active process is set to none
        /// </remarks>
        internal void DeleteProcess(MDbgProcess process)
        {
            lock (m_items)
            {
                Debug.Assert(m_items.Contains(process));

                m_items.Remove(process);

                // If we remove the active process, then pick a new active process.
                if (m_active == process)
                {
                    m_active = null;

                    // Now pick active process. To be deterministic, we'll pick
                    // the process with the lowest logical number.
                    // If no other processes, then we leave m_active as null.
                    int minId = int.MaxValue;

                    foreach (MDbgProcess p in m_items)
                    {
                        if (p.Number < minId)
                        {
                            minId = p.Number;
                            m_active = p;
                        }
                    }
                }
            }

            lock (m_CleanupList)
            {
                // We may need to call IcorDebug::Terminate. However, we can't call that on a callback thread,
                // but this function may be on a callback thread. So queue the call and we can call it later.
                CorDebugger d = process.CorDebugger;
                if (d != null)
                {
                    if (m_CleanupList.ContainsKey(d))
                    {
                        m_CleanupList[d] = true;
                    }
                }
            }
        }

        private MDbgEngine m_engine;
        private List<MDbgProcess> m_items = new List<MDbgProcess>();
        private MDbgProcess m_active;
        private int m_freeProcessNumber;
    }

    /// <summary>
    /// Debug Mode Flags.
    /// </summary>
    public enum DebugModeFlag
    {
        /// <summary>
        /// Run in the same mode as without debugger.
        /// </summary>
        Default,
        /// <summary>
        /// Run in forced optimized mode.
        /// </summary>
        Optimized,
        /// <summary>
        /// Run in debug mode (easy inspection) but slower.
        /// </summary>
        Debug,
        /// <summary>
        /// Run in ENC mode (ENC possible) but even slower than debug 
        /// </summary>
        Enc
    }


    /// <summary>
    /// Stop-Go controller abstracts out details of how a debuggee is stopped and resumed for
    /// debug events. This allows for abstractions like non-invasive debugging. 
    /// </summary>
    public interface IStopGoController
    {
        // Possible implementations could include:
        // -  V2: These would delegate to calls on ICDProcess.
        // -  Dump-debugging: Stop/Go/Kill would fail.

        /// <summary>
        /// Whether the taget can execute (go/stop work). Returns false if target
        /// is non-invasive, otherwise true.  This is an immutable property of
        /// the controller. 
        /// </summary>
        /// <remarks>This can be used as an early check before other invasive commands
        /// like stepping, funceval, go, break. Technically, this isn't needed
        /// since other invasive operations would just throw; but this gives
        /// us a chance to provide more meaningful errors to the user.</remarks>
        bool CanExecute
        {
            get;
        }

        /// <summary>
        /// Returns true iff the debuggee is running. This is after Continue()
        /// but before the process is stopped. 
        /// </summary>
        bool IsRunning
        {
            get;
        }

        /// <summary>
        /// Called when debuggee is stopped. This may also be called after
        /// the debuggee is exited (which can be viewed as a kind
        /// of "stopped" state).
        /// </summary>
        void MarkAsStopped();

        /// <summary>
        /// Do an asynchronous break of the debuggee.
        /// Usually called when debuggee is running. 
        /// This blocks and does not return until the debuggee is in a stopped
        /// state.
        /// </summary>
        /// <remarks>AsyncBreak can be called on non-invasive targets (which are always
        /// conceptually stopped). In this case, AsyncBreak may be called even though CanExecute
        /// returns false.</remarks>
        void AsyncBreak();

        /// <summary>
        /// Called when debuggee is stopped and we want it to resume executing.
        /// </summary>
        void Continue();

        /// <summary>
        /// Terminate the debuggee with the specified exit code.
        /// Called when debuggee is stopped. 
        /// </summary>
        /// <param name="exitCode">exit code that debuggee will terminate
        /// with.</param>
        void Kill(int exitCode);

        /// <summary>
        /// Stop debugging the debuggee; but don't terminate it.
        /// Called when debuggee is stopped.
        /// </summary>
        void Detach();
    }

    /// <summary>
    /// Stop Go controller which provides non-invasive execution control
    /// </summary>
    public class NoninvasiveStopGoController : IStopGoController
    {
        /// <summary>
        /// Constuctor
        /// </summary>
        public NoninvasiveStopGoController()
        {            
        }

        /// <summary>
        /// Connect this controller to the MDbgProcess object that it controls.
        /// </summary>
        /// <param name="process">process being controlled by this execution policy</param>
        public void Init(MDbgProcess process)
        {
            m_process = process;
        }
        
        MDbgProcess m_process;

    #region IStopGoController Members
        /// <summary>
        /// Always false for a non-invasive target
        /// implementation of IStopGoController.CanExecute.
        /// </summary>
        public bool CanExecute
        {
            get { return false; }
        }

        /// <summary>
        /// Always false for a non-invasive target because the target is always conceptually in break-mode.
        /// </summary>
        public bool IsRunning
        {
            // Non-invasive target is never running.
            get { return false; }
        }

        /// <summary>
        /// implementation of IStopGoController.MarkAsStopped
        /// </summary>
        public void MarkAsStopped()
        {
            // Non-invasive targets are always stopped.
        }

        /// <summary>
        /// implementation of IStopGoController.AsyncBreak.
        /// This is a nop for non-invasive targets.
        /// </summary>
        public void AsyncBreak()
        {
            // For a non-invasive target, were always considered stopped. Just succeed here.
        }

        /// <summary>
        /// implementation of IStopGoController.Continue.
        /// This fails for non-invasive targets.
        /// </summary>
        public void Continue()
        {
            ThrowInvalid();
        }

        /// <summary>
        /// implementation of IStopGoController.Kill
        /// This fails for non-invasive targets.
        /// </summary>
        /// <param name="exitCode">ignored for non-invasive targets since this operation will fail</param>
        public void Kill(int exitCode)
        {
            ThrowInvalid();
        }

        /// <summary>
        /// Detach from the target.
        /// </summary>
        public void Detach()
        {
            if (m_process.CorProcess != null)
                m_process.CorProcess.Detach();
        }
        #endregion

        static void ThrowInvalid()
        {
            throw new InvalidOperationException("Non Invasive targets don't support this operation.");
        }
    }

    /// <summary>
    /// StopGo controller for a live debuggee, using ICorDebugProcess.Stop and ICorDebugProcess.Continue.
    /// </summary>
    class V2StopGoController : IStopGoController
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="process">process object that this controller is for</param>
        public V2StopGoController(MDbgProcess process)
        {
            m_process = process;
        }

        MDbgProcess m_process;

        // this is null if the debuggee has exited.
        CorProcess CorProcess
        {
            get { return m_process.CorProcess; }
        }

        #region IStopGoController Members
        /// <summary>
        /// Implementation of IStopGoController.CanExecute.
        /// </summary>        
        public bool CanExecute
        {
            get
            {

                // Default is live non-invasive debugging. 
                // We can execute the debuggee, so don't throw.
                return true;
            }
        }

        /// <summary>
        /// Implementation of IStopGoController.IsRunning.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                // CorProcess gets set to null if the debuggee exits. ExitProcess
                // is considered Stopped, so return false.
                if (CorProcess == null) return false;

                return CorProcess.IsRunning();
            }
        }

        // How many times we need to call ICorDebugProcess::Continue() to resume
        // the process. When the process is running free, this is 0. 
        int m_stopCount = 0;

        /// <summary>
        /// Implementation of IStopGoController.MarkAsStopped.
        /// </summary>
        public void MarkAsStopped()
        {
            //Debug.Assert(!IsRunning);
            m_stopCount++;
            Trace.WriteLine("Increasing StopCount to " + m_stopCount);
        }

        /// <summary>
        /// Implementation of IStopGoController.AsyncBreak.
        /// </summary>
        public void AsyncBreak()
        {
            Debug.Assert(CorProcess != null);
            //Debug.Assert(IsRunning);

            //the timeout value passed to ICDProcess::Stop is ignored.
            CorProcess.Stop(Int32.MaxValue);

            // IcorDebug's Stop() is synchronous and will block until complete.
            // So once it returns, we know we're stopped.
            MarkAsStopped();
        }

        /// <summary>
        /// Implementation of IStopGoController.Continue.
        /// </summary>
        public void Continue()
        {
            if (m_stopCount == 0)
            {
                // we are probably running -- no Continue should be issued
                throw new InvalidOperationException();
            }
            // Debug.Assert(!IsRunning); // struggles on first fake continue

            // this is because when we do AsyncStop and hit breakpoint at the same time we want to be sure that
            // we continue when we want to continue.
            Trace.WriteLine("ReallyContinueProcess(" + m_stopCount + ")");

            while (m_stopCount > 0)
            {
                CorProcess.Continue(false);
                m_stopCount--;
            }

            // We can't assert IsRunning here because the debuggee may get
            // a debug event as soon as we called Continue() above and immediately
            // transitioned back into a stopped state.
        }

        /// <summary>
        /// Implementation of IStopGoController.Kill.
        /// </summary>
        /// <param name="exitCode">exit code to terminate the process with</param>
        public void Kill(int exitCode)
        {
            Debug.Assert(!IsRunning);
            CorProcess.Terminate(exitCode);
        }

        /// <summary>
        /// Implementation of IStopGoController.Detach.
        /// </summary>
        public void Detach()
        {
            Debug.Assert(!IsRunning);
            CorProcess.Detach();
        }

        #endregion
    }


    /// <summary>
    /// MDbg Process Class
    /// </summary>
    public sealed class MDbgProcess : MarshalByRefObject
    {
        // Do common initiliazation and add to the process collection.
        void CommonInit(MDbgEngine engine)
        {
            Debug.Assert(engine != null);
            if (engine == null)
                throw new ArgumentException("Value cannot be null.", "engine");

            m_engine = engine;

            m_threadMgr = new MDbgThreadCollection(this);
            m_appDomainMgr = new MDbgAppDomainCollection(this);
            m_moduleMgr = new MDbgModuleCollection(this);
            m_breakpointMgr = new MDbgBreakpointCollection(this);
            m_debuggerVarMgr = new MDbgDebuggerVarCollection(this);

            // we'll register as last code, so that other fields are already registered.
            m_number = engine.Processes.RegisterProcess(this);
        }

        /// <summary>Creates an empty process object.
        /// This object can be used to start a debugging session by calling
        /// CreateProcess or Attach method on it.
        /// </summary>
        /// <param name="engine">Root engine object that manages this process.</param>
        /// <param name="debugger">CorDebugger object that will be used to do an actual
        /// debugging</param>
        public MDbgProcess(MDbgEngine engine, CorDebugger debugger)
        {
            Debug.Assert(debugger != null);
            if (debugger == null)
                throw new ArgumentNullException("debugger");

            m_corDebugger = debugger;

            CommonInit(engine);
        }

        /// <summary>
        /// Creates an MdbgProcess that can be compatible with the native pipeline
        /// debugging architecture
        /// </summary>
        /// <param name="engine">owning engine object</param>
        /// <param name="stopGo">stopGo controller to provide policy for execution of process.</param>
        public MDbgProcess(MDbgEngine engine)
        {
            CommonInit(engine);
        }

        /// <summary>
        /// Creates an MdbgProcess that can be compatible with dump debuggees.
        /// </summary>
        /// <param name="engine">owning engine object</param>
        /// <param name="stopGo">stopGo controller to provide policy for execution of process.</param>
        internal MDbgProcess(MDbgEngine engine, IStopGoController stopGo)
        {
            CommonInit(engine);
            m_stopGo = stopGo;
        }

        // Called when the CorProcess object is resolved. 
        // This happens late for dump debugging architecture
        internal void BindToCorProcess(CorProcess process)
        {
            Debug.Assert(m_corProcess == null); // only call once
            m_corProcess = process;
            InitDebuggerCallbacks();
        }

        /// <summary>
        /// Called to bind this MdbgProcess instance to a specific CLR in the target.
        /// </summary>
        /// <param name="process">process object representing managed debuggee</param>
        public void BindToExistingClrInTarget(CorProcess process)
        {
            Debug.Assert(this.m_corProcess == null);

            BindToCorProcess(process);

            // If we're binding to a CorProcess that's already 
            Prepopulate();
        }


        // On attach, iterate through and prepopulate MDbg's cache of target
        // objects (AppDomain lists, etc). 
        void Prepopulate()
        {
            Debug.Assert(m_corProcess != null);

            // All enumerations here are in a random order and not necessarily
            // in creation-order.
            foreach (CorAppDomain appdomain in m_corProcess.AppDomains)
            {
                AppDomains.Register(appdomain);

                foreach (CorAssembly assembly in appdomain.Assemblies)
                {
                    // Mdbg doesn't keep any state for assemblies.
                    foreach (CorModule module in assembly.Modules)
                    {
                        MDbgModule m = m_moduleMgr.Register(module);
                        Debug.Assert(m != null);
                    }
                }
            }

            foreach (CorThread thread in m_corProcess.Threads)
            {
                m_threadMgr.Register(thread);

                m_threadMgr.SetActiveThread(thread);
            }

        }

        IStopGoController m_stopGo;

        /// <summary>
        /// Gets the ICorDebug wrapper. This may not be available for all
        /// ICorDebugProcess implementations.
        /// </summary>
        /// <value>The CorDebugger if available. This may be null</value>
        public CorDebugger CorDebugger
        {
            get
            {
                return m_corDebugger;
            }
        }

        /// <summary>
        /// Gets the CorProcess represented by this MDbg Process.
        /// </summary>
        /// <value>The CorProcess.</value>
        public CorProcess CorProcess
        {
            get
            {
                return m_corProcess;
            }
        }

        /// <summary>
        /// Gets the DumpReader used to implement the data target, when this process
        /// is really just a dump file (null otherwise)
        /// </summary>
        /// <value>The DumpReader in use when this process is a dump file.</value>
        public DumpReader DumpReader
        {
            get
            {
                return m_dumpReader;
            }
        }

        /// <summary>
        /// Gets the MDbgThreadCollection in this Process.
        /// </summary>
        /// <value>The MDbgThreadCollection.</value>
        public MDbgThreadCollection Threads
        {
            get
            {
                return m_threadMgr;
            }
        }

        /// <summary>
        /// Gets the MDbgBreakpointCollection for this Process.
        /// </summary>
        /// <value>The MDbgBreakpointCollection.</value>
        public MDbgBreakpointCollection Breakpoints
        {
            get
            {
                return m_breakpointMgr;
            }
        }

        /// <summary>
        /// Gets the MDbgModuleCollection for this Process.
        /// </summary>
        /// <value>The MDbgModuleCollection.</value>
        public MDbgModuleCollection Modules
        {
            get
            {
                return m_moduleMgr;
            }
        }

        /// <summary>
        /// Gets the MDbgAppDomainCollection for this Process.
        /// </summary>
        /// <value>The MDbgAppDomainCollection.</value>
        public MDbgAppDomainCollection AppDomains
        {
            get
            {
                return m_appDomainMgr;
            }
        }

        /// <summary>
        /// Gets the MDbgDebuggerVarCollection for this Process.
        /// </summary>
        /// <value>The MDbgDebuggerVarCollection.</value>
        public MDbgDebuggerVarCollection DebuggerVars
        {
            get
            {
                return m_debuggerVarMgr;
            }
        }

        /// <summary>
        /// Gets if the Process is Alive.  A process is considered to be alive when it has an underlying
        /// native process.  See the comment for m_isAlive for more information.
        /// </summary>
        /// <value>true if Alive, else false.</value>
        public bool IsAlive
        {
            get
            {
                return m_isAlive;
            }
        }

        /// <summary>
        /// This method is called to indicate that the underlying native process for this instance of 
        /// MDbgProcess has been created.
        /// </summary>
        public void NativeProcessCreated()
        {
            m_isAlive = true;
        }

        /// <summary>
        /// Gets if the Process is Running.
        /// </summary>
        /// <value>true if Running, else false.</value>
        public bool IsRunning
        {
            get
            {
                return IsAlive && m_stopGo.IsRunning;
            }
        }

        /// <summary>
        ///  Gets the Number for this process.  This is not the PID.  A debugger may run multiple processes simultaneously.
        /// </summary>
        /// <value>The Number.</value>
        public int Number
        {
            get
            {
                return m_number;
            }
        }

        /// <summary>
        /// Returns command line that started this process or null if the process has been attached to.
        /// </summary>
        /// <value>The Command Line.</value>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// Returns the reason why the Process is stopped.
        /// </summary>
        /// <value>The Reason for stopping, or throws if the process is not stopped.</value>
        public Object StopReason
        {
            get
            {
                Debug.Assert(m_stopReason != null, "StopReason must be non-null when process is stopped");
                return m_stopReason;
            }
        }

        /// <summary>
        /// Gets how many times the process has stopped. Objects can cache
        /// the stop-count and use that later to determine if the process
        /// was continued since the count was last cached.
        /// </summary>
        /// <value>How many times.</value>
        public int StopCounter
        {
            get
            {
                return m_stopCounter;
            }
        }

        /// <summary>
        /// Gets the Stop Event. This is high when the debuggee is stopped,
        /// and low when the debuggee is running. Shells can wait on this
        /// to wait for the debuggee to stop.
        /// </summary>
        /// <value>The Stop Event.</value>
        public WaitHandle StopEvent
        {
            get
            {
                return m_stopEvent;
            }
        }

        /// <summary>
        /// Gets or sets the Symbol Path.
        /// </summary>
        /// <value>The Symbol Path.</value>
        public string SymbolPath
        {
            get
            {
                return m_symPath;
            }
            set
            {
                m_symPath = value;
                foreach (MDbgModule m in Modules)
                    m.ReloadSymbols(false);
            }
        }

        /// <summary>
        /// Gets or sets the DebugMode Flags.  You can only set these before the process starts.
        /// </summary>
        /// <value>The DebugMode Flags.</value>
        public DebugModeFlag DebugMode
        {
            get
            {
                return m_debugMode;
            }
            set
            {
                if (IsAlive)
                    throw new InvalidOperationException("DebugMode can be only set before process is started");
                m_debugMode = value;
            }
        }

        /// <summary>
        /// RawMode enumeration.
        /// </summary>
        public enum RawMode
        {
            /// <summary>
            /// No setting.
            /// </summary>
            None,
            /// <summary>
            /// Always Stop.
            /// </summary>
            AlwaysStop,
            /// <summary>
            /// Never Stop.
            /// </summary>
            NeverStop
        }

        /// <summary>
        ///  Gets or sets the RawModeType for the Process.
        /// </summary>
        /// <value>The RawModeType.</value>
        public RawMode RawModeType
        {
            get
            {
                return m_rawMode;
            }
            set
            {
                m_rawMode = value;
                if (value == RawMode.None && Threads.HaveActive)
                {
                    // We call setActiveThread here because:
                    // a) current active thread might got destroyed during Raw-mode
                    // b) SetActiveThread increases logical clock to cause stack to refresh
                    //    (see m_logicalClock in ThreadMgr)
                    Threads.SetActiveThread(Threads.Active.CorThread);
                }
            }
        }

        private RawMode m_rawMode = RawMode.None;

        /// <summary>
        ///  Gets or sets wheter the debugged program should automatically stop
        ///  on user-entry breakpoint.
        /// </summary>
        /// <value> True if debugged process should stop on user-entry breakpoint as
        /// saved in .pdb.
        /// </value>
        public bool EnableUserEntryBreakpoint
        {
            get
            {
                return m_userEntryBreakpointEnabled;
            }

            set
            {
                // If we're disabling the user-entry breakpoint after it has been created
                // but before it has been hit, then clean it up.
                if (!value && m_userEntryBreakpoint != null)
                {
                    m_userEntryBreakpoint.Delete();
                    m_userEntryBreakpoint = null;
                }
                m_userEntryBreakpointEnabled = value;
            }
        }

        #region Creation Commands
        // Methods used for activating an MdbgProcess around a real debuggee.

        // constants used in CreateProcess functions
        private enum CreateProcessFlags
        {
            CREATE_NEW_CONSOLE = 0x00000010,
            DEBUG_PROCESS = 3,                                //DEBUG_PROCESS|DEBUG_ONLY_THIS_PROCESS
        }

        /// <summary>
        /// Creates a new process.
        /// </summary>
        /// <param name="commandLine">The commandline to run.</param>
        /// <param name="commandArguments">The arguments to pass.</param>
        public void CreateProcess(string commandLine, string commandArguments)
        {
            CreateProcess(commandLine, commandArguments, ".");
        }

        /// <summary>
        /// Creates a new process.
        /// </summary>
        /// <param name="commandLine">The commandline to run.</param>
        /// <param name="commandArguments">The arguments to pass.</param>
        /// <param name="workingDirectory">The working directory for the new process.</param>
        public void CreateProcess(string commandLine, string commandArguments, string workingDirectory)
        {
            CreateProcess(commandLine, commandArguments, workingDirectory, null);
        }

        public void CreateProcess(string commandLine, string commandArguments, string workingDirectory, CorRemoteTarget target)
        {
            Debug.Assert(!IsAlive);
            if (IsAlive)
                throw new InvalidOperationException("cannot call CreateProcess on active process");

            m_stopGo = new V2StopGoController(this);

            int flags = (int)(m_engine.Options.CreateProcessWithNewConsole ? CreateProcessFlags.CREATE_NEW_CONSOLE : 0);

            try
            {
                m_corProcess = m_corDebugger.CreateProcess(commandLine, commandArguments, workingDirectory, flags, target);
                NativeProcessCreated();

            }
            catch
            {
                CleanAfterProcessExit();                    // remove process from process list in case of failure
                throw;
            }

            InitDebuggerCallbacks();

            if (commandLine != null)
                m_name = commandLine;
            else
                m_name = commandArguments;
        }


        /// <summary>
        /// Attach to a process by Process ID (PID)
        /// </summary>
        /// <param name="processId">The PID to attach to.</param>
        public void Attach(int processId)
        {
            Attach(processId, null);
        }

        /// <summary>
        /// Attach to a process by Process ID (PID)
        /// </summary>
        /// <param name="processId">The PID to attach to.</param>
        /// <param name="attachContinuationEvent">A OS event handle which must be set to unblock the debuggee
        /// when first continuing from attach</param>
        public void Attach(int processId, SafeWin32Handle attachContinuationEvent)
        {
            Attach(processId, attachContinuationEvent, null);
        }

        public void Attach(int processId, SafeWin32Handle attachContinuationEvent, CorRemoteTarget target)
        {
            Debug.Assert(!IsAlive);
            if (IsAlive)
                throw new InvalidOperationException("cannot call Attach on active process");

            m_stopGo = new V2StopGoController(this);

            m_processAttaching = true;
            try
            {
                m_corProcess = m_corDebugger.DebugActiveProcess(processId,/*win32Attach*/ false, target);
                NativeProcessCreated();
            }
            catch
            {
                // if an attach fails, we need to remove the process object from active processes.
                CleanAfterProcessExit();
                throw;
            }

            // User's breakpoint should start with number 0. When launching, 1st breakpoint created is
            // special -- user entry point and it's given number 0. 
            // In case of attach we don't create any such breakpoint but we still want
            // to start numbering breakpoints with 1; therefore we'll increase free breakpoint number.
            if (Breakpoints.m_freeBreakpointNumber == 0)
            {
                Breakpoints.m_freeBreakpointNumber = 1;
            }

            InitDebuggerCallbacks();

            // resume the debuggee
            // for pure managed debugging this is legal as long as DebugActiveProcess has returned
            if (attachContinuationEvent != null)
            {
                Microsoft.Samples.Debugging.Native.NativeMethods.SetEvent(attachContinuationEvent);
                attachContinuationEvent.Close();
            }
        }

        /// <summary>
        /// Attaches the process to a dump
        /// </summary>
        /// <param name="dumpReader">A reader for the dump</param>
        /// <param name="clrInstanceId">The moduleBaseAddress of the CLR to debug or null
        /// to debug the first CLR encountered</param>
        public void AttachToDump(DumpReader dumpReader, long? clrInstanceId)
        {
            // bind to a runtime in the dump
            DumpDataTarget dataTarget = new DumpDataTarget(dumpReader);
            AttachToDump(dumpReader, clrInstanceId, dataTarget);
        }

        /// <summary>
        /// Attaches the process to a dump using the specified DumpDataTarget
        /// </summary>
        /// <param name="dumpReader">A reader for the dump</param>
        /// <param name="clrInstanceId">The moduleBaseAddress of the CLR to debug or null
        /// to debug the first CLR encountered</param>
        public void AttachToDump(DumpReader dumpReader, long? clrInstanceId, ICorDebugDataTarget dataTarget)
        {
            Debug.Assert(!IsAlive);

            if (IsAlive)
                throw new InvalidOperationException("cannot call Attach on active process");

            // set up the stopGo controller
            NoninvasiveStopGoController stopGo = new NoninvasiveStopGoController();
            stopGo.Init(this);
            m_stopGo = stopGo;
            NativeProcessCreated();

            foreach (DumpModule module in dumpReader.EnumerateModules())
            {
                // use the selected runtime if there was one, otherwise just attach to the first
                // runtime we find
                if (clrInstanceId.HasValue && (clrInstanceId.Value != (long)module.BaseAddress))
                    continue;

                CLRDebugging clrDebugging = new CLRDebugging();
                Version actualVersion;
                ClrDebuggingProcessFlags flags;
                CorProcess proc;
                int hr = clrDebugging.TryOpenVirtualProcess(module.BaseAddress, dataTarget, LibraryProvider,
                    new Version(4, 0, short.MaxValue, short.MaxValue), out actualVersion, out flags, out proc);

                if (hr < 0)
                {
                    if ((hr == (int)HResult.CORDBG_E_NOT_CLR) ||
                       (hr == (int)HResult.CORDBG_E_UNSUPPORTED_DEBUGGING_MODEL) ||
                       (hr == (int)HResult.CORDBG_E_UNSUPPORTED_FORWARD_COMPAT))
                    {
                        // these errors are all potentially benign, just ignore this module.
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(hr); // the rest are bad
                    }
                }
                else
                {
                    // We must have succeeded 
                    Debug.Assert(proc != null);
                    BindToExistingClrInTarget(proc);
                    m_dumpReader = dumpReader;
                    return; // SUCCESS - done
                }
            }

            // If we've got here, it means there were no supported CLRs in the dump
            Debug.Assert(m_corProcess == null); // Should not yet be bound
            throw new MDbgException("Failed to open the dump - no supported CLRs found in the module list");
        }

        #endregion Creation Commands


        /// <summary>
        /// Ensure the debuggee is alive (CorProcess != null); and throw if it is not.
        /// </summary>
        void EnsureIsAlive()
        {
            Debug.Assert(IsAlive);
            if (!IsAlive)
                throw new MDbgNoActiveInstanceException("process is dead");
        }

        /// <summary>
        /// Helper to check if the target can execute.
        /// </summary>
        /// <returns>A boolean representing whether the target can execute.</returns>
        public bool CanExecute()
        {
            return m_stopGo.CanExecute;
        }

        /// <summary>
        /// Helper to throw if the target is non-invasive. Nop if the target
        /// is invasive.
        /// </summary>
        /// <param name="operationDescription">hint of operation that was
        /// attempted that required invasive access. This is useful for error
        /// strings.</param>
        void EnsureCanExecute(string operationDescription)
        {
            if (!m_stopGo.CanExecute)
            {
                throw new InvalidOperationException("Operation prohibited ('" + operationDescription + "'). Debuggee is non-invasive.");
            }
        }

        /// <summary>
        /// Detach from the Process.
        /// </summary>
        /// <returns>A WaitHandle for the Stop Event.</returns>
        public WaitHandle Detach()
        {
            // Expect that the StopEvent is already set on entry (since Detach is
            // called when we're stopped). We don't reset it and just leave
            // it high. 

            if (m_stopGo != null)
                m_stopGo.Detach();
            // No further callbacks received. In particular, ExitProcess 
            // is not fired for detach.

            CleanAfterProcessExit();


            return StopEvent;
        }

        /// <summary>
        /// Kill the Process.
        /// </summary>
        /// <returns>A WaitHandle for the Stop event.</returns>
        public WaitHandle Kill()
        {
            EnsureIsAlive();

            // Kill is really treated as a "stop-debugging". 
            // For non-invasive targets, that means "detach".
            if (!m_stopGo.CanExecute)
            {
                return Detach();
            }

            // Do some preliminary error checking before resetting the event.
            m_stopEvent.Reset();

            // After we kill the debuggee, we may get the ExitProcess event,
            // which will set the event high. So Reset the event before we
            // terminate so that we have a total ordering and avoid races.
            m_stopGo.Kill(255);

            // This gets set by the managed ExitProcess event handler.
            return StopEvent;
        }

        /// <summary>
        /// Have the Process Go.
        /// </summary>
        /// <returns>A WaitHandle for the Stop event.</returns>
        public WaitHandle Go()
        {
            ReallyContinueProcess();
            return StopEvent;
        }

        #region Stepping Helpers

        /// <summary>
        /// Sets a stepper for the process. Process will stop with step complete,
        /// once this stepper completes the step.
        /// This function just sets the active stepper, but it doesn't continue the process.
        /// Once the stepper is set, a Go() command needs to be called.
        /// The 'active stepper' is also exclusive with registering a custom stepper.
        /// </summary>
        public void SetActiveStepper(CorStepper activeStepper)
        {
            // we are not interested in finishing old step.
            if (m_activeStepper != null)
            {
                m_activeStepper.Deactivate();
            }

            m_activeStepper = activeStepper;
        }

        /// <summary>
        /// Have the Process Step Over.
        /// </summary>
        /// <param name="stepNativeCode">false to step source lines, true to single step instructions.</param>
        /// <returns>A WaitHandle for the Stop event.</returns>
        public WaitHandle StepOver(bool stepNativeCode)
        {
            StepImpl(stepNativeCode, StepperType.Over);
            return StopEvent;
        }

        /// <summary>
        /// Have the Process Step Into.
        /// </summary>
        /// <param name="stepNativeCode">false to step source lines, true to single step instructions.</param>
        /// <returns>A WaitHandle for the Stop event.</returns>
        public WaitHandle StepInto(bool stepNativeCode)
        {
            StepImpl(stepNativeCode, StepperType.In);
            return StopEvent;
        }

        /// <summary>
        /// Have the Process Step Out.
        /// </summary>
        /// <returns>A WaitHandle for the Stop event.</returns>
        public WaitHandle StepOut()
        {
            StepImpl(false, StepperType.Out);
            return StopEvent;
        }


        #endregion // Stepping Helpers

        /// <summary>
        /// Have the Process Asynchronously Stop.
        /// </summary>
        /// <returns>A WaitHandle for the Stop event.</returns>
        public WaitHandle AsyncStop()
        {
            lock (this)
            {
                // we need to check if m_corProcess!=null. IsAlive is doing that.
                // And since we're in the lock (clearing/setting m_process is always done
                // under this lock, we can assume that it won't change.
                if (!IsAlive)
                {
                    throw new MDbgNoActiveInstanceException("process is dead");
                }

                // Let AsyncBreak handle error checking, don't call CanExecute here.
                // AsyncBreak is valid on non-invasive targets, whereas CanExecute returns false.
                // Notify the eventing pipeline to stop.
                m_stopGo.AsyncBreak();
            }

            CorThread activeThread = null;

            if (Threads.HaveActive)
            {
                // old thread #, that we started command from
                int id = Threads.Active.Id;

                foreach (CorThread t in CorProcess.Threads)
                {
                    if (t.Id == id)
                    {
                        activeThread = t;
                        break;
                    }
                }
            }

            // This will set the StopEvent.
            InternalSignalRuntimeIsStopped(activeThread, new AsyncStopStopReason());

            return StopEvent;
        }

        //////////////////////////////////////////////////////////////////////////////////
        //
        // Operations that support for integration of custom debugger primitives
        // (as steppers, breakpoints, evals, ...)
        //
        //////////////////////////////////////////////////////////////////////////////////
        #region Custom callbacks

        // The code is using ListDictionary for storing registered callbacks. This
        // implementation was choosen because it is faster than Hashtable for small
        // collections.
        // We don't expect that the number of registered items will be bigger than 10.
        //
        // The collections are initialized lazily on the first use.
        private ListDictionary customSteppers;
        private ListDictionary customBreakpoints;
        private ListDictionary customEvals;


        /// <summary> Event fired after all debug events. </summary>
        /// <remarks> This event gives extensions a general purpose hook for all debug events.
        /// </remarks>
        public event PostCallbackEventHandler PostDebugEvent;

        /// <summary>
        /// Registers a Custom Stepper
        /// Registrations are valid only for one callback, i.e. the object is
        /// automatically deregistered after the callback is fired. If the user wishes to
        /// receive additional callback, the registration has to be done again in the
        /// callback.
        /// </summary>
        /// <param name="stepper">The CorStepper to run.</param>
        /// <param name="handler">The CustomStepperEventHandler to use.</param>
        public void RegisterCustomStepper(CorStepper stepper, CustomStepperEventHandler handler)
        {
            Debug.Assert(stepper != null);
            if (stepper == null)
                throw new ArgumentException("cannot be null", "stepper");
            if (handler == null)
            {
                // explicit deregistration
                if (customSteppers != null)
                    customSteppers.Remove(stepper);
            }
            else
            {
                // adding registration
                if (customSteppers == null)
                    customSteppers = new ListDictionary();
                else
                    if (customSteppers.Contains(stepper))
                        throw new InvalidOperationException("Handler alrady registered for the custom stepper");
                customSteppers.Add(stepper, handler);
            }
        }

        /// <summary>
        /// Register a handler to be invoked when a Custom Breakpoint is hit.
        /// </summary>
        /// <param name="breakpoint">The CorBreakpoint to register.</param>
        /// <param name="handler">The CustomBreakpointEventHandler to run when the Breakpoint is hit.
        /// 	If this is null, this breakpoints callback is explicitly deregistered.
        /// </param>
        /// <remarks>
        /// 	Only one handler can be specified per breakpoint. The handler could be a 
        /// 	multicast delegate. To change a breakpoints handler, first deregister it and then set
        /// 	a new one.
        /// 	The handler has the same lifespan semantics as the underlying CorBreakpoint object.
        /// 	That means it is alive and the same handler will be invoked every time the breakpoint is hit
        /// 	until the handler is explicitly deregistered. 
        ///
        /// 	A handler can stop the shell at the breakpoint by calling Controller.Stop on the
        ///		CustomBreakpointEventArgs parameter passed into the delegate.
        ///</remarks>
        public void RegisterCustomBreakpoint(CorBreakpoint breakpoint, CustomBreakpointEventHandler handler)
        {
            Debug.Assert(breakpoint != null);
            if (breakpoint == null)
                throw new ArgumentException("cannot be null", "breakpoint");
            if (handler == null)
            {
                // explicit deregistration
                if (customBreakpoints != null)
                {
                    customBreakpoints.Remove(breakpoint);
                }
            }
            else
            {
                // adding registration
                if (customBreakpoints == null)
                {
                    customBreakpoints = new ListDictionary();
                }
                else
                {
                    if (customBreakpoints.Contains(breakpoint))
                    {
                        throw new InvalidOperationException("Handler alrady registered for the custom breakpoint");
                    }
                }
                customBreakpoints.Add(breakpoint, handler);
            }
        }

        /// <summary>
        /// Register a Custom Eval.
        /// </summary>
        /// <param name="eval">The CorEval to register.</param>
        /// <param name="handler">The CustomEvalEventHandler to use.</param>
        public void RegisterCustomEval(CorEval eval, CustomEvalEventHandler handler)
        {
            Debug.Assert(eval != null);
            if (eval == null)
                throw new ArgumentException("cannot be null", "eval");
            if (handler == null)
            {
                // explicit deregistration
                if (customEvals != null)
                    customEvals.Remove(eval);
            }
            else
            {
                // adding registration
                if (customEvals == null)
                    customEvals = new ListDictionary();
                else
                    if (customEvals.Contains(eval))
                        throw new InvalidOperationException("Handler alrady registered for the custom eval");
                customEvals.Add(eval, handler);
            }
        }
        #endregion Custom callbacks

        private class MDbgProcessStopController : IMDbgProcessController, IDisposable
        {
            public MDbgProcessStopController(MDbgProcess process, CorEventArgs eventArgs, bool needAsyncStopCall)
            {
                Debug.Assert(process != null);
                Debug.Assert(eventArgs != null);
                this.process = process;
                this.eventArgs = eventArgs;
                this.needAsyncStopCall = needAsyncStopCall;
            }

            public bool CustomStopRequested
            { get { return customStopRequested; } }

            void IMDbgProcessController.Stop(CorThread activeThread, object stopReason)
            {
                if (process == null)
                    throw new InvalidOperationException("Process controller is not active anymore");

                if (needAsyncStopCall)
                {
                    process.m_stopGo.AsyncBreak();
                }
                eventArgs.Continue = false;         // signal to CorLayer that we want to stop
                process.InternalSignalRuntimeIsStopped(activeThread, stopReason);
                customStopRequested = true;
            }

            public void Dispose()
            {
                process = null;
            }

            private MDbgProcess process;
            private CorEventArgs eventArgs;
            private bool customStopRequested = false;

            // This field indicates if an async stop is needed when the Stop method is called. 
            // This will be true only in the interop debugging scenario when an in-band native event is
            // received.
            private bool needAsyncStopCall;
        }


        //////////////////////////////////////////////////////////////////////////////////
        //
        // Helper functions
        //
        //////////////////////////////////////////////////////////////////////////////////

        #region Resolve and Parsing Helpers
        /// <summary>
        /// Resolves a type from a Variable Name.
        /// </summary>
        /// <param name="typeName">The name of the type to resolve.</param>
        /// <returns>The CorType of that Variable.</returns>
        public CorType ResolveType(string typeName)
        {
            return ResolveType(typeName, null);
        }

        /// <summary>
        /// Resolves a type from a Variable Name.
        /// </summary>
        /// <param name="typeName">The name of the type to resolve.</param>
        /// <param name="appDomain">The AppDomain to resolve the type in or null if any
        /// AppDomain is acceptable</param>
        /// <returns>The CorType of that Variable.</returns>
        public CorType ResolveType(string typeName, CorAppDomain appDomain)
        {
            MDbgModule mod;
            CorClass cc = ResolveClass(typeName, appDomain, out mod);
            if (cc == null)
            {
                return null;
            }
            CorType[] cta = new CorType[0];
            return cc.GetParameterizedType(CorElementType.ELEMENT_TYPE_CLASS, cta);
        }

        /// <summary>
        /// Resolves a class from a Variable Name.
        /// </summary>
        /// <param name="typeName">The name of the type to resolve.</param>
        /// <returns>The CorClass of that Variable.</returns>
        public CorClass ResolveClass(string typeName)
        {
            MDbgModule mod;
            return ResolveClass(typeName, out mod);
        }

        /// <summary>
        /// Resolves a class from a Variable Name.
        /// </summary>
        /// <param name="typeName">The name of the type to resolve.</param>
        /// <param name="mod">
        ///   Returns the module in which the class was resolved. value is
        ///   null if the resolution fails. 
        /// </param>
        /// <returns>The CorClass of that Variable.</returns>
        public CorClass ResolveClass(string typeName, out MDbgModule mod)
        {
            return ResolveClass(typeName, null, out mod);
        }

        /// <summary>
        /// Resolves a class from a Variable Name.
        /// </summary>
        /// <param name="typeName">The name of the type to resolve.</param>
        /// <param name="appDomain">The AppDomain to resolve the type in or null if any AppDomain can be used</param>
        /// <param name="mod">
        ///   Returns the module in which the class was resolved. value is
        ///   null if the resolution fails. 
        /// </param>
        /// <returns>The CorClass of that Variable.</returns>
        public CorClass ResolveClass(string typeName, CorAppDomain appDomain, out MDbgModule mod)
        {
            mod = null;
            int classToken = CorMetadataImport.TokenNotFound;

            Debug.Assert(typeName.Length != 0);
            // we cannot resolve to global token, since there is a global token for module.

            foreach (MDbgModule m in Modules)
            {
                // debugger modules are scoped by app domain
                // we need to find a module in the correct domain to resolve the
                // class in the correct domain
                if (appDomain != null && m.CorModule.Assembly.AppDomain != appDomain)
                    continue;
                classToken = m.Importer.GetTypeTokenFromName(typeName);
                if (classToken != CorMetadataImport.TokenNotFound)
                {
                    mod = m;
                    return mod.CorModule.GetClassFromToken(classToken);
                }
            }

            return null;
        }

        /// <summary>
        /// Resolves a Function from a Module, Class Name, and Function Name.
        /// </summary>
        /// <param name="mdbgModule">The Module that has the Function.</param>
        /// <param name="className">The name of the Class that has the Function.</param>
        /// <param name="functionName">The name of the Function.</param>
        /// <returns>The MDbgFunction that matches the given parameters.</returns>
        public MDbgFunction ResolveFunctionName(MDbgModule mdbgModule, string className, string functionName)
        {
            int typeToken = mdbgModule.Importer.GetTypeTokenFromName(className);
            if (typeToken == CorMetadataImport.TokenNotFound)
                return null;

            MDbgFunction func = null;

            Type t = mdbgModule.Importer.GetType(typeToken);
            foreach (MethodInfo mi in t.GetMethods())
            {
                if (mi.Name.Equals(functionName))
                {
                    func = mdbgModule.GetFunction((mi as MetadataMethodInfo).MetadataToken);
                    break;
                }
            }
            return func;
        }

        /// <summary>
        /// Resolves a Function from a Module, Class Name, Function Name, and AppDomain.
        /// </summary>
        /// <param name="moduleName">The Module name that has the Function.</param>
        /// <param name="className">The name of the Class that has the Function.</param>
        /// <param name="functionName">The name of the Function.</param>
        /// <param name="appDomain">The AppDomain to look in.</param>
        /// <returns>The MDbgFunction that matches the given parameters.</returns>
        public MDbgFunction ResolveFunctionName(string moduleName, string className, string functionName, CorAppDomain appDomain)
        {
            Debug.Assert(className != null);
            Debug.Assert(functionName != null);


            MDbgFunction func = null;

            if (moduleName != null)
            {
                MDbgModule module = Modules.Lookup(moduleName);
                if (module != null)
                {
                    CorAppDomain moduleAppDomain = module.CorModule.Assembly.AppDomain;
                    if (moduleAppDomain == null   // not a shared assembly
                        || appDomain == null      // we don't limit us to the certain appDomain
                        || appDomain == moduleAppDomain // the module is from correct domain
                        )
                    {
                        func = ResolveFunctionName(module, className, functionName);
                    }
                }
            }
            else
            {
                foreach (MDbgModule m in Modules)
                {
                    CorAppDomain moduleAppDomain = m.CorModule.Assembly.AppDomain;
                    if (moduleAppDomain == null   // is a shared assembly
                        || appDomain == null      // we don't limit us to the certain appDomain
                        || appDomain == moduleAppDomain // the module is from correct domain
                        )
                    {
                        func = ResolveFunctionName(m, className, functionName);
                        if (func != null)
                            break;
                    }
                }
            }

            return func;
        }

        /// <summary>
        /// Resolve Function name based on current thread's AppDomain.
        /// </summary>
        /// <param name="functionName">The name of the function to resolve.</param>
        /// <returns>The matching MDbgFunction.</returns>
        public MDbgFunction ResolveFunctionNameFromScope(string functionName)
        {
            return ResolveFunctionNameFromScope(functionName, Threads.Active.CorThread.AppDomain);
        }

        /// <summary>
        /// Resolve Function name based on given AppDomain.
        /// </summary>
        /// <param name="functionName">The name of the function to resolve.</param>
        /// <param name="appDomain">The AppDomain to resolve in.</param>
        /// <returns></returns>
        public MDbgFunction ResolveFunctionNameFromScope(string functionName, CorAppDomain appDomain)
        {
            Debug.Assert(functionName != null &&
                         functionName.Length > 0);
            string moduleName = null;
            string className = null;
            string funcName = null;

            string tempParser = functionName;

            int bangIndex = tempParser.IndexOf("!");
            if (bangIndex != -1)
            {
                moduleName = tempParser.Substring(0, bangIndex);
                tempParser = tempParser.Substring(bangIndex + 1);
            }

            int periodIndex = tempParser.LastIndexOf(".");
            if (periodIndex == -1)
            {
                // only function is specified -- assuming current class
                className = Threads.Active.CurrentFrame.Function.MethodInfo.DeclaringType.Name;
                funcName = tempParser;
            }
            else
            {
                className = tempParser.Substring(0, periodIndex);
                funcName = tempParser.Substring(periodIndex + 1);
            }

            // The last argument to the ResolveFunctionName is appDomain we are refering to.
            // That will cause to resolution happen only on modules loaded into that appdomain.
            return ResolveFunctionName(moduleName, className, funcName, appDomain);
        }

        // Helper to parse args to get a value for a GC handle.
        //
        // Syntax for gchandle. Ultimately need to compute an address.
        //  gchandle(var) where var is System.Runtime.InteropServices.GCHandle, address=var.m_handle
        //  gchandle(integer) where address =integer
        //  gchandle(var, offset) where var is a valuetype, then we do address= (IntPtr*) (&var + offset*sizeof(IntPtr))
        internal MDbgValue ParseGCHandleArgs(string stName, string[] args, MDbgFrame scope)
        {
            if (args.Length != 1 && args.Length != 2)
            {
                throw new MDbgException("Wrong number of args to gchandle function.");
            }

            string stVarBase = args[0];
            MDbgValue varBase = ResolveVariable(stVarBase, scope);
            //MDbgValue varBase = Shell.ExpressionParser.ParseExpression(stVarBase,this, scope);


            IntPtr add;

            if (args.Length == 2)
            {
                if (varBase == null)
                {
                    throw new MDbgException("Can't resolve var '" + stVarBase + "'");
                }

                // Form: gchandle(var, offset) 
                CorGenericValue gv = varBase.CorValue.CastToGenericValue();
                IntPtr[] ar = null;
                if (gv != null)
                {
                    ar = gv.GetValueAsIntPtrArray();
                }
                if (ar == null)
                {
                    throw new MDbgException("Variable '" + stVarBase + "' is not a value type.");
                }

                int offset = Int32.Parse(args[1], CultureInfo.InvariantCulture);
                add = ar[offset];
            }
            else
            {
                if (varBase != null)
                {
                    add = IntPtr.Zero;
                    // Form: gchandle(var)
                    if (varBase.TypeName != "System.Runtime.InteropServices.GCHandle")
                    {
                        throw new MDbgException("Variable is not of type \"System.Runtime.InteropServices.GCHandle\".");
                    }

                    foreach (MDbgValue field in varBase.GetFields())
                    {
                        if (field.Name == "m_handle")
                        {
                            int handleAddress = Int32.Parse(field.GetStringValue(0));
                            add = new IntPtr(handleAddress);
                            break;
                        }
                    }
                }
                else
                {
                    // Trying to resolve as a raw address now
                    // form: gchandle(integer)
                    int handleAddress;
                    if (!Int32.TryParse(stVarBase, out handleAddress))
                    {
                        throw new MDbgException("Couldn't recognize the argument as a variable name or address");
                    }
                    add = new IntPtr(handleAddress);
                }
            }


            CorReferenceValue result;

            try
            {
                result = this.CorProcess.GetReferenceValueFromGCHandle(add);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                if (e.ErrorCode == (int)HResult.CORDBG_E_BAD_REFERENCE_VALUE)
                {
                    throw new MDbgException("Invalid handle address.");
                }
                else
                {
                    throw;
                }
            }
            MDbgValue var = new MDbgValue(this, stName, result);
            return var;
        }

        // Extract the args for a psuedo-function call embedded in an expression.
        // variableName is the input expression.
        // Eg variableName ="gchandle(a,b).xyz", 
        // 
        // variableName - on output, is the RHS after the psuedo function. eg: ".xyz".
        // stFullName is the pretty name of the function wihtout the trailing expressions afterwards.
        //    eg: "gchandle(a,b)"
        // args is an array of strings of the args to the fucntion. eg: {"a", "b" }
        //
        // Caller verified that this actually begins with an function call.
        // Throws on error.
        private void GetExpressionFunctionArgs(ref string variableName, out string stFullName, out string[] args)
        {
            int idxStart = variableName.IndexOf('(');
            Debug.Assert(idxStart != -1); // caller verified we had a function call here.
            int idxEnd = variableName.IndexOf(')', idxStart);
            if (idxEnd == -1)
            {
                throw new MDbgException("Invalid expression. Missing closing ')' ");
            }

            {
                // This parsing implementation is naive and does not handle nested functions.
                // eg, f(f(a,b),c) will break it.
                // Check for that now and explicitly fail it.
                int idxEnd2 = variableName.IndexOf('(', idxStart + 1);
                if (idxEnd2 != -1)
                {
                    throw new MDbgException("Invalid expression. Nested function calls not yet allowed.");
                }
            }

            string argsraw = variableName.Substring(idxStart + 1, idxEnd - idxStart - 1);
            args = argsraw.Split(',');

            stFullName = variableName.Substring(0, idxEnd + 1);

            // Adjust args to go past gchandle(...) goo.
            variableName = variableName.Substring(idxEnd + 1);
        }

        /// <summary>
        /// Resolves a Variable name in a given scope.
        /// </summary>
        /// <param name="variableName">The name of the variable to resolve.</param>
        /// <param name="scope">The MDbgFrame to look in for that variable.</param>
        /// <returns>The MDbgValue that the given variable has in the given scope.</returns>
        public MDbgValue ResolveVariable(string variableName, MDbgFrame scope)
        {
            Debug.Assert(variableName != null);
            Debug.Assert(scope != null);

            // variableName should have this form:
            // [[module][#<appdomain>]!][(([namespace.]+)<type.>)|.]variable([.field]*)

            // Syntax in BNF form:
            //
            // Expr --> module_scope '!' var_expr
            //         | var_expr
            // module_scope --> <module name>  // as determined by Modules.Lookup
            // var_expr --> var_root
            //            | var_expr '.' <id:field>
            //            | var_expr '[' <integer> ']'
            // var_root --> psuedo_var | local_var | parameter_var | global_var | static_class_var\
            //            | 'gchandle(' ... ')' // see ParseGCHandleArgs
            // psuedo_var --> '$' <id>   // as determined by DebuggerVars.HaveVariable
            // local_var --> <id> // as determined by f.GetActiveLocalVars
            // parameter_var --> <id> // as determined by f.GetArguments
            // global_var --> <id> // as determined by fields on global token in each module
            // static_class_var --> (<id:namespace> '.')* <id:class> '.' <id:static field> 

            MDbgModule variableModule;          // name of the module we should look into for variable resolution
            // will contain null, if no module was specified
            { // limit scope of moduleVar
                string[] moduleVar = variableName.Split(new char[] { '!' }, 2);
                Debug.Assert(moduleVar != null);
                if (moduleVar.Length > 2)
                {
                    throw new MDbgException("Illegal variable syntax.");
                }
                else if (moduleVar.Length == 2)
                {
                    variableModule = Modules.Lookup(moduleVar[0]);
                    variableName = moduleVar[1];
                    if (variableModule == null)
                        throw new MDbgException("Module not found");
                }
                else
                    variableModule = null;
            }

            // lookup 1st part
            MDbgValue var = null;
            int nextPart = 0;

            // Check for predicates
            if (variableName.StartsWith("gchandle("))
            {
                string stName;
                string[] args;
                GetExpressionFunctionArgs(ref variableName, out stName, out args);
                nextPart = 1;

                var = this.ParseGCHandleArgs(stName, args, scope);
            } // end gchandle

            string[] nameParts = variableName.Split(new char[] { '.', '[' });

            Debug.Assert(nameParts.Length >= 1);  // there must be at least one part.


            if (var != null)
            {
                // already resolved, no extra work to do.
            }

            // Let's check if we are asking for debugger var. Those vars are prefixed with $.
            // if yes, return the var.
            else if (variableName.StartsWith("$")
               && variableModule == null          // debugger vars cannot have module specifier
               )
            {
                string varName = nameParts[nextPart];
                Debug.Assert(varName.StartsWith("$"));

                if (DebuggerVars.HaveVariable(nameParts[nextPart]))
                {
                    MDbgDebuggerVar dv = DebuggerVars[nameParts[0]];
                    var = new MDbgValue(this, dv.Name, dv.CorValue);
                }
                else
                    var = null;
                nextPart++;
            }
            else
            {
                ArrayList vars = new ArrayList();
                {  // fill up vars with locals+arguments
                    MDbgFunction f = scope.Function;
                    MDbgValue[] vals = f.GetActiveLocalVars(scope);
                    if (vals != null)
                        vars.AddRange(vals);

                    vals = f.GetArguments(scope);
                    if (vals != null)
                        vars.AddRange(vals);
                }

                // try to find a match in locals and arguments first
                foreach (MDbgValue v in vars)
                    if (v.Name == nameParts[nextPart])
                    {
                        var = v;
                        nextPart++;
                        break;
                    }

                // if no match for locals and arguments, look for globals and static class members
                if (var == null)
                {
                    // now let's try to resolve static var of form Namespace.namespace.typeName.var
                    bool bGlobal = (nameParts[nextPart].Length == 0);
                    if (bGlobal)
                        nextPart++;

                    foreach (MDbgModule m in this.Modules)
                    {
                        if (variableModule != null
                           && variableModule != m)
                            continue;                       // we're interested only in certain module

                        if (bGlobal)    // global variables
                        {
                            // nil type token is used to enum global static data members 
                            MetadataType gType = (MetadataType)m.Importer.GetType(0);
                            FieldInfo[] gField = gType.GetFields(0);

                            for (int i = 0; i < gField.Length; i++)
                            {
                                if (nameParts[nextPart] == gField[i].Name)
                                {
                                    var = new MDbgValue(this, "." + gField[i].Name, scope.Function.Module.CorModule.GetGlobalVariableValue(gField[i].MetadataToken));
                                    nextPart++;
                                    break;
                                }
                            }

                            if (var != null)    // done if we find the first match in any module
                                break;
                        }
                        else    // static class members
                        {
                            System.Text.StringBuilder sb = new System.Text.StringBuilder();
                            sb.Append(nameParts[nextPart]);
                            for (int i = nextPart + 1; i < nameParts.Length; i++)
                            {
                                int typeToken = m.Importer.GetTypeTokenFromName(sb.ToString());
                                if (typeToken != CorMetadataImport.TokenNotFound)
                                {
                                    // we resolved type, let's try to get statics
                                    CorClass cl = m.CorModule.GetClassFromToken(typeToken);

                                    Type classType = m.Importer.GetType(cl.Token);
                                    foreach (MetadataFieldInfo fi in classType.GetFields())
                                    {
                                        if (fi.Name != nameParts[i])
                                            continue;

                                        if (fi.IsStatic)
                                        {
                                            sb.Append(".").Append(nameParts[i]);
                                            CorValue fieldValue = cl.GetStaticFieldValue(fi.MetadataToken, scope.CorFrame);
                                            var = new MDbgValue(this, sb.ToString(), fieldValue);
                                            nextPart = i + 1;
                                            goto FieldValueFound;   // done if we find the first match in any module
                                        }
                                    }
                                }
                                sb.Append(".").Append(nameParts[i]);
                            }
                        }
                    }
                FieldValueFound:
                    ;
                }
            };

            if (var != null)
            {
                // now try to resolve remaining parts.
                for (int i = nextPart; i < nameParts.Length; i++)
                {
                    string part = nameParts[i];
                    if (part.EndsWith("]"))
                    {
                        // it is probably array index
                        string[] indexStrings = part.Substring(0, part.Length - 1).Split(',');
                        Debug.Assert(indexStrings != null && indexStrings.Length > 0);
                        int[] indexes = new int[indexStrings.Length];
                        for (int j = 0; j < indexStrings.Length; ++j)
                            indexes[j] = Int32.Parse(indexStrings[j], CultureInfo.InvariantCulture);
                        var = var.GetArrayItem(indexes);
                    }
                    else
                    {
                        // we'll treat it as field name
                        var = var.GetField(part);
                    }
                }
            }
            return var;
        }
        #endregion Resolve and Parsing Helpers

        //////////////////////////////////////////////////////////////////////////////////
        //
        // Private functions
        //
        //////////////////////////////////////////////////////////////////////////////////

        private void StepImpl(bool stepNativeCode, StepperType type)
        {
            EnsureCanExecute("stepping");

            StepperDescriptor s = StepperDescriptor.CreateSourceLevelStep(this, type, stepNativeCode);
            CorStepper stepper = s.Step();

            SetActiveStepper(stepper);
            ReallyContinueProcess();
        }

        private void ReallyContinueProcess()
        {
            // we need to protect m_stopCount from running too many times
            // when m_stopCount is increased in InternalRuntimeIsStopped.
            lock (this)
            {
                // we need to check if m_corProcess!=null. IsAlive is doing that.
                // And since we're in the lock (clearing/setting m_process is always done
                // under this lock, we can assume that it won't change.
                if (!IsAlive)
                {
                    throw new MDbgNoActiveInstanceException("process is dead");
                }

                // Early check to ensure we can execute.
                EnsureCanExecute("go");

                // Once we continue, various cached data becomes invalid
                OnPreContinue();

                m_stopReason = null;

                m_stopEvent.Reset();

                // This is trying to continue the shell. 
                // You can't call this from a callback unless the callback has already stopped the
                // shell. Else it will look like a superflous continue.
                this.m_stopGo.Continue();
            } // lock
        }


        // Key function to mark that we're transitioning the process into
        // a stopped state. 
        private void InternalSignalRuntimeIsStopped(CorThread activeThread, Object stopReason)
        {
            // Note that the debuggee is already stopped at this point. This is
            // just updating the MdbgProcess object to reflect that. So no
            // operations are needed on the underlying event pipeline to stop
            // the debuggee. (eg, don't need to call ICorDebugProcess.Stop).
            lock (this)
            {
                // we need to be very carefull here. Debugger API is not reentrant.
                // Normally this method is called only from callback to signal to main thread
                // that debuggee has stopped but there is one important exception -- async break.
                // When we do async-break we are calling AsyncStop() method from another thread, which
                // is calling InternalSignalRuntimeIsStopped. Therefore we can be running this function
                // from async-break thread and from managed callback thread. Because this method is
                // calling DebuggerAPI and functions there are not reentrant, bad thinkgs are happening.
                Trace.WriteLine("InternalSignalRuntimeIsStopped (" + stopReason + ")");

                // Normally, this method is called when the CLR is already stopped but the
                // MDbgProcess hasn't been marked yet. 
                // However, for async break, the stop count is already bumped up so don't do it again.
                if (!(stopReason is AsyncStopStopReason))
                {
                    m_stopGo.MarkAsStopped();
                }

                m_stopReason = stopReason;

                if (activeThread != null)
                {
                    if (!m_threadMgr.HaveActive || !m_processAttaching)
                    {
                        m_threadMgr.SetActiveThread(activeThread);
                    }
                    MDbgThread t = m_threadMgr.GetThreadFromThreadId(activeThread.Id);

                    // now check if we are in a special seqence point -- if so than just do another step.
                    try
                    {
                        MDbgSourcePosition sp = t.CurrentSourcePosition;

                        if (sp != null && sp.IsSpecial && (stopReason is StepCompleteStopReason))
                        {
                            // we want to perform antother step when we end-up in special seqence point
                            // only if we are stopped because of StepComplete. If we are stopped for any other
                            // reason (e.g. ExceptionThrown, we should not make any other steps since then we
                            // would receive stop reason for StepComplete and not for ExceptionThrown).

                            Trace.WriteLine("Making another step because of special line number (0xfeefee)");

                            StepImpl(false, StepperType.Over);
                            return; // we will be called again
                        }
                    }
                    catch (MDbgNoCurrentFrameException)
                    {
                        // if we don't have current frame, let's ignore it.
                    }
                }
                else
                {
                    m_threadMgr.SetActiveThread(null);
                }

                if (IsAlive)
                {
                    m_engine.Processes.Active = this;
                }

                Trace.WriteLine("Signaling Real Stop (" + stopReason + ")");

                m_processAttaching = false;
                Thread.Sleep(100);

                // we are not interested in completing step
                if (this.m_activeStepper != null)
                {
                    try
                    {
                        this.m_activeStepper.Deactivate();
                    }
                    catch (COMException)
                    {
                        // let's ignore if we cannot deactivate the stepper
                        // This can happen in cases where the app finishes
                        // (we receive ExitProcess callback) but we have outstanding
                        // stepper.
                    }
                }


                m_stopCounter = g_stopCounter++;
                m_stopEvent.Set();
            }
        }
        // This is called after CreateProcess, Attach, or any other simalar function
        private void InitDebuggerCallbacks()
        {
            Debug.Assert(CorProcess != null);

            // Now that we have valid CorProcess object, we can notify any extensions.
            MDbgProcessCollection pc = m_engine.Processes;
            pc.OnProcessResolved(this);


            CorProcess.OnBreakpoint += new BreakpointEventHandler(this.BreakpointEventHandler);
            CorProcess.OnStepComplete += new StepCompleteEventHandler(this.StepCompleteEventHandler);
            CorProcess.OnBreak += new CorThreadEventHandler(this.BreakEventHandler);
            CorProcess.OnException += new CorExceptionEventHandler(this.ExceptionEventHandler);
            CorProcess.OnEvalComplete += new EvalEventHandler(this.EvalCompleteEventHandler);
            CorProcess.OnEvalException += new EvalEventHandler(this.EvalExceptionEventHandler);
            CorProcess.OnCreateProcess += new CorProcessEventHandler(this.CreateProcessEventHandler);
            CorProcess.OnProcessExit += new CorProcessEventHandler(this.ExitProcessEventHandler);
            CorProcess.OnCreateThread += new CorThreadEventHandler(this.CreateThreadEventHandler);
            CorProcess.OnThreadExit += new CorThreadEventHandler(this.ExitThreadEventHandler);
            CorProcess.OnModuleLoad += new CorModuleEventHandler(this.LoadModuleEventHandler);
            CorProcess.OnModuleUnload += new CorModuleEventHandler(this.UnloadModuleEventHandler);
            CorProcess.OnClassLoad += new CorClassEventHandler(this.LoadClassEventHandler);
            CorProcess.OnClassUnload += new CorClassEventHandler(this.UnloadClassEventHandler);
            CorProcess.OnDebuggerError += new DebuggerErrorEventHandler(this.DebuggerErrorEventHandler);
            CorProcess.OnMDANotification += new MDANotificationEventHandler(this.MDAEventHandler);
            CorProcess.OnLogMessage += new LogMessageEventHandler(this.LogMessageEventHandler);
            CorProcess.OnLogSwitch += new LogSwitchEventHandler(this.LogSwitchEventHandler);
            CorProcess.OnCustomNotification += new CustomNotificationEventHandler(this.CustomNotificationEventHandler);
            CorProcess.OnCreateAppDomain += new CorAppDomainEventHandler(this.CreateAppDomainEventHandler);
            CorProcess.OnAppDomainExit += new CorAppDomainEventHandler(this.ExitAppDomainEventHandler);
            CorProcess.OnAssemblyLoad += new CorAssemblyEventHandler(this.LoadAssemblyEventHandler);
            CorProcess.OnAssemblyUnload += new CorAssemblyEventHandler(this.UnloadAssemblyEventHandler);
            CorProcess.OnControlCTrap += new CorProcessEventHandler(this.ControlCTrapEventHandler);
            CorProcess.OnNameChange += new CorThreadEventHandler(this.NameChangeEventHandler);
            CorProcess.OnUpdateModuleSymbols += new UpdateModuleSymbolsEventHandler(this.UpdateModuleSymbolsEventHandler);
            CorProcess.OnFunctionRemapOpportunity += new CorFunctionRemapOpportunityEventHandler(this.OnFunctionRemapOpportunityEventHandler);
            CorProcess.OnFunctionRemapComplete += new CorFunctionRemapCompleteEventHandler(this.OnFunctionRemapCompleteEventHandler);
            CorProcess.OnException2 += new CorException2EventHandler(this.OnException2EventHandler);
            CorProcess.OnExceptionUnwind2 += new CorExceptionUnwind2EventHandler(this.OnExceptionUnwind2EventHandler);
            CorProcess.OnExceptionInCallback += new CorExceptionInCallbackEventHandler(this.ExceptionInCallbackEventHandler);

            m_stopGo.MarkAsStopped();
            m_stopReason = new MDbgInitialContinueNotCalledStopReason(); // set stop reason.
        }

        // Cleans up the process's resources. This may be called on any thread (including callback threads). 
        private void CleanAfterProcessExit()
        {
            lock (this)
            {
                // synchronize with ReallyContinue 
                m_userEntryBreakpoint = null;

                m_threadMgr.Clear();
                m_breakpointMgr.Clear();

                m_moduleMgr.Dispose();

                if (m_corProcess != null)
                {
                    m_corProcess.Dispose();
                    m_corProcess = null;
                    m_isAlive = false;
                }

                m_engine.Processes.DeleteProcess(this);
            }
        }

        private bool InternalHandleRawMode(ManagedCallbackType callbackType, CorEventArgs callbackArgs)
        {
            lock (this)
            {
                switch (RawModeType)
                {
                    case RawMode.None:
                        return false;
                    case RawMode.AlwaysStop:
                        callbackArgs.Continue = false;
                        m_stopReason = new RawModeStopReason(callbackType, callbackArgs);

                        m_stopGo.MarkAsStopped();
                        m_stopCounter = g_stopCounter++;
                        m_stopEvent.Set();
                        return true;
                    case RawMode.NeverStop:
                        return true;
                    default:
                        Debug.Assert(false);
                        return false;
                }
            }
        }

        // True only during the window after a call to DebugActiveProcess,
        // and before the first stop. The primary value of this is:
        // - used to generate a fake AttachComplete event.
        // - minor behavioral swithches (such as avoiding calls that are invalid
        // on attach, such as SetJitCompilerFlags).
        // - skip user-entry breakpoint.
        private bool m_processAttaching = false;

        // This flag is true when there is an underlying native process for this instance of MDbgProcess.
        // The process can be continued only when this flag is true.
        // This flag goes through the following transitions:
        // 1)  Initialized to false on creation.
        // 2)  Set to true when the underlying native process is created.  
        // 3)  Set to false when we detach or when we get the ExitProcess notification.
        private bool m_isAlive = false;

        // The PDB marks the "user entry" method. This is the first method
        // that the user expects to execute (eg, "Main"). Debuggers can set
        // a breakpoint on this method and run to it to skip over code that
        // runs before it (such as startup code, cctors, etc).
        private MDbgBreakpoint m_userEntryBreakpoint;
        private bool m_userEntryBreakpointEnabled = true;

        // May be null.
        // Back-pointer to ICorDebug root object, which is used for CreateProcess,
        // DebugActiveProcess API calls.
        private CorDebugger m_corDebugger;

        // The underying CorProcess object that wraps the ICorDebugProcess
        // exposed by the CLR Debugging services. This is the critical object
        // that we're ultimately wrapping. 
        private CorProcess m_corProcess;

        private MDbgThreadCollection m_threadMgr;

        // Process tracks a list of breakpoints. This lets us try to bind breakpoints on module/class loads.
        private MDbgBreakpointCollection m_breakpointMgr;

        private MDbgModuleCollection m_moduleMgr;
        private MDbgAppDomainCollection m_appDomainMgr;
        private MDbgDebuggerVarCollection m_debuggerVarMgr;

        internal MDbgEngine m_engine;

        // Logical process number of this MDbgProcess instance within the
        // MdbgEngine.
        private int m_number;


        private string m_name;


        private ManualResetEvent m_stopEvent = new ManualResetEvent(false); // this will get signalled whenever we get stopped.

        // An object describing why we stopped. 
        private Object m_stopReason = null;

        // Cached version of g_stopCounter. 
        private int m_stopCounter;

        // Monotonically increasing counter of number of times we've stopped.
        // This is a dispenser for m_stopCounter values. 
        private static int g_stopCounter = 0;

        private CorStepper m_activeStepper = null;
        private DebugModeFlag m_debugMode = DebugModeFlag.Default;

        private string m_symPath = null; // symbol path for current process.

        private DumpReader m_dumpReader = null;

        #region ManagedDebugEventHandlers

        private void BreakpointEventHandler(Object sender, CorBreakpointEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::Breakpoint");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnBreakpoint, e))
                    return;

                bool fHandled = false;

                // custom breakpoint handling. All normal MDbg shell breakpoints (including our user breakpoint)
                // register their own handlers here, so this is the very common case.
                if (customBreakpoints != null
                    && customBreakpoints.Contains(e.Breakpoint))
                {
                    using (MDbgProcessStopController psc = new MDbgProcessStopController(this, e, false))
                    {
                        CustomBreakpointEventHandler handler = (customBreakpoints[e.Breakpoint] as CustomBreakpointEventHandler);

                        // Invoke custom callback handler. This may stop the shell.                    
                        handler(this, new CustomBreakpointEventArgs(psc, e));
                    }
                    fHandled = true;
                }

                if (HandleCustomPostCallback(ManagedCallbackType.OnBreakpoint, e))
                {
                    return;
                }

                if (fHandled)
                {
                    return;         // this was custom breakpoint, no additional action necessary.
                }

                // We have an unknown breakpoint that no handler was registered for. This should be a very
                // uncommon case and indicate some bug in MDbg or an extension.
                e.Continue = false;
                InternalSignalRuntimeIsStopped(e.Thread, "Unexpected raw breakpoint hit");
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }


        private void StepCompleteEventHandler(Object sender, CorStepCompleteEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::StepComplete");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnStepComplete, e))
                    return;

                // custom stepper handling
                if (customSteppers != null
                    && customSteppers.Contains(e.Stepper))
                {
                    using (MDbgProcessStopController psc = new MDbgProcessStopController(this, e, false))
                    {
                        CustomStepperEventHandler handler = (customSteppers[e.Stepper] as CustomStepperEventHandler);
                        customSteppers.Remove(e.Stepper);
                        handler(this, new CustomStepCompleteEventArgs(psc, e));
                    }

                    return;         // this was custom stepper, no additional action necessary.
                }

                // we need to deliver step complete for cordbg skin, so that we can print
                // enhanced diagnostics.
                if (HandleCustomPostCallback(ManagedCallbackType.OnStepComplete, e))
                    return;

                // we will stop only if this callback is from our own stepper.
                if (e.Stepper == m_activeStepper)
                {
                    m_activeStepper = null;
                    e.Continue = false;
                    InternalSignalRuntimeIsStopped(e.Thread, new StepCompleteStopReason(e.Stepper, e.StepReason));
                }
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        private void BreakEventHandler(Object sender, CorThreadEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::Break");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnBreak, e))
                    return;

                if (HandleCustomPostCallback(ManagedCallbackType.OnBreak, e))
                    return;

                e.Continue = false;
                InternalSignalRuntimeIsStopped(e.Thread, new UserBreakStopReason());
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }


        private void ExceptionEventHandler(Object sender, CorExceptionEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::Exception");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnException, e))
                    return;

                // This callback is deprecated by more recent Exception2 callback that
                // contains all new functionality.

                // See more info in Exception2 callback

                if (HandleCustomPostCallback(ManagedCallbackType.OnException, e))
                    return;
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        private void EvalCompleteEventHandler(Object sender, CorEvalEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::EvalComplete");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnEvalComplete, e))
                    return;

                // custom eval handling
                if (customEvals != null
                    && customEvals.Contains(e.Eval))
                {
                    using (MDbgProcessStopController psc = new MDbgProcessStopController(this, e, false))
                    {
                        CustomEvalEventHandler handler = (customEvals[e.Eval] as CustomEvalEventHandler);
                        customEvals.Remove(e.Eval);
                        handler(this, new CustomEvalEventArgs(psc, e, CustomEvalEventArgs.EvalCallbackType.EvalComplete));
                    }
                    return;         // this was custom eval, no additional action necessary.
                }

                e.Continue = false;
                InternalSignalRuntimeIsStopped(e.Thread, new EvalCompleteStopReason(e.Eval));
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        private void EvalExceptionEventHandler(Object sender, CorEvalEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::EvalException");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnEvalException, e))
                    return;

                // custom eval handling
                if (customEvals != null
                    && customEvals.Contains(e.Eval))
                {
                    using (MDbgProcessStopController psc = new MDbgProcessStopController(this, e, false))
                    {
                        CustomEvalEventHandler handler = (customEvals[e.Eval] as CustomEvalEventHandler);
                        customEvals.Remove(e.Eval);
                        handler(this, new CustomEvalEventArgs(psc, e, CustomEvalEventArgs.EvalCallbackType.EvalException));
                    }
                    return;         // this was custom eval, no additional action necessary.
                }

                e.Continue = false;
                InternalSignalRuntimeIsStopped(e.Thread, new EvalExceptionStopReason(e.Eval));
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }



        private void CreateProcessEventHandler(Object sender, CorProcessEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::CreateProcess");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnCreateProcess, e))
                    return;

                Debug.Assert(m_corProcess == e.Process);

                if (!m_processAttaching
                    && (m_debugMode != DebugModeFlag.Default)
                    && (m_debugMode != DebugModeFlag.Enc)       // currently we cannot force ignoring native images
                    // as would be desirable for ENC.
                    )
                {
                    CorDebugJITCompilerFlags flags = MapDebugModeToJITCompilerFlags(m_debugMode);
                    Trace.WriteLine("Setting Desired NGEN compiler flags:" + flags);

                    m_corProcess.DesiredNGENCompilerFlags = flags;
                }

                if (HandleCustomPostCallback(ManagedCallbackType.OnCreateProcess, e))
                {
                    return;
                }
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        private void ExitProcessEventHandler(Object sender, CorProcessEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::ExitProcess");
            BeginManagedDebugEvent();
            try
            {
                CleanAfterProcessExit();
                if (InternalHandleRawMode(ManagedCallbackType.OnProcessExit, e))
                    return;

                if (HandleCustomPostCallback(ManagedCallbackType.OnProcessExit, e))
                    return;

                e.Continue = false;
                InternalSignalRuntimeIsStopped(null, new ProcessExitedStopReason());
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        private void CreateThreadEventHandler(Object sender, CorThreadEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::CreateThread");
            BeginManagedDebugEvent();
            try
            {
                m_threadMgr.Register(e.Thread);

                if (InternalHandleRawMode(ManagedCallbackType.OnCreateThread, e))
                    return;

                if (m_engine.Options.StopOnNewThread)
                {
                    e.Continue = false;
                    InternalSignalRuntimeIsStopped(e.Thread,
                                                   new ThreadCreatedStopReason(Threads.GetThreadFromThreadId(e.Thread.Id))
                                                   );
                    return;
                }

                if (HandleCustomPostCallback(ManagedCallbackType.OnCreateThread, e))
                    return;

                if (m_processAttaching)
                {
                    // ICorDebug has "fake" debug events on attach. However, it does not have an "AttachComplete" to
                    // let us know when the attach is done and we're now getting "real" debug event.
                    // So MDbg simulates an "attach complete" event, which will come after all the CreateThread events have
                    // been dispatched. If multiple CreateThreads come in a single callback queue, then drain the entire
                    // queue. In other words, don't send the AttachComplete until after the queue has been drained.
                    if (!this.CorProcess.HasQueuedCallbacks(null))
                    {
                        if (!m_threadMgr.HaveActive)
                        {
                            m_threadMgr.SetActiveThread(e.Thread);
                        }
                        InternalSignalRuntimeIsStopped(e.Thread, new AttachCompleteStopReason());
                        e.Continue = false;
                    }
                }
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        private void ExitThreadEventHandler(Object sender, CorThreadEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::ExitThread");
            BeginManagedDebugEvent();
            try
            {
                m_threadMgr.UnRegister(e.Thread);

                if (InternalHandleRawMode(ManagedCallbackType.OnThreadExit, e))
                    return;

                if (HandleCustomPostCallback(ManagedCallbackType.OnThreadExit, e))
                    return;
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        private void LoadModuleEventHandler(Object sender, CorModuleEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::LoadModule(" + e.Module.Name + ")");
            BeginManagedDebugEvent();
            try
            {
                MDbgModule m = m_moduleMgr.Register(e.Module);


                if (!m_processAttaching
                        && (m_debugMode != DebugModeFlag.Default))
                {

                    // translate DebugModeFlags to JITCompilerFlags
                    CorDebugJITCompilerFlags jcf = MapDebugModeToJITCompilerFlags(m_debugMode);

                    Trace.WriteLine("Setting module jit compiler flags:" + jcf.ToString());

                    try
                    {
                        e.Module.JITCompilerFlags = jcf;

                        // Flags may succeed but not set all bits, so requery.
                        CorDebugJITCompilerFlags jcfActual = e.Module.JITCompilerFlags;
                        if (jcf != jcfActual)
                        {
                            Trace.WriteLine("Couldn't set all flags. Actual flags:" + jcfActual.ToString());
                        }

                    }
                    catch (COMException ex)
                    {
                        // we'll ignore the error if we cannot set the jit flags
                        Trace.WriteLine(string.Format("Failed to set flags with hr=0x{0:x}", ex.ErrorCode));
                    }
                }

                // let's try to bind all unboud breakpoints (maybe the types got loaded this time)
                // Note that for dynamic and in-memory modules we won't have symbols until the 
                // first UpdateModuleSymbols or LoadClass callback, so this can't bind any source-level
                // breakpoints.  In fact, this is probably useless for dynamic modules since they don't have
                // any code in them until the first LoadClass event.
                m_breakpointMgr.BindBreakpoints(m);

                if (InternalHandleRawMode(ManagedCallbackType.OnModuleLoad, e))
                    return;


                // Symbols track a user entry method.
                // We set a breakpoint at the user entry method and then just run to that to skip any 
                // compiler-injected non-user code before main.
                if (!m_processAttaching
                    && m_userEntryBreakpointEnabled
                    && m_userEntryBreakpoint == null)
                    SetUserEntryBreakpointInModule(m);

                if (HandleCustomPostCallback(ManagedCallbackType.OnModuleLoad, e))
                    return;

                if (m_engine.Options.StopOnModuleLoad)
                {
                    e.Continue = false;
                    InternalSignalRuntimeIsStopped(null, new ModuleLoadedStopReason(Modules.Lookup(e.Module)));
                }
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        private void UnloadModuleEventHandler(Object sender, CorModuleEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::UnloadModule");
            BeginManagedDebugEvent();
            try
            {
                bool handled = HandleCustomPostCallback(ManagedCallbackType.OnModuleUnload, e);

                MDbgModule module = m_moduleMgr.Lookup(e.Module);
                m_moduleMgr.Unregister(e.Module);

                // unbind breakpoints that were in the module which just got unloaded
                if (module != null)
                {
                    m_breakpointMgr.OnModuleUnloaded(module);
                }

                if (handled)
                    return;

                if (InternalHandleRawMode(ManagedCallbackType.OnModuleUnload, e))
                    return;
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        private void LoadClassEventHandler(Object sender, CorClassEventArgs e)
        {
            CorClass c = e.Class;
            Trace.WriteLine(string.Format("ManagedCallback::LoadClass:[{0}],0x{1:x}", c.Module.Name, c.Token));
            BeginManagedDebugEvent();
            try
            {
                CorModule corModule = e.Class.Module;

                if (corModule.IsDynamic)
                {
                    // We receive LoadClass callbacks always for dynamic modules.
                    // If the module is dynamic, then we have a newly defined class.
                    // In this case we should flush our symbols cache and bind any 
                    // breakpoints on this module (which will attempt to eagerly
                    // load symbols if supported)
                    MDbgModule m = m_moduleMgr.Lookup(corModule);
                    Debug.Assert(m != null);
                    m.ReloadSymbols(true);
                    m_breakpointMgr.BindBreakpoints(m);
                }

                if (InternalHandleRawMode(ManagedCallbackType.OnClassLoad, e))
                    return;

                if (HandleCustomPostCallback(ManagedCallbackType.OnClassLoad, e))
                    return;

                if (m_engine.Options.StopOnClassLoad)
                {
                    e.Continue = false;
                    InternalSignalRuntimeIsStopped(null, new ClassLoadedStopReason(c));
                }
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        private void UnloadClassEventHandler(Object sender, CorClassEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::UnloadClass");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnClassUnload, e))
                    return;

                if (HandleCustomPostCallback(ManagedCallbackType.OnClassUnload, e))
                    return;

                if (m_engine.Options.StopOnClassLoad)
                {
                    e.Continue = false;
                    InternalSignalRuntimeIsStopped(null, new ClassUnloadedStopReason());
                }
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        private void DebuggerErrorEventHandler(Object sender, CorDebuggerErrorEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::DebuggerError");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnDebuggerError, e))
                    return;

                e.Continue = false;
                InternalSignalRuntimeIsStopped(null, new DebuggerErrorStopReason(e.HResult));
                Debug.Assert(false, "Critical failures -- received DebuggerError callback.");
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        private void MDAEventHandler(Object sender, CorMDAEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::MDA(" + e.MDA.Name + ")");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnMDANotification, e))
                    return;

                if (HandleCustomPostCallback(ManagedCallbackType.OnMDANotification, e))
                    return;

                if (m_engine.Options.StopOnLogMessage)
                {
                    e.Continue = false;
                    InternalSignalRuntimeIsStopped(e.Thread, new MDANotificationStopReason(e.MDA));
                }
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        // Log Messages are not dispatched unless ICorDebugProcess::EnableLogMessages is called.
        private void LogMessageEventHandler(Object sender, CorLogMessageEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::LogMessage(" + e.LogSwitchName + ", " + e.Message + ")");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnLogMessage, e))
                    return;

                if (HandleCustomPostCallback(ManagedCallbackType.OnLogMessage, e))
                    return;

                if (m_engine.Options.StopOnLogMessage)
                {
                    e.Continue = false;
                    InternalSignalRuntimeIsStopped(e.Thread, new LogMessageStopReason(e.LogSwitchName, e.Message));
                }
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        private void LogSwitchEventHandler(Object sender, CorLogSwitchEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::LogSwitch");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnLogSwitch, e))
                    return;
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        /// <summary>
        /// Handler for CustomNotification events
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event arguments (thread and appdomain</param>

        private void CustomNotificationEventHandler(Object sender, CorCustomNotificationEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::CustomNotification");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnCustomNotification, e))
                    return;

                if (HandleCustomPostCallback(ManagedCallbackType.OnCustomNotification, e))
                    return;
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }


        private void CreateAppDomainEventHandler(Object sender, CorAppDomainEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::CreateAppDomain");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnCreateAppDomain, e))
                    return;

                e.AppDomain.Attach();
                AppDomains.Register(e.AppDomain);

                if (HandleCustomPostCallback(ManagedCallbackType.OnCreateAppDomain, e))
                    return;
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        private void ExitAppDomainEventHandler(Object sender, CorAppDomainEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::ExitAppDomain");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnAppDomainExit, e))
                    return;

                AppDomains.Unregister(e.AppDomain);

                if (HandleCustomPostCallback(ManagedCallbackType.OnAppDomainExit, e))
                    return;
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        private void LoadAssemblyEventHandler(Object sender, CorAssemblyEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::LoadAssembly");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnAssemblyLoad, e))
                    return;

                if (HandleCustomPostCallback(ManagedCallbackType.OnAssemblyLoad, e))
                    return;

                if (m_engine.Options.StopOnAssemblyLoad)
                {
                    e.Continue = false;
                    InternalSignalRuntimeIsStopped(null, new AssemblyLoadedStopReason(e.Assembly));
                }
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        private void UnloadAssemblyEventHandler(Object sender, CorAssemblyEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::UnloadAssembly");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnAssemblyUnload, e))
                    return;

                if (HandleCustomPostCallback(ManagedCallbackType.OnAssemblyUnload, e))
                    return;

                if (m_engine.Options.StopOnAssemblyUnload)
                {
                    e.Continue = false;
                    InternalSignalRuntimeIsStopped(null, new AssemblyUnloadedStopReason());
                }
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        private void ControlCTrapEventHandler(Object sender, CorProcessEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::ControlCTrap");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnControlCTrap, e))
                    return;

                e.Continue = false;
                InternalSignalRuntimeIsStopped(null, new ControlCTrappedStopReason());
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        private void NameChangeEventHandler(Object sender, CorThreadEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::NameChange");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnNameChange, e))
                    return;
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }


        private void UpdateModuleSymbolsEventHandler(Object sender, CorUpdateModuleSymbolsEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::UpdateModuleSymbols");
            BeginManagedDebugEvent();
            try
            {
                // If the CLR supports ICorDebugModule3::CreateReaderForInMemorySymbols and this is a dynamic
                // module, then this callback is useless - we already retrieved the symbols.  In most cases
                // (eg. when not specifically being compatible with the Mix07 VS tools) the CLR won't even
                // dispatch this callback to us when it supports this new API.  
                if (e.Module.SupportsCreateReaderForInMemorySymbols && e.Module.IsDynamic)
                {
                    Trace.WriteLine("ManagedCallback::UpdateModuleSymbols - Ignored");
                    return;
                }


                MDbgModule m = Modules.Lookup(e.Module);

                Debug.Assert(m != null);                        // all modules should always be registered
                bool ok = m.UpdateSymbols(e.Stream);
                Debug.Assert(ok, "UpdateSymbolStore failed");

                // Anytime we udpate symbols, we need to check for if we can bind any source-level breakpoints.            
                //m_breakpointMgr.BindBreakpoints(m);
                foreach (MDbgBreakpoint b in m_breakpointMgr)
                {
                    b.BindToModule(m);
                }

                // if IsInMemory holds this is probably a module in an assembly
                // loaded by byte-array, and a user entry breakpoint might not have
                // been found at Load time; so check again for one.
                if (e.Module.IsInMemory && !m_processAttaching &&
                    m_userEntryBreakpointEnabled && m_userEntryBreakpoint == null)
                    SetUserEntryBreakpointInModule(m);

                if (InternalHandleRawMode(ManagedCallbackType.OnUpdateModuleSymbols, e))
                    return;

                if (HandleCustomPostCallback(ManagedCallbackType.OnUpdateModuleSymbols, e))
                    return;
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        private void OnFunctionRemapOpportunityEventHandler(Object sender, CorFunctionRemapOpportunityEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::OnFunctionRemapOpportunity");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnFunctionRemapOpportunity, e))
                    return;
                e.Continue = false;
                InternalSignalRuntimeIsStopped(e.Thread, new RemapOpportunityReachedStopReason(e.AppDomain,
                                                                                              e.Thread,
                                                                                              e.OldFunction,
                                                                                              e.NewFunction,
                                                                                              e.OldILOffset));
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        private void OnFunctionRemapCompleteEventHandler(Object sender, CorFunctionRemapCompleteEventArgs e)
        {
            Trace.WriteLine("ManagedCallback::OnFunctionRemapComplete");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnFunctionRemapComplete, e))
                    return;
                e.Continue = false;
                InternalSignalRuntimeIsStopped(e.Thread, new FunctionRemapCompleteStopReason(e.AppDomain,
                                                                                              e.Thread,
                                                                                              e.Function));
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        private void OnException2EventHandler(Object sender, CorException2EventArgs e)
        {
            Trace.WriteLine("ManagedCallback::OnException2");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnException2, e))
                    return;

                if (e.EventType == CorDebugExceptionCallbackType.DEBUG_EXCEPTION_UNHANDLED &&
                    m_engine.Options.StopOnUnhandledException)
                {
                    e.Continue = false;
                    // just for historical reasons we are stopping with different StopReason
                    InternalSignalRuntimeIsStopped(e.Thread, new UnhandledExceptionThrownStopReason(e.AppDomain, e.Thread, e.Frame,
                                                                                                   e.Offset, e.EventType, e.Flags));
                    return;
                }

                if (e.EventType == CorDebugExceptionCallbackType.DEBUG_EXCEPTION_FIRST_CHANCE &&
                    m_engine.Options.StopOnException)
                {
                    e.Continue = false;
                    InternalSignalRuntimeIsStopped(e.Thread, new ExceptionThrownStopReason(e.AppDomain, e.Thread, e.Frame,
                                                                                          e.Offset, e.EventType, e.Flags));
                    return;
                }

                if (HandleCustomPostCallback(ManagedCallbackType.OnException2, e))
                    return;

                // if user has requested stops on enhanced exception,
                // we should always stop
                if (m_engine.Options.StopOnExceptionEnhanced)
                {
                    e.Continue = false;
                    InternalSignalRuntimeIsStopped(e.Thread, new ExceptionThrownStopReason(e.AppDomain, e.Thread, e.Frame,
                                                                                           e.Offset, e.EventType, e.Flags));
                    return;
                }
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        private void OnExceptionUnwind2EventHandler(Object sender, CorExceptionUnwind2EventArgs e)
        {
            Trace.WriteLine("ManagedCallback::OnExceptionUnwind2");
            BeginManagedDebugEvent();
            try
            {
                if (InternalHandleRawMode(ManagedCallbackType.OnExceptionUnwind2, e))
                    return;

                if (HandleCustomPostCallback(ManagedCallbackType.OnExceptionUnwind2, e))
                    return;

                if (m_engine.Options.StopOnExceptionEnhanced)
                {
                    e.Continue = false;
                    InternalSignalRuntimeIsStopped(e.Thread, new ExceptionUnwindStopReason(e.AppDomain, e.Thread,
                                                                                           e.EventType, e.Flags));
                }
            }
            finally
            {
                EndManagedDebugEvent(e);
            }
        }

        #endregion


        /// <summary>
        /// Called just before the process continues
        /// </summary>
        private void OnPreContinue()
        {
            // Once we continue, all frames become invalid.
            // Invalidating the stack once we stop the shell is insufficient
            // in case somebody tries to run a callstack inside a callback (before the shell is stopped)
            this.Threads.InvalidateAllStacks();
        }

        /// <summary>
        /// This must be called at the beginning of every debug event handler
        /// </summary>
        private void BeginManagedDebugEvent()
        {
            // nothing to do right now, but we might want to mark that we are stopped
            // in the future, do tracing, or run some other code here
        }

        /// <summary>
        /// This must be called at the end of every debug event handler
        /// </summary>
        /// <param name="args"></param>
        private void EndManagedDebugEvent(CorEventArgs args)
        {
            // if we are exiting an event with the continue flag set then
            // the process will be continuing. Otherwise we will trigger this
            // later in ReallyContinueProcess
            if (args.Continue)
            {
                OnPreContinue();
            }
        }

        // returns true if the CustomPostCallback requested stop.
        private bool HandleCustomPostCallback(ManagedCallbackType callbackType, CorEventArgs callbackArgs)
        {
            CorNativeStopEventArgs nativeStopEventArgs = (callbackArgs as CorNativeStopEventArgs);
            
            // If the event is in-band, then we have to lock the MDbgProcess to make sure 
            // the callback is synchronized with other threads.
            lock (this)
            {
                return HandleCustomPostCallbackWorker(callbackType, callbackArgs);
            }
        }

        // helper for HandleCustomPostCallback()
        private bool HandleCustomPostCallbackWorker(ManagedCallbackType callbackType, CorEventArgs callbackArgs)
        {
            bool stopRequested = false;
            bool needAsyncStopCall = false;

            using (MDbgProcessStopController psc = new MDbgProcessStopController(this, callbackArgs, needAsyncStopCall))
            {
                if (PostDebugEvent != null)
                {
                    PostDebugEvent(this, new CustomPostCallbackEventArgs(psc, callbackType, callbackArgs));
                    stopRequested = psc.CustomStopRequested;
                }
            } // end using

            return stopRequested;
        }

        // helper function
        private static CorDebugJITCompilerFlags MapDebugModeToJITCompilerFlags(DebugModeFlag debugMode)
        {
            CorDebugJITCompilerFlags jcf;
            switch (debugMode)
            {
                case DebugModeFlag.Optimized:
                    jcf = CorDebugJITCompilerFlags.CORDEBUG_JIT_DEFAULT; // DEFAULT really means force optimized.
                    break;
                case DebugModeFlag.Debug:
                    jcf = CorDebugJITCompilerFlags.CORDEBUG_JIT_DISABLE_OPTIMIZATION;
                    break;
                case DebugModeFlag.Enc:
                    jcf = CorDebugJITCompilerFlags.CORDEBUG_JIT_ENABLE_ENC;
                    break;
                default:
                    Debug.Assert(false, "Invalid debugMode");
                    // we don't have mapping from default to "default",
                    // therefore we'll use DISABLE_OPTIMIZATION.
                    jcf = CorDebugJITCompilerFlags.CORDEBUG_JIT_DISABLE_OPTIMIZATION;
                    break;
            }
            return jcf;
        }

        private bool SetUserEntryBreakpointInModule(MDbgModule m)
        {
            bool ok = true;
            if (m.SymReader != null)
            {
                int st = 0;
                st = m.SymReader.UserEntryPoint.GetToken();
                if (st != 0)
                {
                    MDbgFunction mfunc = m.GetFunction(st);
                    m_userEntryBreakpoint = new UserEntryBreakpoint(this, mfunc);
                    ok = m_userEntryBreakpoint.BindToModule(m);

                    // Issue a warning if we failed to set the user entry breakpoint.
                    if (!ok)
                    {
                        Trace.WriteLine(string.Format("Failed to set user entry breakpoint at {0}", mfunc.FullName));
                    }

                    // now we cannot call BindBreakpoints again otherwise userEntrBreakpoint will be bound
                    // twice
                }

                // We explicitly don't set JMC. An extension can hook up to this module and set JMC policy if it wants.


            }
            return ok;
        }


        // Update process's state that to let them know we've hit this we've hit.
        internal void OnUserEntryBreakpointHit()
        {
            m_userEntryBreakpoint.Delete();
            m_userEntryBreakpointEnabled = false;
            m_userEntryBreakpoint = null;
        }

        // Class for special breakpoint for user-entry.
        // This is set by the shell.
        class UserEntryBreakpoint : MDbgFunctionBreakpoint
        {
            MDbgProcess m_process;
            internal UserEntryBreakpoint(MDbgProcess p, MDbgFunction mfunc)
                : base(null, new BreakpointFunctionToken(mfunc, 0))
            {
                m_process = p;
            }
            public override object OnHitHandler(CustomBreakpointEventArgs e)
            {
                m_process.OnUserEntryBreakpointHit();
                // Chain to base handlers.
                return base.OnHitHandler(e);
            }
            public override string ToString()
            {
                return base.ToString() + "(user entry breakpoint)";
            }
        }


        private void ExceptionInCallbackEventHandler(Object sender, CorExceptionInCallbackEventArgs e)
        {
            Trace.WriteLine("CorProcess::ExceptionInCallback");

            e.Continue = false;
            m_stopReason = new MDbgErrorStopReason(e.ExceptionThrown);
            m_stopGo.MarkAsStopped();
            m_stopCounter = g_stopCounter++;
            m_stopEvent.Set();
        }

        /// <summary>
        /// Controls the policy for locating CLR version specific debugging
        /// components such as mscordacwks.dll and mscordbi.dll
        /// </summary>
        public ICLRDebuggingLibraryProvider LibraryProvider
        {
            get { return m_libraryProvider; }
            set { m_libraryProvider = value; }
        }

        private ICLRDebuggingLibraryProvider m_libraryProvider = new LibraryProvider();
    }

    /// <summary>
    /// Custom Event Arguments class.
    /// </summary>
    public class CustomEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of the CustomEventArgs object.
        /// </summary>
        /// <param name="processController">Which IMDbgProcessController to store in the Object.</param>
        public CustomEventArgs(IMDbgProcessController processController)
        {
            this.processController = processController;
        }

        /// <summary>
        /// Gets the stored IMDbgProcessController.
        /// </summary>
        /// <value>The IMDbgProcessController.</value>
        public IMDbgProcessController Controller
        {
            get
            {
                return processController;
            }
        }

        private IMDbgProcessController processController;
    }

    /// <summary>
    /// CustomStepCompleteEventArgs class.
    /// </summary>
    public class CustomStepCompleteEventArgs : CustomEventArgs
    {
        /// <summary>
        /// Creates a new instance of the CustomStepCompleteEventArgs class.
        /// </summary>
        /// <param name="processController">Which IMDbgProcessController to store in the Object.</param>
        /// <param name="callbackArgs">Which CorStepCompleteEventArgs to encapsulate in this wrapper.</param>
        public CustomStepCompleteEventArgs(IMDbgProcessController processController,
                                           CorStepCompleteEventArgs callbackArgs)
            : base(processController)
        {
            this.callbackArgs = callbackArgs;
        }

        /// <summary>
        /// Gets the CorStepCompleteEventArgs.
        /// </summary>
        /// <value>The CorStepCompleteEventArgs.</value>
        public CorStepCompleteEventArgs StepCompleteCallbackArgs
        {
            get
            {
                return callbackArgs;
            }
        }

        private CorStepCompleteEventArgs callbackArgs;
    }

    /// <summary>
    /// Delegate for creating Custom Steppers.
    /// </summary>
    /// <param name="sender">Object that sent the event.</param>
    /// <param name="e">CustomStepCompleteEventArgs for the event.</param>
    public delegate void CustomStepperEventHandler(Object sender, CustomStepCompleteEventArgs e);

    /// <summary>
    /// CustomBreakpointEventArgs class.
    /// </summary>
    public class CustomBreakpointEventArgs : CustomEventArgs
    {
        /// <summary>
        /// Creates a new instance of the CustomBreakpointEventArgs class.
        /// </summary>
        /// <param name="processController"></param>
        /// <param name="callbackArgs"></param>
        public CustomBreakpointEventArgs(IMDbgProcessController processController,
                                  CorBreakpointEventArgs callbackArgs)
            : base(processController)
        {
            this.callbackArgs = callbackArgs;
        }

        /// <summary>
        /// Gets the CorBreakpointEventArgs.
        /// </summary>
        /// <value>The ConBreakpointEventArgs.</value>
        public CorBreakpointEventArgs BreakpointHitCallbackArgs
        {
            get
            {
                return callbackArgs;
            }
        }

        private CorBreakpointEventArgs callbackArgs;
    }

    /// <summary>
    /// Delegate for creating Custom Breakpoints.
    /// </summary>
    /// <param name="sender">Object that tent the event.</param>
    /// <param name="e">CustomBreakpointEventArgs for the event.</param>
    public delegate void CustomBreakpointEventHandler(object sender, CustomBreakpointEventArgs e);

    /// <summary>
    /// CustomEvalEventArgs class.
    /// </summary>
    public class CustomEvalEventArgs : CustomEventArgs
    {
        /// <summary>
        /// Creates a new instance of the CustomEvalEventArgs class.
        /// </summary>
        /// <param name="processController">Which IMDbgProcessController to store in the Object.</param>
        /// <param name="callbackArgs">Which CorEvalEventArgs to encapsulate in this wrapper.</param>
        /// <param name="callbackType">What Callback type this was.</param>
        public CustomEvalEventArgs(IMDbgProcessController processController,
                            CorEvalEventArgs callbackArgs,
                            EvalCallbackType callbackType)
            : base(processController)
        {
            this.callbackArgs = callbackArgs;
            this.callbackType = callbackType;
        }

        /// <summary>
        /// Gets the Callback Type
        /// </summary>
        /// <value>The Callback Type.</value>
        public EvalCallbackType CallbackType
        {
            get
            {
                return callbackType;
            }
        }

        /// <summary>
        /// Callback Type Enumeration.
        /// </summary>
        public enum EvalCallbackType
        {
            /// <summary>
            /// Eval completed.
            /// </summary>
            EvalComplete,
            /// <summary>
            /// Eval completed with exception.
            /// </summary>
            EvalException,
        }

        /// <summary>
        /// Gets the CorEvalEventArgs.
        /// </summary>
        /// <value>The CorEvalEventArgs.</value>
        public CorEvalEventArgs EvalCallbackArgs
        {
            get
            {
                return callbackArgs;
            }
        }

        private EvalCallbackType callbackType;
        private CorEvalEventArgs callbackArgs;
    }

    /// <summary>
    /// Delegate for creating Custom Evals.
    /// </summary>
    /// <param name="sender">Object that sent the event.</param>
    /// <param name="e">CustomEvalEventArgs for the event.</param>
    public delegate void CustomEvalEventHandler(object sender, CustomEvalEventArgs e);

    /// <summary>
    /// CustomPostCallbackEventArgs class.
    /// </summary>
    public class CustomPostCallbackEventArgs : CustomEventArgs
    {
        /// <summary>
        /// Creates a new instance of the CustomEvalEventArgs class.
        /// </summary>
        /// <param name="processController">Which IMDbgProcessController to store in the Object.</param>
        /// <param name="callbackArgs">Which CorEvalEventArgs to encapsulate in this wrapper.</param>
        /// <param name="callbackType">What Callback type this was.</param>
        public CustomPostCallbackEventArgs(IMDbgProcessController processController,
                                           ManagedCallbackType callbackType,
                                           Object callbackArgs)
            : base(processController)
        {
            this.callbackType = callbackType;
            this.callbackArgs = callbackArgs;
        }

        /// <summary>
        /// Gets the Callback Type
        /// </summary>
        /// <value>The Callback Type.</value>
        public ManagedCallbackType CallbackType
        {
            get
            {
                return callbackType;
            }
        }

        /// <summary>
        /// Gets the CorEvalEventArgs. 
        /// </summary>
        /// <value>The CorEventArgs.</value>
        /// <remarks> Multiple callback types may share the same Callback event args. Use the
        /// CallbackType to determine the exact type of debug event here instead of relying
        /// on the typeof the CallbackArgs object.
        /// </remarks>
        public Object CallbackArgs
        {
            get
            {
                return callbackArgs;
            }
        }

        private ManagedCallbackType callbackType;
        private Object callbackArgs;
    }

    /// <summary>
    /// Delegate for custom handling of any debugger callbacks.
    /// </summary>
    /// <param name="sender">Object that sent the event.</param>
    /// <param name="e">CustomPostCallbackEventArgs for the event.</param>
    public delegate void PostCallbackEventHandler(object sender, CustomPostCallbackEventArgs e);

    /// <summary>
    /// Interface for MDbg Process Controlling.
    /// </summary>
    public interface IMDbgProcessController
    {
        /// <summary>
        /// Stops the Process on a given thread for a given stop reason.
        /// </summary>
        /// <param name="activeThread">Which thread to stop.</param>
        /// <param name="stopReason">What reason to stop.</param>
        void Stop(CorThread activeThread, object stopReason);
    }


}
