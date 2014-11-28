//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------


// These interfaces serve as an extension to the BCL's SymbolStore interfaces.
namespace Microsoft.Samples.Debugging.CorSymbolStore 
{
    using System.Diagnostics.SymbolStore;

	using System.Runtime.InteropServices;
	using System;

    // This interface isn't directly returned or used by any of the classes,
    // but the implementation of the ISymbolMethod also implements ISymEncMethod
    // so you could explicitly cast it to that.
    [
        ComVisible(false)
    ]
    public interface ISymbolEnCMethod: ISymbolMethod
    {
        String GetFileNameFromOffset(int dwOffset);
   
        int GetLineFromOffset(int dwOffset,
                                  out int column,
                                  out int endLine,
                                  out int endColumn,
                                  out int startOffset);
    }
}

