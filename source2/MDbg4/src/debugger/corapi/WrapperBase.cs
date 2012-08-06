//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Diagnostics;

namespace Microsoft.Samples.Debugging.CorDebug
{
    /* This class is base class for all the wrapper classes we have.
     * We overload equivalance operators, so that we can figure out,
     * compare object.
     *
     * The reason why we have to have this is described here:
     * Suppose we have a Wrapper WX for COM object X. We create an object WX
     * with operator new. The wrapper will create in it's constructor real
     * instance of native object X.
     * Now suppose that some oter native object Y have callback with an argument
     * X. We create a wrapper for it WY, which will implment the
     * callback that way that it converts argument X to WX and calls some event
     * defined in WY OnXXX. The conversion from X to WX is usually done by
     * creating new wrapper WX and attaching it to X.
     *
     * But now we cannot determine if the object WX returned from callback in WY
     * is the same as the one we have a reference to. We cannot use == operator
     * becuase we have two different wrappers.
     *
     * This class WrapperBase overloads ==,GetHashCode and Equals classes that
     * way that they operate on the inner pointers to native interfaces rather
     * than on wrapper object themselves.
     *
     * Also note that the COMobject is of type Object. This will actually cast
     * an object to IUnknown, which is the only realiable way to compare two
     * COM objects
     *
     * An alternative to the design of overloading == opeartor would be to have
     * a hash table of X=>WX and on each callback instead of creating new
     * wrapper, lookup an existing wrapper for an object.
     * I didn't use this technique here, because the debugger interfaces are
     * havily based on callbacks an looking up something in hashtable is more
     * expansive operation than creating new wrapper. Further the wrappers are
     * really light-weight -- they generally contain only pointer to the COM
     * obejct.
     */

    public abstract class WrapperBase : MarshalByRefObject
    {
        protected WrapperBase(Object value)
        {
            Debug.Assert(value!=null);
            m_comObject = value;
        }

        public override bool Equals(Object value) 
        {
            if(!(value is WrapperBase))
                return false;
            return ((value as WrapperBase).m_comObject == this.m_comObject);
        }
            
        public override int GetHashCode() 
        {
            return m_comObject.GetHashCode();
        }

        public static bool operator ==( WrapperBase operand,WrapperBase operand2)
        {
            if(Object.ReferenceEquals(operand,operand2))
                return true;

            if(Object.ReferenceEquals(operand, null))               // this means that operand==null && operand2 is not null 
                return false;

            return operand.Equals(operand2);
        }
        
        public static bool operator !=( WrapperBase operand,WrapperBase operand2)
        {
            return !(operand==operand2);
        }

        private Object m_comObject;
    }
    
} /* namespace */
