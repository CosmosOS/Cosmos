using System;
using Cosmos.VS.DebugEngine.Engine.Impl;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Cosmos.VS.DebugEngine.AD7.Impl {
  // This class implements IDebugThread2 which represents a thread running in a program.
  public class AD7Thread : IDebugThread2 {
    protected readonly AD7Engine mEngine;
    protected readonly AD7Process mProcess;
    //readonly DebuggedThread m_debuggedThread;
    const string ThreadNameString = "Cosmos Kernel Main Thread";

    public AD7Thread(AD7Engine aEngine, AD7Process aProcess) { //, DebuggedThread debuggedThread)
      mEngine = aEngine;
      mProcess = aProcess;
    }

    string GetCurrentLocation(bool fIncludeModuleName) {
      uint ip = this.mProcess.mCurrentAddress.HasValue ? this.mProcess.mCurrentAddress.Value : 0u;// GetThreadContext().eip;
      return mEngine.GetAddressDescription(ip);
    }

    #region IDebugThread2 Members

    // Determines whether the next statement can be set to the given stack frame and code context.
    // The sample debug engine does not support set next statement, so S_FALSE is returned.
    int IDebugThread2.CanSetNextStatement(IDebugStackFrame2 stackFrame, IDebugCodeContext2 codeContext) {
      return VSConstants.S_FALSE;
    }

    // Retrieves a list of the stack frames for this thread.
    // For the sample engine, enumerating the stack frames requires walking the callstack in the debuggee for this thread
    // and coverting that to an implementation of IEnumDebugFrameInfo2. 
    // Real engines will most likely want to cache this information to avoid recomputing it each time it is asked for,
    // and or construct it on demand instead of walking the entire stack.
    int IDebugThread2.EnumFrameInfo(enum_FRAMEINFO_FLAGS aFieldSpec, uint aRadix, out IEnumDebugFrameInfo2 oEnumObject) {
      // Check mStackFrame, not address because it is possible for 2 sequential breaks to be on the same address
      // but in that case we would need a new stack frame.
      //
      // EnumFrameInfo is called several times on each break becuase "different callers can call with different flags".
      // We ignore flags through and always return full, but EnumFrameInfo gets called half a dozen times which is slow
      // if we refresh each and every time. So we cache our info.
      if (mProcess.mStackFrame == null || true) {
        // Ask the lower-level to perform a stack walk on this thread
        //m_engine.DebuggedProcess.DoStackWalk(this.m_debuggedThread);
        oEnumObject = null;
        try {
          //System.Collections.Generic.List<X86ThreadContext> stackFrames = this.m_debuggedThread.StackFrames;
          //int numStackFrames = stackFrames.Count;
          FRAMEINFO[] xFrameInfoArray;

          //if (numStackFrames == 0) {
          // failed to walk any frames. Only return the top frame.

          xFrameInfoArray = new FRAMEINFO[1];
          var xFrame = new AD7StackFrame(mEngine, this, mProcess);
          xFrame.SetFrameInfo(aFieldSpec, out xFrameInfoArray[0]);

          //} else {
          //frameInfoArray = new FRAMEINFO[numStackFrames];
          //for (int i = 0; i < numStackFrames; i++) {
          //AD7StackFrame frame = new AD7StackFrame(m_engine, this, stackFrames[i]);
          //frame.SetFrameInfo(dwFieldSpec, out frameInfoArray[i]);
          //}
          //}

          mProcess.mStackFrame = new AD7FrameInfoEnum(xFrameInfoArray);
        } catch (Exception e) {
          //catch (ComponentException e) {
          //    return e.HResult;
          //}
          return EngineUtils.UnexpectedException(e);
        }
      }
      oEnumObject = mProcess.mStackFrame;
      return VSConstants.S_OK;
    }

    // Get the name of the thread. For the sample engine, the name of the thread is always "Sample Engine Thread"
    int IDebugThread2.GetName(out string threadName) {
      threadName = ThreadNameString;
      return VSConstants.S_OK;
    }

    // Return the program that this thread belongs to.
    int IDebugThread2.GetProgram(out IDebugProgram2 program) {
      program = mEngine;
      return VSConstants.S_OK;
    }

    // Gets the system thread identifier.
    int IDebugThread2.GetThreadId(out uint threadId) {
      threadId = 0;// (uint)m_debuggedThread.Id;
      return VSConstants.S_OK;
    }

    // Gets properties that describe a thread.
    int IDebugThread2.GetThreadProperties(enum_THREADPROPERTY_FIELDS dwFields, THREADPROPERTIES[] propertiesArray) {
      try {
        THREADPROPERTIES props = new THREADPROPERTIES();

        if (dwFields.HasFlag(enum_THREADPROPERTY_FIELDS.TPF_ID)) {
          //props.dwThreadId = (uint)m_debuggedThread.Id;
          props.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_ID;
        }
        if (dwFields.HasFlag(enum_THREADPROPERTY_FIELDS.TPF_SUSPENDCOUNT)) {
          // sample debug engine doesn't support suspending threads
          props.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_SUSPENDCOUNT;
        }
        if (dwFields.HasFlag(enum_THREADPROPERTY_FIELDS.TPF_STATE)) {
          props.dwThreadState = (uint)enum_THREADSTATE.THREADSTATE_RUNNING;
          props.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_STATE;
        }
        if (dwFields.HasFlag(enum_THREADPROPERTY_FIELDS.TPF_PRIORITY)) {
          props.bstrPriority = "Normal";
          props.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_PRIORITY;
        }
        if (dwFields.HasFlag(enum_THREADPROPERTY_FIELDS.TPF_NAME)) {
          props.bstrName = ThreadNameString;
          props.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_NAME;
        }
        if (dwFields.HasFlag(enum_THREADPROPERTY_FIELDS.TPF_LOCATION)) {
          props.bstrLocation = GetCurrentLocation(true);
          props.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_LOCATION;
        }

        return VSConstants.S_OK;
      }
        //catch (ComponentException e)
        //{
        //    return e.HResult;
        //}
      catch (Exception e) {
        return EngineUtils.UnexpectedException(e);
      }
    }

    // Resume a thread.
    // This is called when the user chooses "Unfreeze" from the threads window when a thread has previously been frozen.
    int IDebugThread2.Resume(out uint suspendCount) {
      // The sample debug engine doesn't support suspending/resuming threads
      suspendCount = 0;
      return VSConstants.E_NOTIMPL;
    }

    // Sets the next statement to the given stack frame and code context.
    // The sample debug engine doesn't support set next statment
    int IDebugThread2.SetNextStatement(IDebugStackFrame2 stackFrame, IDebugCodeContext2 codeContext) {
      return VSConstants.E_NOTIMPL;
    }

    // suspend a thread.
    // This is called when the user chooses "Freeze" from the threads window
    int IDebugThread2.Suspend(out uint suspendCount) {
      // The sample debug engine doesn't support suspending/resuming threads
      suspendCount = 0;
      return VSConstants.E_NOTIMPL;
    }

    #endregion

    #region Uncalled interface methods
    // These methods are not currently called by the Visual Studio debugger, so they don't need to be implemented

    int IDebugThread2.GetLogicalThread(IDebugStackFrame2 stackFrame, out IDebugLogicalThread2 logicalThread) {
      System.Diagnostics.Debug.Fail("This function is not called by the debugger");
      logicalThread = null;
      return VSConstants.E_NOTIMPL;
    }

    int IDebugThread2.SetThreadName(string name) {
      System.Diagnostics.Debug.Fail("This function is not called by the debugger");
      return VSConstants.E_NOTIMPL;
    }

    #endregion
  }
}
