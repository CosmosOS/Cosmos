//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Text;
using System.Security.Permissions;
using System.Globalization;

using Microsoft.Samples.Debugging.CorDebug.NativeApi;


namespace Microsoft.Samples.Debugging.CorDebug
{
    /**
     * Wraps the native CLR Debugger.
     * Note that we don't derive the class from WrapperBase, becuase this
     * class will never be returned in any callback.
     */
    public sealed class CorDebugger : MarshalByRefObject
    {
        private const int MaxVersionStringLength = 256; // == MAX_PATH

        public static string GetDebuggerVersionFromFile(string pathToExe)
        {
            Debug.Assert(!string.IsNullOrEmpty(pathToExe));
            if (string.IsNullOrEmpty(pathToExe))
                throw new ArgumentException("Value cannot be null or empty.", "pathToExe");
            int neededSize;
            StringBuilder sb = new StringBuilder(MaxVersionStringLength);
            NativeMethods.GetRequestedRuntimeVersion(pathToExe, sb, sb.Capacity, out neededSize);
            return sb.ToString();
        }

        public static string GetDebuggerVersionFromPid(int pid)
        {
            using (ProcessSafeHandle ph = NativeMethods.OpenProcess((int)(NativeMethods.ProcessAccessOptions.ProcessVMRead |
                                                                         NativeMethods.ProcessAccessOptions.ProcessQueryInformation |
                                                                         NativeMethods.ProcessAccessOptions.ProcessDupHandle |
                                                                         NativeMethods.ProcessAccessOptions.Synchronize),
                                                                   false, // inherit handle
                                                                   pid))
            {
                if (ph.IsInvalid)
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                int neededSize;
                StringBuilder sb = new StringBuilder(MaxVersionStringLength);
                NativeMethods.GetVersionFromProcess(ph, sb, sb.Capacity, out neededSize);
                return sb.ToString();
            }
        }

        public static string GetDefaultDebuggerVersion()
        {
            return RuntimeEnvironment.GetSystemVersion();
        }


        /// <summary>Creates a debugger wrapper from Guid.</summary>
        public CorDebugger(Guid debuggerGuid)
        {
            ICorDebug rawDebuggingAPI;
            Guid ifaceId = typeof(ICorDebug).GUID;
            NativeMethods.CoCreateInstance(ref debuggerGuid,
                                           IntPtr.Zero, // pUnkOuter
                                           1, // CLSCTX_INPROC_SERVER
                                           ref ifaceId,
                                           out rawDebuggingAPI);
            InitFromICorDebug(rawDebuggingAPI);
        }

        [CLSCompliant(false)]
        public CorDebugger(ICorDebug rawDebuggingAPI)
        {
            InitFromICorDebug(rawDebuggingAPI);
        }

        /// <summary>Creates a debugger interface that is able debug requested version of CLR</summary>
        /// <param name="debuggerVerison">Version number of the debugging interface.</param>
        /// <remarks>The version number is usually retrieved either by calling one of following mscoree functions:
        /// GetCorVerison, GetRequestedRuntimeVersion or GetVersionFromProcess.</remarks>
        public CorDebugger(string debuggerVersion)
        {
            InitFromVersion(debuggerVersion);
        }

        [CLSCompliant(false)]
        public ICorDebug Raw
        {
            get 
            { 
                return m_debugger;
            }
        }

        /**
         * Closes the debugger.  After this method is called, it is an error
         * to call any other methods on this object.
         */
        public void Terminate()
        {
            Debug.Assert(m_debugger != null);
            ICorDebug d = m_debugger;
            m_debugger = null;
            d.Terminate();
        }

        /**
         * Specify the callback object to use for managed events.
         */
        internal void SetManagedHandler(ICorDebugManagedCallback managedCallback)
        {
            m_debugger.SetManagedHandler(managedCallback);
        }

        /**
         * Specify the callback object to use for unmanaged events.
         */
        internal void SetUnmanagedHandler(ICorDebugUnmanagedCallback nativeCallback)
        {
            m_debugger.SetUnmanagedHandler(nativeCallback);
        }

        /**
         * Launch a process under the control of the debugger.
         *
         * Parameters are the same as the Win32 CreateProcess call.
         */
        public CorProcess CreateProcess(
                                         String applicationName,
                                         String commandLine
                                         )
        {
            return CreateProcess(applicationName, commandLine, ".");
        }

        /**
         * Launch a process under the control of the debugger.
         *
         * Parameters are the same as the Win32 CreateProcess call.
         */
        public CorProcess CreateProcess(
                                         String applicationName,
                                         String commandLine,
                                         String currentDirectory
                                         )
        {
            return CreateProcess(applicationName, commandLine, currentDirectory, 0);
        }

        /**
         * Launch a process under the control of the debugger.
         *
         * Parameters are the same as the Win32 CreateProcess call.
         */
        public CorProcess CreateProcess(
                                         String applicationName,
                                         String commandLine,
                                         String currentDirectory,
                                         int flags
                                         )
        {
            return CreateProcess(applicationName, commandLine, currentDirectory, flags, null);
        }

        public CorProcess CreateProcess(String applicationName,
                                        String commandLine,
                                        String currentDirectory,
                                        int flags,
                                        CorRemoteTarget target)
        {
            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();

            STARTUPINFO si = new STARTUPINFO();
            si.cb = Marshal.SizeOf(si);

            // initialize safe handles 
            si.hStdInput = new Microsoft.Win32.SafeHandles.SafeFileHandle(new IntPtr(0), false);
            si.hStdOutput = new Microsoft.Win32.SafeHandles.SafeFileHandle(new IntPtr(0), false);
            si.hStdError = new Microsoft.Win32.SafeHandles.SafeFileHandle(new IntPtr(0), false);

            CorProcess ret;

            //constrained execution region (Cer)
            System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                ret = CreateProcess(target,
                                     applicationName,
                                     commandLine,
                                     null,
                                     null,
                                     true,   // inherit handles
                                     flags,  // creation flags
                                     new IntPtr(0),      // environment
                                     currentDirectory,
                                     si,     // startup info
                                     ref pi, // process information
                                     CorDebugCreateProcessFlags.DEBUG_NO_SPECIAL_OPTIONS);
                NativeMethods.CloseHandle(pi.hProcess);
                NativeMethods.CloseHandle(pi.hThread);
            }

            return ret;
        }

        /**
         * Launch a process under the control of the debugger.
         *
         * Parameters are the same as the Win32 CreateProcess call.
         *
         * The caller should remember to execute:
         *
         *    Microsoft.Win32.Interop.Windows.CloseHandle (
         *      processInformation.hProcess);
         *
         * after CreateProcess returns.
         */
        [CLSCompliant(false)]
        public CorProcess CreateProcess(
                                         String applicationName,
                                         String commandLine,
                                         SECURITY_ATTRIBUTES processAttributes,
                                         SECURITY_ATTRIBUTES threadAttributes,
                                         bool inheritHandles,
                                         int creationFlags,
                                         IntPtr environment,
                                         String currentDirectory,
                                         STARTUPINFO startupInfo,
                                         ref PROCESS_INFORMATION processInformation,
                                         CorDebugCreateProcessFlags debuggingFlags)
        {
            return CreateProcess(null,
                                 applicationName,
                                 commandLine,
                                 processAttributes,
                                 threadAttributes,
                                 inheritHandles,
                                 creationFlags,
                                 environment,
                                 currentDirectory,
                                 startupInfo,
                                 ref processInformation,
                                 debuggingFlags);
        }

        [CLSCompliant(false)]
        public CorProcess CreateProcess(CorRemoteTarget target,
                                         String applicationName,
                                         String commandLine,
                                         SECURITY_ATTRIBUTES processAttributes,
                                         SECURITY_ATTRIBUTES threadAttributes,
                                         bool inheritHandles,
                                         int creationFlags,
                                         IntPtr environment,
                                         String currentDirectory,
                                         STARTUPINFO startupInfo,
                                         ref PROCESS_INFORMATION processInformation,
                                         CorDebugCreateProcessFlags debuggingFlags)
        {
            /*
             * If commandLine is: <c:\a b\a arg1 arg2> and c:\a.exe does not exist, 
             *    then without this logic, "c:\a b\a.exe" would be tried next.
             * To prevent this ambiguity, this forces the user to quote if the path 
             *    has spaces in it: <"c:\a b\a" arg1 arg2>
             */
            if (null == applicationName && !commandLine.StartsWith("\""))
            {
                int firstSpace = commandLine.IndexOf(" ");
                if (firstSpace != -1)
                    commandLine = String.Format(CultureInfo.InvariantCulture, "\"{0}\" {1}", commandLine.Substring(0, firstSpace), commandLine.Substring(firstSpace, commandLine.Length - firstSpace));
            }

            ICorDebugProcess proc = null;

            if (target == null)
            {
                m_debugger.CreateProcess(
                                      applicationName,
                                      commandLine,
                                      processAttributes,
                                      threadAttributes,
                                      inheritHandles ? 1 : 0,
                                      (uint)creationFlags,
                                      environment,
                                      currentDirectory,
                                      startupInfo,
                                      processInformation,
                                      debuggingFlags,
                                      out proc);
            }
            else
            {
                m_remoteDebugger.CreateProcessEx(target,
                                                 applicationName,
                                                 commandLine,
                                                 processAttributes,
                                                 threadAttributes,
                                                 inheritHandles ? 1 : 0,
                                                 (uint)creationFlags,
                                                 environment,
                                                 currentDirectory,
                                                 startupInfo,
                                                 processInformation,
                                                 debuggingFlags,
                                                 out proc);

            }

            return CorProcess.GetCorProcess(proc);
        }

        /** 
         * Attach to an active process
         */
        public CorProcess DebugActiveProcess(int processId, bool win32Attach)
        {
            return DebugActiveProcess(processId, win32Attach, null);
        }

        public CorProcess DebugActiveProcess(int processId, bool win32Attach, CorRemoteTarget target)
        {
            ICorDebugProcess proc = null;
            if (target == null)
            {
                m_debugger.DebugActiveProcess((uint)processId, win32Attach ? 1 : 0, out proc);
            }
            else
            {
                m_remoteDebugger.DebugActiveProcessEx(target, (uint)processId, win32Attach ? 1 : 0, out proc);
            }
            return CorProcess.GetCorProcess(proc);
        }

        /**
         * Enumerate all processes currently being debugged.
         */
        public IEnumerable Processes
        {
            get
            {
                ICorDebugProcessEnum eproc = null;
                m_debugger.EnumerateProcesses(out eproc);
                return new CorProcessEnumerator(eproc);
            }
        }

        /**
         * Get the Process object for the given PID.
         */
        public CorProcess GetProcess(int processId)
        {
            ICorDebugProcess proc = null;
            m_debugger.GetProcess((uint)processId, out proc);
            return CorProcess.GetCorProcess(proc);
        }

        /**
         * Warn us of potentional problems in using debugging (eg. whether a kernel debugger is 
         * attached).  This API should probably be renamed or the warnings turned into errors
         * in CreateProcess/DebugActiveProcess
         */
        public void CanLaunchOrAttach(int processId, bool win32DebuggingEnabled)
        {
            m_debugger.CanLaunchOrAttach((uint)processId,
                                         win32DebuggingEnabled ? 1 : 0);
        }

        ////////////////////////////////////////////////////////////////////////////////
        //
        // CorDebugger private implement part
        //
        ////////////////////////////////////////////////////////////////////////////////

        // called by constructors during initialization
        private void InitFromVersion(string debuggerVersion)
        {
            if (debuggerVersion.StartsWith("v1"))
            {
                // ICorDebug before V2 did not cooperate well with COM-intop. MDbg's managed
                // wrappers over ICorDebug only work on V2 and beyond.
                throw new ArgumentException("Can't debug a version 1 CLR process (\"" + debuggerVersion +
                    "\").  Run application in a version 2 CLR, or use a version 1 debugger instead.");
            }

            bool fUseV2 = false;
            ICorDebug rawDebuggingAPI = null;
            try
            {
                CLRMetaHost mh = new CLRMetaHost();
                CLRRuntimeInfo rti = mh.GetRuntime(debuggerVersion);
                rawDebuggingAPI = rti.GetLegacyICorDebugInterface();
            }
            catch (NotImplementedException)
            {
                fUseV2 = true;
            }
            catch (EntryPointNotFoundException)
            {
                fUseV2 = true;
            }

            if (fUseV2)
            {
                // fallback to v2 method

                try
                {
                    rawDebuggingAPI = NativeMethods.CreateDebuggingInterfaceFromVersion((int)CorDebuggerVersion.Whidbey, debuggerVersion);
                }
                catch (ArgumentException)
                {
                    // This can commonly happen if:
                    // 1) the debuggee is missing a config file 
                    // 2) the debuggee has a config file for a not-installed CLR.
                    // 
                    // Give a more descriptive error. 
                    // We explicitly don't pass the inner exception because:
                    // - it's uninteresting. It's really just from a pinvoke and so there are no
                    //    extra managed frames.
                    // - MDbg's error reporting will call Exception.GetBaseException() and so just
                    //    grab the inner exception.
                    throw new ArgumentException("Failed to create debugging services for version '" + debuggerVersion + "'");
                }
            }
            Debug.Assert(rawDebuggingAPI != null);
            InitFromICorDebug(rawDebuggingAPI);
        }

        private void InitFromICorDebug(ICorDebug rawDebuggingAPI)
        {
            Debug.Assert(rawDebuggingAPI != null);
            if (rawDebuggingAPI == null)
                throw new ArgumentException("Cannot be null.", "rawDebugggingAPI");

            m_debugger = rawDebuggingAPI;
            m_debugger.Initialize();
            m_debugger.SetManagedHandler(new ManagedCallback(this));

            // This may return null.
            m_remoteDebugger = rawDebuggingAPI as ICorDebugRemote;
        }

        /**
         * Helper for invoking events.  Checks to make sure that handlers
         * are hooked up to a handler before the handler is invoked.
         *
         * We want to allow maximum flexibility by our callers.  As such,
         * we don't require that they call <code>e.Controller.Continue</code>,
         * nor do we require that this class call it.  <b>Someone</b> needs
         * to call it, however.
         *
         * Consequently, if an exception is thrown and the process is stopped,
         * the process is continued automatically.
         */

        void InternalFireEvent(ManagedCallbackType callbackType, CorEventArgs e)
        {
            CorProcess owner = e.Process;

            Debug.Assert(owner != null);
            try
            {
                owner.DispatchEvent(callbackType, e);
            }
            finally
            {
                // If the callback marked the event to be continued, then call Continue now.
                if (e.Continue)
                {
                    e.Controller.Continue(false);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////
        //
        // ManagedCallback
        //
        ////////////////////////////////////////////////////////////////////////////////

        /**
         * This is the object that gets passed to the debugger.  It's
         * the intermediate "source" of the events, which repackages
         * the event arguments into a more approprate form and forwards
         * the call to the appropriate function.
         */
        private class ManagedCallback : ManagedCallbackBase
        {
            public ManagedCallback(CorDebugger outer)
            {
                m_outer = outer;
            }
            protected override void HandleEvent(ManagedCallbackType eventId, CorEventArgs args)
            {
                m_outer.InternalFireEvent(eventId, args);
            }
            private CorDebugger m_outer;
        }

        private ICorDebug m_debugger = null;
        private ICorDebugRemote m_remoteDebugger = null;
    } /* class Debugger */


    public class ProcessSafeHandle : Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid
    {
        private ProcessSafeHandle()
            : base(true)
        {
        }

        private ProcessSafeHandle(IntPtr handle, bool ownsHandle)
            : base(ownsHandle)
        {
            SetHandle(handle);
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        override protected bool ReleaseHandle()
        {
            return NativeMethods.CloseHandle(handle);
        }
    }

    public static class NativeMethods
    {
        private const string Kernel32LibraryName = "kernel32.dll";
        private const string Ole32LibraryName = "ole32.dll";
        private const string ShlwapiLibraryName = "shlwapi.dll";
        private const string ShimLibraryName = "mscoree.dll";

        public const int MAX_PATH = 260;

        [
         System.Runtime.ConstrainedExecution.ReliabilityContract(System.Runtime.ConstrainedExecution.Consistency.WillNotCorruptState, System.Runtime.ConstrainedExecution.Cer.Success),
         DllImport(Kernel32LibraryName)
        ]
        public static extern bool CloseHandle(IntPtr handle);


        [
         DllImport(ShimLibraryName, CharSet = CharSet.Unicode, PreserveSig = false)
        ]
        public static extern ICorDebug CreateDebuggingInterfaceFromVersion(int iDebuggerVersion
                                                                           , string szDebuggeeVersion);

        [
         DllImport(ShimLibraryName, CharSet = CharSet.Unicode, PreserveSig = false)
        ]
        public static extern void GetVersionFromProcess(ProcessSafeHandle hProcess, StringBuilder versionString,
                                                        Int32 bufferSize, out Int32 dwLength);

        [
         DllImport(ShimLibraryName, CharSet = CharSet.Unicode, PreserveSig = false)
        ]
        public static extern void GetRequestedRuntimeVersion(string pExe, StringBuilder pVersion,
                                                             Int32 cchBuffer, out Int32 dwLength);

        [
         DllImport(ShimLibraryName, CharSet = CharSet.Unicode, PreserveSig = false)
        ]
        public static extern void CLRCreateInstance(ref Guid clsid, ref Guid riid,
            [MarshalAs(UnmanagedType.Interface)]out object metahostInterface);

        public enum ProcessAccessOptions : int
        {
            ProcessTerminate = 0x0001,
            ProcessCreateThread = 0x0002,
            ProcessSetSessionID = 0x0004,
            ProcessVMOperation = 0x0008,
            ProcessVMRead = 0x0010,
            ProcessVMWrite = 0x0020,
            ProcessDupHandle = 0x0040,
            ProcessCreateProcess = 0x0080,
            ProcessSetQuota = 0x0100,
            ProcessSetInformation = 0x0200,
            ProcessQueryInformation = 0x0400,
            ProcessSuspendResume = 0x0800,
            Synchronize = 0x100000,
        }

        [
         DllImport(Kernel32LibraryName, PreserveSig = true)
        ]
        public static extern ProcessSafeHandle OpenProcess(Int32 dwDesiredAccess, bool bInheritHandle, Int32 dwProcessId);

        [
         DllImport(Kernel32LibraryName, CharSet = CharSet.Unicode, PreserveSig = true)
        ]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool QueryFullProcessImageName(ProcessSafeHandle hProcess,
                                                            int dwFlags,
                                                            StringBuilder lpExeName,
                                                            ref int lpdwSize);

        [
         DllImport(Ole32LibraryName, PreserveSig = false)
        ]
        public static extern void CoCreateInstance(ref Guid rclsid, IntPtr pUnkOuter,
                                                   Int32 dwClsContext,
                                                   ref Guid riid, // must use "typeof(ICorDebug).GUID"
                                                   [MarshalAs(UnmanagedType.Interface)]out ICorDebug debuggingInterface
                                                   );

        public enum Stgm
        {
            StgmRead = 0x00000000,
            StgmWrite = 0x00000001,
            StgmReadWrite = 0x00000002,
            StgmShareDenyNone = 0x00000040,
            StgmShareDenyRead = 0x00000030,
            StgmShareDenyWrite = 0x00000020,
            StgmShareExclusive = 0x00000010,
            StgmPriority = 0x00040000,
            StgmCreate = 0x00001000,
            StgmConvert = 0x00020000,
            StgmFailIfThere = 0x00000000,
            StgmDirect = 0x00000000,
            StgmTransacted = 0x00010000,
            StgmNoScratch = 0x00100000,
            StgmNoSnapshot = 0x00200000,
            StgmSimple = 0x08000000,
            StgmDirectSwmr = 0x00400000,
            StgmDeleteOnRelease = 0x04000000
        }

        // SHCreateStreamOnFile* is used to create IStreams to pass to ICLRMetaHostPolicy::GetRequestedRuntime().
        // Since we can't count on the EX version being available, we have SHCreateStreamOnFile as a fallback.
        [
         DllImport(ShlwapiLibraryName, PreserveSig = false)
        ]
        // Only in version 6 and later
        public static extern void SHCreateStreamOnFileEx([MarshalAs(UnmanagedType.LPWStr)]string file,
                                                        Stgm dwMode,
                                                        Int32 dwAttributes, // Used if a file is created.  Identical to dwFlagsAndAttributes param of CreateFile.
                                                        bool create,
                                                        IntPtr pTemplate,   // Reserved, always pass null.
                                                        [MarshalAs(UnmanagedType.Interface)]out IStream openedStream);

        [
         DllImport(ShlwapiLibraryName, PreserveSig = false)
        ]
        public static extern void SHCreateStreamOnFile(string file,
                                                        Stgm dwMode,
                                                        [MarshalAs(UnmanagedType.Interface)]out IStream openedStream);

    }

    // Wrapper for ICLRMetaHost.  Used to find information about runtimes.
    public sealed class CLRMetaHost
    {
        private ICLRMetaHost m_metaHost;

        public const int MaxVersionStringLength = 26; // 24 + NULL and an extra
        private static readonly Guid clsidCLRMetaHost = new Guid("9280188D-0E8E-4867-B30C-7FA83884E8DE");

        public CLRMetaHost()
        {
            object o;
            Guid ifaceId = typeof(ICLRMetaHost).GUID;
            Guid clsid = clsidCLRMetaHost;
            NativeMethods.CLRCreateInstance(ref clsid, ref ifaceId, out o);
            m_metaHost = (ICLRMetaHost)o;
        }

        public CLRRuntimeInfo GetInstalledRuntimeByVersion(string version)
        {
            IEnumerable<CLRRuntimeInfo> runtimes = EnumerateInstalledRuntimes();

            foreach (CLRRuntimeInfo rti in runtimes)
            {
                if (rti.GetVersionString().ToString().ToLower() == version.ToLower())
                {
                    return rti;
                }
            }

            return null;
        }

        public CLRRuntimeInfo GetLoadedRuntimeByVersion(Int32 processId, string version)
        {
            IEnumerable<CLRRuntimeInfo> runtimes = EnumerateLoadedRuntimes(processId);

            foreach (CLRRuntimeInfo rti in runtimes)
            {
                if (rti.GetVersionString().Equals(version, StringComparison.OrdinalIgnoreCase))
                {
                    return rti;
                }
            }

            return null;
        }

        // Retrieve information about runtimes installed on the machine (i.e. in %WINDIR%\Microsoft.NET\)
        public IEnumerable<CLRRuntimeInfo> EnumerateInstalledRuntimes()
        {
            List<CLRRuntimeInfo> runtimes = new List<CLRRuntimeInfo>();
            IEnumUnknown enumRuntimes = m_metaHost.EnumerateInstalledRuntimes();

            // Since we're only getting one at a time, we can pass NULL for count.
            // S_OK also means we got the single element we asked for.
            for (object oIUnknown; enumRuntimes.Next(1, out oIUnknown, IntPtr.Zero) == 0; /* empty */)
            {
                runtimes.Add(new CLRRuntimeInfo(oIUnknown));
            }

            return runtimes;
        }

        // Retrieve information about runtimes that are currently loaded into the target process.
        public IEnumerable<CLRRuntimeInfo> EnumerateLoadedRuntimes(Int32 processId)
        {
            List<CLRRuntimeInfo> runtimes = new List<CLRRuntimeInfo>();
            IEnumUnknown enumRuntimes;

            using (ProcessSafeHandle hProcess = NativeMethods.OpenProcess((int)(NativeMethods.ProcessAccessOptions.ProcessVMRead |
                                                                        NativeMethods.ProcessAccessOptions.ProcessQueryInformation |
                                                                        NativeMethods.ProcessAccessOptions.ProcessDupHandle |
                                                                        NativeMethods.ProcessAccessOptions.Synchronize),
                                                                        false, // inherit handle
                                                                        processId))
            {
                if (hProcess.IsInvalid)
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }

                enumRuntimes = m_metaHost.EnumerateLoadedRuntimes(hProcess);
            }

            // Since we're only getting one at a time, we can pass NULL for count.
            // S_OK also means we got the single element we asked for.
            for (object oIUnknown; enumRuntimes.Next(1, out oIUnknown, IntPtr.Zero) == 0; /* empty */)
            {
                runtimes.Add(new CLRRuntimeInfo(oIUnknown));
            }

            return runtimes;
        }

        public CLRRuntimeInfo GetRuntime(string version)
        {
            Guid ifaceId = typeof(ICLRRuntimeInfo).GUID;
            return new CLRRuntimeInfo(m_metaHost.GetRuntime(version, ref ifaceId));
        }
    }


    // You're expected to get this interface from mscoree!GetCLRMetaHost.
    // Details for APIs are in metahost.idl.
    [ComImport, InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown), Guid("D332DB9E-B9B3-4125-8207-A14884F53216")]
    internal interface ICLRMetaHost
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        System.Object GetRuntime(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwzVersion,
            [In] ref Guid riid /*must use typeof(ICLRRuntimeInfo).GUID*/);

        void GetVersionFromFile(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwzBuffer,
            [In, Out] ref uint pcchBuffer);

        [return: MarshalAs(UnmanagedType.Interface)]
        IEnumUnknown EnumerateInstalledRuntimes();

        [return: MarshalAs(UnmanagedType.Interface)]
        IEnumUnknown EnumerateLoadedRuntimes(
            [In] ProcessSafeHandle hndProcess);
    }


    // Wrapper for ICLRMetaHostPolicy.
    public sealed class CLRMetaHostPolicy
    {
        public enum MetaHostPolicyFlags
        {
            metaHostPolicyHighCompat = 0,
            metaHostPolicyLowFootprint = 1
        }

        private ICLRMetaHostPolicy m_MHPolicy;
        private int MaxVersionStringLength = 26; //24 for version, + 2 terminating NULLs
        private static readonly Guid clsidCLRMetaHostPolicy = new Guid("2EBCD49A-1B47-4a61-B13A-4A03701E594B");

        public CLRMetaHostPolicy()
        {
            object o;
            Guid ifaceId = typeof(ICLRMetaHostPolicy).GUID;
            Guid clsid = clsidCLRMetaHostPolicy;
            NativeMethods.CLRCreateInstance(ref clsid, ref ifaceId, out o);
            m_MHPolicy = (ICLRMetaHostPolicy)o;
        }

        // Returns a CLRRuntimeInfo for the runtime that the specified binary
        // will run against.
        public CLRRuntimeInfo GetRequestedRuntime(MetaHostPolicyFlags flags,
                                                    String binaryPath,
                                                    String configPath,
                                                    ref StringBuilder version,
                                                    ref StringBuilder imageVersion)
        {
            IStream configStream = null;

            if (configPath != null)
            {
                try
                {
                    NativeMethods.SHCreateStreamOnFileEx(configPath,
                                                        NativeMethods.Stgm.StgmRead,
                                                        0,      // We're not creating a file, so no flags needed.
                                                        false,  // Do NOT create a new file.
                                                        IntPtr.Zero,
                                                        out configStream);
                }
                catch (EntryPointNotFoundException)
                {
                    // Fall back on the older method.
                    NativeMethods.SHCreateStreamOnFile(configPath,
                                                        NativeMethods.Stgm.StgmRead,
                                                        out configStream);
                }
            }

            // In case they're empty.
            version.EnsureCapacity(MaxVersionStringLength);
            uint versionCapacity = System.Convert.ToUInt32(version.Capacity);
            imageVersion.EnsureCapacity(MaxVersionStringLength);
            uint imageVersionCapacity = System.Convert.ToUInt32(imageVersion.Capacity);


            Guid ifaceId = typeof(ICLRRuntimeInfo).GUID;
            uint configFlags;
            object o = m_MHPolicy.GetRequestedRuntime(flags,
                                                        binaryPath,
                                                        configStream,
                                                        version,
                                                        ref versionCapacity,
                                                        imageVersion,
                                                        ref imageVersionCapacity,
                                                        out configFlags,
                                                        ref ifaceId);

            return new CLRRuntimeInfo(o);
        }

    }

    // You're expected to get this interface from mscoree!CLRCreateInstance.
    // Details for APIs are in metahost.idl.
    [ComImport, InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown), Guid("E2190695-77B2-492E-8E14-C4B3A7FDD593")]
    internal interface ICLRMetaHostPolicy
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        System.Object GetRequestedRuntime([In, ComAliasName("metahost.assembly.MetaHostPolicyFlags")] CLRMetaHostPolicy.MetaHostPolicyFlags dwPolicyFlags,
                                    [In, MarshalAs(UnmanagedType.LPWStr)] string pwzBinary,
                                    [In, MarshalAs(UnmanagedType.Interface)] IStream pCfgStream,
                                    [In, Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwzVersion,
                                    [In, Out] ref uint pcchVersion,
                                    [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwzImageVersion,
                                    [In, Out] ref uint pcchImageVersion,
                                    [Out] out uint pdwConfigFlags,
                                    [In] ref Guid riid /* must use typeof(ICLRRuntimeInfo).GUID */);
    }

    // Wrapper for ICLRRuntimeInfo.  Represents information about a CLR install instance.
    public sealed class CLRRuntimeInfo
    {

        private static Guid m_ClsIdClrDebuggingLegacy = new Guid("DF8395B5-A4BA-450b-A77C-A9A47762C520");
        private ICLRRuntimeInfo m_runtimeInfo;

        public CLRRuntimeInfo(System.Object clrRuntimeInfo)
        {
            m_runtimeInfo = (ICLRRuntimeInfo)clrRuntimeInfo;
        }

        public string GetVersionString()
        {
            StringBuilder sb = new StringBuilder(CLRMetaHost.MaxVersionStringLength);
            int verStrLength = sb.Capacity;
            m_runtimeInfo.GetVersionString(sb, ref verStrLength);
            return sb.ToString();
        }

        public string GetRuntimeDirectory()
        {
            StringBuilder sb = new StringBuilder();
            int strLength = 0;
            m_runtimeInfo.GetRuntimeDirectory(sb, ref strLength);
            sb.Capacity = strLength;
            int ret = m_runtimeInfo.GetRuntimeDirectory(sb, ref strLength);
            if (ret < 0)
                Marshal.ThrowExceptionForHR(ret);
            return sb.ToString();
        }

        public ICorDebug GetLegacyICorDebugInterface()
        {
            Guid ifaceId = typeof(ICorDebug).GUID;
            Guid clsId = m_ClsIdClrDebuggingLegacy;
            return (ICorDebug)m_runtimeInfo.GetInterface(ref clsId, ref ifaceId);
        }

    }


    // Details about this interface are in metahost.idl.
    [ComImport, InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown), Guid("BD39D1D2-BA2F-486A-89B0-B4B0CB466891")]
    internal interface ICLRRuntimeInfo
    {
        // Marshalling pcchBuffer as int even though it's unsigned. Max version string is 24 characters, so we should not need to go over 2 billion soon.
        void GetVersionString([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwzBuffer,
                              [In, Out, MarshalAs(UnmanagedType.U4)] ref int pcchBuffer);

        // Marshalling pcchBuffer as int even though it's unsigned. MAX_PATH is 260, unicode paths are 65535, so we should not need to go over 2 billion soon.
        [PreserveSig]
        int GetRuntimeDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwzBuffer,
                                [In, Out, MarshalAs(UnmanagedType.U4)] ref int pcchBuffer);

        int IsLoaded([In] IntPtr hndProcess);

        // Marshal pcchBuffer as int even though it's unsigned. Error strings approaching 2 billion characters are currently unheard-of.
        [LCIDConversion(3)]
        void LoadErrorString([In, MarshalAs(UnmanagedType.U4)] int iResourceID,
                             [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwzBuffer,
                             [In, Out, MarshalAs(UnmanagedType.U4)] ref int pcchBuffer,
                             [In] int iLocaleID);

        IntPtr LoadLibrary([In, MarshalAs(UnmanagedType.LPWStr)] string pwzDllName);

        IntPtr GetProcAddress([In, MarshalAs(UnmanagedType.LPStr)] string pszProcName);

        [return: MarshalAs(UnmanagedType.IUnknown)]
        System.Object GetInterface([In] ref Guid rclsid, [In] ref Guid riid);

    }


    /// <summary>
    /// Wrapper for the ICLRDebugging shim interface. This interface exposes the native pipeline
    /// architecture startup APIs
    /// </summary>
    public sealed class CLRDebugging
    {

        private static readonly Guid clsidCLRDebugging = new Guid("BACC578D-FBDD-48a4-969F-02D932B74634");
        private ICLRDebugging m_CLRDebugging;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>Creates the underlying interface from mscoree!CLRCreateInstance</remarks>
        public CLRDebugging()
        {
            object o;
            Guid ifaceId = typeof(ICLRDebugging).GUID;
            Guid clsid = clsidCLRDebugging;
            NativeMethods.CLRCreateInstance(ref clsid, ref ifaceId, out o);
            m_CLRDebugging = (ICLRDebugging)o;
        }

        /// <summary>
        /// Detects if a native module represents a CLR and if so provides the debugging interface
        /// and versioning information
        /// </summary>
        /// <param name="moduleBaseAddress">The native base address of a module which might be a CLR</param>
        /// <param name="dataTarget">The process abstraction which can be used for inspection</param>
        /// <param name="libraryProvider">A callback interface for locating version specific debug libraries
        /// such as mscordbi.dll and mscordacwks.dll</param>
        /// <param name="maxDebuggerSupportedVersion">The highest version of the CLR/debugging libraries which
        /// the caller can support</param>
        /// <param name="version">The version of the CLR detected or null if no CLR was detected</param>
        /// <param name="flags">Flags which have additional information about the CLR.
        /// See ClrDebuggingProcessFlags for more details</param>
        /// <returns>The CLR's debugging interface</returns>
        public CorProcess OpenVirtualProcess(ulong moduleBaseAddress,
            ICorDebugDataTarget dataTarget,
            ICLRDebuggingLibraryProvider libraryProvider,
            Version maxDebuggerSupportedVersion,
            out Version version,
            out ClrDebuggingProcessFlags flags)
        {
            CorProcess process;
            int hr = TryOpenVirtualProcess(moduleBaseAddress, dataTarget, libraryProvider, maxDebuggerSupportedVersion, out version, out flags, out process);
            if (hr < 0)
                throw new COMException("Failed to OpenVirtualProcess for module at " + moduleBaseAddress + ".", hr);
            return process;
        }

        /// <summary>
        /// Version of the above that doesn't throw exceptions on failure
        /// </summary>        
        public int TryOpenVirtualProcess(ulong moduleBaseAddress,
            ICorDebugDataTarget dataTarget,
            ICLRDebuggingLibraryProvider libraryProvider,
            Version maxDebuggerSupportedVersion,
            out Version version,
            out ClrDebuggingProcessFlags flags,
            out CorProcess process)
        {
            ClrDebuggingVersion maxSupport = new ClrDebuggingVersion();
            ClrDebuggingVersion clrVersion = new ClrDebuggingVersion();
            maxSupport.StructVersion = 0;
            maxSupport.Major = (short)maxDebuggerSupportedVersion.Major;
            maxSupport.Minor = (short)maxDebuggerSupportedVersion.Minor;
            maxSupport.Build = (short)maxDebuggerSupportedVersion.Build;
            maxSupport.Revision = (short)maxDebuggerSupportedVersion.Revision;
            object processIface = null;
            clrVersion.StructVersion = 0;
            Guid iid = typeof(ICorDebugProcess).GUID;

            int result = m_CLRDebugging.OpenVirtualProcess(moduleBaseAddress, dataTarget, libraryProvider,
                ref maxSupport, ref iid, out processIface, ref clrVersion, out flags);

            // This may be set regardless of success/failure
            version = new Version(clrVersion.Major, clrVersion.Minor, clrVersion.Build, clrVersion.Revision);

            if (result < 0)
            {
                // OpenVirtualProcess failed
                process = null;
                return result;
            }

            // Success
            process = CorProcess.GetCorProcess((ICorDebugProcess)processIface);
            return 0;
        }

        /// <summary>
        /// Determines if the module is no longer in use
        /// </summary>
        /// <param name="moduleHandle">A module handle that was provided via the ILibraryProvider</param>
        /// <returns>True if the module can be unloaded, False otherwise</returns>
        public bool CanUnloadNow(IntPtr moduleHandle)
        {
            int ret = m_CLRDebugging.CanUnloadNow(moduleHandle);
            if (ret == (int)HResult.S_OK)
                return true;
            else if (ret == (int)HResult.S_FALSE)
                return false;
            else
                Marshal.ThrowExceptionForHR(ret);

            //unreachable
            throw new Exception();
        }
    }

    /// <summary>
    /// Represents a version of the CLR runtime
    /// </summary>
    public struct ClrDebuggingVersion
    {
        public short StructVersion;
        public short Major;
        public short Minor;
        public short Build;
        public short Revision;
    }

    /// <summary>
    /// Information flags about the state of a CLR when it is being attached
    /// to in the native pipeline debugging model
    /// </summary>
    public enum ClrDebuggingProcessFlags
    {
        // This CLR has a non-catchup managed debug event to send after jit attach is complete
        ManagedDebugEventPending = 1
    }

    /// <summary>
    /// This interface exposes the native pipeline architecture startup APIs
    /// </summary>
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("D28F3C5A-9634-4206-A509-477552EEFB10")]
    public interface ICLRDebugging
    {
        /// <summary>
        /// Detects if a native module represents a CLR and if so provides the debugging interface
        /// and versioning information
        /// </summary>
        /// <param name="moduleBaseAddress">The native base address of a module which might be a CLR</param>
        /// <param name="dataTarget">The process abstraction which can be used for inspection</param>
        /// <param name="libraryProvider">A callback interface for locating version specific debug libraries
        /// such as mscordbi.dll and mscordacwks.dll</param>
        /// <param name="maxDebuggerSupportedVersion">The highest version of the CLR/debugging libraries which
        /// the caller can support</param>
        /// <param name="process">The CLR's debugging interface or null if no debugger was detected</param>
        /// <param name="version">The version of the CLR detected or null if no CLR was detected</param>
        /// <param name="flags">Flags which have additional information about the CLR.
        /// See ClrDebuggingProcessFlags for more details</param>
        /// <returns>HResults.S_OK if an appropriate version CLR was detected, otherwise an appropriate
        /// error hresult</returns>
        [PreserveSig]
        int OpenVirtualProcess([In] ulong moduleBaseAddress,
                                [In, MarshalAs(UnmanagedType.IUnknown)] object dataTarget,
                                [In, MarshalAs(UnmanagedType.Interface)] ICLRDebuggingLibraryProvider libraryProvider,
                                [In] ref ClrDebuggingVersion maxDebuggerSupportedVersion,
                                [In] ref Guid riidProcess,
                                [Out, MarshalAs(UnmanagedType.IUnknown)] out object process,
                                [In, Out] ref ClrDebuggingVersion version,
                                [Out] out ClrDebuggingProcessFlags flags);

        /// <summary>
        /// Determines if the module is no longer in use
        /// </summary>
        /// <param name="moduleHandle">A module handle that was provided via the ILibraryProvider</param>
        /// <returns>HResults.S_OK if the module can be unloaded, HResults.S_FALSE if it is in use
        /// or an appropriate error hresult otherwise</returns>
        [PreserveSig]
        int CanUnloadNow(IntPtr moduleHandle);
    }

    /// <summary>
    /// Provides version specific debugging libraries such as mscordbi.dll and mscordacwks.dll during
    /// startup in the native pipeline debugging architecture
    /// </summary>
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("3151C08D-4D09-4f9b-8838-2880BF18FE51")]
    public interface ICLRDebuggingLibraryProvider
    {
        /// <summary>
        /// Provides a version specific debugging library
        /// </summary>
        /// <param name="fileName">The name of the library being requested</param>
        /// <param name="timestamp">The timestamp of the library being requested as specified
        /// in the PE header</param>
        /// <param name="sizeOfImage">The SizeOfImage of the library being requested as specified
        /// in the PE header</param>
        /// <param name="moduleHandle">An OS handle to the requested library</param>
        /// <returns>HResults.S_OK if the library was located, otherwise any appropriate
        /// error hresult</returns>
        [PreserveSig]
        int ProvideLibrary([In, MarshalAs(UnmanagedType.LPWStr)]string fileName,
            int timestamp,
            int sizeOfImage,
            out IntPtr hModule);
    }

    // Wrapper for standard COM IEnumUnknown, needed for ICLRMetaHost enumeration APIs.
    [ComImport, InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown), Guid("00000100-0000-0000-C000-000000000046")]
    internal interface IEnumUnknown
    {

        [PreserveSig]
        int Next(
            [In, MarshalAs(UnmanagedType.U4)]
             int celt,
            [Out, MarshalAs(UnmanagedType.IUnknown)]
            out System.Object rgelt,
            IntPtr pceltFetched);

        [PreserveSig]
        int Skip(
        [In, MarshalAs(UnmanagedType.U4)]
            int celt);

        void Reset();

        void Clone(
            [Out] 
            out IEnumUnknown ppenum);
    }



    ////////////////////////////////////////////////////////////////////////////////
    //
    // CorEvent Classes & Corresponding delegates
    //
    ////////////////////////////////////////////////////////////////////////////////

    /**
     * All of the Debugger events make a Controller available (to specify
     * whether or not to continue the program, or to stop, etc.).
     *
     * This serves as the base class for all events used for debugging.
     *
     * NOTE: If you don't want <b>Controller.Continue(false)</b> to be
     * called after event processing has finished, you need to set the
     * <b>Continue</b> property to <b>false</b>.
     */

    public class CorEventArgs : EventArgs
    {
        private CorController m_controller;

        private bool m_continue;

        private ManagedCallbackType m_callbackType;

        private CorThread m_thread;

        public CorEventArgs(CorController controller)
        {
            m_controller = controller;
            m_continue = true;
        }

        public CorEventArgs(CorController controller, ManagedCallbackType callbackType)
        {
            m_controller = controller;
            m_continue = true;
            m_callbackType = callbackType;
        }

        /** The Controller of the current event. */
        public CorController Controller
        {
            get
            {
                return m_controller;
            }
        }

        /** 
         * The default behavior after an event is to Continue processing
         * after the event has been handled.  This can be changed by
         * setting this property to false.
         */
        public virtual bool Continue
        {
            get
            {
                return m_continue;
            }
            set
            {
                m_continue = value;
            }
        }

        /// <summary>
        /// The type of callback that returned this CorEventArgs object.
        /// </summary>
        public ManagedCallbackType CallbackType
        {
            get
            {
                return m_callbackType;
            }
        }

        /// <summary>
        /// The CorThread associated with the callback event that returned
        /// this CorEventArgs object. If here is no such thread, Thread is null.
        /// </summary>
        public CorThread Thread
        {
            get
            {
                return m_thread;
            }
            protected set
            {
                m_thread = value;
            }
        }

        /// <summary>
        /// The CorProcess associated with this event.
        /// </summary>
        public CorProcess Process
        {
            get
            {
                CorProcess process = m_controller as CorProcess;
                if (process != null)
                {
                    return process;
                }
                else
                {
                    Debug.Assert(m_controller is CorAppDomain);
                    return ((CorAppDomain)m_controller).Process;
                }
            }
        }

    }


    /**
     * This class is used for all events that only have access to the 
     * CorProcess that is generating the event.
     */
    public class CorProcessEventArgs : CorEventArgs
    {
        public CorProcessEventArgs(CorProcess process)
            : base(process)
        {
        }

        public CorProcessEventArgs(CorProcess process, ManagedCallbackType callbackType)
            : base(process, callbackType)
        {
        }

        public override string ToString()
        {
            switch (CallbackType)
            {
                case ManagedCallbackType.OnCreateProcess:
                    return "Process Created";
                case ManagedCallbackType.OnProcessExit:
                    return "Process Exited";
                case ManagedCallbackType.OnControlCTrap:
                    break;
            }
            return base.ToString();
        }
    }

    public delegate void CorProcessEventHandler(Object sender,
                                                 CorProcessEventArgs e);


    /**
     * The event arguments for events that contain both a CorProcess
     * and an CorAppDomain.
     */
    public class CorAppDomainEventArgs : CorProcessEventArgs
    {
        private CorAppDomain m_ad;

        public CorAppDomainEventArgs(CorProcess process, CorAppDomain ad)
            : base(process)
        {
            m_ad = ad;
        }

        public CorAppDomainEventArgs(CorProcess process, CorAppDomain ad,
                                      ManagedCallbackType callbackType)
            : base(process, callbackType)
        {
            m_ad = ad;
        }

        /** The AppDomain that generated the event. */
        public CorAppDomain AppDomain
        {
            get
            {
                return m_ad;
            }
        }

        public override string ToString()
        {
            switch (CallbackType)
            {
                case ManagedCallbackType.OnCreateAppDomain:
                    return "AppDomain Created: " + m_ad.Name;
                case ManagedCallbackType.OnAppDomainExit:
                    return "AppDomain Exited: " + m_ad.Name;
            }
            return base.ToString();
        }
    }

    public delegate void CorAppDomainEventHandler(Object sender,
                                                   CorAppDomainEventArgs e);


    /**
     * The base class for events which take an CorAppDomain as their
     * source, but not a CorProcess.
     */
    public class CorAppDomainBaseEventArgs : CorEventArgs
    {
        public CorAppDomainBaseEventArgs(CorAppDomain ad)
            : base(ad)
        {
        }

        public CorAppDomainBaseEventArgs(CorAppDomain ad, ManagedCallbackType callbackType)
            : base(ad, callbackType)
        {
        }

        public CorAppDomain AppDomain
        {
            get
            {
                return (CorAppDomain)Controller;
            }
        }
    }


    /**
     * Arguments for events dealing with threads.
     */
    public class CorThreadEventArgs : CorAppDomainBaseEventArgs
    {
        public CorThreadEventArgs(CorAppDomain appDomain, CorThread thread)
            : base(appDomain != null ? appDomain : thread.AppDomain)
        {
            Thread = thread;
        }

        public CorThreadEventArgs(CorAppDomain appDomain, CorThread thread,
            ManagedCallbackType callbackType)
            : base(appDomain != null ? appDomain : thread.AppDomain, callbackType)
        {
            Thread = thread;
        }

        public override string ToString()
        {
            switch (CallbackType)
            {
                case ManagedCallbackType.OnBreak:
                    return "Break";
                case ManagedCallbackType.OnCreateThread:
                    return "Thread Created";
                case ManagedCallbackType.OnThreadExit:
                    return "Thread Exited";
                case ManagedCallbackType.OnNameChange:
                    return "Name Changed";
            }
            return base.ToString();
        }
    }

    public delegate void CorThreadEventHandler(Object sender,
                                                CorThreadEventArgs e);


    /**
     * Arguments for events involving breakpoints.
     */
    public class CorBreakpointEventArgs : CorThreadEventArgs
    {
        private CorBreakpoint m_break;

        public CorBreakpointEventArgs(CorAppDomain appDomain,
                                       CorThread thread,
                                       CorBreakpoint managedBreakpoint)
            : base(appDomain, thread)
        {
            m_break = managedBreakpoint;
        }

        public CorBreakpointEventArgs(CorAppDomain appDomain,
                                       CorThread thread,
                                       CorBreakpoint managedBreakpoint,
                                       ManagedCallbackType callbackType)
            : base(appDomain, thread, callbackType)
        {
            m_break = managedBreakpoint;
        }

        /** The breakpoint involved. */
        public CorBreakpoint Breakpoint
        {
            get
            {
                return m_break;
            }
        }

        public override string ToString()
        {
            if (CallbackType == ManagedCallbackType.OnBreakpoint)
            {
                return "Breakpoint Hit";
            }
            return base.ToString();
        }
    }

    public delegate void BreakpointEventHandler(Object sender,
                                                 CorBreakpointEventArgs e);


    /**
     * Arguments for when a Step operation has completed.
     */
    public class CorStepCompleteEventArgs : CorThreadEventArgs
    {
        private CorStepper m_stepper;
        private CorDebugStepReason m_stepReason;

        [CLSCompliant(false)]
        public CorStepCompleteEventArgs(CorAppDomain appDomain, CorThread thread,
                                         CorStepper stepper, CorDebugStepReason stepReason)
            : base(appDomain, thread)
        {
            m_stepper = stepper;
            m_stepReason = stepReason;
        }

        [CLSCompliant(false)]
        public CorStepCompleteEventArgs(CorAppDomain appDomain, CorThread thread,
                                         CorStepper stepper, CorDebugStepReason stepReason,
                                         ManagedCallbackType callbackType)
            : base(appDomain, thread, callbackType)
        {
            m_stepper = stepper;
            m_stepReason = stepReason;
        }

        public CorStepper Stepper
        {
            get
            {
                return m_stepper;
            }
        }

        [CLSCompliant(false)]
        public CorDebugStepReason StepReason
        {
            get
            {
                return m_stepReason;
            }
        }

        public override string ToString()
        {
            if (CallbackType == ManagedCallbackType.OnStepComplete)
            {
                return "Step Complete";
            }
            return base.ToString();
        }
    }

    public delegate void StepCompleteEventHandler(Object sender,
                                                   CorStepCompleteEventArgs e);


    /**
     * For events dealing with exceptions.
     */
    public class CorExceptionEventArgs : CorThreadEventArgs
    {
        bool m_unhandled;

        public CorExceptionEventArgs(CorAppDomain appDomain,
                                      CorThread thread,
                                      bool unhandled)
            : base(appDomain, thread)
        {
            m_unhandled = unhandled;
        }

        public CorExceptionEventArgs(CorAppDomain appDomain,
                                      CorThread thread,
                                      bool unhandled,
                                      ManagedCallbackType callbackType)
            : base(appDomain, thread, callbackType)
        {
            m_unhandled = unhandled;
        }

        /** Has the exception been handled yet? */
        public bool Unhandled
        {
            get
            {
                return m_unhandled;
            }
        }
    }

    public delegate void CorExceptionEventHandler(Object sender,
                                                   CorExceptionEventArgs e);


    /**
     * For events dealing the evaluation of something...
     */
    public class CorEvalEventArgs : CorThreadEventArgs
    {
        CorEval m_eval;

        public CorEvalEventArgs(CorAppDomain appDomain, CorThread thread,
                                 CorEval eval)
            : base(appDomain, thread)
        {
            m_eval = eval;
        }

        public CorEvalEventArgs(CorAppDomain appDomain, CorThread thread,
                                 CorEval eval, ManagedCallbackType callbackType)
            : base(appDomain, thread, callbackType)
        {
            m_eval = eval;
        }

        /** The object being evaluated. */
        public CorEval Eval
        {
            get
            {
                return m_eval;
            }
        }

        public override string ToString()
        {
            switch (CallbackType)
            {
                case ManagedCallbackType.OnEvalComplete:
                    return "Eval Complete";
                case ManagedCallbackType.OnEvalException:
                    return "Eval Exception";
            }
            return base.ToString();
        }
    }

    public delegate void EvalEventHandler(Object sender, CorEvalEventArgs e);


    /**
     * For events dealing with module loading/unloading.
     */
    public class CorModuleEventArgs : CorAppDomainBaseEventArgs
    {
        CorModule m_managedModule;

        public CorModuleEventArgs(CorAppDomain appDomain, CorModule managedModule)
            : base(appDomain)
        {
            m_managedModule = managedModule;
        }

        public CorModuleEventArgs(CorAppDomain appDomain, CorModule managedModule,
            ManagedCallbackType callbackType)
            : base(appDomain, callbackType)
        {
            m_managedModule = managedModule;
        }

        public CorModule Module
        {
            get
            {
                return m_managedModule;
            }
        }

        public override string ToString()
        {
            switch (CallbackType)
            {
                case ManagedCallbackType.OnModuleLoad:
                    return "Module loaded: " + m_managedModule.Name;
                case ManagedCallbackType.OnModuleUnload:
                    return "Module unloaded: " + m_managedModule.Name;
            }
            return base.ToString();
        }
    }

    public delegate void CorModuleEventHandler(Object sender,
                                                CorModuleEventArgs e);


    /**
     * For events dealing with class loading/unloading.
     */
    public class CorClassEventArgs : CorAppDomainBaseEventArgs
    {
        CorClass m_class;

        public CorClassEventArgs(CorAppDomain appDomain, CorClass managedClass)
            : base(appDomain)
        {
            m_class = managedClass;
        }

        public CorClassEventArgs(CorAppDomain appDomain, CorClass managedClass,
            ManagedCallbackType callbackType)
            : base(appDomain, callbackType)
        {
            m_class = managedClass;
        }

        public CorClass Class
        {
            get
            {
                return m_class;
            }
        }

        public override string ToString()
        {
            // I'd like to get the actual class name here, but we don't have 
            // access to the metadata inside the corapi layer. 
            string className = string.Format("{0} typedef={1:X}",
                m_class.Module.Name,
                m_class.Token);

            switch (CallbackType)
            {
                case ManagedCallbackType.OnClassLoad:
                    return "Class loaded: " + className;
                case ManagedCallbackType.OnClassUnload:
                    return "Class unloaded: " + className;
            }
            return base.ToString();
        }
    }

    public delegate void CorClassEventHandler(Object sender,
                                               CorClassEventArgs e);


    /**
     * For events dealing with debugger errors.
     */
    public class CorDebuggerErrorEventArgs : CorProcessEventArgs
    {
        int m_hresult;
        int m_errorCode;

        public CorDebuggerErrorEventArgs(CorProcess process, int hresult,
                                          int errorCode)
            : base(process)
        {
            m_hresult = hresult;
            m_errorCode = errorCode;
        }

        public CorDebuggerErrorEventArgs(CorProcess process, int hresult,
                                          int errorCode, ManagedCallbackType callbackType)
            : base(process, callbackType)
        {
            m_hresult = hresult;
            m_errorCode = errorCode;
        }

        public int HResult
        {
            get
            {
                return m_hresult;
            }
        }

        public int ErrorCode
        {
            get
            {
                return m_errorCode;
            }
        }

        public override string ToString()
        {
            if (CallbackType == ManagedCallbackType.OnDebuggerError)
            {
                return "Debugger Error";
            }
            return base.ToString();
        }
    }

    public delegate void DebuggerErrorEventHandler(Object sender,
                                                    CorDebuggerErrorEventArgs e);


    /**
     * For events dealing with Assemblies.
     */
    public class CorAssemblyEventArgs : CorAppDomainBaseEventArgs
    {
        private CorAssembly m_assembly;
        public CorAssemblyEventArgs(CorAppDomain appDomain,
                                     CorAssembly assembly)
            : base(appDomain)
        {
            m_assembly = assembly;
        }

        public CorAssemblyEventArgs(CorAppDomain appDomain,
                                     CorAssembly assembly, ManagedCallbackType callbackType)
            : base(appDomain, callbackType)
        {
            m_assembly = assembly;
        }

        /** The Assembly of interest. */
        public CorAssembly Assembly
        {
            get
            {
                return m_assembly;
            }
        }

        public override string ToString()
        {
            switch (CallbackType)
            {
                case ManagedCallbackType.OnAssemblyLoad:
                    return "Assembly loaded: " + m_assembly.Name;
                case ManagedCallbackType.OnAssemblyUnload:
                    return "Assembly unloaded: " + m_assembly.Name;
            }
            return base.ToString();
        }
    }

    public delegate void CorAssemblyEventHandler(Object sender,
                                                  CorAssemblyEventArgs e);


    /**
     * For events dealing with logged messages.
     */
    public class CorLogMessageEventArgs : CorThreadEventArgs
    {
        int m_level;
        string m_logSwitchName;
        string m_message;

        public CorLogMessageEventArgs(CorAppDomain appDomain, CorThread thread,
                                       int level, string logSwitchName, string message)
            : base(appDomain, thread)
        {
            m_level = level;
            m_logSwitchName = logSwitchName;
            m_message = message;
        }

        public CorLogMessageEventArgs(CorAppDomain appDomain, CorThread thread,
                                       int level, string logSwitchName, string message,
                                       ManagedCallbackType callbackType)
            : base(appDomain, thread, callbackType)
        {
            m_level = level;
            m_logSwitchName = logSwitchName;
            m_message = message;
        }

        public int Level
        {
            get
            {
                return m_level;
            }
        }

        public string LogSwitchName
        {
            get
            {
                return m_logSwitchName;
            }
        }

        public string Message
        {
            get
            {
                return m_message;
            }
        }

        public override string ToString()
        {
            if (CallbackType == ManagedCallbackType.OnLogMessage)
            {
                return "Log message(" + m_logSwitchName + ")";
            }
            return base.ToString();
        }
    }

    public delegate void LogMessageEventHandler(Object sender,
                                                 CorLogMessageEventArgs e);


    /**
     * For events dealing with logged messages.
     */
    public class CorLogSwitchEventArgs : CorThreadEventArgs
    {
        int m_level;

        int m_reason;

        string m_logSwitchName;

        string m_parentName;

        public CorLogSwitchEventArgs(CorAppDomain appDomain, CorThread thread,
                                      int level, int reason, string logSwitchName, string parentName)
            : base(appDomain, thread)
        {
            m_level = level;
            m_reason = reason;
            m_logSwitchName = logSwitchName;
            m_parentName = parentName;
        }

        public CorLogSwitchEventArgs(CorAppDomain appDomain, CorThread thread,
                                      int level, int reason, string logSwitchName, string parentName,
                                      ManagedCallbackType callbackType)
            : base(appDomain, thread, callbackType)
        {
            m_level = level;
            m_reason = reason;
            m_logSwitchName = logSwitchName;
            m_parentName = parentName;
        }

        public int Level
        {
            get
            {
                return m_level;
            }
        }

        public int Reason
        {
            get
            {
                return m_reason;
            }
        }

        public string LogSwitchName
        {
            get
            {
                return m_logSwitchName;
            }
        }

        public string ParentName
        {
            get
            {
                return m_parentName;
            }
        }

        public override string ToString()
        {
            if (CallbackType == ManagedCallbackType.OnLogSwitch)
            {
                return "Log Switch" + "\n" +
                    "Level: " + m_level + "\n" +
                    "Log Switch Name: " + m_logSwitchName;
            }
            return base.ToString();
        }
    }

    public delegate void LogSwitchEventHandler(Object sender,
                                                CorLogSwitchEventArgs e);

    /**
      * For events dealing with custom notifications.
      */
    public class CorCustomNotificationEventArgs : CorThreadEventArgs
    {
        // thread on which the notification occurred
        CorThread m_thread;

        // constructor
        // Arguments: thread: thread on which the notification occurred
        //            appDomain: appdomain in which the notification occurred
        //            callbackType: the type of the callback for theis event
        public CorCustomNotificationEventArgs(CorThread thread, CorAppDomain appDomain,
                                              ManagedCallbackType callbackType)
            : base(appDomain, thread, callbackType)
        {
            m_thread = thread;
        }

        // we're not really doing anything with this (yet), so we don't need much in the
        // way of functionality
        public override string ToString()
        {
            if (CallbackType == ManagedCallbackType.OnCustomNotification)
            {
                return "Custom Notification";
            }
            return base.ToString();
        }
    }

    /**
     * Handler for custom notification events.
     */

    public delegate void CustomNotificationEventHandler(Object sender,
                                                        CorCustomNotificationEventArgs e);

    /**
     * For events dealing with MDA messages.
     */
    public class CorMDAEventArgs : CorProcessEventArgs
    {
        // Thread may be null.
        public CorMDAEventArgs(CorMDA mda, CorThread thread, CorProcess proc)
            : base(proc)
        {
            m_mda = mda;
            Thread = thread;
            //m_proc = proc;
        }

        public CorMDAEventArgs(CorMDA mda, CorThread thread, CorProcess proc,
            ManagedCallbackType callbackType)
            : base(proc, callbackType)
        {
            m_mda = mda;
            Thread = thread;
            //m_proc = proc;
        }

        CorMDA m_mda;
        public CorMDA MDA { get { return m_mda; } }

        public override string ToString()
        {
            if (CallbackType == ManagedCallbackType.OnMDANotification)
            {
                return "MDANotification" + "\n" +
                    "Name=" + m_mda.Name + "\n" +
                    "XML=" + m_mda.XML;
            }
            return base.ToString();
        }

        //CorProcess m_proc;
        //CorProcess Process { get { return m_proc; } }
    }

    public delegate void MDANotificationEventHandler(Object sender, CorMDAEventArgs e);



    /**
     * For events dealing module symbol updates.
     */
    public class CorUpdateModuleSymbolsEventArgs : CorModuleEventArgs
    {
        IStream m_stream;

        [CLSCompliant(false)]
        public CorUpdateModuleSymbolsEventArgs(CorAppDomain appDomain,
                                                CorModule managedModule,
                                                IStream stream)
            : base(appDomain, managedModule)
        {
            m_stream = stream;
        }

        [CLSCompliant(false)]
        public CorUpdateModuleSymbolsEventArgs(CorAppDomain appDomain,
                                                CorModule managedModule,
                                                IStream stream,
                                                ManagedCallbackType callbackType)
            : base(appDomain, managedModule, callbackType)
        {
            m_stream = stream;
        }

        [CLSCompliant(false)]
        public IStream Stream
        {
            get
            {
                return m_stream;
            }
        }

        public override string ToString()
        {
            if (CallbackType == ManagedCallbackType.OnUpdateModuleSymbols)
            {
                return "Module Symbols Updated";
            }
            return base.ToString();
        }
    }

    public delegate void UpdateModuleSymbolsEventHandler(Object sender,
                                                          CorUpdateModuleSymbolsEventArgs e);

    public sealed class CorExceptionInCallbackEventArgs : CorEventArgs
    {
        public CorExceptionInCallbackEventArgs(CorController controller, Exception exceptionThrown)
            : base(controller)
        {
            m_exceptionThrown = exceptionThrown;
        }

        public CorExceptionInCallbackEventArgs(CorController controller, Exception exceptionThrown,
            ManagedCallbackType callbackType)
            : base(controller, callbackType)
        {
            m_exceptionThrown = exceptionThrown;
        }

        public Exception ExceptionThrown
        {
            get
            {
                return m_exceptionThrown;
            }
        }

        public override string ToString()
        {
            if (CallbackType == ManagedCallbackType.OnExceptionInCallback)
            {
                return "Callback Exception: " + m_exceptionThrown.Message;
            }
            return base.ToString();
        }

        private Exception m_exceptionThrown;
    }

    public delegate void CorExceptionInCallbackEventHandler(Object sender,
                                             CorExceptionInCallbackEventArgs e);


    /**
     * Edit and Continue callbacks
     */
    public class CorEditAndContinueRemapEventArgs : CorThreadEventArgs
    {
        public CorEditAndContinueRemapEventArgs(CorAppDomain appDomain,
                                        CorThread thread,
                                        CorFunction managedFunction,
                                        int accurate)
            : base(appDomain, thread)
        {
            m_managedFunction = managedFunction;
            m_accurate = accurate;
        }

        public CorEditAndContinueRemapEventArgs(CorAppDomain appDomain,
                                        CorThread thread,
                                        CorFunction managedFunction,
                                        int accurate,
                                        ManagedCallbackType callbackType)
            : base(appDomain, thread, callbackType)
        {
            m_managedFunction = managedFunction;
            m_accurate = accurate;
        }

        public CorFunction Function
        {
            get
            {
                return m_managedFunction;
            }
        }

        public bool IsAccurate
        {
            get
            {
                return m_accurate != 0;
            }
        }

        private CorFunction m_managedFunction;
        private int m_accurate;
    }
    public delegate void CorEditAndContinueRemapEventHandler(Object sender,
                                                              CorEditAndContinueRemapEventArgs e);


    public class CorBreakpointSetErrorEventArgs : CorThreadEventArgs
    {
        public CorBreakpointSetErrorEventArgs(CorAppDomain appDomain,
                                        CorThread thread,
                                        CorBreakpoint breakpoint,
                                        int errorCode)
            : base(appDomain, thread)
        {
            m_breakpoint = breakpoint;
            m_errorCode = errorCode;
        }

        public CorBreakpointSetErrorEventArgs(CorAppDomain appDomain,
                                        CorThread thread,
                                        CorBreakpoint breakpoint,
                                        int errorCode,
                                        ManagedCallbackType callbackType)
            : base(appDomain, thread, callbackType)
        {
            m_breakpoint = breakpoint;
            m_errorCode = errorCode;
        }

        public CorBreakpoint Breakpoint
        {
            get
            {
                return m_breakpoint;
            }
        }

        public int ErrorCode
        {
            get
            {
                return m_errorCode;
            }
        }

        public override string ToString()
        {
            if (CallbackType == ManagedCallbackType.OnBreakpointSetError)
            {
                return "Error Setting Breakpoint";
            }
            return base.ToString();
        }

        private CorBreakpoint m_breakpoint;
        private int m_errorCode;
    }
    public delegate void CorBreakpointSetErrorEventHandler(Object sender,
                                                           CorBreakpointSetErrorEventArgs e);


    public sealed class CorFunctionRemapOpportunityEventArgs : CorThreadEventArgs
    {
        public CorFunctionRemapOpportunityEventArgs(CorAppDomain appDomain,
                                           CorThread thread,
                                           CorFunction oldFunction,
                                           CorFunction newFunction,
                                           int oldILoffset
                                           )
            : base(appDomain, thread)
        {
            m_oldFunction = oldFunction;
            m_newFunction = newFunction;
            m_oldILoffset = oldILoffset;
        }

        public CorFunctionRemapOpportunityEventArgs(CorAppDomain appDomain,
                                           CorThread thread,
                                           CorFunction oldFunction,
                                           CorFunction newFunction,
                                           int oldILoffset,
                                           ManagedCallbackType callbackType
                                           )
            : base(appDomain, thread, callbackType)
        {
            m_oldFunction = oldFunction;
            m_newFunction = newFunction;
            m_oldILoffset = oldILoffset;
        }

        public CorFunction OldFunction
        {
            get
            {
                return m_oldFunction;
            }
        }

        public CorFunction NewFunction
        {
            get
            {
                return m_newFunction;
            }
        }

        public int OldILOffset
        {
            get
            {
                return m_oldILoffset;
            }
        }

        public override string ToString()
        {
            if (CallbackType == ManagedCallbackType.OnFunctionRemapOpportunity)
            {
                return "Function Remap Opportunity";
            }
            return base.ToString();
        }

        private CorFunction m_oldFunction, m_newFunction;
        private int m_oldILoffset;
    }

    public delegate void CorFunctionRemapOpportunityEventHandler(Object sender,
                                                       CorFunctionRemapOpportunityEventArgs e);

    public sealed class CorFunctionRemapCompleteEventArgs : CorThreadEventArgs
    {
        public CorFunctionRemapCompleteEventArgs(CorAppDomain appDomain,
                                           CorThread thread,
                                           CorFunction managedFunction
                                           )
            : base(appDomain, thread)
        {
            m_managedFunction = managedFunction;
        }

        public CorFunctionRemapCompleteEventArgs(CorAppDomain appDomain,
                                           CorThread thread,
                                           CorFunction managedFunction,
                                           ManagedCallbackType callbackType
                                           )
            : base(appDomain, thread, callbackType)
        {
            m_managedFunction = managedFunction;
        }

        public CorFunction Function
        {
            get
            {
                return m_managedFunction;
            }
        }

        private CorFunction m_managedFunction;
    }

    public delegate void CorFunctionRemapCompleteEventHandler(Object sender,
                                                              CorFunctionRemapCompleteEventArgs e);


    public class CorExceptionUnwind2EventArgs : CorThreadEventArgs
    {

        [CLSCompliant(false)]
        public CorExceptionUnwind2EventArgs(CorAppDomain appDomain, CorThread thread,
                                            CorDebugExceptionUnwindCallbackType eventType,
                                            int flags)
            : base(appDomain, thread)
        {
            m_eventType = eventType;
            m_flags = flags;
        }

        [CLSCompliant(false)]
        public CorExceptionUnwind2EventArgs(CorAppDomain appDomain, CorThread thread,
                                            CorDebugExceptionUnwindCallbackType eventType,
                                            int flags,
                                            ManagedCallbackType callbackType)
            : base(appDomain, thread, callbackType)
        {
            m_eventType = eventType;
            m_flags = flags;
        }

        [CLSCompliant(false)]
        public CorDebugExceptionUnwindCallbackType EventType
        {
            get
            {
                return m_eventType;
            }
        }

        public int Flags
        {
            get
            {
                return m_flags;
            }
        }

        public override string ToString()
        {
            if (CallbackType == ManagedCallbackType.OnExceptionUnwind2)
            {
                return "Exception unwind\n" +
                    "EventType: " + m_eventType;
            }
            return base.ToString();
        }

        CorDebugExceptionUnwindCallbackType m_eventType;
        int m_flags;
    }

    public delegate void CorExceptionUnwind2EventHandler(Object sender,
                                                   CorExceptionUnwind2EventArgs e);


    public class CorException2EventArgs : CorThreadEventArgs
    {

        [CLSCompliant(false)]
        public CorException2EventArgs(CorAppDomain appDomain,
                                      CorThread thread,
                                      CorFrame frame,
                                      int offset,
                                      CorDebugExceptionCallbackType eventType,
                                      int flags)
            : base(appDomain, thread)
        {
            m_frame = frame;
            m_offset = offset;
            m_eventType = eventType;
            m_flags = flags;
        }

        [CLSCompliant(false)]
        public CorException2EventArgs(CorAppDomain appDomain,
                                      CorThread thread,
                                      CorFrame frame,
                                      int offset,
                                      CorDebugExceptionCallbackType eventType,
                                      int flags,
                                      ManagedCallbackType callbackType)
            : base(appDomain, thread, callbackType)
        {
            m_frame = frame;
            m_offset = offset;
            m_eventType = eventType;
            m_flags = flags;
        }

        public CorFrame Frame
        {
            get
            {
                return m_frame;
            }
        }

        public int Offset
        {
            get
            {
                return m_offset;
            }
        }

        [CLSCompliant(false)]
        public CorDebugExceptionCallbackType EventType
        {
            get
            {
                return m_eventType;
            }
        }

        public int Flags
        {
            get
            {
                return m_flags;
            }
        }

        public override string ToString()
        {
            if (CallbackType == ManagedCallbackType.OnException2)
            {
                return "Exception Thrown";
            }
            return base.ToString();
        }

        CorFrame m_frame;
        int m_offset;
        CorDebugExceptionCallbackType m_eventType;
        int m_flags;
    }

    public delegate void CorException2EventHandler(Object sender,
                                                   CorException2EventArgs e);
    public class CorNativeStopEventArgs : CorProcessEventArgs
    {
        [CLSCompliant(false)]
        public CorNativeStopEventArgs(CorProcess process,
                                      int threadId,
                                      IntPtr debugEvent,
                                      bool isOutOfBand)
            : base(process)
        {
            m_threadId = threadId;
            m_debugEvent = debugEvent;
            m_isOutOfBand = isOutOfBand;
        }

        [CLSCompliant(false)]
        public CorNativeStopEventArgs(CorProcess process,
                                      int threadId,
                                      IntPtr debugEvent,
                                      bool isOutOfBand,
                                      ManagedCallbackType callbackType)
            : base(process, callbackType)
        {
            m_threadId = threadId;
            m_debugEvent = debugEvent;
            m_isOutOfBand = isOutOfBand;
        }

        public int ThreadId
        {
            get
            {
                return m_threadId;
            }
        }

        public bool IsOutOfBand
        {
            get
            {
                return m_isOutOfBand;
            }
        }

        [CLSCompliant(false)]
        public IntPtr DebugEvent
        {
            get
            {
                return m_debugEvent;
            }
        }

        public override bool Continue
        {
            get
            {
                // we should not be able to change default for OOB events
                return base.Continue;
            }
            set
            {
                if (m_isOutOfBand && (value == false))
                {
                    Debug.Assert(false, "Cannot stop on OOB events");
                    throw new InvalidOperationException("Cannot stop on OOB events");
                }
                base.Continue = value;
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }

        private int m_threadId;
        private IntPtr m_debugEvent;
        private bool m_isOutOfBand;
    }

    public delegate void CorNativeStopEventHandler(Object sender,
                                                    CorNativeStopEventArgs e);

    public enum ManagedCallbackType
    {
        OnBreakpoint,
        OnStepComplete,
        OnBreak,
        OnException,
        OnEvalComplete,
        OnEvalException,
        OnCreateProcess,
        OnProcessExit,
        OnCreateThread,
        OnThreadExit,
        OnModuleLoad,
        OnModuleUnload,
        OnClassLoad,
        OnClassUnload,
        OnDebuggerError,
        OnLogMessage,
        OnLogSwitch,
        OnCreateAppDomain,
        OnAppDomainExit,
        OnAssemblyLoad,
        OnAssemblyUnload,
        OnControlCTrap,
        OnNameChange,
        OnUpdateModuleSymbols,
        OnFunctionRemapOpportunity,
        OnFunctionRemapComplete,
        OnBreakpointSetError,
        OnException2,
        OnExceptionUnwind2,
        OnMDANotification,
        OnExceptionInCallback,
        OnCustomNotification
    }
    internal enum ManagedCallbackTypeCount
    {
        Last = ManagedCallbackType.OnCustomNotification,
    }

    // Helper class to convert from COM-classic callback interface into managed args.
    // Derived classes can overide the HandleEvent method to define the handling.
    abstract public class ManagedCallbackBase : ICorDebugManagedCallback, ICorDebugManagedCallback2, ICorDebugManagedCallback3
    {
        // Derived class overrides this methdos 
        protected abstract void HandleEvent(ManagedCallbackType eventId, CorEventArgs args);

        void ICorDebugManagedCallback.Breakpoint(ICorDebugAppDomain appDomain,
                                ICorDebugThread thread,
                                ICorDebugBreakpoint breakpoint)
        {
            HandleEvent(ManagedCallbackType.OnBreakpoint,
                               new CorBreakpointEventArgs(appDomain == null ? null : new CorAppDomain(appDomain),
                                                           thread == null ? null : new CorThread(thread),
                                                           breakpoint == null ? null : new CorFunctionBreakpoint((ICorDebugFunctionBreakpoint)breakpoint),
                                                           ManagedCallbackType.OnBreakpoint
                                                           ));
        }

        void ICorDebugManagedCallback.StepComplete(ICorDebugAppDomain appDomain,
                                   ICorDebugThread thread,
                                   ICorDebugStepper stepper,
                                   CorDebugStepReason stepReason)
        {
            HandleEvent(ManagedCallbackType.OnStepComplete,
                               new CorStepCompleteEventArgs(appDomain == null ? null : new CorAppDomain(appDomain),
                                                            thread == null ? null : new CorThread(thread),
                                                            stepper == null ? null : new CorStepper(stepper),
                                                            stepReason,
                                                            ManagedCallbackType.OnStepComplete));
        }

        void ICorDebugManagedCallback.Break(
                           ICorDebugAppDomain appDomain,
                           ICorDebugThread thread)
        {
            HandleEvent(ManagedCallbackType.OnBreak,
                               new CorThreadEventArgs(appDomain == null ? null : new CorAppDomain(appDomain),
                                                      thread == null ? null : new CorThread(thread),
                                                      ManagedCallbackType.OnBreak));
        }

        void ICorDebugManagedCallback.Exception(
                                                 ICorDebugAppDomain appDomain,
                                                 ICorDebugThread thread,
                                                 int unhandled)
        {
            HandleEvent(ManagedCallbackType.OnException,
                               new CorExceptionEventArgs(appDomain == null ? null : new CorAppDomain(appDomain),
                                                         thread == null ? null : new CorThread(thread),
                                                         !(unhandled == 0),
                                                         ManagedCallbackType.OnException));
        }
        /* pass false if ``unhandled'' is 0 -- mapping TRUE to true, etc. */

        void ICorDebugManagedCallback.EvalComplete(
                                  ICorDebugAppDomain appDomain,
                                  ICorDebugThread thread,
                                  ICorDebugEval eval)
        {
            HandleEvent(ManagedCallbackType.OnEvalComplete,
                              new CorEvalEventArgs(appDomain == null ? null : new CorAppDomain(appDomain),
                                                    thread == null ? null : new CorThread(thread),
                                                    eval == null ? null : new CorEval(eval),
                                                    ManagedCallbackType.OnEvalComplete));
        }

        void ICorDebugManagedCallback.EvalException(
                                   ICorDebugAppDomain appDomain,
                                   ICorDebugThread thread,
                                   ICorDebugEval eval)
        {
            HandleEvent(ManagedCallbackType.OnEvalException,
                              new CorEvalEventArgs(appDomain == null ? null : new CorAppDomain(appDomain),
                                                    thread == null ? null : new CorThread(thread),
                                                    eval == null ? null : new CorEval(eval),
                                                    ManagedCallbackType.OnEvalException));
        }

        void ICorDebugManagedCallback.CreateProcess(
                                   ICorDebugProcess process)
        {
            HandleEvent(ManagedCallbackType.OnCreateProcess,
                              new CorProcessEventArgs(process == null ? null : CorProcess.GetCorProcess(process),
                                                       ManagedCallbackType.OnCreateProcess));
        }

        void ICorDebugManagedCallback.ExitProcess(
                                 ICorDebugProcess process)
        {
            HandleEvent(ManagedCallbackType.OnProcessExit,
                               new CorProcessEventArgs(process == null ? null : CorProcess.GetCorProcess(process),
                                                        ManagedCallbackType.OnProcessExit));
        }

        void ICorDebugManagedCallback.CreateThread(
                                  ICorDebugAppDomain appDomain,
                                  ICorDebugThread thread)
        {
            HandleEvent(ManagedCallbackType.OnCreateThread,
                              new CorThreadEventArgs(appDomain == null ? null : new CorAppDomain(appDomain),
                                                      thread == null ? null : new CorThread(thread),
                                                      ManagedCallbackType.OnCreateThread));
        }

        void ICorDebugManagedCallback.ExitThread(
                                ICorDebugAppDomain appDomain,
                                ICorDebugThread thread)
        {
            HandleEvent(ManagedCallbackType.OnThreadExit,
                              new CorThreadEventArgs(appDomain == null ? null : new CorAppDomain(appDomain),
                                                      thread == null ? null : new CorThread(thread),
                                                      ManagedCallbackType.OnThreadExit));
        }

        void ICorDebugManagedCallback.LoadModule(
                                ICorDebugAppDomain appDomain,
                                ICorDebugModule managedModule)
        {
            HandleEvent(ManagedCallbackType.OnModuleLoad,
                              new CorModuleEventArgs(appDomain == null ? null : new CorAppDomain(appDomain),
                                                      managedModule == null ? null : new CorModule(managedModule),
                                                      ManagedCallbackType.OnModuleLoad));
        }

        void ICorDebugManagedCallback.UnloadModule(
                                  ICorDebugAppDomain appDomain,
                                  ICorDebugModule managedModule)
        {
            HandleEvent(ManagedCallbackType.OnModuleUnload,
                              new CorModuleEventArgs(appDomain == null ? null : new CorAppDomain(appDomain),
                                                      managedModule == null ? null : new CorModule(managedModule),
                                                      ManagedCallbackType.OnModuleUnload));
        }

        void ICorDebugManagedCallback.LoadClass(
                               ICorDebugAppDomain appDomain,
                               ICorDebugClass c)
        {
            HandleEvent(ManagedCallbackType.OnClassLoad,
                               new CorClassEventArgs(appDomain == null ? null : new CorAppDomain(appDomain),
                                                     c == null ? null : new CorClass(c),
                                                     ManagedCallbackType.OnClassLoad));
        }

        void ICorDebugManagedCallback.UnloadClass(
                                 ICorDebugAppDomain appDomain,
                                 ICorDebugClass c)
        {
            HandleEvent(ManagedCallbackType.OnClassUnload,
                              new CorClassEventArgs(appDomain == null ? null : new CorAppDomain(appDomain),
                                                     c == null ? null : new CorClass(c),
                                                     ManagedCallbackType.OnClassUnload));
        }

        void ICorDebugManagedCallback.DebuggerError(
                                   ICorDebugProcess process,
                                   int errorHR,
                                   uint errorCode)
        {
            HandleEvent(ManagedCallbackType.OnDebuggerError,
                              new CorDebuggerErrorEventArgs(process == null ? null : CorProcess.GetCorProcess(process),
                                                             errorHR,
                                                             (int)errorCode,
                                                             ManagedCallbackType.OnDebuggerError));
        }

        void ICorDebugManagedCallback.LogMessage(
                                ICorDebugAppDomain appDomain,
                                ICorDebugThread thread,
                                int level,
                                string logSwitchName,
                                string message)
        {
            HandleEvent(ManagedCallbackType.OnLogMessage,
                               new CorLogMessageEventArgs(appDomain == null ? null : new CorAppDomain(appDomain),
                                                          thread == null ? null : new CorThread(thread),
                                                          level, logSwitchName, message,
                                                          ManagedCallbackType.OnLogMessage));
        }

        void ICorDebugManagedCallback.LogSwitch(
                               ICorDebugAppDomain appDomain,
                               ICorDebugThread thread,
                               int level,
                               uint reason,
                               string logSwitchName,
                               string parentName)
        {
            HandleEvent(ManagedCallbackType.OnLogSwitch,
                              new CorLogSwitchEventArgs(appDomain == null ? null : new CorAppDomain(appDomain),
                                                         thread == null ? null : new CorThread(thread),
                                                         level, (int)reason, logSwitchName, parentName,
                                                         ManagedCallbackType.OnLogSwitch));
        }

        void ICorDebugManagedCallback.CreateAppDomain(
                                     ICorDebugProcess process,
                                     ICorDebugAppDomain appDomain)
        {
            HandleEvent(ManagedCallbackType.OnCreateAppDomain,
                              new CorAppDomainEventArgs(process == null ? null : CorProcess.GetCorProcess(process),
                                                         appDomain == null ? null : new CorAppDomain(appDomain),
                                                         ManagedCallbackType.OnCreateAppDomain));
        }

        void ICorDebugManagedCallback.ExitAppDomain(
                                   ICorDebugProcess process,
                                   ICorDebugAppDomain appDomain)
        {
            HandleEvent(ManagedCallbackType.OnAppDomainExit,
                              new CorAppDomainEventArgs(process == null ? null : CorProcess.GetCorProcess(process),
                                                         appDomain == null ? null : new CorAppDomain(appDomain),
                                                         ManagedCallbackType.OnAppDomainExit));
        }

        void ICorDebugManagedCallback.LoadAssembly(
                                  ICorDebugAppDomain appDomain,
                                  ICorDebugAssembly assembly)
        {
            HandleEvent(ManagedCallbackType.OnAssemblyLoad,
                              new CorAssemblyEventArgs(appDomain == null ? null : new CorAppDomain(appDomain),
                                                        assembly == null ? null : new CorAssembly(assembly),
                                                        ManagedCallbackType.OnAssemblyLoad));
        }

        void ICorDebugManagedCallback.UnloadAssembly(
                                    ICorDebugAppDomain appDomain,
                                    ICorDebugAssembly assembly)
        {
            HandleEvent(ManagedCallbackType.OnAssemblyUnload,
                              new CorAssemblyEventArgs(appDomain == null ? null : new CorAppDomain(appDomain),
                                                        assembly == null ? null : new CorAssembly(assembly),
                                                        ManagedCallbackType.OnAssemblyUnload));
        }

        void ICorDebugManagedCallback.ControlCTrap(ICorDebugProcess process)
        {
            HandleEvent(ManagedCallbackType.OnControlCTrap,
                              new CorProcessEventArgs(process == null ? null : CorProcess.GetCorProcess(process),
                                                       ManagedCallbackType.OnControlCTrap));
        }

        void ICorDebugManagedCallback.NameChange(
                                ICorDebugAppDomain appDomain,
                                ICorDebugThread thread)
        {
            HandleEvent(ManagedCallbackType.OnNameChange,
                              new CorThreadEventArgs(appDomain == null ? null : new CorAppDomain(appDomain),
                                                      thread == null ? null : new CorThread(thread),
                                                      ManagedCallbackType.OnNameChange));
        }

        
        void ICorDebugManagedCallback.UpdateModuleSymbols(
                                         ICorDebugAppDomain appDomain,
                                         ICorDebugModule managedModule,
                                         IStream stream)
        {
            HandleEvent(ManagedCallbackType.OnUpdateModuleSymbols,
                              new CorUpdateModuleSymbolsEventArgs(appDomain == null ? null : new CorAppDomain(appDomain),
                                                                  managedModule == null ? null : new CorModule(managedModule),
                                                                  stream,
                                                                  ManagedCallbackType.OnUpdateModuleSymbols));
        }

        void ICorDebugManagedCallback.EditAndContinueRemap(
                                         ICorDebugAppDomain appDomain,
                                         ICorDebugThread thread,
                                         ICorDebugFunction managedFunction,
                                         int isAccurate)
        {
            Debug.Assert(false); //OBSOLETE callback
        }


        void ICorDebugManagedCallback.BreakpointSetError(
                                       ICorDebugAppDomain appDomain,
                                       ICorDebugThread thread,
                                       ICorDebugBreakpoint breakpoint,
                                       UInt32 errorCode)
        {
            HandleEvent(ManagedCallbackType.OnBreakpointSetError,
                              new CorBreakpointSetErrorEventArgs(appDomain == null ? null : new CorAppDomain(appDomain),
                                                        thread == null ? null : new CorThread(thread),
                                                        null,
                                                        (int)errorCode,
                                                        ManagedCallbackType.OnBreakpointSetError));
        }

        void ICorDebugManagedCallback2.FunctionRemapOpportunity(ICorDebugAppDomain appDomain,
                                                                       ICorDebugThread thread,
                                                                       ICorDebugFunction oldFunction,
                                                                       ICorDebugFunction newFunction,
                                                                       uint oldILoffset)
        {
            HandleEvent(ManagedCallbackType.OnFunctionRemapOpportunity,
                                      new CorFunctionRemapOpportunityEventArgs(appDomain == null ? null : new CorAppDomain(appDomain),
                                                                               thread == null ? null : new CorThread(thread),
                                                                               oldFunction == null ? null : new CorFunction(oldFunction),
                                                                               newFunction == null ? null : new CorFunction(newFunction),
                                                                               (int)oldILoffset,
                                                                               ManagedCallbackType.OnFunctionRemapOpportunity));
        }

        void ICorDebugManagedCallback2.FunctionRemapComplete(ICorDebugAppDomain appDomain,
                                                             ICorDebugThread thread,
                                                             ICorDebugFunction managedFunction)
        {
            HandleEvent(ManagedCallbackType.OnFunctionRemapComplete,
                               new CorFunctionRemapCompleteEventArgs(appDomain == null ? null : new CorAppDomain(appDomain),
                                                      thread == null ? null : new CorThread(thread),
                                                      managedFunction == null ? null : new CorFunction(managedFunction),
                                                      ManagedCallbackType.OnFunctionRemapComplete));
        }

        void ICorDebugManagedCallback2.CreateConnection(ICorDebugProcess process, uint connectionId, ref ushort connectionName)
        {
            Debug.Assert(false);
        }

        void ICorDebugManagedCallback2.ChangeConnection(ICorDebugProcess process, uint connectionId)
        {
            Debug.Assert(false);
        }

        void ICorDebugManagedCallback2.DestroyConnection(ICorDebugProcess process, uint connectionId)
        {
            Debug.Assert(false);
        }

        void ICorDebugManagedCallback2.Exception(ICorDebugAppDomain ad, ICorDebugThread thread,
                                                 ICorDebugFrame frame, uint offset,
                                                 CorDebugExceptionCallbackType eventType, uint flags)
        {
            HandleEvent(ManagedCallbackType.OnException2,
                                      new CorException2EventArgs(ad == null ? null : new CorAppDomain(ad),
                                                        thread == null ? null : new CorThread(thread),
                                                        frame == null ? null : new CorFrame(frame),
                                                        (int)offset,
                                                        eventType,
                                                        (int)flags,
                                                        ManagedCallbackType.OnException2));
        }

        void ICorDebugManagedCallback2.ExceptionUnwind(ICorDebugAppDomain ad, ICorDebugThread thread,
                                                       CorDebugExceptionUnwindCallbackType eventType, uint flags)
        {
            HandleEvent(ManagedCallbackType.OnExceptionUnwind2,
                                      new CorExceptionUnwind2EventArgs(ad == null ? null : new CorAppDomain(ad),
                                                        thread == null ? null : new CorThread(thread),
                                                        eventType,
                                                        (int)flags,
                                                        ManagedCallbackType.OnExceptionUnwind2));
        }

        // wrapper for CustomNotification event handler to convert argument types
        void ICorDebugManagedCallback3.CustomNotification(ICorDebugThread thread, ICorDebugAppDomain ad)
        {
            HandleEvent(ManagedCallbackType.OnCustomNotification,
                               new CorCustomNotificationEventArgs(thread == null ? null : new CorThread(thread),
                                                                  ad == null ? null : new CorAppDomain(ad),
                                                                  ManagedCallbackType.OnCustomNotification));
        }

        // Get process from controller 
        static private CorProcess GetProcessFromController(ICorDebugController pController)
        {
            CorProcess p;
            ICorDebugProcess p2 = pController as ICorDebugProcess;
            if (p2 != null)
            {
                p = CorProcess.GetCorProcess(p2);
            }
            else
            {
                ICorDebugAppDomain a2 = (ICorDebugAppDomain)pController;
                p = new CorAppDomain(a2).Process;
            }
            return p;
        }

        void ICorDebugManagedCallback2.MDANotification(ICorDebugController pController,
                                                       ICorDebugThread thread,
                                                       ICorDebugMDA pMDA)
        {
            CorMDA c = new CorMDA(pMDA);
            string szName = c.Name;
            CorDebugMDAFlags f = c.Flags;
            CorProcess p = GetProcessFromController(pController);


            HandleEvent(ManagedCallbackType.OnMDANotification,
                                      new CorMDAEventArgs(c,
                                                           thread == null ? null : new CorThread(thread),
                                                           p, ManagedCallbackType.OnMDANotification));
        }


    }

} /* namespace */