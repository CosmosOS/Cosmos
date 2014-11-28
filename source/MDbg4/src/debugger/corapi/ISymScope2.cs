//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------


// These interfaces serve as an extension to the BCL's SymbolStore interfaces.
namespace Microsoft.Samples.Debugging.CorSymbolStore 
{
    using System.Diagnostics.SymbolStore;

    // Interface does not need to be marked with the serializable attribute
    using System;
    using System.Text;
    using System.Runtime.InteropServices;

    
    // This interface isn't directly returned, but SymbolScope which implements ISymbolScope
    // also implements ISymbolScope2 and thus you may want to explicitly cast it to use these methods.
    [
        ComVisible(false)
    ]
    public interface ISymbolScope2 : ISymbolScope
    {
    
        int LocalCount{ get; }
        
        int ConstantCount{ get; }

        ISymbolConstant[] GetConstants();
    
    }
}
