using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

using Cosmos.Debug.Common;
using Cosmos.VS.DebugEngine.AD7.Definitions;
using Cosmos.VS.DebugEngine.Engine.Impl;

namespace Cosmos.VS.DebugEngine.AD7.Impl
{
    // AD7Engine is the primary entrypoint object for the the debugger.
    //
    // It implements:
    //
    // IDebugEngine2: This interface represents a debug engine (DE). It is used to manage various aspects of a debugging session,
    // from creating breakpoints to setting and clearing exceptions.
    //
    // IDebugEngineLaunch2: Used by a debug engine (DE) to launch and terminate programs.
    //
    // IDebugProgram3: This interface represents a program that is running in a process. Since this engine only debugs one process at a time and each
    // process only contains one program, it is implemented on the engine.
    //
    // IDebugEngineProgram2: This interface provides simultanious debugging of multiple threads in a debuggee.

    [ComVisible(true)]
    [Guid(Guids.guidDebugEngineString)]
    public class AD7Engine : IDebugEngine2, IDebugEngineLaunch2, IDebugProgram3, IDebugEngineProgram2
    {
        internal IDebugProgram2 mProgram;
        // We only support one process, so we just keep a ref to it and save a lot of accounting.
        internal AD7Process mProcess;
        // A unique identifier for the program being debugged.
        Guid mProgramID;
        public static readonly Guid EngineID = new Guid("fa1da3a6-66ff-4c65-b077-e65f7164ef83");
        internal AD7Module mModule;
        internal AD7Thread mThread;
        private AD7ProgramNode mProgNode;
        public IList<IDebugBoundBreakpoint2> Breakpoints = null;

        // This object facilitates calling from this thread into the worker thread of the engine. This is necessary because the Win32 debugging
        // api requires thread affinity to several operations.
        // This object manages breakpoints in the sample engine.
        public BreakpointManager BPMgr { get; }

        public AD7Engine()
        {
            BPMgr = new BreakpointManager(this);
        }

        internal EngineCallback Callback { get; private set; }

        #region Startup Methods
        // During startup these methods are called in this order:
        // -LaunchSuspended
        // -ResumeProcess
        //   -Attach - Triggered by Attach

        int IDebugEngineLaunch2.LaunchSuspended(string aPszServer, IDebugPort2 aPort, string aDebugInfo
          , string aArgs, string aDir, string aEnv, string aOptions, enum_LAUNCH_FLAGS aLaunchFlags
          , uint aStdInputHandle, uint aStdOutputHandle, uint hStdError, IDebugEventCallback2 aAD7Callback
          , out IDebugProcess2 oProcess)
        {
            // Launches a process by means of the debug engine.
            // Normally, Visual Studio launches a program using the IDebugPortEx2::LaunchSuspended method and then attaches the debugger
            // to the suspended program. However, there are circumstances in which the debug engine may need to launch a program
            // (for example, if the debug engine is part of an interpreter and the program being debugged is an interpreted language),
            // in which case Visual Studio uses the IDebugEngineLaunch2::LaunchSuspended method
            // The IDebugEngineLaunch2::ResumeProcess method is called to start the process after the process has been successfully launched in a suspended state.

            oProcess = null;
            try
            {
                Callback = new EngineCallback(this, aAD7Callback);

                var xDebugInfo = new Dictionary<string, string>();
                DictionaryHelper.LoadFromString(xDebugInfo, aDebugInfo);

                //TODO: In the future we might support command line args for kernel etc
                //string xCmdLine = EngineUtils.BuildCommandLine(exe, args);
                //var processLaunchInfo = new ProcessLaunchInfo(exe, xCmdLine, dir, env, options, launchFlags, hStdInput, hStdOutput, hStdError);

                AD7EngineCreateEvent.Send(this);
                oProcess = mProcess = new AD7Process(xDebugInfo, Callback, this, aPort);
                // We only support one process, so just use its ID for the program ID
                mProgramID = mProcess.ID;
                //AD7ThreadCreateEvent.Send(this, xProcess.Thread);
                mModule = new AD7Module();
                mProgNode = new AD7ProgramNode(mProcess.PhysID);
            }
            catch (NotSupportedException)
            {
                return VSConstants.S_FALSE;
            }
            catch (Exception e)
            {
                return EngineUtils.UnexpectedException(e);
            }
            return VSConstants.S_OK;
        }

        int IDebugEngine2.Attach(IDebugProgram2[] rgpPrograms, IDebugProgramNode2[] rgpProgramNodes, uint aCeltPrograms, IDebugEventCallback2 ad7Callback, enum_ATTACH_REASON dwReason)
        {
            // Attach the debug engine to a program.
            //
            // Attach can either be called to attach to a new process, or to complete an attach
            // to a launched process.
            // So could we simplify and move code from LaunchSuspended to here and maybe even
            // eliminate the debughost? Although I supposed DebugHost has some other uses as well.

            if (aCeltPrograms != 1)
            {
                System.Diagnostics.Debug.Fail("Cosmos Debugger only supports one debug target at a time.");
                throw new InvalidOperationException();
            }

            try
            {
                EngineUtils.RequireOk(rgpPrograms[0].GetProgramId(out mProgramID));

                mProgram = rgpPrograms[0];
                AD7EngineCreateEvent.Send(this);
                AD7ProgramCreateEvent.Send(this);
                AD7ModuleLoadEvent.Send(this, mModule, true);

                // Dummy main thread
                // We dont support threads yet, but the debugger expects threads.
                // So we create a dummy object to represente our only "thread".
                mThread = new AD7Thread(this, mProcess);
                AD7LoadCompleteEvent.Send(this, mThread);
            }
            catch (Exception e)
            {
                return EngineUtils.UnexpectedException(e);
            }
            return VSConstants.S_OK;
        }

        int IDebugEngineLaunch2.ResumeProcess(IDebugProcess2 aProcess)
        {
            // Resume a process launched by IDebugEngineLaunch2.LaunchSuspended
            try
            {
                // Send a program node to the SDM. This will cause the SDM to turn around and call IDebugEngine2.Attach
                // which will complete the hookup with AD7
                if (!(aProcess is AD7Process xProcess))
                {
                    return VSConstants.E_INVALIDARG;
                }

                EngineUtils.RequireOk(aProcess.GetPort(out var xPort));

                var xDefPort = (IDebugDefaultPort2)xPort;
                EngineUtils.RequireOk(xDefPort.GetPortNotify(out var xNotify));

                // This triggers Attach
                EngineUtils.RequireOk(xNotify.AddProgramNode(mProgNode));

                Callback.OnModuleLoad(mModule);
                Callback.OnSymbolSearch(mModule, xProcess.mISO.Replace("iso", "pdb"), enum_MODULE_INFO_FLAGS.MIF_SYMBOLS_LOADED);
                // Important!
                //
                // This call triggers setting of breakpoints that exist before run.
                // So it must be called before we resume the process.
                // If not called VS will call it after our 3 startup events, but thats too late.
                // This line was commented out in earlier Cosmos builds and caused problems with
                // breakpoints and timing.
                Callback.OnThreadStart(mThread);

                // Not sure what this does exactly. It was commented out before
                // but so was a lot of stuff we actually needed. If its uncommented it
                // throws:
                //  "Operation is not valid due to the current state of the object."
                //AD7EntrypointEvent.Send(this);

                // Now finally release our process to go after breakpoints are set
                mProcess.ResumeFromLaunch();
            }
            catch (Exception e)
            {
                return EngineUtils.UnexpectedException(e);
            }
            return VSConstants.S_OK;
        }
        #endregion

        #region Other implemented support methods
        int IDebugEngine2.ContinueFromSynchronousEvent(IDebugEvent2 aEvent)
        {
            // Called by the SDM to indicate that a synchronous debug event, previously sent by the DE to the SDM,
            // was received and processed. The only event the  engine sends in this fashion is Program Destroy.
            // It responds to that event by shutting down the engine.
            //
            // This is used in some cases - I set a BP here and it does get hit sometime during breakpoints
            // being triggered for example.
            try
            {
                if (aEvent is AD7ProgramDestroyEvent)
                {
                    Callback = null;
                    mProgramID = Guid.Empty;
                    mThread = null;
                    mProgNode = null;
                }
                else
                {
                    System.Diagnostics.Debug.Fail("Unknown synchronious event");
                }
            }
            catch (Exception e)
            {
                return EngineUtils.UnexpectedException(e);
            }
            return VSConstants.S_OK;
        }

        int IDebugEngine2.CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, out IDebugPendingBreakpoint2 ppPendingBP)
        {
            // Creates a pending breakpoint in the engine. A pending breakpoint is contains all the information needed to bind a breakpoint to
            // a location in the debuggee.

            ppPendingBP = null;
            try
            {
                BPMgr.CreatePendingBreakpoint(pBPRequest, out ppPendingBP);
            }
            catch (Exception e)
            {
                return EngineUtils.UnexpectedException(e);
            }
            return VSConstants.S_OK;
        }

        int IDebugEngine2.DestroyProgram(IDebugProgram2 pProgram)
        {
            // Informs a DE that the program specified has been atypically terminated and that the DE should
            // clean up all references to the program and send a program destroy event.
            //
            // Tell the SDM that the engine knows that the program is exiting, and that the
            // engine will send a program destroy. We do this because the Win32 debug api will always
            // tell us that the process exited, and otherwise we have a race condition.
            return AD7_HRESULT.E_PROGRAM_DESTROY_PENDING;
        }

        int IDebugEngine2.GetEngineId(out Guid oGuidEngine)
        {
            // Gets the GUID of the DebugEngine.
            oGuidEngine = EngineID;
            return VSConstants.S_OK;
        }

        int IDebugEngineLaunch2.TerminateProcess(IDebugProcess2 aProcess)
        {
            // This function is used to terminate a process that the SampleEngine launched
            // The debugger will call IDebugEngineLaunch2::CanTerminateProcess before calling this method.
            try
            {
                mProcess.Terminate();

                Callback.OnProcessExit(0);
                mProgram = null;
            }
            catch (Exception e)
            {
                return EngineUtils.UnexpectedException(e);
            }
            return VSConstants.S_OK;
        }

        public int Continue(IDebugThread2 pThread)
        {
            // We don't appear to use or support this currently.

            // Continue is called from the SDM when it wants execution to continue in the debugee
            // but have stepping state remain. An example is when a tracepoint is executed,
            // and the debugger does not want to actually enter break mode.

            var xThread = (AD7Thread)pThread;
            //if (AfterBreak) {
            //Callback.OnBreak(xThread);
            //}
            return VSConstants.S_OK;
        }

        int IDebugEngine2.CauseBreak()
        {
            // Requests that all programs being debugged by this DE stop execution the next time one of their threads attempts to run.
            // This is normally called in response to the user clicking on the pause button in the debugger.
            // When the break is complete, an AsyncBreakComplete event will be sent back to the debugger.

            return ((IDebugProgram2)this).CauseBreak();
        }

        public int Detach()
        {
            // Detach is called when debugging is stopped and the process was attached to (as opposed to launched)
            // or when one of the Detach commands are executed in the UI.

            BPMgr.ClearBoundBreakpoints();
            return VSConstants.S_OK;
        }

        public int EnumModules(out IEnumDebugModules2 ppEnum)
        {
            // EnumModules is called by the debugger when it needs to enumerate the modules in the program.

            ppEnum = new AD7ModuleEnum(new[] { mModule });
            return VSConstants.S_OK;
        }

        public int EnumThreads(out IEnumDebugThreads2 ppEnum)
        {
            // EnumThreads is called by the debugger when it needs to enumerate the threads in the program.

            ppEnum = new AD7ThreadEnum(new[] { mThread });
            return VSConstants.S_OK;
        }

        public int GetEngineInfo(out string pbstrEngine, out Guid pguidEngine)
        {
            // Gets the name and identifier of the debug engine (DE) running this program.

            pbstrEngine = "Cosmos Debug Engine";
            pguidEngine = EngineID;

            return VSConstants.S_OK;
        }

        public int GetProgramId(out Guid pguidProgramId)
        {
            // Gets a GUID for this program. A debug engine (DE) must return the program identifier originally passed to the IDebugProgramNodeAttach2::OnAttach
            // or IDebugEngine2::Attach methods. This allows identification of the program across debugger components.

            pguidProgramId = mProgramID;
            return VSConstants.S_OK;
        }

        public int Step(IDebugThread2 pThread, enum_STEPKIND sk, enum_STEPUNIT Step)
        {
            // This method is deprecated. Use the IDebugProcess3::Step method instead.

            mProcess.Step(sk);
            return VSConstants.S_OK;
        }

        public int ExecuteOnThread(IDebugThread2 pThread)
        {
            // ExecuteOnThread is called when the SDM wants execution to continue and have
            // stepping state cleared.

            mProcess.Continue();
            return VSConstants.S_OK;
        }
        #endregion

        #region Unimplemented methods

        // Gets the name of the program.
        // The name returned by this method is always a friendly, user-displayable name that describes the program.
        public int GetName(out string pbstrName)
        {
            // The Sample engine uses default transport and doesn't need to customize the name of the program,
            // so return NULL.
            pbstrName = null;
            return VSConstants.S_OK;
        }

        // This method gets the Edit and Continue (ENC) update for this program. A custom debug engine always returns E_NOTIMPL
        public int GetENCUpdate(out object ppUpdate)
        {
            // The sample engine does not participate in managed edit & continue.
            ppUpdate = null;

            return VSConstants.S_OK;
        }

        // Removes the list of exceptions the IDE has set for a particular run-time architecture or language.
        // We dont support exceptions in the debuggee so this method is not actually implemented.
        int IDebugEngine2.RemoveAllSetExceptions(ref Guid guidType)
        {
            return VSConstants.S_OK;
        }

        // Removes the specified exception so it is no longer handled by the debug engine.
        // The sample engine does not support exceptions in the debuggee so this method is not actually implemented.
        int IDebugEngine2.RemoveSetException(EXCEPTION_INFO[] pException)
        {
            // We stop on all exceptions.
            return VSConstants.S_OK;
        }

        // Specifies how the DE should handle a given exception.
        // We dont support exceptions in the debuggee so this method is not actually implemented.
        int IDebugEngine2.SetException(EXCEPTION_INFO[] pException)
        {
            return VSConstants.S_OK;
        }

        // Sets the locale of the DE.
        // This method is called by the session debug manager (SDM) to propagate the locale settings of the IDE so that
        // strings returned by the DE are properly localized. The sample engine is not localized so this is not implemented.
        int IDebugEngine2.SetLocale(ushort wLangID)
        {
            return VSConstants.S_OK;
        }
        // A metric is a registry value used to change a debug engine's behavior or to advertise supported functionality.
        // This method can forward the call to the appropriate form of the Debugging SDK Helpers function, SetMetric.
        int IDebugEngine2.SetMetric(string pszMetric, object varValue)
        {
            // The sample engine does not need to understand any metric settings.
            return VSConstants.S_OK;
        }

        // Sets the registry root currently in use by the DE. Different installations of Visual Studio can change where their registry information is stored
        // This allows the debugger to tell the engine where that location is.
        int IDebugEngine2.SetRegistryRoot(string pszRegistryRoot)
        {
            // The sample engine does not read settings from the registry.
            return VSConstants.S_OK;
        }

        public string GetAddressDescription(uint ip)
        {
            //    DebuggedModule module = m_debuggedProcess.ResolveAddress(ip);
            return EngineUtils.GetAddressDescription(/*module,*/this, ip);
        }

        // Determines if a debug engine (DE) can detach from the program.
        public int CanDetach()
        {
            // We always support detach
            return VSConstants.S_OK;
        }

        // The debugger calls CauseBreak when the user clicks on the pause button in VS. The debugger should respond by entering
        // breakmode.
        public int CauseBreak() => mProcess.CauseBreak();

        // EnumCodePaths is used for the step-into specific feature -- right click on the current statment and decide which
        // function to step into. This is not something that the SampleEngine supports.
        public int EnumCodePaths(string pszHint, IDebugCodeContext2 pStart, IDebugStackFrame2 pFrame, int fSource, out IEnumCodePaths2 ppEnum, out IDebugCodeContext2 ppSafety)
        {
            ppEnum = null;
            ppSafety = null;
            return VSConstants.E_NOTIMPL;
        }

        // The properties returned by this method are specific to the program. If the program needs to return more than one property,
        // then the IDebugProperty2 object returned by this method is a container of additional properties and calling the
        // IDebugProperty2::EnumChildren method returns a list of all properties.
        // A program may expose any number and type of additional properties that can be described through the IDebugProperty2 interface.
        // An IDE might display the additional program properties through a generic property browser user interface.
        // The sample engine does not support this
        public int GetDebugProperty(out IDebugProperty2 ppProperty)
        {
            throw new NotImplementedException();
        }

        // The debugger calls this when it needs to obtain the IDebugDisassemblyStream2 for a particular code-context.
        // The sample engine does not support dissassembly so it returns E_NOTIMPL
        public int GetDisassemblyStream(enum_DISASSEMBLY_STREAM_SCOPE dwScope, IDebugCodeContext2 pCodeContext, out IDebugDisassemblyStream2 ppDisassemblyStream)
        {
            ppDisassemblyStream = null;
            return VSConstants.E_NOTIMPL;
        }

        // The memory bytes as represented by the IDebugMemoryBytes2 object is for the program's image in memory and not any memory
        // that was allocated when the program was executed.
        public int GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        // Writes a dump to a file.
        public int WriteDump(enum_DUMPTYPE DUMPTYPE, string pszDumpUrl)
        {
            // The sample debugger does not support creating or reading mini-dumps.
            return VSConstants.E_NOTIMPL;
        }

        // Stops all threads running in this program.
        // This method is called when this program is being debugged in a multi-program environment. When a stopping event from some other program
        // is received, this method is called on this program. The implementation of this method should be asynchronous;
        // that is, not all threads should be required to be stopped before this method returns. The implementation of this method may be
        // as simple as calling the IDebugProgram2::CauseBreak method on this program.
        //
        // The sample engine only supports debugging native applications and therefore only has one program per-process
        public int Stop()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        // WatchForExpressionEvaluationOnThread is used to cooperate between two different engines debugging
        // the same process. The sample engine doesn't cooperate with other engines, so it has nothing
        // to do here.
        public int WatchForExpressionEvaluationOnThread(IDebugProgram2 pOriginatingProgram, uint dwTid, uint dwEvalFlags, IDebugEventCallback2 pExprCallback, int fWatch)
        {
            return VSConstants.S_OK;
        }

        // WatchForThreadStep is used to cooperate between two different engines debugging the same process.
        // The sample engine doesn't cooperate with other engines, so it has nothing to do here.
        public int WatchForThreadStep(IDebugProgram2 pOriginatingProgram, uint dwTid, int fWatch, uint dwFrame)
        {
            return VSConstants.S_OK;
        }


        // Terminates the program.
        public int Terminate()
        {
            mProgram = null;

            // Because the sample engine is a native debugger, it implements IDebugEngineLaunch2, and will terminate
            // the process in IDebugEngineLaunch2.TerminateProcess

            return VSConstants.S_OK;
        }

        // Enumerates the code contexts for a given position in a source file.
        public int EnumCodeContexts(IDebugDocumentPosition2 pDocPos, out IEnumDebugCodeContexts2 ppEnum)
        {
            throw new NotImplementedException();
        }

        // Determines if a process can be terminated.
        int IDebugEngineLaunch2.CanTerminateProcess(IDebugProcess2 process)
        {
            return VSConstants.S_OK;

            //try {
            //  int processId = EngineUtils.GetProcessId(process);

            //  //if (processId == m_debuggedProcess.Id)
            //  {
            //    return VSConstants.S_OK;
            //  }
            //  //else
            //  {
            //    //return VSConstants.S_FALSE;
            //  }
            //}
            //  //catch (ComponentException e)
            //  //{
            //  //    return e.HResult;
            //  //}
            //catch (Exception e) {
            //  return EngineUtils.UnexpectedException(e);
            //}
        }

        #endregion

        #region Deprecated interface methods
        // These methods are not called by the Visual Studio debugger, so they don't need to be implemented

        int IDebugEngine2.EnumPrograms(out IEnumDebugPrograms2 programs)
        {
            System.Diagnostics.Debug.Fail("This function is not called by the debugger");

            programs = null;
            return VSConstants.E_NOTIMPL;
        }

        public int Attach(IDebugEventCallback2 pCallback)
        {
            System.Diagnostics.Debug.Fail("This function is not called by the debugger");

            return VSConstants.E_NOTIMPL;
        }

        public int GetProcess(out IDebugProcess2 ppProcess)
        {
            System.Diagnostics.Debug.Fail("This function is not called by the debugger");

            ppProcess = null;
            return VSConstants.E_NOTIMPL;
        }

        public int Execute()
        {
            System.Diagnostics.Debug.Fail("This function is not called by the debugger.");
            return VSConstants.E_NOTIMPL;
        }

        #endregion
    }
}
