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
    /** 
     * Exposes an enumerator for Chains. 
     *
     * This is horribly broken at this point, as Chains aren't implemented yet.
     */
    internal class CorChainEnumerator : IEnumerable, IEnumerator, ICloneable
    {
        private ICorDebugChainEnum m_enum;

        private CorChain m_chain;

        internal CorChainEnumerator (ICorDebugChainEnum chainEnumerator)
        {
            m_enum = chainEnumerator;
        }

        //
        // ICloneable interface
        //
        public Object Clone ()
        {
            ICorDebugEnum clone = null;
            m_enum.Clone (out clone);
            return new CorChainEnumerator ((ICorDebugChainEnum)clone);
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
            ICorDebugChain[] a = new ICorDebugChain[1];
            uint c = 0;
            int r = m_enum.Next ((uint)a.Length, a, out c);
            if (r==0 && c==1) // S_OK && we got 1 new element
                m_chain = new CorChain (a[0]);
            else
                m_chain = null;
            return m_chain != null;
        }

        public void Reset ()
        {
            m_enum.Reset ();
            m_chain = null;
        }

        public Object Current
        {
            get 
            {
                return m_chain;
            }
        }
    } /* class ChainEnumerator */
} /* namespace */
