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
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    [
        ComVisible(false)
    ]
    public interface ISymbolReader2 : ISymbolReader, IDisposable
    {
        // Initialize the symbol reader with the metadata importer interface
        // that this reader will be associated with, along with the filename
        // of the module. This can only be called once, and must be called
        // before any other reader methods are called.
        //
        // Note: you need only specify one of the filename or the stream,
        // not both. The searchPath parameter is optional.
        //
        void Initialize(Object importer, String filename,
                       String searchPath, IStream stream);

        // Update the existing symbol reader with a delta symbol store. This
        // is used in EnC scenarios as a way to update the symbol store to
        // match deltas to the original PE file.
        //
        // Only one of the filename or stream parameters need be specified.
        // If a filename is specified, the symbol store will be updated with
        // the symbols in that file. If a IStream is specified, the store will
        // be updated with the data from the IStream.
        //       
        void UpdateSymbolStore(String fileName, IStream stream);

        // Update the existing symbol reader with a delta symbol
        // store. This is much like UpdateSymbolStore, but the given detla
        // acts as a complete replacement rather than an update.
        //
        // Only one of the filename or stream parameters need be specified.
        // If a filename is specified, the symbol store will be updated with
        // the symbols in that file. If a IStream is specified, the store will
        // be updated with the data from the IStream.
        //
        void ReplaceSymbolStore(String fileName, IStream stream);

        // Provides the on disk filename of the symbol store.
        //
        String GetSymbolStoreFileName();
        
        // Given a position in a document, return the ISymUnmanagedMethods that
        // contains that position.
        //
        ISymbolMethod[] GetMethodsFromDocumentPosition(
                ISymbolDocument document, int line, int column);
        
        // The document version starts at 1 and is incremented each time
        // the document is updated via UpdateSymbols.
        // bCurrent is true is this is the latest version of the document.
        //
        int GetDocumentVersion(ISymbolDocument document,
                                     out Boolean isCurrent);
        
        // The method version starts at 1 and is incremented each time
        // the method is recompiled.  (This can happen  changes to the method.)
        //
        int GetMethodVersion(ISymbolMethod method);
    }

    // This interface is implemented by the internal SymReader
    // so it could be converted to this and have it's methods called.
    [
        ComVisible(false)
    ]
    public interface ISymbolReaderSymbolSearchInfo
    {
        int GetSymbolSearchInfoCount();
    
        ISymbolSearchInfo[] GetSymbolSearchInfo();
    }
   
}
