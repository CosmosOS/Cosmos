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
    /** Exposes an enumerator for Assemblies. */
    internal class CorBreakpointEnumerator : IEnumerable, IEnumerator, ICloneable
    {
        private ICorDebugBreakpointEnum m_enum;
        private CorBreakpoint m_br;

        internal CorBreakpointEnumerator (ICorDebugBreakpointEnum breakpointEnumerator)
        {
            m_enum = breakpointEnumerator;
        }

        //
        // ICloneable interface
        //
        public Object Clone ()
        {
            ICorDebugEnum clone = null;
            m_enum.Clone (out clone);
            return new CorBreakpointEnumerator ((ICorDebugBreakpointEnum)clone);
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
            ICorDebugBreakpoint[] a = new ICorDebugBreakpoint[1];
            uint c = 0;
            int r = m_enum.Next ((uint) a.Length, a, out c);
            if (r==0 && c==1) // S_OK && we got 1 new element
            {
                ICorDebugBreakpoint br = a[0];
                throw new NotImplementedException();
                /*
                if(a is ICorDebugFunctionBreakpoint)
                    m_br = new CorFunctionBreakpoint((ICorDebugFunctionBreakpoint)br);
                else if( a is ICorDebugModuleBreakpoint)
                    m_br = new CorModuleBreakpoint((ICorDebugModuleBreakpoint)br);
                else if( a is ICorDebugValueBreakpoint)
                    m_br = new ValueBreakpoint((ICorDebugValueBreakpoint)m_br);
                else
                    Debug.Assert(false);
                */
            }
            else
                m_br = null;
            return m_br != null;
        }

        public void Reset ()
        {
            m_enum.Reset ();
            m_br = null;
        }

        public Object Current
        {
            get 
            {
                return m_br;
            }
        }
    } /* class BreakpointEnumerator */
} /* namespace */
