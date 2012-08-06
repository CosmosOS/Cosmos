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
    /** Exposes an enumerator for AppDomains. */
    internal class CorAppDomainEnumerator : IEnumerable, IEnumerator, ICloneable
    {
        private ICorDebugAppDomainEnum m_enum;
        private CorAppDomain m_ad;

        internal CorAppDomainEnumerator (ICorDebugAppDomainEnum appDomainEnumerator)
        {
            m_enum = appDomainEnumerator;
        }

        //
        // ICloneable interface
        //
        public Object Clone ()
        {
            ICorDebugEnum clone = null;
            m_enum.Clone (out clone);
            return new CorAppDomainEnumerator ((ICorDebugAppDomainEnum)clone);
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
            ICorDebugAppDomain[] a = new ICorDebugAppDomain [1];
            uint c = 0;
            int r = m_enum.Next ((uint) a.Length, a, out c);
            if (r==0 && c==1) // S_OK && we got 1 new element
                m_ad = new CorAppDomain (a[0]);
            else
                m_ad = null;
            return m_ad != null;
        }

        public void Reset ()
        {
            m_enum.Reset ();
            m_ad = null;
        }

        public Object Current
        {
            get 
            {
                return m_ad;
            }
        }
    } /* class AppDomainEnumerator */
} /* namespace */
