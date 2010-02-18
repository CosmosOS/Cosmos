
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace CoreLib.Threading
{
    [SuppressUnmanagedCodeSecurity]
    internal class NativeMethods
    {
        public const int WaitObject0 = 0x0;
        public const int WaitAbandoned = 0x80;
        public const int WaitTimeout = 0x102;
        public const int WaitFailed = -1;

        public static readonly int SpinCount = Environment.ProcessorCount != 1 ? 4000 : 0;
        public static readonly bool SpinEnabled = Environment.ProcessorCount != 1;

        // We need to import some stuff. We can't use 
        // ProcessHacker.Native because it depends on this library.

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(
            [In] IntPtr Handle
            );

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateEvent(
            [In] [Optional] IntPtr EventAttributes,
            [In] bool ManualReset,
            [In] bool InitialState,
            [In] [Optional] string Name
            );

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateSemaphore(
            [In] [Optional] IntPtr SemaphoreAttributes,
            [In] int InitialCount,
            [In] int MaximumCount,
            [In] [Optional] string Name
            );

        [DllImport("kernel32.dll")]
        public static extern bool ReleaseSemaphore(
            [In] IntPtr SemaphoreHandle,
            [In] int ReleaseCount,
            [In] IntPtr PreviousCount // out int
            );

        [DllImport("kernel32.dll")]
        public static extern bool ResetEvent(
            [In] IntPtr EventHandle
            );

        [DllImport("kernel32.dll")]
        public static extern bool SetEvent(
            [In] IntPtr EventHandle
            );

        [DllImport("kernel32.dll")]
        public static extern int WaitForSingleObject(
            [In] IntPtr Handle,
            [In] int Milliseconds
            );

        [DllImport("ntdll.dll")]
        public static extern int NtCreateKeyedEvent(
            [Out] out IntPtr KeyedEventHandle,
            [In] int DesiredAccess,
            [In] [Optional] IntPtr ObjectAttributes,
            [In] int Flags
            );

        [DllImport("ntdll.dll")]
        public static extern int NtReleaseKeyedEvent(
            [In] IntPtr KeyedEventHandle,
            [In] IntPtr KeyValue,
            [In] bool Alertable,
            [In] [Optional] IntPtr Timeout
            );

        [DllImport("ntdll.dll")]
        public static extern int NtWaitForKeyedEvent(
            [In] IntPtr KeyedEventHandle,
            [In] IntPtr KeyValue,
            [In] bool Alertable,
            [In] [Optional] IntPtr Timeout
            );
    }
}



