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
    /** Exposes an enumerator for Modules. */
    internal class CorModuleEnumerator : IEnumerable, IEnumerator, ICloneable
    {
        private ICorDebugModuleEnum m_enum;
        private CorModule m_mod;

        internal CorModuleEnumerator (ICorDebugModuleEnum moduleEnumerator)
        {
            m_enum = moduleEnumerator;
        }

        //
        // ICloneable interface
        //
        public Object Clone ()
        {
            ICorDebugEnum clone = null;
            m_enum.Clone (out clone);
            return new CorModuleEnumerator ((ICorDebugModuleEnum)clone);
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
            ICorDebugModule[] a = new ICorDebugModule[1];
            uint c = 0;
            int r = m_enum.Next ((uint) a.Length, a, out c);
            if (r==0 && c==1) // S_OK && we got 1 new element
                m_mod = new CorModule (a[0]);
            else
                m_mod = null;
            return m_mod != null;
        }

        public void Reset ()
        {
            m_enum.Reset ();
            m_mod = null;
        }

        public Object Current
        {
            get 
            {
                return m_mod;
            }
        }
    } /* class ModuleEnumerator */
} /* namespace  */
