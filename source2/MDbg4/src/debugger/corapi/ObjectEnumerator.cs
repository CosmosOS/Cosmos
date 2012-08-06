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
     * Exposes an enumerator for Objects. 
     *
     * Apparently the "Object"'s this enumerator returns is the address of
     * each object, not a description of the object itself.
     *
     * At least, the ``Next'' method in the IDL returns a uint64, so there
     * isn't much else it could be returning...
     */
    internal class CorObjectEnumerator : IEnumerable, IEnumerator, ICloneable
    {
        private ICorDebugObjectEnum m_enum;

        private ulong m_obj;

        internal CorObjectEnumerator (ICorDebugObjectEnum objectEnumerator)
        {
            m_enum = objectEnumerator;
        }

        //
        // ICloneable interface
        //
        public Object Clone ()
        {
            ICorDebugEnum clone = null;
            m_enum.Clone (out clone);
            return new CorObjectEnumerator ((ICorDebugObjectEnum)clone);
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
            ulong[] a = new ulong[1];
            uint c = 0;
            int r = m_enum.Next ((uint)a.Length, a, out c);
            if (r==0 && c==1) // S_OK && we got 1 new element
            {
                m_obj = a[0];
                return true;
            }
            return false;
        }

        public void Reset ()
        {
            m_enum.Reset ();
            m_obj = 0;
        }

        public Object Current
        {
            get 
            {
                return m_obj;
            }
        }
    } /* class CorObjectEnumerator */
} /* namespace */
