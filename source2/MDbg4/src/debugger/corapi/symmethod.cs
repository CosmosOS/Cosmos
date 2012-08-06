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
	using System.Text;
    
    [
        ComImport,
        Guid("B62B923C-B500-3158-A543-24F307A8B7E1"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        ComVisible(false)
    ]
    interface ISymUnmanagedMethod
    {
        void GetToken(out SymbolToken pToken);
        void GetSequencePointCount(out int retVal);
        void GetRootScope([MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedScope retVal);
        void GetScopeFromOffset(int offset, [MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedScope retVal);
        void GetOffset(ISymUnmanagedDocument document,
                         int line,
                         int column,
                         out int retVal);
        void GetRanges(ISymUnmanagedDocument document,
                          int line,
                          int column,
                          int cRanges,
                          out int pcRanges,
                          [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] int[] ranges);
        void GetParameters(int cParams,
                              out int pcParams,
                              [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] ISymUnmanagedVariable[] parms);
        void GetNamespace([MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedNamespace retVal);
        void GetSourceStartEnd(ISymUnmanagedDocument[] docs,
                                  [In, Out, MarshalAs(UnmanagedType.LPArray)] int[] lines,
                                  [In, Out, MarshalAs(UnmanagedType.LPArray)] int[] columns,
                                  out Boolean retVal);
        void GetSequencePoints(int cPoints,
                                  out int pcPoints,
                                  [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] int[] offsets,
                                  [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] ISymUnmanagedDocument[] documents,
                                  [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] int[] lines,
                                  [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] int[] columns,
                                  [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] int[] endLines,
                                  [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] int[] endColumns);
    }
    
    [
        ComImport,
        Guid("85E891DA-A631-4c76-ACA2-A44A39C46B8C"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        ComVisible(false)
    ]
    internal interface ISymENCUnmanagedMethod
    {
        void GetFileNameFromOffset(int dwOffset,
                                         int cchName,
                                         out int pcchName,
                                         [MarshalAs(UnmanagedType.LPWStr)] StringBuilder name);
    
        void GetLineFromOffset(int dwOffset,
                                   out int pline,
                                   out int pcolumn,
                                   out int pendLine,
                                   out int pendColumn,
                                   out int pdwStartOffset);
    }
    

    internal class SymMethod : ISymbolMethod, ISymbolEnCMethod
    {
        ISymUnmanagedMethod m_unmanagedMethod;

        public SymMethod(ISymUnmanagedMethod unmanagedMethod)
        {
            // We should not wrap null instances
            if (unmanagedMethod == null)
                throw new ArgumentNullException("unmanagedMethod");

            m_unmanagedMethod = unmanagedMethod;
        }
        
        public SymbolToken Token 
        { 
            get
            {
                SymbolToken token;                
                m_unmanagedMethod.GetToken(out token);
                return token;
            }
        }

        public int SequencePointCount
        { 
            get
            {
                int retval = 0;                
                m_unmanagedMethod.GetSequencePointCount(out retval);
                return retval;
            }
        }

        public void GetSequencePoints(int[] offsets,
                               ISymbolDocument[] documents,
                               int[] lines,
                               int[] columns,
                               int[] endLines,
                               int[] endColumns)
        {
            int spCount = 0;
            if (offsets != null)
                spCount = offsets.Length;
            else if (documents != null)
                spCount = documents.Length;
            else if (lines != null)
                spCount = lines.Length;
            else if (columns != null)
                spCount = columns.Length;
            else if (endLines != null)
                spCount = endLines.Length;
            else if (endColumns != null)
                spCount = endColumns.Length;

            // Don't do anything if they're not really asking for anything.
            if (spCount == 0)
                return;

            // Make sure all arrays are the same length.
            if ((offsets != null) && (spCount != offsets.Length))
                throw new ArgumentException();

            if ((lines != null) && (spCount != lines.Length))
                throw new ArgumentException();

            if ((columns != null) && (spCount != columns.Length))
                throw new ArgumentException();

            if ((endLines != null) && (spCount != endLines.Length))
                throw new ArgumentException();

            if ((endColumns != null) && (spCount != endColumns.Length))
                throw new ArgumentException();

            ISymUnmanagedDocument[] unmanagedDocuments = new ISymUnmanagedDocument[documents.Length];
            int cPoints = 0;
            uint i;
            m_unmanagedMethod.GetSequencePoints(documents.Length, out cPoints,
                offsets, unmanagedDocuments,
                lines, columns,
                endLines, endColumns);

            // Create the SymbolDocument form the IntPtr's
            for (i = 0; i < documents.Length; i++)
            {
                documents[i] = new SymbolDocument(unmanagedDocuments[i]);
            }

            return;
  
        }

        public ISymbolScope RootScope
        { 
            get
            {
                ISymUnmanagedScope retval = null;                
                m_unmanagedMethod.GetRootScope(out retval);
                return new SymScope(retval);
            }
        }

        public ISymbolScope GetScope(int offset)
        {
            ISymUnmanagedScope retVal = null;
            m_unmanagedMethod.GetScopeFromOffset(offset, out retVal);
            return new SymScope(retVal);
        }

        public int GetOffset(ISymbolDocument document,
                             int line,
                             int column)
        {
            int retVal = 0;
            m_unmanagedMethod.GetOffset(((SymbolDocument)document).InternalDocument, line, column, out retVal);
            return retVal;
        }

        public int[] GetRanges(ISymbolDocument document,
                               int line,
                               int column)
        {                              
            int cRanges = 0;
            m_unmanagedMethod.GetRanges(((SymbolDocument)document).InternalDocument, line, column, 0, out cRanges, null);
            int[] Ranges = new int[cRanges];
            m_unmanagedMethod.GetRanges(((SymbolDocument)document).InternalDocument, line, column, cRanges, out cRanges, Ranges);
            return Ranges;
        }
                                

        public ISymbolVariable[] GetParameters()
        {
            int cVariables = 0;
            uint i;
            m_unmanagedMethod.GetParameters(0, out cVariables, null);
            ISymUnmanagedVariable[] unmanagedVariables = new ISymUnmanagedVariable[cVariables];
            m_unmanagedMethod.GetParameters(cVariables, out cVariables, unmanagedVariables);

            ISymbolVariable[] Variables = new ISymbolVariable[cVariables];
            for (i = 0; i < cVariables; i++)
            {
                Variables[i] = new SymVariable(unmanagedVariables[i]);
            }

            return Variables;
        }

        public ISymbolNamespace GetNamespace()
        {
            ISymUnmanagedNamespace retVal = null;
            m_unmanagedMethod.GetNamespace(out retVal);
            return new SymNamespace(retVal);
        }

        public bool GetSourceStartEnd(ISymbolDocument[] docs,
                                         int[] lines,
                                         int[] columns)
        {
            uint i;
            bool pRetVal = false;
            int spCount = 0;
            if (docs != null)
                spCount = docs.Length;
            else if (lines != null)
                spCount = lines.Length;
            else if (columns != null)
                spCount = columns.Length;

            // If we don't have at least 2 entries then return an error
            if (spCount < 2)
                throw new ArgumentException();

            // Make sure all arrays are the same length.
            if ((docs != null) && (spCount != docs.Length))
                throw new ArgumentException();

            if ((lines != null) && (spCount != lines.Length))
                throw new ArgumentException();

            if ((columns != null) && (spCount != columns.Length))
                throw new ArgumentException();

            ISymUnmanagedDocument[] unmanagedDocuments = new ISymUnmanagedDocument[docs.Length];
            m_unmanagedMethod.GetSourceStartEnd(unmanagedDocuments, lines, columns, out pRetVal);
            if (pRetVal)
            {
                for (i = 0; i < docs.Length;i++)
                {
                    docs[i] = new SymbolDocument(unmanagedDocuments[i]);
                }
            }
            return pRetVal;
            
        }

        public String GetFileNameFromOffset(int dwOffset)
        {
             int cchName = 0;
             ((ISymENCUnmanagedMethod)m_unmanagedMethod).GetFileNameFromOffset(dwOffset, 0, out cchName, null);
             StringBuilder Name = new StringBuilder(cchName);
             ((ISymENCUnmanagedMethod)m_unmanagedMethod).GetFileNameFromOffset(dwOffset, cchName, out cchName, Name);
             return Name.ToString();
        }
    
        public int GetLineFromOffset(int dwOffset,
                                  out int pcolumn,
                                  out int pendLine,
                                  out int pendColumn,
                                  out int pdwStartOffset)
        {
            int line = 0;
            ((ISymENCUnmanagedMethod)m_unmanagedMethod).GetLineFromOffset(
                dwOffset, out line, out pcolumn, out pendLine, out pendColumn, out pdwStartOffset);
            return line;
        }

        public ISymUnmanagedMethod InternalMethod
        {
            get 
            {
                return m_unmanagedMethod;
            }
        }
    }
       
}
