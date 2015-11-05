//---------------------------------------------------------------------
//  This file is part of the Microsoft .NET Framework SDK Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
// 
//This source code is intended only as a supplement to Microsoft
//Development Tools and/or on-line documentation.  See these other
//materials for detailed information regarding Microsoft code samples.
// 
//THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY
//KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//PARTICULAR PURPOSE.
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

using Microsoft.Samples.Tools.Mdbg;
using Microsoft.Samples.Debugging.MdbgEngine;
using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;


namespace Microsoft.Samples.Tools.Mdbg.Extension
{
    public static class NativeMethods
    {
        public const string Kernel32LibraryName = "kernel32.dll";
        public const string MscordbiLibraryName = "mscordbi.dll";
        public const string ShimLibraryName = "mscoree.dll";
        public const string Ole32LibraryName = "ole32.dll";
        public const string NativeDumpGenLibraryName = "NativeDumpGen.dll";
        public const string DbgHelpLibraryName = "dbghelp.dll";
        public const string PsapiLibraryName = "psapi.dll";
        public const int MAX_PATH = 260;

        #region kernel32.dll Imports
        /*****************************************************************************/
        /*************  Imported methods from kernel32.dll  **************************/
        /*****************************************************************************/

        [DllImport(Kernel32LibraryName, SetLastError = true)]
        public static extern int CreateProcess(
            string applicationName,
            string commandLine,
            SECURITY_ATTRIBUTES processAttributes,
            SECURITY_ATTRIBUTES threadAttributes,
            bool bInheritHandles,
            int dwCreationFlags,
            IntPtr environment,
            string currentDirectory,
            STARTUPINFO startupInfo,
            PROCESS_INFORMATION processInformation);

        [DllImport(Kernel32LibraryName)]
        public static extern IntPtr CreateToolhelp32Snapshot(
            [MarshalAs(UnmanagedType.U4)] SnapshotFlags flags,
            [MarshalAs(UnmanagedType.U4)] int pid);


        [DllImport(Kernel32LibraryName, SetLastError = true)]
        public static extern bool DuplicateHandle(
            IntPtr hSourceProcessHandle,
            IntPtr hSourceHandle,
            IntPtr hTargetProcessHandle,
            out IntPtr targetHandle,
            int dwDesiredAccess,
            bool inheritHandle,
            int dwOptions);

        [DllImport(Kernel32LibraryName, SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();
[DllImport(Kernel32LibraryName, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern IntPtr GetProcAddress(
            IntPtr hModule,
            string procName);

        [DllImport(Kernel32LibraryName)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport(Kernel32LibraryName)]
        public static extern bool Process32First(
            IntPtr hSnapshot,
            ref PROCESSENTRY32 pe32);

        [DllImport(Kernel32LibraryName)]
        public static extern bool Process32Next(
            IntPtr hSnapshot,
            ref PROCESSENTRY32 pe32);

        [DllImport(Kernel32LibraryName, SetLastError = true)]
        public static extern int ResumeThread(IntPtr hThread);

        [DllImport(Kernel32LibraryName)]
        public static extern bool Thread32First(
            IntPtr hSnapshot,
            ref THREADENTRY32 te32);

        [DllImport(Kernel32LibraryName)]
        public static extern bool Thread32Next(
            IntPtr hSnapshot,
            ref THREADENTRY32 te32);

        [DllImport(Kernel32LibraryName, SetLastError = true)]
        public static extern uint WaitForSingleObject(
            IntPtr hObject,
            int dwMilliSeconds);

        #endregion


        #region Miscellaneous Native Objects

        [Flags]
        public enum DuplicateHandleOptions : uint
        {
            DUPLICATE_CLOSE_SOURCE = 0x1,
            DUPLICATE_SAME_ACCESS = 0x2,
        }    

        [Flags]
        public enum ProcessCreationFlags : int
        {
            CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_NO_WINDOW = 0x08000000,
            CREATE_PROTECTED_PROCESS = 0x00040000,
            CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
            CREATE_SEPARATE_WOW_VDM = 0x00000800,
            CREATE_SHARED_WOW_VDM = 0x00001000,
            CREATE_SUSPENDED = 0x00000004,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            DEBUG_ONLY_THIS_PROCESS = 0x00000002,
            DEBUG_PROCESS = 0x00000001,
            DETACHED_PROCESS = 0x00000008,
            EXTENDED_STARTUPINFO_PRESENT = 0x00080000
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESSENTRY32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExeFile;
        }

        [Flags]
        public enum SnapshotFlags : uint
        {
            TH32CS_SNAPHEAPLIST = 0x1,
            TH32CS_SNAPPROCESS = 0x2,
            TH32CS_SNAPTHREAD = 0x4,
            TH32CS_SNAPMODULE = 0x8,
            TH32CS_SNAPMODULE32 = 0x10,
            TH32CS_SNAPALL = 0xF,
            TH32CS_INHERIT = 0x80000000,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct THREADENTRY32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ThreadID;
            public uint th32OwnerProcessID;
            public int tpBasePri;
            public int tpDeltaPri;
            public uint dwFlags;
        }

        [Flags]
        public enum WaitForSingleObjectReturnValues : uint
        {
            WAIT_OBJECT_0 = 0x00000000, // The state of the specified object is signaled
            WAIT_ABANDONED = 0x00000080, // Mutex object was abandonded by holding thread
            WAIT_TIMEOUT = 0x00000102, // Object was still not-signalled at timeout
            WAIT_FAILED = 0xFFFFFFFF, // The function has failed. Call GetLastError() for more info
        }

        #endregion
    }


}
