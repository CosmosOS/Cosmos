//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------


// These interfaces serve as an extension to the BCL's SymbolStore interfaces.
namespace Microsoft.Samples.Debugging.CorSymbolStore 
{
    using System.Diagnostics.SymbolStore;
    
    using System;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    [
        ComVisible(false)
    ]
    public interface ISymbolBinder2
    {
        ISymbolReader GetReaderForFile(Object importer, String filename, String searchPath);
                                
        ISymbolReader GetReaderForFile(Object importer, String fileName,
                                           String searchPath, SymSearchPolicies searchPolicy);
        
        ISymbolReader GetReaderForFile(Object importer, String fileName,
                                           String searchPath, SymSearchPolicies searchPolicy,
                                           object callback);
      
        ISymbolReader GetReaderFromStream(Object importer, IStream stream);
    }
}
