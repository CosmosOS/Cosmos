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

    // This interface is returned by ISymbolReaderSymbolSearchInfo
    // and thus must be public
    [
        ComVisible(false)
    ]
    public interface ISymbolSearchInfo
    {
        int SearchPathLength{ get; }
    
        String SearchPath{ get; }
    
        int HResult{ get; }
    }
}
