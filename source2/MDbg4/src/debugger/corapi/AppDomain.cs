//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Text;

using Microsoft.Samples.Debugging.CorDebug.NativeApi;

namespace Microsoft.Samples.Debugging.CorDebug
{
    public sealed class CorAppDomain : CorController
    {
        /** Create an CorAppDomain object. */
        internal CorAppDomain (ICorDebugAppDomain appDomain)
            : base (appDomain)
        {
        }

        [CLSCompliant(false)]
        public ICorDebugAppDomain Raw
        {
            get 
            { 
                return _ad();
            }
        }

        /** Get the ICorDebugAppDomain interface back from the Controller. */
        private ICorDebugAppDomain _ad ()
        {
            return (ICorDebugAppDomain) GetController();
        }

        /** Get the process containing the CorAppDomain. */
        public CorProcess Process
        {
            get
            {
                ICorDebugProcess proc = null;
                _ad().GetProcess (out proc);
                return  CorProcess.GetCorProcess (proc);
            }
        }

        /** Get all Assemblies in the CorAppDomain. */
        public IEnumerable Assemblies
        {
            get
            {
                ICorDebugAssemblyEnum eas = null;
                _ad().EnumerateAssemblies (out eas);
                return new CorAssemblyEnumerator (eas);
            }
        }

        /** All active breakpoints in the CorAppDomain */
        public IEnumerable Breakpoints
        {
            get
            {
                ICorDebugBreakpointEnum bpoint = null;
                _ad().EnumerateBreakpoints (out bpoint);
                return new CorBreakpointEnumerator (bpoint);
            }
        }

        /** All active steppers in the CorAppDomain */
        public IEnumerable Steppers
        {
            get
            {
                ICorDebugStepperEnum step = null;
                _ad().EnumerateSteppers (out step);
                return new CorStepperEnumerator (step);
            }
        }

        /** Is the debugger attached to the CorAppDomain? */
        public bool IsAttached ()
        {
            int attach = 0;
            _ad().IsAttached (out attach);
            return !(attach==0);
        }

        /** The name of the CorAppDomain */
        public String Name
        {
            get 
            {
                uint size = 0;
                _ad().GetName (0, out size,  null);
                StringBuilder szName = new StringBuilder((int)size);
                _ad().GetName ((uint)szName.Capacity, out size,  szName);
                return szName.ToString();
            }
        }

        /** Get the runtime App domain object */
        public CorValue AppDomainVariable
        {
            get
            {
                ICorDebugValue val = null;
                _ad().GetObject (out val);
                return new CorValue (val);
            }
        }

        /** 
         * Attach the AppDomain to receive all CorAppDomain related events (e.g.
         * load assembly, load module, etc.) in order to debug the AppDomain.
         */
        public void Attach ()
        {
            _ad().Attach ();
        }

        /** Get the ID of this CorAppDomain */
        public int Id
        {
            get
            {
                uint id = 0;
                _ad().GetID (out id);
                return (int) id;
            }
        }

        /** Returns CorType object for an array of or pointer to the given type */
        public CorType GetArrayOrPointerType(CorElementType elementType, int rank, CorType parameterTypes)
        {
            ICorDebugType ct = null;
            uint urank = (uint) rank;
            (_ad() as ICorDebugAppDomain2).GetArrayOrPointerType(elementType, urank, parameterTypes.m_type, out ct);
            return ct==null?null:new CorType (ct);
        }
        
        /** Returns CorType object for a pointer to a function */
        public CorType GetFunctionPointerType(CorType[] parameterTypes)
        {
            ICorDebugType[] types = null;
            if (parameterTypes != null)
            {
                types = new ICorDebugType[parameterTypes.Length];
                for (int i = 0; i < parameterTypes.Length; i++)
                    types[i] = parameterTypes[i].m_type;
            }

            ICorDebugType ct = null;
            (_ad() as ICorDebugAppDomain2).GetFunctionPointerType((uint)types.Length, types, out ct);
            return ct==null?null:new CorType (ct);
        }

    }
}
