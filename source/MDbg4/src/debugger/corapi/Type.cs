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
    public sealed class CorType : WrapperBase
    {
        internal ICorDebugType m_type;
        
        internal CorType (ICorDebugType type)
            : base(type)
        {
            m_type = type;
        }


        internal ICorDebugType GetInterface ()
        {
            return m_type;
        }

        [CLSCompliant(false)]
        public ICorDebugType Raw
        {
            get 
            { 
                return m_type;
            }
        }

        /** Element type of the type. */
        public CorElementType Type
        {
            get 
            {
                CorElementType type;
                m_type.GetType (out type);
                return type;
            }
        }

        /** Class of the type */
        public CorClass Class
        {
            get 
            {
                ICorDebugClass c = null;
                m_type.GetClass(out c);
                return c==null?null:new CorClass (c);
            }
        }

        public int Rank
        {
            get 
            {
                uint pRank= 0;
                m_type.GetRank (out pRank);
                return (int)pRank;
            }
        }

        // Provide the first CorType parameter in the TypeParameters collection.
        // This is a convenience operator.
        public CorType FirstTypeParameter
        {
            get
            {
                ICorDebugType dt = null;
                m_type.GetFirstTypeParameter(out dt);
                return dt==null?null:new CorType (dt);
            }
        }

        public CorType Base
        {
            get
            {
                ICorDebugType dt = null;
                m_type.GetBase(out dt);
                return dt==null?null:new CorType (dt);
            }
        }

        public CorValue GetStaticFieldValue(int fieldToken, CorFrame frame)
        {
            ICorDebugValue dv = null;
            m_type.GetStaticFieldValue((uint)fieldToken, frame.m_frame, out dv);
            return dv==null?null:new CorValue (dv);
        }

        // Expose IEnumerable, which can be used with for-each constructs.
        // This will provide an collection of CorType parameters.
        public IEnumerable TypeParameters
        {
            get
            {
                ICorDebugTypeEnum etp = null;
                m_type.EnumerateTypeParameters (out etp);
                if (etp==null) return null;
                return new CorTypeEnumerator (etp);
            }
        }
    } /* class Type */
} /* namespace */
