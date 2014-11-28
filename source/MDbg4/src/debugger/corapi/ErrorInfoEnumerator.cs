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
     * Exposes an enumerator for ErrorInfo objects. 
     *
     * This is horribly broken at this point, as ErrorInfo isn't implemented yet.
     */
    internal class CorErrorInfoEnumerator : IEnumerable, IEnumerator, ICloneable
    {
        private ICorDebugErrorInfoEnum m_enum;

        private Object m_einfo;

        internal CorErrorInfoEnumerator (ICorDebugErrorInfoEnum erroInfoEnumerator)
        {
            m_enum = erroInfoEnumerator;
        }

        //
        // ICloneable interface
        //
        public Object Clone ()
        {
            ICorDebugEnum clone = null;
            m_enum.Clone (out clone);
            return new CorErrorInfoEnumerator ((ICorDebugErrorInfoEnum)clone);
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
            return false;
        }

        public void Reset ()
        {
            m_enum.Reset ();
            m_einfo = null;
        }

        public Object Current
        {
            get 
            {
                return m_einfo;
            }
        }
    } /* class ErrorInfoEnumerator */
} /* namespace */
