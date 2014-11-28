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
    internal class CorAssemblyEnumerator : IEnumerable, IEnumerator, ICloneable
    {
        private ICorDebugAssemblyEnum m_enum;
        private CorAssembly m_asm;

        internal CorAssemblyEnumerator (ICorDebugAssemblyEnum assemblyEnumerator)
        {
            m_enum = assemblyEnumerator;
        }

        //
        // ICloneable interface
        //
        public Object Clone ()
        {
            ICorDebugEnum clone = null;
            m_enum.Clone (out clone);
            return new CorAssemblyEnumerator ((ICorDebugAssemblyEnum)clone);
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
            ICorDebugAssembly[] a = new ICorDebugAssembly[1];
            uint c = 0;
            int r = m_enum.Next ((uint) a.Length, a, out c);
            if (r==0 && c==1) // S_OK && we got 1 new element
                m_asm = new CorAssembly (a[0]);
            else
                m_asm = null;
            return m_asm != null;
        }

        public void Reset ()
        {
            m_enum.Reset ();
            m_asm = null;
        }

        public Object Current
        {
            get 
            {
                return m_asm;
            }
        }
    } /* class AssemblyEnumerator */
} /* namespace */
