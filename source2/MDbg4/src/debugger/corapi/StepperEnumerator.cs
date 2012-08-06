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
    /** Exposes an enumerator for Steppers. */
    internal class CorStepperEnumerator : IEnumerable, IEnumerator, ICloneable
    {
        private ICorDebugStepperEnum m_enum;
        private CorStepper m_step;

        internal CorStepperEnumerator (ICorDebugStepperEnum stepEnumerator)
        {
            m_enum = stepEnumerator;
        }

        //
        // ICloneable interface
        //
        public Object Clone ()
        {
            ICorDebugEnum clone = null;
            m_enum.Clone (out clone);
            return new CorStepperEnumerator ((ICorDebugStepperEnum)clone);
        }

        //
        // IEnumerable interface
        //
        public IEnumerator GetEnumerator ()
        {
            return this;
        }

        //
        // IEnumerator interface
        //
        public bool MoveNext ()
        {
            ICorDebugStepper[] a = new ICorDebugStepper[1];
            uint c = 0;
            int r = m_enum.Next ((uint) a.Length, a, out c);
            if (r==0 && c==1) // S_OK && we got 1 new element
                m_step = new CorStepper (a[0]);
            else
                m_step = null;
            return m_step != null;
        }

        public void Reset ()
        {
            m_enum.Reset ();
            m_step= null;
        }

        public Object Current
        {
            get 
            {
                return m_step;
            }
        }
    } /* class StepperEnumerator */
} /* namespace  */
