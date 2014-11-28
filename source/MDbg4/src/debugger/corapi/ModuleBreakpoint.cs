//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;

using Microsoft.Samples.Debugging.CorDebug.NativeApi;

namespace Microsoft.Samples.Debugging.CorDebug
{
    public sealed class CorModuleBreakpoint : CorBreakpoint
    {
        private ICorDebugModuleBreakpoint m_br;
          
        internal CorModuleBreakpoint (ICorDebugModuleBreakpoint managedModule): base(managedModule)
        {
            m_br = managedModule;
        }
          
        public CorModule Module
        {
            get 
            {
                ICorDebugModule m = null;
                m_br.GetModule (out m);
                return new CorModule (m);
            }
        }
    } /* class ModuleBreakpoint */
} /* namespace */
