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
    /** Exposes an enumerator for Threads. */
    internal class CorThreadEnumerator : IEnumerable, IEnumerator, ICloneable
    {
        private ICorDebugThreadEnum m_enum;
        private CorThread m_th;

        internal CorThreadEnumerator (ICorDebugThreadEnum threadEnumerator)
        {
            m_enum = threadEnumerator;
        }

        //
        // ICloneable interface
        //
        public Object Clone ()
        {
            ICorDebugEnum clone = null;
            m_enum.Clone (out clone);
            return new CorThreadEnumerator ((ICorDebugThreadEnum)clone);
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
            ICorDebugThread[] a = new ICorDebugThread[1];
            uint c = 0;
            int r = m_enum.Next ((uint) a.Length, a, out c);
            if (r==0 && c==1) // S_OK && we got 1 new element
                m_th = new CorThread (a[0]);
            else
                m_th = null;
            return m_th != null;
        }

        public void Reset ()
        {
            m_enum.Reset ();
            m_th = null;
        }

        public Object Current
        {
            get 
            {
                return m_th;
            }
        }
    } /* class ThreadEnumerator */
} /* namespace */
