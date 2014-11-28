//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
// Imports the win32 structures needed by the ICorDebug interfaces.
//---------------------------------------------------------------------

using System;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Microsoft.Samples.Debugging.CorMetadata.NativeApi;
using Microsoft.Samples.Debugging.Native;
using Microsoft.Win32.SafeHandles;


namespace Microsoft.Samples.Debugging.CorDebug.NativeApi
{
    #region X86 Context
    [StructLayout(LayoutKind.Sequential)]
    public struct WIN32_CONTEXT
    {
        public uint ContextFlags;
        public uint Dr0;
        public uint Dr1;
        public uint Dr2;
        public uint Dr3;
        public uint Dr6;
        public uint Dr7;
        public WIN32_FLOATING_SAVE_AREA FloatSave;
        public uint SegGs;
        public uint SegFs;
        public uint SegEs;
        public uint SegDs;
        public uint Edi;
        public uint Esi;
        public uint Ebx;
        public uint Edx;
        public uint Ecx;
        public uint Eax;
        public uint Ebp;
        public uint Eip;
        public uint SegCs;
        public uint EFlags;
        public uint Esp;
        public uint SegSs;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x200)]
        public byte[] ExtendedRegisters;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WIN32_FLOATING_SAVE_AREA
    {
        public uint ControlWord;
        public uint StatusWord;
        public uint TagWord;
        public uint ErrorOffset;
        public uint ErrorSelector;
        public uint DataOffset;
        public uint DataSelector;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
        public byte[] RegisterArea;
        public uint Cr0NpxState;
    }

    #endregion // X86 Context


    #region Structures for CreateProcess
    [StructLayout(LayoutKind.Sequential, Pack = 8), ComVisible(false)]
    public class PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public int dwProcessId;
        public int dwThreadId;
        public PROCESS_INFORMATION() { }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8), ComVisible(false)]
    public class SECURITY_ATTRIBUTES
    {
        public int nLength;
        private IntPtr lpSecurityDescriptor;
        public bool bInheritHandle;
        public SECURITY_ATTRIBUTES() { }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 8), ComVisible(false)]
    public class STARTUPINFO
    {
        public int cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public int dwX;
        public int dwY;
        public int dwXSize;
        public int dwYSize;
        public int dwXCountChars;
        public int dwYCountChars;
        public int dwFillAttribute;
        public int dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        private IntPtr lpReserved2;
        public SafeFileHandle hStdInput;
        public SafeFileHandle hStdOutput;
        public SafeFileHandle hStdError;
        public STARTUPINFO() { }
    }

    #endregion // Structures for CreateProcess

} // Microsoft.Samples.Debugging.CorDebug.NativeApi
