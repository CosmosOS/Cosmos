//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Diagnostics;
using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;
using System.Text;

namespace Microsoft.Samples.Debugging.MdbgEngine
{
    /// <summary>
    /// Describes a step operation for ICorDebugStepper. Default is essentially the smallest possible
    /// step for source-code.
    /// </summary>
    /// <remarks>ICorDebugStepper is very flexible and can be initialized with many different
    /// properties. This encapsulates the various properties and adds extra runtime error checking.
    /// The defaults is conceptually to single-step the next IL instruction, which means
    /// no step ranges, the current thread, no jmc, stop on all interceptors, etc. 
    /// </remarks> 
    public class StepperDescriptor
    {
        #region Helpers for Creation
        /// <summary>
        /// Creates an empty stepper.
        /// </summary>
        /// <param name="process">non-null process that this is stepping</param>
        public StepperDescriptor(MDbgProcess process)
        {
            if (process == null)
            {
                throw new ArgumentNullException("process");
            }
            m_process = process;
        }

        /// <summary>
        /// Creates a source level stepper
        /// </summary>
        /// <param name="process"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static StepperDescriptor CreateSourceLevelStep(MDbgProcess process, StepperType type)
        {
            bool stepNativeCode = false;
            return CreateSourceLevelStep(process, type, stepNativeCode);
        }

        /// <summary>
        /// Creates a default 'source level step' on process with current active frame and active thread. 
        /// </summary>
        /// <param name="process">non-null process </param>
        /// <param name="type">type of step (in, out, over)</param>
        /// <param name="singleStepInstructions">false to step source level, true to step single instructions</param>
        /// <returns></returns>
        public static StepperDescriptor CreateSourceLevelStep(MDbgProcess process, StepperType type, bool singleStepInstructions)
        {
            StepperDescriptor s = new StepperDescriptor(process);
            s.StepperType = type;

            //
            // Handle Step-out case.
            // 
            if (type == StepperType.Out)
            {
                 return s;
            }

            //
            // Handle step-over / step-in case
            // 
            bool stepInto = (type == StepperType.In);

            
            // Cache current 
            s.Thread = process.Threads.Active.CorThread;
            s.Frame = s.Thread.ActiveFrame;
                       
            CorDebugMappingResult mappingResult;
            uint ip;

            if (!singleStepInstructions)
            {
                // For source-level stepping, skip some interceptors. These are random, and cause
                // random differences in stepping across different runtimes; and user generally don't care
                // about interceptors.
                // It's actually a debatable policy about which interceptors to skip and stop on.
                s.InterceptMask = CorDebugIntercept.INTERCEPT_ALL & 
                    ~(CorDebugIntercept.INTERCEPT_SECURITY | CorDebugIntercept.INTERCEPT_CLASS_INIT);
            }


            s.Frame.GetIP(out ip, out mappingResult);
            if (singleStepInstructions ||
                (mappingResult != CorDebugMappingResult.MAPPING_EXACT &&
                 mappingResult != CorDebugMappingResult.MAPPING_APPROXIMATE))
            {
                // Leave step ranges null
            }
            else
            {
                // Getting the step ranges is what really makes this a source-level step.
                MDbgFunction f = process.Modules.LookupFunction(s.Frame.Function);
                COR_DEBUG_STEP_RANGE[] sr = f.GetStepRangesFromIP((int)ip);
                if (sr != null)
                {
                    s.SetStepRanges(sr, true);
                }
                else
                {
                    // Leave step ranges null.
                }
            }

            return s;
        }
        #endregion // Creation

        #region Properties

        /// <summary>
        /// Converts the Stepper to a user-friendly string
        /// </summary>
        /// <returns>user-friendly string</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Stepper:");
            sb.AppendFormat("{0} ", m_type);
            if (m_isJmc)
            {
                sb.Append("[jmc]");
            }
            if (m_stepRanges != null)
            {
                if (m_isRangeIl)
                {
                    sb.Append("IL:");
                }
                else
                {
                    sb.Append("Native:");
                }
                foreach (COR_DEBUG_STEP_RANGE r in m_stepRanges)
                {
                    sb.AppendFormat("({0}-{1})", r.startOffset, r.endOffset);
                }
            }

            return sb.ToString();
        }

        // The process that this stepper is operating on.
        MDbgProcess m_process;

        // Properties that directly translate to the stepper.
        
        // Maps to ICDStepper::StepOut() vs. ICDStepper::Step(in, *) vs. ICDStepper::Step(over, *)
        StepperType m_type = StepperType.In;

        // Maps to ICDStepper::SetInterceptMask
        CorDebugIntercept m_InterceptMask = CorDebugIntercept.INTERCEPT_ALL;

        // Maps to ICDStepper::SetStopMask        
        CorDebugUnmappedStop m_StopMask = CorDebugUnmappedStop.STOP_NONE;
        
        // Maps to ICDStepper2::SetJMC
        bool m_isJmc = false;

        // Sets step ranges. Step(bool) vs. Step(bool, COR_DEBUG_STEP_RANGE[])
        COR_DEBUG_STEP_RANGE[] m_stepRanges = null;
        bool m_isRangeIl = true;

        // If null, calls ICorDebugThread::CreateStepper. Else call ICorDebugFrame::CreateStepper
        CorFrame m_frame;

        // If null, use the active thread, else use m_thread.
        CorThread m_thread;


        //
        // Other flags
        // 
        
        // Can't change properties after we've called ICorDebugStepper::Step*() 
        // So track this, and then we can enforce it.
        bool m_HasIssuedStep;

        void EnsureNotYetStepped()
        {
            if (m_HasIssuedStep)
            {
                // Cancel the stepper and create a new one.
                throw new InvalidOperationException("Can't change stepper properties after the Step is issued.");
            }
        }
        #endregion // Properties

        #region Set properties
        /// <summary>
        /// Thread the stepper is on. Null for the current thread. This controls how the
        /// ICorDebugStepper is created.
        /// Default is null.
        /// </summary>
        public CorThread Thread
        {
            get { return m_thread; }
            set {
                EnsureNotYetStepped(); 
                m_thread = value;
            }
        }

        /// <summary>
        /// Frame the stepper is on within the Thread. Null for the leaf-most frame .
        /// This controls how the ICorDebugStepper is created.
        /// Default is null
        /// </summary>
        public CorFrame Frame
        {
            get { return m_frame; }
            set {
                EnsureNotYetStepped(); 
                m_frame = value;
            }
        }

        /// <summary>
        /// Enable or Disable Just-my-code (JMC) stepping. The stepper will only stop in functions that have been
        /// marked as JMC.
        /// Default is false.
        /// This affects ICorDebugStepper2::SetJMC.
        /// </summary>
        public bool IsJustMyCode
        {
            get { return m_isJmc; }
            set {
                EnsureNotYetStepped(); 
                m_isJmc = value;
            }
        }

        /// <summary>
        /// General type of stepper (in, out, over). Default is In.
        /// </summary>
        /// <remarks>
        /// This controls which ICorDebugStepper::Step*() method will be called.
        /// If this is Out, then it calls StepOut(). 
        /// If this is In, then it calls Step(true, *)
        /// If this is Over, then it calls Step(false, *)
        /// </remarks>
        public StepperType StepperType
        {
            get { return m_type; }
            set {
                EnsureNotYetStepped(); 
                m_type = value;
            }
        }

        /// <summary>
        /// Sets step ranges for the stepper. 
        /// </summary>
        /// <param name="ranges">Ranges to step over</param>
        /// <param name="isRangeIl">True if the ranges are IL offsets, false if they are native offsets</param>
        /// <remarks>If no ranges are set, the stepper single steps an instruction.</remarks> 
        [CLSCompliant(false)]
        public void SetStepRanges(COR_DEBUG_STEP_RANGE[] ranges, bool isRangeIl)
        {
            EnsureNotYetStepped();
            if (ranges == null)
            {
                throw new ArgumentNullException("ranges");
            }
            m_stepRanges = ranges;
            m_isRangeIl = isRangeIl;
        }
        

        /// <summary>
        /// Interception mask to determine what interceptors (eg, class-initializers) the stepper
        /// may step into. 
        /// </summary>
        [CLSCompliant(false)]
        public CorDebugIntercept InterceptMask 
        {
            get { return m_InterceptMask; }
            set {
                EnsureNotYetStepped(); 
                m_InterceptMask = value;
            }            
        }

        /// <summary>
        /// Stop mask to determine what types of mappings the stepper may stop in (prolog, epilog,
        /// unmapped, etc)
        /// </summary>
        [CLSCompliant(false)]
        public CorDebugUnmappedStop StopMask
        {
            get { return m_StopMask; }
            set {
                EnsureNotYetStepped(); 
                m_StopMask = value;
            }
        }
        

        #endregion // Set properties

        /// <summary>
        /// Issue the step and creates the underlying ICorDebugStepper object. 
        /// This does not Continue the process, nor does it register for a
        /// step-complete handler.
        /// </summary>
        /// <remarks>No other flags can be set after this.</remarks> 
        /// <returns>A CorStepper object which can be associated with the StepComplete handler.</returns>
        public CorStepper Step()
        {
            EnsureNotYetStepped();

            m_HasIssuedStep = true;

            //
            // 1. Create the stepper on the proper thread / frame.
            // 
            
            CorThread thread = m_thread;
            if (thread == null)
            {
                thread = m_process.Threads.Active.CorThread;
            }

            CorStepper stepper;
            if (m_frame == null)
            {
                stepper = thread.CreateStepper();
            }
            else
            {                
                stepper = m_frame.CreateStepper();
            }
            Debug.Assert(stepper != null);

            //
            // 2. Set the flags
            // 
            stepper.SetUnmappedStopMask(m_StopMask);
            stepper.SetInterceptMask(m_InterceptMask);
            if (m_isJmc)
            {
                stepper.SetJmcStatus(true);
            }
            stepper.SetRangeIL(m_isRangeIl);
            

            //
            // 3. Issue the proper Step() command.
            // 
            
            // 3a. Step-out
            if (m_type == StepperType.Out)
            {
                stepper.StepOut();
                return stepper;
            }

            // 
            // Step In-Over
            // 
            bool stepInto = (m_type == StepperType.In);

            if (m_stepRanges == null)
            {
                // 3b. Single-Step In/Over, no ranges
                stepper.Step(stepInto);
            }
            else
            {
                // 3c. Step-range in/over.
                stepper.StepRange(stepInto, m_stepRanges);
            }
            return stepper;            
        }
    }

    /// <summary>
    /// Source-level stepping type for managed stepping. Stepping behavior also depends on other stepping flags.
    /// </summary>
    public enum StepperType
    {
        /// <summary>
        /// Step into a function 
        /// Set the Interceptor flags to control whether this stops in interecptors like Class constuction.
        /// </summary>
        In,

        /// <summary>
        /// Step out of the current function.
        /// </summary>
        Out,

        /// <summary>
        /// Step over a source line, without stepping into function calls.
        /// At the end of the function, this becomes a step-out.
        /// </summary>
        Over
    }
}