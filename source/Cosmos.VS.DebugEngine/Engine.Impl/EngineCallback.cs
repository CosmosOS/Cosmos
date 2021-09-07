using System;
using System.Collections.Generic;
using Cosmos.VS.DebugEngine.AD7.Impl;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Cosmos.VS.DebugEngine.Engine.Impl {
  public class EngineCallback { //: ISampleEngineCallback
    readonly IDebugEventCallback2 m_ad7Callback;
    readonly AD7Engine m_engine;

    public EngineCallback(AD7Engine engine, IDebugEventCallback2 ad7Callback) {
      m_ad7Callback = ad7Callback;
      m_engine = engine;
    }

    public void Send(IDebugEvent2 eventObject, string iidEvent, IDebugProgram2 program, IDebugThread2 thread) {
      var riidEvent = new Guid(iidEvent);

      EngineUtils.RequireOk(eventObject.GetAttributes(out var attributes));
      EngineUtils.RequireOk(m_ad7Callback.Event(m_engine, null, program, thread, eventObject, ref riidEvent, attributes));
    }

    public void Send(IDebugEvent2 eventObject, string iidEvent, IDebugThread2 thread) {
      Send(eventObject, iidEvent, m_engine, thread);
    }

    public void OnError(int hrErr) {
      //System.Diagnostics.Debug.Assert(Worker.CurrentThreadId == m_engine.DebuggedProcess.PollThreadId);

      // IDebugErrorEvent2 is used to report error messages to the user when something goes wrong in the debug engine.
      // The sample engine doesn't take advantage of this.
    }

    public void OnModuleLoad(AD7Module aModule) {
      // This will get called when the entrypoint breakpoint is fired because the engine sends a mod-load event
      // for the exe.
      //if (m_engine.DebuggedProcess != null) {
      //    System.Diagnostics.Debug.Assert(Worker.CurrentThreadId == m_engine.DebuggedProcess.PollThreadId);
      //}

      var eventObject = new AD7ModuleLoadEvent(aModule, true /* this is a module load */);
      Send(eventObject, AD7ModuleLoadEvent.IID, null);
    }

    public void OnModuleUnload() { //DebuggedModule debuggedModule)
      //System.Diagnostics.Debug.Assert(Worker.CurrentThreadId == m_engine.DebuggedProcess.PollThreadId);

      //AD7Module ad7Module = (AD7Module)debuggedModule.Client;
      //System.Diagnostics.Debug.Assert(ad7Module != null);

      //AD7ModuleLoadEvent eventObject = new AD7ModuleLoadEvent(ad7Module, false /* this is a module unload */);

      //Send(eventObject, AD7ModuleLoadEvent.IID, null);
    }

    // Call this one for internal Cosmos dev.
    // Can be turned off and should be turned off by default. Use an IFDEF or something.
    public void OnOutputString(string outputString) {
#if DEV
        //System.Diagnostics.Debug.Assert(Worker.CurrentThreadId == m_engine.DebuggedProcess.PollThreadId);
        var eventObject = new AD7OutputDebugStringEvent(outputString);
        Send(eventObject, AD7OutputDebugStringEvent.IID, null);
#endif
    }

    // This is the user version, messages directly from Cosmos user code
    public void OnOutputStringUser(string outputString) {
      //System.Diagnostics.Debug.Assert(Worker.CurrentThreadId == m_engine.DebuggedProcess.PollThreadId);
      var eventObject = new AD7OutputDebugStringEvent(outputString);
      Send(eventObject, AD7OutputDebugStringEvent.IID, null);
    }

    public void OnProcessExit(uint exitCode) {
      //System.Diagnostics.Debug.Assert(Worker.CurrentThreadId == m_engine.DebuggedProcess.PollThreadId);

      var eventObject = new AD7ProgramDestroyEvent(exitCode);

      Send(eventObject, AD7ProgramDestroyEvent.IID, null);
    }

    public void OnThreadExit() { //DebuggedThread debuggedThread, uint exitCode)
      //System.Diagnostics.Debug.Assert(Worker.CurrentThreadId == m_engine.DebuggedProcess.PollThreadId);

      //AD7Thread ad7Thread = (AD7Thread)debuggedThread.Client;
      //System.Diagnostics.Debug.Assert(ad7Thread != null);

      //AD7ThreadDestroyEvent eventObject = new AD7ThreadDestroyEvent(exitCode);

      //Send(eventObject, AD7ThreadDestroyEvent.IID, ad7Thread);
    }

    public void OnThreadStart(AD7Thread debuggedThread) {
      // This will get called when the entrypoint breakpoint is fired because the engine sends a thread start event
      // for the main thread of the application.
      //if (m_engine.DebuggedProcess != null)
      //{
      //    System.Diagnostics.Debug.Assert(Worker.CurrentThreadId == m_engine.DebuggedProcess.PollThreadId);
      //}

      var eventObject = new AD7ThreadCreateEvent();
      Send(eventObject, AD7ThreadCreateEvent.IID, debuggedThread);
    }

    public void OnBreak(AD7Thread aThread) {
      var mBreak = new AD7BreakEvent();
      Send(mBreak, AD7BreakEvent.IID, aThread);
    }

    public void OnBreakpoint(AD7Thread thread, IList<IDebugBoundBreakpoint2> clients) {
      var boundBreakpoints = new IDebugBoundBreakpoint2[clients.Count];
      int i = 0;
      foreach (var objCurrentBreakpoint in clients) {
        boundBreakpoints[i] = objCurrentBreakpoint;
        i++;
      }

      // An engine that supports more advanced breakpoint features such as hit counts, conditions and filters
      // should notify each bound breakpoint that it has been hit and evaluate conditions here.
      // The sample engine does not support these features.
      var boundBreakpointsEnum = new AD7BoundBreakpointsEnum(boundBreakpoints);
      var eventObject = new AD7BreakpointEvent(boundBreakpointsEnum);
      Send(eventObject, AD7BreakpointEvent.IID, thread);
    }

    public void OnException() { //DebuggedThread thread, uint code)
      // Exception events are sent when an exception occurs in the debuggee that the debugger was not expecting.
      // The sample engine does not support these.
      throw new Exception("The method or operation is not implemented.");
    }

    public void OnStepComplete() { //DebuggedThread thread)
      AD7StepCompletedEvent.Send(m_engine);
    }

    public void OnAsyncBreakComplete(AD7Thread aThread) {
      // This will get called when the engine receives the breakpoint event that is created when the user
      // hits the pause button in vs.
      //System.Diagnostics.Debug.Assert(Worker.CurrentThreadId == m_engine.DebuggedProcess.PollThreadId);

      var xEvent = new AD7AsyncBreakCompleteEvent();
      Send(xEvent, AD7AsyncBreakCompleteEvent.IID, aThread);
      //AD7Thread ad7Thread = (AD7Thread)thread.Client;
      //AD7AsyncBreakCompleteEvent eventObject = new AD7AsyncBreakCompleteEvent();
      //Send(eventObject, AD7AsyncBreakCompleteEvent.IID, ad7Thread);
    }

    public void OnLoadComplete(AD7Thread aThread) {
      var xMsg = new AD7LoadCompleteEvent();
      Send(xMsg, AD7LoadCompleteEvent.IID, aThread);
      //AD7Thread ad7Thread = (AD7Thread)thread.Client;
      //AD7LoadCompleteEvent eventObject = new AD7LoadCompleteEvent();
      //Send(eventObject, AD7LoadCompleteEvent.IID, ad7Thread);
    }

    public void OnProgramDestroy(uint exitCode) {
      var eventObject = new AD7ProgramDestroyEvent(exitCode);
      Send(eventObject, AD7ProgramDestroyEvent.IID, null);
    }

    // Engines notify the debugger about the results of a symbol serach by sending an instance of IDebugSymbolSearchEvent2
    public void OnSymbolSearch(AD7Module module, string status, enum_MODULE_INFO_FLAGS dwStatusFlags) {
      string statusString = (dwStatusFlags == enum_MODULE_INFO_FLAGS.MIF_SYMBOLS_LOADED ? "Symbols Loaded - " : "No symbols loaded") + status;

      var eventObject = new AD7SymbolSearchEvent(module, statusString, dwStatusFlags);
      Send(eventObject, AD7SymbolSearchEvent.IID, null);
    }

    // Engines notify the debugger that a breakpoint has bound through the breakpoint bound event.
    public void OnBreakpointBound(object objBoundBreakpoint, uint address) {
      var boundBreakpoint = (AD7BoundBreakpoint)objBoundBreakpoint;
      ((IDebugBoundBreakpoint2)boundBreakpoint).GetPendingBreakpoint(out var pendingBreakpoint);

      var eventObject = new AD7BreakpointBoundEvent((AD7PendingBreakpoint)pendingBreakpoint, boundBreakpoint);
      Send(eventObject, AD7BreakpointBoundEvent.IID, null);
    }

  }

  internal class AD7BreakEvent : AD7StoppingEvent, IDebugBreakEvent2 {
    public const string IID = "C7405D1D-E24B-44E0-B707-D8A5A4E1641B";
  }
}
