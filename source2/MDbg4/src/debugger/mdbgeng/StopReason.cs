//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Diagnostics;
using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;

namespace Microsoft.Samples.Debugging.MdbgEngine
{
    /// <summary>
    /// Abstract Builtin Stop Reason class to make additional stop reasons.
    /// </summary>
    public abstract class BuiltInStopReason
    {
        /// <summary>
        /// Makes the BuiltInStopReason look better when printed.
        /// </summary>
        /// <returns>For a Full.Namespace.ReasonForStoppingStopReason, this returns "ReasonForStopping"</returns>
        public override string ToString()
        {
            const string suffix = "StopReason";

            string s = base.ToString();
            int idx = s.LastIndexOf(".");

            Debug.Assert(idx > 0);
            Debug.Assert(s.EndsWith(suffix));

            string name = s.Substring(idx + 1);
            return name.Substring(0, name.Length - suffix.Length);
        }
    }

    /// <summary>
    /// The default starting stop reason before continue has been called to really start the process.
    /// </summary>
    public class MDbgInitialContinueNotCalledStopReason : BuiltInStopReason
    {
        internal MDbgInitialContinueNotCalledStopReason()
        {
        }
    }

    /// <summary>
    /// The step is complete.
    /// </summary>
    public class StepCompleteStopReason : BuiltInStopReason
    {
        /// <summary>
        /// Create a new instance of the StepCompleteStopReason class.
        /// </summary>
        /// <param name="stepper">The stepper that has stepped.</param>
        /// <param name="stepReason">The reason that the stepper stepped.</param>
        [CLSCompliant(false)]
        public StepCompleteStopReason(CorStepper stepper, CorDebugStepReason stepReason)
        {
            Debug.Assert(stepper != null);
            m_stepReason = stepReason;
            m_stepper = stepper;
        }

        /// <summary>
        /// The reason that the stepper stepped.
        /// </summary>
        /// <value>The CorDebugStepReason</value>
        [CLSCompliant(false)]
        public CorDebugStepReason StepReason
        {
            get
            {
                return m_stepReason;
            }
        }

        /// <summary>
        /// The stepper that stepped.
        /// </summary>
        /// <value>The stepper.</value>
        public CorStepper Stepper
        {
            get
            {
                return m_stepper;
            }
        }

        private CorDebugStepReason m_stepReason;
        private CorStepper m_stepper;
    }

    /// <summary>
    /// The process has exited.
    /// </summary>
    public class ProcessExitedStopReason : BuiltInStopReason
    {
    }

    /// <summary>
    /// A new thread has been created.
    /// </summary>
    public class ThreadCreatedStopReason : BuiltInStopReason
    {
        /// <summary>
        /// Create a new instance of the ThreadCreatedStopReason class.
        /// </summary>
        /// <param name="thread">The thread that has been created.</param>
        public ThreadCreatedStopReason(MDbgThread thread)
        {
            Debug.Assert(thread != null);
            m_thread = thread;
        }
        /// <summary>
        /// The thread that got created.
        /// </summary>
        /// <value>The thread.</value>
        public MDbgThread Thread
        {
            get
            {
                return m_thread;
            }
        }

        private MDbgThread m_thread;
    }

    /// <summary>
    /// A breakpoint has been hit.
    /// </summary>
    public class BreakpointHitStopReason : BuiltInStopReason
    {
        /// <summary>
        /// Create a new instance of the BreakpointHitStopReason class.
        /// </summary>
        /// <param name="breakpoint">The breakpoint that has been hit.</param>
        public BreakpointHitStopReason(MDbgBreakpoint breakpoint)
        {
            Debug.Assert(breakpoint != null);
            m_breakpoint = breakpoint;
        }

        /// <summary>
        /// The Breakpoint that has been hit.
        /// </summary>
        /// <value>The Breakpoint.</value>
        public MDbgBreakpoint Breakpoint
        {
            get
            {
                return m_breakpoint;
            }
        }

        private MDbgBreakpoint m_breakpoint;
    }

    /// <summary>
    /// A Module has been loaded.
    /// </summary>
    public class ModuleLoadedStopReason : BuiltInStopReason
    {
        /// <summary>
        /// Create a new instance of the ModuleLoadedStopReason class.
        /// </summary>
        /// <param name="managedModule">The module that has been loaded.</param>
        public ModuleLoadedStopReason(MDbgModule managedModule)
        {
            Debug.Assert(managedModule != null);
            m_module = managedModule;
        }
        /// <summary>
        /// The Module that got loaded.
        /// </summary>
        /// <value>The Module.</value>
        public MDbgModule Module
        {
            get
            {
                return m_module;
            }
        }

        private MDbgModule m_module;
    }

    /// <summary>
    /// A Class has been loaded.
    /// </summary>
    public class ClassLoadedStopReason : BuiltInStopReason
    {
        /// <summary>
        /// Create a new instance of the ClassLoadedStopReason class.
        /// </summary>
        /// <param name="managedClass">The class that has been loaded.</param>
        public ClassLoadedStopReason(CorClass managedClass)
        {
            Debug.Assert(managedClass != null);
            m_class = managedClass;
        }

        /// <summary>
        /// The Class that got loaded.
        /// </summary>
        /// <value>The Class.</value>
        public CorClass Class
        {
            get
            {
                return m_class;
            }
        }

        private CorClass m_class;
    }

    /// <summary>
    /// A Class has been unloaded.
    /// </summary>
    public class ClassUnloadedStopReason : BuiltInStopReason
    {
    }

    /// <summary>
    /// An Assemble has been loaded.
    /// </summary>
    public class AssemblyLoadedStopReason : BuiltInStopReason
    {
        /// <summary>
        /// Create a new instance of the AssemblyLoadedStopReason class.
        /// </summary>
        /// <param name="assembly">The assembly that has been loaded.</param>
        public AssemblyLoadedStopReason(CorAssembly assembly)
        {
            Debug.Assert(assembly != null);
            m_assembly = assembly;
        }

        /// <summary>
        /// The Assembly that has been loaded.
        /// </summary>
        /// <value>The Assembly.</value>
        public CorAssembly Assembly
        {
            get
            {
                return m_assembly;
            }
        }

        private CorAssembly m_assembly;
    }

    /// <summary>
    /// An Assembly has been unloaded.
    /// </summary>
    public class AssemblyUnloadedStopReason : BuiltInStopReason
    {
    }

    /// <summary>
    /// The Control-C Signal has been trapped.
    /// </summary>
    public class ControlCTrappedStopReason : BuiltInStopReason
    {
    }

    /// <summary>
    /// An exception is getting unwound.
    /// </summary>
    public class ExceptionUnwindStopReason : BuiltInStopReason
    {

        /// <summary>
        /// Create a new instance of the ExceptionUnwindStopReason class.
        /// </summary>
        /// <param name="appDomain">The appDomain where exception has been thrown.</param>
        /// <param name="thread">The thread on which the exception has been thrown.</param>
        /// <param name="eventType">Reason for the notification.</param>
        /// <param name="flags">Flags passed to the exception callback.</param>
        [CLSCompliant(false)]
        public ExceptionUnwindStopReason(CorAppDomain appDomain, CorThread thread,
                                           CorDebugExceptionUnwindCallbackType eventType, int flags)
        {
            m_appDomain = appDomain;
            m_thread = thread;
            m_eventtype = eventType;
            m_flags = flags;
        }

        /// <summary>
        /// AppDomain where exception occured.
        /// </summary>
        /// <value></value>
        public CorAppDomain AppDomain
        {
            get
            {
                return m_appDomain;
            }
        }

        /// <summary>
        /// Thread where exception occured.
        /// </summary>
        /// <value></value>
        public CorThread Thread
        {
            get
            {
                return m_thread;
            }
        }

        /// <summary>
        /// The ExceptionUnwindCallbackType for the Exception that's getting unwound.
        /// </summary>
        /// <value></value>
        [CLSCompliant(false)]
        public CorDebugExceptionUnwindCallbackType EventType
        {
            get
            {
                return m_eventtype;
            }
        }

        /// <summary>
        /// Flags for the Exception Unwind.
        /// </summary>
        /// <value></value>
        public int Flags
        {
            get
            {
                return m_flags;
            }
        }
        private CorAppDomain m_appDomain;
        private CorThread m_thread;
        private CorDebugExceptionUnwindCallbackType m_eventtype;
        private int m_flags;
    }

    /// <summary>
    /// An Exception has been thrown.
    /// </summary>
    public class ExceptionThrownStopReason : BuiltInStopReason
    {
        /// <summary>
        /// Create a new instance of the ExceptionThrownStopReason class.
        /// </summary>
        /// <param name="appDomain">The appDomain where exception has been thrown.</param>
        /// <param name="thread">The thread on which the exception has been thrown.</param>
        /// <param name="frame">The frame where the exception has been thrown.</param>
        /// <param name="offset">IL offset within the function where exception has been thrown.</param>
        /// <param name="eventType">Reason for the notification.</param>
        /// <param name="flags">Flags passed to the exception callback.</param>
        [CLSCompliant(false)]
        public ExceptionThrownStopReason(CorAppDomain appDomain, CorThread thread, CorFrame frame,
                                           int offset, CorDebugExceptionCallbackType eventType, int flags)
        {
            m_appDomain = appDomain;
            m_thread = thread;
            m_frame = frame;
            m_offset = offset;
            m_eventtype = eventType;
            m_flags = flags;
        }

        /// <summary>
        /// Create a new instance of the ExceptionThrownStopReason class.
        /// </summary>
        /// <param name="appDomain">The appDomain where exception has been thrown.</param>
        /// <param name="thread">The thread on which the exception has been thrown.</param>
        /// <param name="frame">The frame where the exception has been thrown.</param>
        /// <param name="offset">IL offset within the function where exception has been thrown.</param>
        /// <param name="eventType">Reason for the notification.</param>
        /// <param name="flags">Flags passed to the exception callback.</param>
        /// <param name="exceptionEnhancedOn">Whether or not Exception Enhanced has been turned
        /// on by the user for this exception.</param>
        [CLSCompliant(false)]
        public ExceptionThrownStopReason(CorAppDomain appDomain, CorThread thread, CorFrame frame,
                                           int offset, CorDebugExceptionCallbackType eventType, int flags,
                                           bool exceptionEnhancedOn)
        {
            m_appDomain = appDomain;
            m_thread = thread;
            m_frame = frame;
            m_offset = offset;
            m_eventtype = eventType;
            m_flags = flags;
            m_exceptionEnhancedOn = exceptionEnhancedOn;
        }

        /// <summary>
        /// AppDomain where exception occured.
        /// </summary>
        /// <value></value>
        public CorAppDomain AppDomain
        {
            get
            {
                return m_appDomain;
            }
        }

        /// <summary>
        /// Thread where exception occured.
        /// </summary>
        /// <value></value>
        public CorThread Thread
        {
            get
            {
                return m_thread;
            }
        }

        /// <summary>
        ///  The Frame where the exception was thrown.
        /// </summary>
        /// <value>The Frame.</value>
        public CorFrame Frame
        {
            get
            {
                return m_frame;
            }
        }

        /// <summary>
        /// The Offset where the exception was thrown
        /// </summary>
        /// <value>The Offset.</value>
        public int Offset
        {
            get
            {
                return m_offset;
            }
        }

        /// <summary>
        /// The EventType for the Exception.
        /// </summary>
        /// <value>The EventType.</value>
        [CLSCompliant(false)]
        public CorDebugExceptionCallbackType EventType
        {
            get
            {
                return m_eventtype;
            }
        }

        /// <summary>
        /// The Flags for the Exception.
        /// </summary>
        /// <value>The Flags.</value>
        public int Flags
        {
            get
            {
                return m_flags;
            }
        }

        /// <summary>
        /// Whether or not Exception Enhanced has been turned
        /// on by the user for this Exception.
        /// </summary>
        public bool ExceptionEnhancedOn
        {
            get
            {
                return m_exceptionEnhancedOn;
            }
        }

        private CorAppDomain m_appDomain;
        private CorThread m_thread;
        private CorFrame m_frame;
        private int m_offset;
        private CorDebugExceptionCallbackType m_eventtype;
        private int m_flags;
        private bool m_exceptionEnhancedOn;
    }

    /// <summary>
    /// An Unhandled Exception has been thrown.
    /// </summary>
    public class UnhandledExceptionThrownStopReason : ExceptionThrownStopReason
    {
        /// <summary>
        /// Create a new instance of the UnhandledExceptionThrownStopReason class.
        /// </summary>
        /// <param name="appDomain">The appDomain where exception has been thrown.</param>
        /// <param name="thread">The thread on which the exception has been thrown.</param>
        /// <param name="frame">The frame where the exception has been thrown.</param>
        /// <param name="offset">IL offset within the function where exception has been thrown.</param>
        /// <param name="eventType">Reason for the notification.</param>
        /// <param name="flags">Flags passed to the exception callback.</param>
        [CLSCompliant(false)]
        public UnhandledExceptionThrownStopReason(CorAppDomain appDomain, CorThread thread, CorFrame frame,
                                                    int offset, CorDebugExceptionCallbackType eventType, int flags)
            : base(appDomain, thread, frame, offset, eventType, flags)
        {
        }
    }

    /// <summary>
    /// The Debugger has Asynchronously stopped.
    /// </summary>
    public class AsyncStopStopReason : BuiltInStopReason
    {
    }

    /// <summary>
    /// At Attach has completed.
    /// </summary>
    public class AttachCompleteStopReason : BuiltInStopReason
    {
    }

    /// <summary>
    /// A User Break has been hit.
    /// </summary>
    public class UserBreakStopReason : BuiltInStopReason
    {
    }

    /// <summary>
    ///  A Function Evaluation has completed.
    /// </summary>
    public class EvalCompleteStopReason : BuiltInStopReason
    {
        // extensions might want to use this command stop reason 
        // to signal finish of custom func-eval
        /// <summary>
        /// Creates a new instance of the EvalCompleteStopReason object.
        /// </summary>
        /// <param name="eval">The Function Evaluation that has completed.</param>
        public EvalCompleteStopReason(CorEval eval)
        {
            Debug.Assert(eval != null);
            m_eval = eval;
        }

        /// <summary>
        /// The Function Evaluation that has completed.
        /// </summary>
        /// <value>The Function Evaluation.</value>
        public CorEval Eval
        {
            get
            {
                return m_eval;
            }
        }

        private CorEval m_eval;
    }

    /// <summary>
    /// A Function Evaluation has caused an Exception.
    /// </summary>
    public class EvalExceptionStopReason : EvalCompleteStopReason
    {
        // extensions might want to use this command stop reason 
        // to signal finish of custom func-eval
        /// <summary>
        /// Creates a new instance of the EvalExceptionStopReason object.
        /// </summary>
        /// <param name="eval">The Function Evaluation that caused the exception.</param>
        public EvalExceptionStopReason(CorEval eval)
            : base(eval)
        {
        }
    }

    /// <summary>
    /// A Remap Opportunity has been reached
    /// </summary>
    public class RemapOpportunityReachedStopReason : BuiltInStopReason
    {
        /// <summary>
        /// Create a new instance of the RemapOpportunityReachedStopReason class.
        /// </summary>
        /// <param name="appDomain">The appDomain where remapping is occuring.</param>
        /// <param name="thread">The thread on which the remapping is occuring.</param>
        /// <param name="oldFunction">The old version of function we are remapping from.</param>
        /// <param name="newFunction">The latest version of function we are remapping to.</param>
        /// <param name="oldILOffset">The IL-offset of location in old function we're remaping from.</param>
        [CLSCompliant(false)]
        public RemapOpportunityReachedStopReason(CorAppDomain appDomain,
                                                 CorThread thread,
                                                 CorFunction oldFunction,
                                                 CorFunction newFunction,
                                                 int oldILOffset)
        {
            Debug.Assert(appDomain != null);
            Debug.Assert(thread != null);
            Debug.Assert(oldFunction != null);
            Debug.Assert(newFunction != null);
            m_appDomain = appDomain;
            m_thread = thread;
            m_oldFunction = oldFunction;
            m_newFunction = newFunction;
            m_oldILOffset = oldILOffset;
        }

        /// <summary>
        /// The AppDomain where the remap opportunity is.
        /// </summary>
        /// <value>The AppDomain.</value>
        public CorAppDomain AppDomain
        {
            get
            {
                return m_appDomain;
            }
        }

        /// <summary>
        /// The Thread where the remap opportunity is.
        /// </summary>
        /// <value>The Thread.</value>
        public CorThread Thread
        {
            get
            {
                return m_thread;
            }
        }

        /// <summary>
        /// The Old Function before the remap.
        /// </summary>
        /// <value>The Old Function.</value>
        public CorFunction OldFunction
        {
            get
            {
                return m_oldFunction;
            }
        }

        /// <summary>
        /// The New Function after the remap.
        /// </summary>
        /// <value>The New Function.</value>
        public CorFunction NewFunction
        {
            get
            {
                return m_newFunction;
            }
        }

        /// <summary>
        /// The Old Intermediate Language Offset before the remap.
        /// </summary>
        /// <value>The Old IL Offset.</value>
        public int OldILOffset
        {
            get
            {
                return m_oldILOffset;
            }
        }

        private CorAppDomain m_appDomain;
        private CorThread m_thread;
        private CorFunction m_oldFunction, m_newFunction;
        private int m_oldILOffset;
    }

    /// <summary>
    /// A Function Remap has completed.
    /// </summary>
    public class FunctionRemapCompleteStopReason : BuiltInStopReason
    {
        /// <summary>
        /// Create a new instance of the FunctionRemapCompleteStopReason class.
        /// </summary>
        /// <param name="appDomain">The appDomain where remapping is occuring.</param>
        /// <param name="thread">The thread on which the remapping is occuring.</param>
        /// <param name="managedFunction">The version of function the debugger remapped to.</param>
        public FunctionRemapCompleteStopReason(CorAppDomain appDomain,
                                                 CorThread thread,
                                                 CorFunction managedFunction)
        {
            Debug.Assert(appDomain != null);
            Debug.Assert(thread != null);
            Debug.Assert(managedFunction != null);
            m_appDomain = appDomain;
            m_thread = thread;
            m_function = managedFunction;
        }

        /// <summary>
        /// The AppDomain of the Function Remap.
        /// </summary>
        /// <value>The AppDomain.</value>
        public CorAppDomain AppDomain
        {
            get
            {
                return m_appDomain;
            }
        }

        /// <summary>
        /// The Thread of the Function Remap.
        /// </summary>
        /// <value>The Thread.</value>
        public CorThread Thread
        {
            get
            {
                return m_thread;
            }
        }

        /// <summary>
        /// The Function that got remapped.
        /// </summary>
        /// <value>The Function.</value>
        public CorFunction Function
        {
            get
            {
                return m_function;
            }
        }

        private CorAppDomain m_appDomain;
        private CorThread m_thread;
        private CorFunction m_function;
    }

    /// <summary>
    /// Base class for message stop-reasons.
    /// Messages are just notes to the user (such as logging or MDAs) and can
    /// be ignored.
    /// </summary>
    public abstract class MessageStopReason : BuiltInStopReason
    {
        /// <summary>
        /// Get name for category of this message.
        /// </summary>        
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Get detailed message string.
        /// </summary>
        public abstract string Message
        {
            get;
        }

        /// <summary>
        /// Returns more detailed information about stop reason.
        /// </summary>
        public override string ToString()
        {
            return base.ToString() + "(" + Name + ")";
        }
    }

    /// <summary>
    /// A Message has been logged.
    /// </summary>
    public class LogMessageStopReason : MessageStopReason
    {
        /// <summary>
        /// Create a new instance of the LogMessageStopReason class.
        /// </summary>
        /// <param name="switchName">Name of switch (category) from Debugger.Log method call.</param>
        /// <param name="message">Message passed to the Debugger.Log method call.</param>
        public LogMessageStopReason(string switchName, string message)
        {
            Debug.Assert(switchName != null);
            Debug.Assert(message != null);
            m_switchName = switchName;
            m_message = message;
        }

        /// <summary>
        /// The Message that got logged.
        /// </summary>
        /// <value>The Message.</value>
        public override string Message
        {
            get
            {
                return m_message;
            }
        }

        /// <summary>
        /// The SwitchName for the Message Logging
        /// </summary>
        /// <value>The SwitchName.</value>
        public string SwitchName
        {
            get
            {
                return m_switchName;
            }
        }

        private string m_switchName;
        private string m_message;
        /// <summary>
        /// Get name for category of this message.
        /// </summary>        
        public override string Name
        {
            get { return SwitchName; }
        }

    }

    /// <summary>
    /// A MDA (Managed Debug Assistant) has occured.
    /// </summary>
    public class MDANotificationStopReason : MessageStopReason
    {
        /// <summary>
        /// Create a new instance of the MDANotificationStopReason class.
        /// </summary>
        /// <param name="mda">Generated MDA notification.</param>
        public MDANotificationStopReason(CorMDA mda)
        {
            Debug.Assert(mda != null);
            m_mda = mda;
        }
        CorMDA m_mda;

        /// <summary>
        /// The Message that got logged.
        /// </summary> 
        /// <value>The Message.</value>
        public override string Message
        {
            get
            {
                return m_mda.Description;
            }
        }

        /// <summary>
        /// Get name for category of this message.
        /// </summary>        
        public override string Name
        {
            get { return m_mda.Name; }
        }

        /// <summary>
        /// Mda object that caused the debugged program to stop.
        /// </summary>        
        public CorMDA CorMDA
        {
            get { return m_mda; }
        }
    }

    /// <summary>
    /// In Raw Mode, all stop reasons are the same and this is what they are called.
    /// You may then decide for yourself what to do about the stop event.
    /// </summary>
    public class RawModeStopReason : BuiltInStopReason
    {
        internal RawModeStopReason(ManagedCallbackType callbackType, CorEventArgs callbackArgs)
        {
            Debug.Assert(callbackArgs != null);
            m_callbackType = callbackType;
            m_callbackArgs = callbackArgs;
        }

        /// <summary>
        /// What type of stop reason this would have been, if it weren't in Raw Mode.
        /// </summary>
        public ManagedCallbackType Callback
        {
            get
            {
                return m_callbackType;
            }
        }

        /// <summary>
        /// Arguments for the callback.
        /// </summary>
        public CorEventArgs Arguments
        {
            get
            {
                return m_callbackArgs;
            }
        }

        private ManagedCallbackType m_callbackType;
        private CorEventArgs m_callbackArgs;
    }

    /// <summary>
    /// The Debugger has encountered an unrecoverable error, i.e. from a 
    /// ICorDebugManagedCallback::DebuggerError callback.
    /// </summary>
    public class DebuggerErrorStopReason : BuiltInStopReason
    {
        /// <summary>
        /// Create a DebuggerErrorStopReason
        /// </summary>
        /// <param name="hresult">The hresult representing the reason for the error.</param>
        public DebuggerErrorStopReason(int hresult)
        {
            m_hr = hresult;
        }

        /// <summary>
        /// Create a string representation of the error, including any information available about the hresult
        /// </summary>
        /// <returns>The descriptive error message</returns>
        public override string ToString()
        {
            string hrMsg = System.Runtime.InteropServices.Marshal.GetExceptionForHR(m_hr).Message;
            return "Debugger Error: " + hrMsg;
        }

        private int m_hr;
    }

    /// <summary>
    /// Mdbg has encountered an Error.
    /// </summary>
    public class MDbgErrorStopReason : BuiltInStopReason
    {
        /// <summary>
        /// Create a new instance of the MDbgErrorStopReason class.
        /// </summary>
        /// <param name="e">Exception that caused errror in the debugger.</param>
        public MDbgErrorStopReason(Exception e)
        {
            Debug.Assert(e != null);
            m_exception = e;
        }

        /// <summary>
        /// The Exception for the error.
        /// </summary>
        /// <value>The Exception.</value>
        public Exception ExceptionThrown
        {
            get
            {
                return m_exception;
            }
        }

        Exception m_exception;
    }

}
