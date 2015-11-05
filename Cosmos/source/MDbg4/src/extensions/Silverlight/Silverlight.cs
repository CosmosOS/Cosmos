using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.Security.AccessControl;

using Microsoft.Samples.Tools.Mdbg;
using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.MdbgEngine;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;
using Microsoft.Samples.Debugging.CorPublish;


namespace Microsoft.Samples.Tools.Mdbg.Extension
{
    [MDbgExtensionEntryPointClass]
    public abstract class SilverlightExtension : CommandBase
    {
        public enum CorDebugInterfaceVersion
        {
            CorDebugInvalidVersion = 0,
            CorDebugVersion_1_0 = CorDebugInvalidVersion + 1,
            CorDebugVersion_1_1 = CorDebugVersion_1_0 + 1,
            CorDebugVersion_2_0 = CorDebugVersion_1_1 + 1,
            CorDebugVersion_4_0 = CorDebugVersion_2_0 + 1,
            CorDebugLatestVersion = CorDebugVersion_4_0
        };

        private static string sVersionString;

        /// <summary>
        /// Loads the Silverlight Extension for Mdbg
        /// </summary>
        public static void LoadExtension()
        {
            sVersionString = "";

            MDbgAttributeDefinedCommand.AddCommandsFromType(Shell.Commands, typeof(SilverlightExtension));

            WriteOutput("Silverlight Extension Loaded");
        }

        /// <summary>
        /// Unloads the Silverlight Extension for Mdbg
        /// </summary>
        public static void UnloadExtension()
        {
           MDbgAttributeDefinedCommand.RemoveCommandsFromType(Shell.Commands, typeof(SilverlightExtension));

           WriteOutput("Silverlight Extension Unloaded");
        }

        /// <summary>
        /// Allows debugger to attach to a Silverlight instance.
        /// Overrides the default "attach" command.
        /// </summary>
        /// <param name="arguments"></param>
        [CommandDescription(
            CommandName = "attach",
            MinimumAbbrev = 1,
            IsRepeatable = false,
            ShortHelp = "Allows user to attach to a Silverlight (running inside a host) under the debugger.",
            LongHelp = "Allows user to attach to a Silverlight (running inside a host) under the debugger.\n" +
                      "Usage: attach <pid>"
        )]
        public static void AttachCmd(string arguments)
        {
            ArgParser ap = new ArgParser(arguments);

            if (ap.Count > 1)
            {
                throw new MDbgShellException("Wrong # of arguments.");
            }

            if (!ap.Exists(0))
            {
                throw new MDbgShellException("Wrong # of arguments.");
            }

            int pid = ap.AsInt(0);

            if (Process.GetCurrentProcess().Id == pid)
            {
                throw new MDbgShellException("Cannot attach to myself!");
            }
           
            // time to attach, since the user isn't able to specify the DebugModeFlag for attach, use Debug which is the
            // default
            if (false == DebugActiveSilverlightProcess(pid, DebugModeFlag.Debug))
            {
                throw new MDbgShellException("Could not find a match for *");
            }
        }

        private static bool DebugActiveSilverlightProcess(int processId, DebugModeFlag debugMode)
        {
            MDbgProcess p = null;
            string[] fullPaths;
            EventWaitHandle[] continueStartupEvents;
            bool bMatchFound = false;

            // some pre-condition checks
            if (processId <= 0)
            {
                throw new MDbgShellException("Invalid arguments passed in");
            }

            // Get all funcs exported by the coreclr's dbg shim.
            Silverlight.InitSLApi();

            // Enumerate all coreclr instances in the process
            Silverlight.EnumerateCLRs((uint)processId, out fullPaths, out continueStartupEvents);
            int nSilverlight = fullPaths.Length;
            if (fullPaths == null || nSilverlight == 0)
            {
                throw new MDbgShellException("Could not enumerate any CLRs in specifed Silverlight process");
            }

            // for each coreclr instance found.....
            for (int i = 0; i < nSilverlight && !bMatchFound; i++)
            {
                  
                // Attach to the first one

                WriteOutput("FOUND: " + fullPaths[i]);
                string slVersion = Silverlight.CreateVersionStringFromModule((uint)processId, fullPaths[i]);
                sVersionString = slVersion;
                // we'll get the required ICorDebug interface from dbgshim.dll
                ICorDebug cordbg = null;
                try
                {
                    cordbg = Silverlight.CreateDebuggingInterfaceFromVersionEx(CorDebugInterfaceVersion.CorDebugLatestVersion, slVersion);
                }
                catch (COMException ce)
                {
                    Console.WriteLine("CDIFVEx failed, will retry with CDIFV.\n" + ce.ToString());
                }

                if (cordbg == null)
                {
                    cordbg = Silverlight.CreateDebuggingInterfaceFromVersion(slVersion);
                }

                p = GetProcessFromCordb(cordbg);

                // specify JIT flages here
                p.DebugMode = debugMode;
                p.Attach((int)processId);
                bMatchFound = true;
               

                // signal the continue event

                if (!continueStartupEvents[i].SafeWaitHandle.IsInvalid)
                {
                    continueStartupEvents[i].Set();
                }

                if (null != p)
                {
                    p.Go().WaitOne();
                }
            }

            return bMatchFound;
        }

        static private MDbgProcess GetProcessFromCordb(ICorDebug cordbg)
        {
            Debugger.Processes.FreeStaleUnmanagedResources();
            CorDebugger cordebugger = new CorDebugger(cordbg);
            Debugger.Processes.RegisterDebuggerForCleanup(cordebugger);
            return new MDbgProcess(Debugger, cordebugger);
        }
    }



    //////////////////////////////////////////////////////////////////////////////////
    //
    // Helper class for MDbg's Silverlight extension 
    //
    //////////////////////////////////////////////////////////////////////////////////
    public class Silverlight
    {

        private static bool m_bSLApiInit = false;
        private static string m_strNetFramework = @"SOFTWARE\Microsoft\.NETFramework";
        private static string m_strValDbgPackShimPath = "DbgPackShimPath";

        private static string m_strDbgPackShimPath = null;

        private static delgCreateDebuggingInterfaceFromVersion m_CreateDebuggingInterfaceFromVersion;
        private static delgCreateDebuggingInterfaceFromVersionEx m_CreateDebuggingInterfaceFromVersionEx;
        private static delgCreateVersionStringFromModule m_CreateVersionStringFromModule;
        private static delgEnumerateCLRs m_EnumerateCLRs;
        private static delgCloseCLREnumeration m_CloseCLREnumeration;
        private static delgGetStartupNotificationEvent m_GetStartupNotificationEvent;


        private static IntPtr m_hndModuleMscoree = IntPtr.Zero;
        private static IntPtr m_hndModuleMscordbi_MacX86 = IntPtr.Zero;

        public static bool IsProcessActive(IntPtr hProcess)
        {
            return ((uint)(NativeMethods.WaitForSingleObjectReturnValues.WAIT_OBJECT_0) !=
                WaitForSingleObj(hProcess, 0));
        }


        public static void InitSLApi()
        {
            // check if already initialized
            if (true == m_bSLApiInit)
            {
                return;
            }

            // we must check if debug pack is installed

            Win32.RegistryKey netFrameworkKey = null;
            IntPtr procAddr = IntPtr.Zero;

            try
            {
                netFrameworkKey = Win32.Registry.LocalMachine.OpenSubKey(
                    m_strNetFramework,
                    false);

                if (null == netFrameworkKey)
                {
                    throw new System.Exception("Unable to open the .NET Registry key: HKLM\\" + m_strNetFramework);
                }

                m_strDbgPackShimPath = (string)netFrameworkKey.GetValue(
                    m_strValDbgPackShimPath,
                    null,
                    RegistryValueOptions.DoNotExpandEnvironmentNames);

                if (null == m_strDbgPackShimPath)
                {
                    throw new System.Exception("Unable to open up the DbgPackShimPath registry key: HKLM\\" + m_strNetFramework + "\\" + m_strDbgPackShimPath);
                }

                // now initializing all the func ptrs
                m_hndModuleMscoree = LoadLib(m_strDbgPackShimPath);

                procAddr = GetProcAddr(m_hndModuleMscoree, "CreateDebuggingInterfaceFromVersion");
                if (IntPtr.Zero == procAddr)
                {
                    throw new Exception("Failed to get address of dbgshim!CreateDebuggingInterfaceFromVersion");
                }
                else
                {
                    m_CreateDebuggingInterfaceFromVersion = (delgCreateDebuggingInterfaceFromVersion)Marshal.GetDelegateForFunctionPointer(
                            procAddr,
                            typeof(delgCreateDebuggingInterfaceFromVersion));
                }

                try
                {
                    procAddr = GetProcAddr(m_hndModuleMscoree, "CreateDebuggingInterfaceFromVersionEx");
                }
                catch (Win32Exception)
                {
                    procAddr = IntPtr.Zero;
                }
                if (IntPtr.Zero == procAddr)
                {
                    m_CreateDebuggingInterfaceFromVersionEx = null;
                    Console.WriteLine("This Silverlight/CoreCLR version doesn't have CDIFVex");
                }
                else
                {
                    m_CreateDebuggingInterfaceFromVersionEx = (delgCreateDebuggingInterfaceFromVersionEx)Marshal.GetDelegateForFunctionPointer(
                            procAddr,
                            typeof(delgCreateDebuggingInterfaceFromVersionEx));
                }

                procAddr = GetProcAddr(m_hndModuleMscoree, "CreateVersionStringFromModule");
                if (IntPtr.Zero == procAddr)
                {
                    throw new Exception("Failed to get address of dbgshim!CreateVersionStringFromModule");
                }
                else
                {
                    m_CreateVersionStringFromModule = (delgCreateVersionStringFromModule)Marshal.GetDelegateForFunctionPointer(
                            procAddr,
                            typeof(delgCreateVersionStringFromModule));
                }

                procAddr = GetProcAddr(m_hndModuleMscoree, "EnumerateCLRs");
                if (IntPtr.Zero == procAddr)
                {
                    throw new Exception("Failed to get address of dbgshim!EnumerateCLRs");
                }
                else
                {
                    m_EnumerateCLRs = (delgEnumerateCLRs)Marshal.GetDelegateForFunctionPointer(
                            procAddr,
                            typeof(delgEnumerateCLRs));
                }

                procAddr = GetProcAddr(m_hndModuleMscoree, "CloseCLREnumeration");
                if (IntPtr.Zero == procAddr)
                {
                    throw new Exception("Failed to get address of dbgshim!CloseCLREnumeration");
                }
                else
                {
                    m_CloseCLREnumeration = (delgCloseCLREnumeration)Marshal.GetDelegateForFunctionPointer(
                            procAddr,
                            typeof(delgCloseCLREnumeration));
                }

                procAddr = GetProcAddr(m_hndModuleMscoree, "GetStartupNotificationEvent");
                if (IntPtr.Zero == procAddr)
                {
                    throw new Exception("Failed to get address of dbgshim!GetStartupNotificationEvent");
                }
                else
                {
                    m_GetStartupNotificationEvent = (delgGetStartupNotificationEvent)Marshal.GetDelegateForFunctionPointer(
                            procAddr,
                            typeof(delgGetStartupNotificationEvent));
                }

                // We are done initializing
                m_bSLApiInit = true;
            }


            finally
            {
                if (null != netFrameworkKey)
                {
                    netFrameworkKey.Close();
                }

                // we will keep the cached handle to loaded mscoree.dll 
                // around for a while..........
            }

        }

        private static unsafe void MarshalCLREnumeration(
            IntPtr ptrEventArray, IntPtr ptrStringArray, UInt32 elementCount,
            out string[] fullPaths, out EventWaitHandle[] continueStartupEvents)
        {
            fullPaths = new string[elementCount];
            continueStartupEvents = new EventWaitHandle[elementCount];

            IntPtr* phEvents = (IntPtr*)ptrEventArray.ToPointer();
            char** pstrStrings = (char**)ptrStringArray.ToPointer();

            IntPtr hDuppedHandle;
            IntPtr hCurrentProcess = NativeMethods.GetCurrentProcess();

            for (int i = 0; i < elementCount; i++)
            {
                IntPtr hEvent = *phEvents;

                if ((hEvent == IntPtr.Zero) || (hEvent == new IntPtr(-1)))
                {
                    hDuppedHandle = hEvent;
                }

                else
                {
                    if (!NativeMethods.DuplicateHandle(
                            hCurrentProcess,
                            hEvent,
                            hCurrentProcess,
                            out hDuppedHandle,
                            0,
                            false,
                            (int)NativeMethods.DuplicateHandleOptions.DUPLICATE_SAME_ACCESS))
                    {
                        throw new Exception(
                            "could not duplicate handle in MarshalCLREnumeration.  GLE: " + Marshal.GetLastWin32Error());
                    }

                }

                continueStartupEvents[i] = new AutoResetEvent(false);
                continueStartupEvents[i].SafeWaitHandle = new SafeWaitHandle(hDuppedHandle, true);
                fullPaths[i] = new string(*pstrStrings);
                pstrStrings++;
                phEvents++;
            }
        }



        ///////////////////////////////////////////////////////////////////////////////////////////

        //
        // Wrapper methods for P-Invoked Win32 functions
        //

        public static IntPtr LoadLib(string strFileName)
        {
            IntPtr hndModule = NativeMethods.LoadLibrary(strFileName);

            if (IntPtr.Zero == hndModule)
            {
                CommandBase.WriteError("LoadLibrary failed in Silverlight Extension for " +
                    strFileName + ": " + Marshal.GetLastWin32Error());
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return hndModule;
        }


        public static IntPtr GetProcAddr(IntPtr hndModule, string strProcName)
        {
            IntPtr farProc = NativeMethods.GetProcAddress(hndModule, strProcName);

            if (IntPtr.Zero == farProc)
            {
                Console.WriteLine("GetProcAddress failed in Silverlight Extension for " + strProcName + ": " + Marshal.GetLastWin32Error());
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return farProc;
        }


        public static uint WaitForSingleObj(IntPtr hObject, int dwMilliSeconds)
        {
            uint uRet = NativeMethods.WaitForSingleObject(hObject, dwMilliSeconds);

            if ((uint)NativeMethods.WaitForSingleObjectReturnValues.WAIT_FAILED == uRet)
            {
                CommandBase.WriteError("WaitForSingleObject failed in Silverlight Extension: " + Marshal.GetLastWin32Error());
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return uRet;
        }

        public static int ResumeThr(IntPtr hThread)
        {
            int iRet = NativeMethods.ResumeThread(hThread);

            // ResumeThread will return -1 on an error, otherwise it returns the thread's suspend count
            // which can be 0 or greater.
            // See http://msdn.microsoft.com/en-us/library/ms685086(VS.85).aspx for more information
            if (iRet == -1)
            {
                CommandBase.WriteError("ResumeThread failed in Silverlight Extension: " + Marshal.GetLastWin32Error());
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return iRet;
        }


        public static int CreateProc(
            string applicationName,
            string commandLine,
            SECURITY_ATTRIBUTES processAttributes,
            SECURITY_ATTRIBUTES threadAttributes,
            bool bInheritHandles,
            int dwCreationFlags,
            IntPtr environment,
            string currentDirectory,
            STARTUPINFO startupInfo,
            ref PROCESS_INFORMATION processInformation)
        {
            int iRet = NativeMethods.CreateProcess(
                                applicationName,
                                commandLine,
                                processAttributes,
                                threadAttributes,
                                bInheritHandles,
                                dwCreationFlags,
                                environment,
                                currentDirectory,
                                startupInfo,
                                processInformation);

            if (0 == iRet)
            {
                CommandBase.WriteError("CreateProcess failed in Silverlight Extension: " + Marshal.GetLastWin32Error());
                CommandBase.WriteError("    CreateProcess-ApplicationName=" + applicationName);
                CommandBase.WriteError("    CreateProcess-commandLine=" + commandLine);
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return iRet;
        }


        ///////////////////////////////////////////////////////////////////////////////////////////


        public static string CreateVersionStringFromModule(UInt32 debuggeePid, string clrPath)
        {
            UInt32 reqBufferSize = 0;

            // first call is getting the reqBufferSize
            m_CreateVersionStringFromModule(
                0,
                clrPath,
                null,
                0,
                out reqBufferSize);

            StringBuilder sb = new StringBuilder((int)reqBufferSize);

            // this call can fail because the underlying call uses CreateToolhelp32Snapshot
            //
            int ret;
            int numTries = 0;
            do
            {
                Trace.WriteLine("In CreateVersionStringFromModule, numTries=" + numTries.ToString());
                ret = (int)m_CreateVersionStringFromModule(
                                            debuggeePid,
                                            clrPath,
                                            sb,
                                            (UInt32)sb.Capacity,
                                            out reqBufferSize);
                // m_CreateVersionStringFromModule uses the OS API CreateToolhelp32Snapshot which can return 
                // ERROR_BAD_LENGTH or ERROR_PARTIAL_COPY. If we get either of those, we try wait 1/10th of a second
                // try again (that is the recommendation of the OS API owners)
                if (((int)HResult.E_BAD_LENGTH == ret) || ((int)HResult.E_PARTIAL_COPY == ret))
                {
                    System.Threading.Thread.Sleep(100);
                }
                else
                {
                    break;
                }
                numTries++;
            } while (numTries < 10);

            if ((int)HResult.S_OK != ret)
            {
                throw new COMException("CreateVersionStringFromModule failed returning the following HResult: " + Silverlight.HResultToString(ret), ret);
            }

            return sb.ToString();
        }


        public static ICorDebug CreateDebuggingInterfaceFromVersion(string strDebuggeeVersion)
        {
            ICorDebug ICorDbgIntf;
            int ret;
            int numTries = 0;
            do
            {
                Trace.WriteLine("In CreateDebuggingInterfaceFromVersion, numTries=" + numTries.ToString());
                ret = (int)m_CreateDebuggingInterfaceFromVersion(
                    strDebuggeeVersion,
                    out ICorDbgIntf
                    );
                // CreateDebuggingInterfaceFromVersionEx uses the OS API CreateToolhelp32Snapshot which can return 
                // ERROR_BAD_LENGTH or ERROR_PARTIAL_COPY. If we get either of those, we try wait 1/10th of a second
                // try again (that is the recommendation of the OS API owners)
                if (((int)HResult.E_BAD_LENGTH == ret) || ((int)HResult.E_PARTIAL_COPY == ret))
                {
                    System.Threading.Thread.Sleep(100);
                }
                else
                {
                    break;
                }
                numTries++;
            }
            while (numTries < 10);

            // if we're not OK then throw an exception with the returned hResult
            if ((int)HResult.S_OK != ret)
            {
                throw new COMException("CreateDebuggingInterfaceFromVersion for debuggee version " + strDebuggeeVersion +
                    " failed returning " + Silverlight.HResultToString(ret), ret);
            }
            return ICorDbgIntf;
        }

        public static ICorDebug CreateDebuggingInterfaceFromVersionEx(SilverlightExtension.CorDebugInterfaceVersion iDebuggerVersion, string strDebuggeeVersion)
        {
            if (null == m_CreateDebuggingInterfaceFromVersionEx)
            {
                return null;
            }
            ICorDebug ICorDbgIntf;
            int ret;
            int numTries = 0;
            do
            {
                Trace.WriteLine("In CreateDebuggingInterfaceFromVersionEx, numTries=" + numTries.ToString());
                ret = (int)m_CreateDebuggingInterfaceFromVersionEx(
                    (int)iDebuggerVersion,
                    strDebuggeeVersion,
                    out ICorDbgIntf
                   );
                // CreateDebuggingInterfaceFromVersionEx uses the OS API CreateToolhelp32Snapshot which can return 
                // ERROR_BAD_LENGTH or ERROR_PARTIAL_COPY. If we get either of those, we try wait 1/10th of a second
                // try again (that is the recommendation of the OS API owners)
                if (((int)HResult.E_BAD_LENGTH == ret) || ((int)HResult.E_PARTIAL_COPY == ret))
                {
                    System.Threading.Thread.Sleep(100);
                }
                else
                {
                    // else we've hit one of the HRESULTS that we shouldn't try again for, if the result isn't OK then the error will ge reported below
                    break;
                }
                numTries++;
            }
            while (numTries < 10);

            // if we're not OK then throw an exception with the returned hResult
            if ((int)HResult.S_OK != ret)
            {
                throw new COMException("CreateDebuggingInterfaceFromVersionEx for debuggee version " + strDebuggeeVersion +
                    " requesting interface version " + iDebuggerVersion +
                    " failed returning " + Silverlight.HResultToString(ret), ret);
            }
            return ICorDbgIntf;
        }

        public static void EnumerateCLRs(
            UInt32 debuggeePid, out string[] fullPaths, out EventWaitHandle[] continueStartupEvents)
        {
            UInt32 elementCount;
            IntPtr pEventArray;
            IntPtr pStringArray;

            int ret;
            int numTries = 0;
            do
            {
                Trace.WriteLine(numTries > 0, "In EnumerateCLRs, numTries=" + numTries.ToString());
                ret = (int)m_EnumerateCLRs(
                    debuggeePid,
                    out pEventArray,
                    out pStringArray,
                    out elementCount
                    );
                // EnumerateCLRs uses the OS API CreateToolhelp32Snapshot which can return 
                // ERROR_BAD_LENGTH or ERROR_PARTIAL_COPY. If we get either of those, we try wait 1/10th of a second
                // try again (that is the recommendation of the OS API owners)
                if (((int)HResult.E_BAD_LENGTH == ret) || ((int)HResult.E_PARTIAL_COPY == ret))
                {
                    System.Threading.Thread.Sleep(100);
                }
                else
                {
                    break;
                }
                numTries++;
            } while (((int)HResult.S_OK != ret) && (numTries < 10));

            if ((int)HResult.S_OK != ret)
            {
                fullPaths = new string[0];
                continueStartupEvents = new EventWaitHandle[0];
                return;
            }

            MarshalCLREnumeration(
                pEventArray,
                pStringArray,
                elementCount,
                out fullPaths,
                out continueStartupEvents);

            ret = (int)m_CloseCLREnumeration(
                pEventArray,
                pStringArray,
                elementCount);

            if ((int)HResult.S_OK != ret)
            {
                throw new COMException("CloseCLREnumeration failed for process PID=" + debuggeePid + ", HResult= " + Silverlight.HResultToString(ret), ret);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////

        private delegate UInt32 delgCreateDebuggingInterfaceFromVersion(
            [MarshalAs(UnmanagedType.LPWStr)] string strDebuggeeVersion,
            out ICorDebug ICorDebugIntf
            );

        private delegate UInt32 delgCreateDebuggingInterfaceFromVersionEx(
            [MarshalAs(UnmanagedType.I4)] int iDebuggerVersion,
            [MarshalAs(UnmanagedType.LPWStr)] string strDebuggeeVersion,
            out ICorDebug ICorDebugIntf
            );

        private delegate UInt32 delgCreateVersionStringFromModule(
            UInt32 pidDebuggee,
            [MarshalAs(UnmanagedType.LPWStr)] string strModuleName,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder strBuffer,
            UInt32 cchBufferSize,
            out UInt32 cchRequiredBufferSize
            );

        private delegate UInt32 delgEnumerateCLRs(
            UInt32 pidDebuggee,
            out IntPtr pEventArray,
            out IntPtr pStringArray,
            out UInt32 elementCount
            );

        private delegate UInt32 delgCloseCLREnumeration(
            IntPtr pEventArray,
            IntPtr pStringArray,
            UInt32 elementCount
            );

        private delegate UInt32 delgGetStartupNotificationEvent(
            UInt32 debuggeePid,
            out SafeWaitHandle startupNotifyEvent
            );


        ///////////////////////////////////////////////////////////////////////////////////////////


        private delegate UInt32 delgCreateCordbObject(
            int debuggeeVersion,
            out ICorDebug ICorDebugIntf
            );

        private delegate UInt32 delgInitDbgTransportManager();

        private delegate UInt32 delgShutdownDbgTransportManager();

        ///////////////////////////////////////////////////////////////////////////////////////////


        public static String HResultToString(int hResult)
        {
            String returnValue = "";
            try
            {
                if (Enum.IsDefined(typeof(HResult), hResult))
                {
                    returnValue = ((HResult)hResult).ToString();
                }
                else
                {
                    returnValue = "0x" + hResult.ToString("X");
                }
            }
            catch (Exception)
            {
                // default just convert the hresult to hex
                returnValue = "0x" + hResult.ToString("X");
            }
            return returnValue;
        }
    }
}
