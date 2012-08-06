//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Globalization;

using Microsoft.Samples.Debugging.CorDebug.NativeApi;

namespace Microsoft.Samples.Debugging.CorDebug
{

    public enum CorDebuggerVersion
    {
        RTM     = 1, //v1.0
        Everett = 2, //v1.1
        Whidbey = 3, //v2.0
    }

    // copied from Cordebug.idl
    [Flags]
    public enum CorDebugJITCompilerFlags
    {
        CORDEBUG_JIT_DEFAULT = 0x1,
        CORDEBUG_JIT_DISABLE_OPTIMIZATION = 0x3,
        CORDEBUG_JIT_ENABLE_ENC = 0x7
    }

    // keep in sync with CorHdr.h
    public enum CorTokenType
    {
        mdtModule               = 0x00000000,       //          
        mdtTypeRef              = 0x01000000,       //          
        mdtTypeDef              = 0x02000000,       //          
        mdtFieldDef             = 0x04000000,       //           
        mdtMethodDef            = 0x06000000,       //       
        mdtParamDef             = 0x08000000,       //           
        mdtInterfaceImpl        = 0x09000000,       //  
        mdtMemberRef            = 0x0a000000,       //       
        mdtCustomAttribute      = 0x0c000000,       //      
        mdtPermission           = 0x0e000000,       //       
        mdtSignature            = 0x11000000,       //       
        mdtEvent                = 0x14000000,       //           
        mdtProperty             = 0x17000000,       //           
        mdtModuleRef            = 0x1a000000,       //       
        mdtTypeSpec             = 0x1b000000,       //           
        mdtAssembly             = 0x20000000,       //
        mdtAssemblyRef          = 0x23000000,       //
        mdtFile                 = 0x26000000,       //
        mdtExportedType         = 0x27000000,       //
        mdtManifestResource     = 0x28000000,       //
        mdtGenericParam         = 0x2a000000,       //
        mdtMethodSpec           = 0x2b000000,       //
        mdtGenericParamConstraint = 0x2c000000,
        
        mdtString               = 0x70000000,       //          
        mdtName                 = 0x71000000,       //
        mdtBaseType             = 0x72000000,       // Leave this on the high end value. This does not correspond to metadata table
    }

    public abstract class TokenUtils
    {
        public static CorTokenType TypeFromToken(int token)
        {
            return (CorTokenType) ((UInt32)token & 0xff000000);
        }

        public static int RidFromToken(int token)
        {
            return (int)( (UInt32)token & 0x00ffffff);
        }

        public static bool IsNullToken(int token)
        {
            return (RidFromToken(token)==0);
        }
    }


    abstract class HRUtils
    {
        public static bool IsFailingHR(int hr)
        {
            return hr<0;
        }

        public static bool IsSOK(int hr)
        {
            return hr==0;
        }
    }
}
