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
    /** Exposes an enumerator for Processes. */
    internal class CorProcessEnumerator : 
        IEnumerable, IEnumerator, ICloneable
    {
        private ICorDebugProcessEnum m_enum;
        private CorProcess m_proc;

        internal CorProcessEnumerator (ICorDebugProcessEnum processEnumerator)
        {
            m_enum = processEnumerator;
        }

        //
        // ICloneable interface
        //
        public Object Clone ()
        {
            ICorDebugEnum clone = null;
            m_enum.Clone (out clone);
            return new CorProcessEnumerator ((ICorDebugProcessEnum)clone);
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
            ICorDebugProcess[] a = new ICorDebugProcess[1];
            uint c = 0;
            int r = m_enum.Next ((uint) a.Length, a, out c);
            if (r==0 && c==1) // S_OK && we got 1 new element
                m_proc =  CorProcess.GetCorProcess(a[0]);
            else
                m_proc = null;
            return m_proc != null;
        }

        public void Reset ()
        {
            m_enum.Reset ();
            m_proc = null;
        }

        public Object Current
        {
            get 
            {
                return m_proc;
            }
        }
    } /* class ProcessEnumerator */
} /* namespace */
