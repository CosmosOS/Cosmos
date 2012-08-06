//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;

using Microsoft.Samples.Debugging.CorDebug.NativeApi;

namespace Microsoft.Samples.Debugging.CorDebug
{
    public sealed  class CorClass : WrapperBase
    {
        internal ICorDebugClass m_class;
        
        internal CorClass (ICorDebugClass managedClass)
            : base(managedClass)
        {
            m_class = managedClass;
        }

        [CLSCompliant(false)]
        public ICorDebugClass Raw
        {
            get 
            { 
                return m_class;
            }
        }

        /** The module containing the class */
        public CorModule Module
        {
            get 
            {
                ICorDebugModule m = null;
                m_class.GetModule (out m);
                return new CorModule (m);
            }
        }

        /** The metadata typedef token of the class. */
        public int Token
        {
            get 
            {
                uint td = 0;
                m_class.GetToken (out td);
                return (int) td;
            }
        }

        public bool JMCStatus
        {
            set
            {
                (m_class as ICorDebugClass2).SetJMCStatus(value?1:0);
            }
        }

        public CorType GetParameterizedType(CorElementType elementType, CorType[] typeArguments)
        {
            ICorDebugType[] types = null;
            uint length = 0;
            if (typeArguments != null)
            {
                types = new ICorDebugType[typeArguments.Length];
                for (int i = 0; i < typeArguments.Length; i++)
                    types[i] = typeArguments[i].m_type;
                length = (uint)typeArguments.Length;
            }

            ICorDebugType pType;
            (m_class as ICorDebugClass2).GetParameterizedType(elementType, length, types, out pType);
            return pType==null?null:new CorType (pType);
        }

        public CorValue GetStaticFieldValue(int fieldToken, CorFrame managedFrame)
        {
            ICorDebugValue pValue;
            m_class.GetStaticFieldValue((uint)fieldToken, (managedFrame==null)?null:managedFrame.m_frame, out pValue);
            return pValue==null?null:new CorValue(pValue);
        }

    } /* class Class */

} /* namespace */
