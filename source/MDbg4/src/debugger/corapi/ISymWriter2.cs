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
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    [StructLayout(LayoutKind.Sequential)]
    public struct ImageDebugDirectory {
        int     Characteristics;
        int     TimeDateStamp;
        short   MajorVersion;
        short   MinorVersion;
        int     Type;
        int     SizeOfData;
        int     AddressOfRawData;
        int     PointerToRawData;

        public override string ToString()
        {
            return String.Format( @"Characteristics: {0}
TimeDateStamp: {1}
MajorVersion: {2}
MinorVersion: {3}
Type: {4}
SizeOfData: {5}
AddressOfRawData: {6}
PointerToRawData: {7}
", 
                      Characteristics, 
                      TimeDateStamp, 
                      MajorVersion, 
                      MinorVersion, 
                      Type, 
                      SizeOfData, 
                      AddressOfRawData, 
                      PointerToRawData);
        }
    };

    [
        ComVisible(false)
    ]
    public interface ISymbolWriter2 : ISymbolWriter, IDisposable
    {
        void Initialize(Object emitter,
                    String fileName,
                    Boolean fullBuild);

        void Initialize(Object emitter,
                        String fileName,
                        IStream stream,
                        Boolean fullBuild);

        void Initialize(Object emitter,
                        String temporaryFileName,
                        IStream stream,
                        Boolean fullBuild,
                        String finalFileName);

        byte[] GetDebugInfo(out ImageDebugDirectory imageDebugDirectory);
                             
        void RemapToken(SymbolToken oldToken,
                            SymbolToken newToken);
                             
        void DefineConstant(String name,
                               Object value,
                               byte[] signature);
    
        void Abort();   

        void DefineLocalVariable(String name,
                                     int attributes,
                                     SymbolToken sigToken,
                                     int addressKind,
                                     int addr1,
                                     int addr2,
                                     int addr3,
                                     int startOffset,
                                     int endOffset);
    
        void DefineGlobalVariable(String name,
                                       int attributes,
                                       SymbolToken sigToken,
                                       int addressKind,
                                       int addr1,
                                       int addr2,
                                       int addr3);
        
        
         void DefineConstant(String name,
                                  Object value,
                                  SymbolToken sigToken);
    }
}
