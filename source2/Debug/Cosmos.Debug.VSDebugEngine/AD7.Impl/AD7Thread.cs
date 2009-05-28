using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Diagnostics;

namespace Cosmos.Debugger.VSDebugEngine
{
    // This class implements IDebugThread2 which represents a thread running in a program.
    class AD7Thread : IDebugThread2
    {
        readonly AD7Engine m_engine;
        //readonly DebuggedThread m_debuggedThread;
        const string ThreadNameString = "Sample Engine Thread";

        public AD7Thread(AD7Engine engine)//, DebuggedThread debuggedThread)
        {
            m_engine = engine;
            //m_debuggedThread = debuggedThread;
        }      
        
        //X86ThreadContext GetThreadContext()
        //{
        //    X86ThreadContext threadContext = m_engine.DebuggedProcess.GetThreadContext(m_debuggedThread.Handle);
        //
        //    return threadContext;
        //}

        string GetCurrentLocation(bool fIncludeModuleName)
        {
            uint ip = 0;// GetThreadContext().eip;
            string location = m_engine.GetAddressDescription(ip);

            return location;
        }

        //internal DebuggedThread GetDebuggedThread()
        //{
        //    return m_debuggedThread;
        //}

        #region IDebugThread2 Members

        // Determines whether the next statement can be set to the given stack frame and code context.
        // The sample debug engine does not support set next statement, so S_FALSE is returned.
        int IDebugThread2.CanSetNextStatement(IDebugStackFrame2 stackFrame, IDebugCodeContext2 codeContext)
        {
            return VSConstants.S_FALSE; 
        }

        // Retrieves a list of the stack frames for this thread.
        // For the sample engine, enumerating the stack frames requires walking the callstack in the debuggee for this thread
        // and coverting that to an implementation of IEnumDebugFrameInfo2. 
        // Real engines will most likely want to cache this information to avoid recomputing it each time it is asked for,
        // and or construct it on demand instead of walking the entire stack.
        int IDebugThread2.EnumFrameInfo(uint dwFieldSpec, uint nRadix, out IEnumDebugFrameInfo2 enumObject)
        {
            // Ask the lower-level to perform a stack walk on this thread
            //m_engine.DebuggedProcess.DoStackWalk(this.m_debuggedThread);
            enumObject = null;

            try
            {
                //System.Collections.Generic.List<X86ThreadContext> stackFrames = this.m_debuggedThread.StackFrames;
                //int numStackFrames = stackFrames.Count;
                FRAMEINFO[] frameInfoArray;

                //if (numStackFrames == 0)
                {
                    // failed to walk any frames. Only return the top frame.
                    frameInfoArray = new FRAMEINFO[1];
                    //AD7StackFrame frame = new AD7StackFrame(m_engine, this, GetThreadContext());
                    //frame.SetFrameInfo(dwFieldSpec, out frameInfoArray[0]);
                }
                //else
                {
                    //frameInfoArray = new FRAMEINFO[numStackFrames];

                    //for (int i = 0; i < numStackFrames; i++)
                    {
                        //AD7StackFrame frame = new AD7StackFrame(m_engine, this, stackFrames[i]);
                        //frame.SetFrameInfo(dwFieldSpec, out frameInfoArray[i]);
                    }
                }

                enumObject = new AD7FrameInfoEnum(frameInfoArray);
                return VSConstants.S_OK;
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

        // Get the name of the thread. For the sample engine, the name of the thread is always "Sample Engine Thread"
        int IDebugThread2.GetName(out string threadName)
        {
            threadName = ThreadNameString;
            return VSConstants.S_OK;
        }

        // Return the program that this thread belongs to.
        int IDebugThread2.GetProgram(out IDebugProgram2 program)
        {
            program = m_engine;
            return VSConstants.S_OK;
        }

        // Gets the system thread identifier.
        int IDebugThread2.GetThreadId(out uint threadId)
        {
            threadId = 0;// (uint)m_debuggedThread.Id;
            return VSConstants.S_OK;
        }

        // Gets properties that describe a thread.
        int IDebugThread2.GetThreadProperties(uint dwFields, THREADPROPERTIES[] propertiesArray)
        {
            try
            {
                THREADPROPERTIES props = new THREADPROPERTIES();

                if ((dwFields & (uint)enum_THREADPROPERTY_FIELDS.TPF_ID) != 0)
                {
                    //props.dwThreadId = (uint)m_debuggedThread.Id;
                    props.dwFields |= (uint)enum_THREADPROPERTY_FIELDS.TPF_ID;
                }
                if ((dwFields & (uint)enum_THREADPROPERTY_FIELDS.TPF_SUSPENDCOUNT) != 0) 
                {
                    // sample debug engine doesn't support suspending threads
                    props.dwFields |= (uint)enum_THREADPROPERTY_FIELDS.TPF_SUSPENDCOUNT;
                }
                if ((dwFields & (uint)enum_THREADPROPERTY_FIELDS.TPF_STATE) != 0) 
                {
                    props.dwThreadState = (uint)enum_THREADSTATE.THREADSTATE_RUNNING;
                    props.dwFields |= (uint)enum_THREADPROPERTY_FIELDS.TPF_STATE;
                }
                if ((dwFields & (uint)enum_THREADPROPERTY_FIELDS.TPF_PRIORITY) != 0) 
                {
                    props.bstrPriority = "Normal";
                    props.dwFields |= (uint)enum_THREADPROPERTY_FIELDS.TPF_PRIORITY;
                }
                if ((dwFields & (uint)enum_THREADPROPERTY_FIELDS.TPF_NAME) != 0)
                {
                    props.bstrName = ThreadNameString;
                    props.dwFields |= (uint)enum_THREADPROPERTY_FIELDS.TPF_NAME;
                }
                if ((dwFields & (uint)enum_THREADPROPERTY_FIELDS.TPF_LOCATION) != 0)
                {
                    props.bstrLocation = GetCurrentLocation(true);
                    props.dwFields |= (uint)enum_THREADPROPERTY_FIELDS.TPF_LOCATION;
                }

                return VSConstants.S_OK;
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

        // Resume a thread.
        // This is called when the user chooses "Unfreeze" from the threads window when a thread has previously been frozen.
        int IDebugThread2.Resume(out uint suspendCount)
        {
            // The sample debug engine doesn't support suspending/resuming threads
            suspendCount = 0;
            return VSConstants.E_NOTIMPL;
        }

        // Sets the next statement to the given stack frame and code context.
        // The sample debug engine doesn't support set next statment
        int IDebugThread2.SetNextStatement(IDebugStackFrame2 stackFrame, IDebugCodeContext2 codeContext)
        {
            return VSConstants.E_NOTIMPL;
        }

        // suspend a thread.
        // This is called when the user chooses "Freeze" from the threads window
        int IDebugThread2.Suspend(out uint suspendCount)
        {
            // The sample debug engine doesn't support suspending/resuming threads
            suspendCount = 0;
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region Uncalled interface methods
        // These methods are not currently called by the Visual Studio debugger, so they don't need to be implemented

        int IDebugThread2.GetLogicalThread(IDebugStackFrame2 stackFrame, out IDebugLogicalThread2 logicalThread)
        {
            Debug.Fail("This function is not called by the debugger");

            logicalThread = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugThread2.SetThreadName(string name)
        {
            Debug.Fail("This function is not called by the debugger");

            return VSConstants.E_NOTIMPL;
        }

        #endregion
    }
}
