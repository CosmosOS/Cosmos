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
	
	// Interface does not need to be marked with the serializable attribute
    /// <include file='doc\ISymDocumentWriter.uex' path='docs/doc[@for="ISymbolDocumentWriter"]/*' />
    [
        ComImport,
        Guid("B01FAFEB-C450-3A4D-BEEC-B4CEEC01E006"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        ComVisible(false)
    ]
    internal interface ISymUnmanagedDocumentWriter
    {
        void SetSource(int sourceSize,
                          [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] byte[] source);
    
        void SetCheckSum(Guid algorithmId,
                              int checkSumSize,
                              [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] byte[] checkSum);
    };
        
    
    internal class SymDocumentWriter: ISymbolDocumentWriter
    {
        ISymUnmanagedDocumentWriter m_unmanagedDocumentWriter;
        
        public SymDocumentWriter(ISymUnmanagedDocumentWriter unmanagedDocumentWriter)
        {
            // We should not wrap null instances
            if (unmanagedDocumentWriter == null)
                throw new ArgumentNullException("unmanagedDocumentWriter");

            m_unmanagedDocumentWriter = unmanagedDocumentWriter;
        }
        
        public void SetSource(byte[] source)
        {
            m_unmanagedDocumentWriter.SetSource(source.Length, source);
        }

        public void SetCheckSum(Guid algorithmId, byte[] checkSum)
        {
            m_unmanagedDocumentWriter.SetCheckSum(algorithmId, checkSum.Length, checkSum);
        }

        // Public API
        internal ISymUnmanagedDocumentWriter InternalDocumentWriter
        {
            get
            {
                return m_unmanagedDocumentWriter;
            }
        }
                                      
 
    }
}
