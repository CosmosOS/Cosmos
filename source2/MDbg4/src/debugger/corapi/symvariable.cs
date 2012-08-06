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
    using System.Runtime.InteropServices.ComTypes;

    [
        ComImport,
        Guid("9F60EEBE-2D9A-3F7C-BF58-80BC991C60BB"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        ComVisible(false)
    ]
    internal interface ISymUnmanagedVariable 
    {
        void GetName(int cchName,
                        out int pcchName,
                        [MarshalAs(UnmanagedType.LPWStr)] StringBuilder szName);

        void GetAttributes(out int pRetVal);

        void GetSignature(int cSig,
                             out int pcSig,
                             [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] byte[] sig);

        void GetAddressKind(out int  pRetVal);

        void GetAddressField1(out int  pRetVal);

        void GetAddressField2(out int  pRetVal);

        void GetAddressField3(out int  pRetVal);

        void GetStartOffset(out int  pRetVal);

        void GetEndOffset(out int  pRetVal);
    }    

    /// <include file='doc\ISymVariable.uex' path='docs/doc[@for="ISymbolVariable"]/*' />

    internal class SymVariable : ISymbolVariable
    {
        ISymUnmanagedVariable m_unmanagedVariable;

        internal SymVariable(ISymUnmanagedVariable variable)
        {
            // We should not wrap null instances
            if (variable == null)
                throw new ArgumentNullException("variable");

            m_unmanagedVariable = variable;
        }
        
        public String Name 
        {
            get
            {
                StringBuilder Name;
                int cchName;
                m_unmanagedVariable.GetName(0, out cchName, null);
                Name = new StringBuilder(cchName);
                m_unmanagedVariable.GetName(cchName, out cchName, Name);
                return Name.ToString();
            }
          }

        public Object Attributes 
        { 
            get
            {
                int RetVal;
                m_unmanagedVariable.GetAttributes(out RetVal);
                return (object)RetVal;
            }
        }

        public byte[] GetSignature()
        {
            byte[] Data;
            int cData;
            m_unmanagedVariable.GetSignature(0, out cData, null);
            Data = new byte[cData];
            m_unmanagedVariable.GetSignature(cData, out cData, Data);
            return Data;
        }
    
        public SymAddressKind AddressKind 
        {
           get
           {
                int RetVal;
                m_unmanagedVariable.GetAddressKind(out RetVal);
                return (SymAddressKind)RetVal;
           }
        }

        public int AddressField1 
        {
            get
            {
                int RetVal;
                m_unmanagedVariable.GetAddressField1(out RetVal);
                return RetVal;
            }
        }

        public int AddressField2 
        {
            get
            {
                int RetVal;
                m_unmanagedVariable.GetAddressField2(out RetVal);
                return RetVal;
            }
        }

        public int AddressField3 
        { 
            get
            {
                int RetVal;
                m_unmanagedVariable.GetAddressField3(out RetVal);
                return RetVal;
            }
        }

        public int StartOffset 
        { 
            get
            {
                int RetVal;
                m_unmanagedVariable.GetStartOffset(out RetVal);
                return RetVal;
            }
        }

        public int EndOffset 
        { 
            get
            {
                int RetVal;
                m_unmanagedVariable.GetEndOffset(out RetVal);
                return RetVal;
            }
        }
    }
}
