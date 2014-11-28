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
    /** Exposes an enumerator for Types. */
    public class CorTypeEnumerator : IEnumerable, IEnumerator, ICloneable
    {
        private ICorDebugTypeEnum m_enum;
        private CorType m_ty;

        internal CorTypeEnumerator (ICorDebugTypeEnum typeEnumerator)
        {
            m_enum = typeEnumerator;
        }

        //
        // ICloneable interface
        //
        public Object Clone ()
        {
            ICorDebugEnum clone = null;
            if( m_enum!=null )
                m_enum.Clone (out clone);
            return new CorTypeEnumerator ((ICorDebugTypeEnum)clone);
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
            if( m_enum==null )
                return false;
            
            ICorDebugType[] a = new ICorDebugType[1];
            uint c = 0;
            int r = m_enum.Next ((uint) a.Length, a, out c);
            if (r==0 && c==1) // S_OK && we got 1 new element
                m_ty = new CorType (a[0]);
            else
                m_ty = null;
            return m_ty != null;
        }

        public void Reset ()
        {
            if( m_enum!=null )
                m_enum.Reset ();
            m_ty = null;
        }

        public void Skip (int celt)
        {
            m_enum.Skip ((uint)celt);
            m_ty = null;
        }

        public Object Current
        {
            get 
            {
                return m_ty;
            }
        }

        // Returns total elements in the collection.
        public int Count
        {
            get
            {
                if (m_enum == null) return 0;
                uint count = 0;
                m_enum.GetCount(out count);
                return (int) count;
                
            }
        }
    } /* class TypeEnumerator */
} /* namespace */
