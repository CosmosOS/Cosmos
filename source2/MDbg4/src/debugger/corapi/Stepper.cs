//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;

using Microsoft.Samples.Debugging.CorDebug.NativeApi;

namespace Microsoft.Samples.Debugging.CorDebug
{
    /** Represents a stepping operation performed by the debugger. */
    public sealed class CorStepper : WrapperBase
    {
        internal CorStepper (ICorDebugStepper stepper)
            :base(stepper)
        {
            m_step = stepper;
        }

        [CLSCompliant(false)]
        public ICorDebugStepper Raw
        {
            get 
            { 
                return m_step;
            }
        }

        /** Is the stepper active and stepping? */
        public bool IsActive ()
        {
            int a = 9;
            m_step.IsActive (out a);
            return !(a==0);
        }

        /** cancel the last stepping command received. */
        public void Deactivate ()
        {
            m_step.Deactivate ();
        }

        /** which intercept code will be stepped into by the debugger? */
        [CLSCompliant(false)]
        public void SetInterceptMask (CorDebugIntercept mask)
        {
            m_step.SetInterceptMask (mask);
        }

        /** Should the stepper stop in jitted code not mapped to IL? */
        [CLSCompliant(false)]
        public void SetUnmappedStopMask (CorDebugUnmappedStop mask)
        {
            m_step.SetUnmappedStopMask (mask);
        }

        /** single step the tread. */
        public void Step (bool into)
        {
            m_step.Step (into ? 1 : 0);
        }

        /** Step until code outside of the range is reached. */
        [CLSCompliant(false)]
        public void StepRange (bool stepInto, COR_DEBUG_STEP_RANGE[] stepRanges)
        {
            m_step.StepRange (stepInto ? 1 : 0, stepRanges, (uint) stepRanges.Length);
        }

        /*
         * Completes after the current frame is returned from normally & the
         * previous frame is reactivated.
         */
        public void StepOut ()
        {
            m_step.StepOut ();
        }

        /** 
         * Set whether the ranges passed to StepRange are relative to the
         * IL code or the native code for the method being stepped in.
         */
        public void SetRangeIL (bool value)
        {
            m_step.SetRangeIL (value ? 1 : 0);
        }

        /// <summary>
        /// Enable Just-my-code stepping for this stepper. The default is 'false.
        /// </summary>
        /// <param name="isJustMyCode">true to make this a JMC-stepper, false to make it a traditional stepper.</param>
        public void SetJmcStatus(bool isJustMyCode)
        {            
            ICorDebugStepper2 stepper2 = m_step as ICorDebugStepper2;
            if (stepper2 != null)
            {
                stepper2.SetJMC(isJustMyCode ? 1 : 0);
            }
        }

        private ICorDebugStepper m_step;
    } /* class Stepper */
} /* namespace */
