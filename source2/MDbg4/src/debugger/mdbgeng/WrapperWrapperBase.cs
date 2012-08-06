//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Diagnostics;

using Microsoft.Samples.Debugging.CorDebug;

namespace Microsoft.Samples.Debugging.MdbgEngine
{
    /// <summary>
    /// Base class for Wrappers
    /// </summary>
    public abstract class WrapperWrapperBase : MarshalByRefObject
    {
        /// <summary>
        /// Creates a new instance of the WrapperWrapperBase object.
        /// </summary>
        /// <param name="value">The WrapperBase to Wrap.</param>
        protected WrapperWrapperBase(WrapperBase value)
        {
            Debug.Assert(value!=null);
            m_corObject = value;
        }

        /// <summary>
        /// Determines if the wrapped object is equal to another.
        /// </summary>
        /// <param name="value">The object to compare to.</param>
        /// <returns>true if equal, else false.</returns>
        public override bool Equals(Object value) 
        {
            if(!(value is WrapperWrapperBase))
                return false;
            return ((value as WrapperWrapperBase).m_corObject == this.m_corObject);
        }

        /// <summary>
        /// Required to implement MarshalByRefObject.
        /// </summary>
        /// <returns>Hash Code.</returns>
        public override int GetHashCode() 
        {
            return m_corObject.GetHashCode();
        }

        /// <summary>
        /// Equality testing.  Allows for things like "if(thing1 == thing2)" to work properly.
        /// </summary>
        /// <param name="operand">First Operand.</param>
        /// <param name="operand2">Second Operand.</param>
        /// <returns>true if equal, else false.</returns>
        public static bool operator ==( WrapperWrapperBase operand,WrapperWrapperBase operand2)
        {
            if(Object.ReferenceEquals(operand,operand2))
                return true;
            return operand.Equals(operand2);
        }

        /// <summary>
        /// Inequality testing.  Allows for things like "if(thing1 != thing2)" to work properly.
        /// </summary>
        /// <param name="operand">First Operand.</param>
        /// <param name="operand2">Second Operand.</param>
        /// <returns>true if not equal, else false.</returns>
        public static bool operator !=( WrapperWrapperBase operand,WrapperWrapperBase operand2)
        {
            return !(operand==operand2);
        }
        internal WrapperBase m_corObject;
    }
}

