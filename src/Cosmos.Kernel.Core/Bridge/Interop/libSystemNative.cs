using System.Diagnostics;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Scheduler;
using SysThread = System.Threading.Thread;

namespace Cosmos.Kernel.Core.Bridge.Interop;

internal static unsafe partial class libSystemNative
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ProcessCpuInformation
    {
        internal ulong _lastRecordedCurrentTime;
        internal ulong _lastRecordedKernelTime;
        internal ulong _lastRecordedUserTime;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_GetCpuUtilization")]
    internal static unsafe double SystemNative_GetCpuUtilization(ProcessCpuInformation* previousCpuInfo)
    {
        if (SchedulerManager.Threads is null)
        {
            return 0.0;
        }

        ulong currentTime = GetMonotonicNs();
        ulong busyTime = SchedulerManager.GetBusyCpuTimeNs();

        ulong lastTime = previousCpuInfo->_lastRecordedCurrentTime;
        ulong lastBusy = previousCpuInfo->_lastRecordedUserTime;

        // First call: seed snapshot only.
        if (lastTime == 0)
        {
            previousCpuInfo->_lastRecordedCurrentTime = currentTime;
            previousCpuInfo->_lastRecordedUserTime = busyTime;
            previousCpuInfo->_lastRecordedKernelTime = 0;
            return 0.0;
        }

        // Window too short for meaningful sample (< 5 ticks at 10 ms quantum).
        // Leave snapshot untouched so the next call sees a longer window.
        if (currentTime - lastTime < 50_000_000UL)
        {
            return 0.0;
        }

        double utilization = 0.0;
        if (busyTime >= lastBusy)
        {
            ulong totalElapsed = (currentTime - lastTime) * SchedulerManager.CpuCount;
            ulong busyElapsed = busyTime - lastBusy;
            if (totalElapsed > 0 && busyElapsed > 0)
            {
                utilization = (double)busyElapsed * 100.0 / (double)totalElapsed;
                if (utilization > 100.0)
                {
                    utilization = 100.0;
                }
            }
        }

        previousCpuInfo->_lastRecordedCurrentTime = currentTime;
        previousCpuInfo->_lastRecordedUserTime = busyTime;
        previousCpuInfo->_lastRecordedKernelTime = 0;
        return utilization;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_SchedGetCpu")]
    internal static int SystemNative_SchedGetCpu()
    {
        return (int)SchedulerManager.GetCurrentCpuId();
    }

    /// <summary>
    /// Smallest honored thread stack — CoreCLR's arbitrary minimum for
    /// stack-size settings (RhConfig.cpp), standing in for PTHREAD_STACK_MIN
    /// in upstream's pal_threading.c. Requests below CoreLib's 128KB
    /// MinExecutionStackSize are still honored, like upstream on Linux:
    /// EnsureSufficientExecutionStack then throws on that thread.
    /// </summary>
    private const nuint MinStackSize = 64 * 1024;

    /// <summary>Stack sizes are rounded up to whole pages.</summary>
    private const nuint StackSizeAlignment = 4096;

#if ARCH_X64
    [LibraryImport("*", EntryPoint = "_native_x64_get_code_selector")]
    [SuppressGCTransition]
    private static partial ulong GetCurrentCodeSelector();
#endif

    /// <summary>
    /// Backs CoreLib's <c>Interop.Sys.CreateThread</c> P/Invoke. Upstream
    /// <c>Thread.CreateThread</c> resolves the stack size itself — the
    /// constructor's <c>maxStackSize</c>, or <c>RhGetDefaultStackSize</c> when
    /// unset — and passes it here, so honoring
    /// <c>new Thread(start, maxStackSize)</c> needs no access to Thread's
    /// private StartHelper. A plug of <c>Thread.CreateThread</c> could not
    /// reach that field: an <c>UnsafeAccessor</c> rejects a byref return of a
    /// field whose type is inaccessible, so the seam runs here, below it.
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "SystemNative_CreateThread")]
    internal static int SystemNative_CreateThread(IntPtr stackSize, delegate* unmanaged<IntPtr, IntPtr> startAddress, IntPtr parameter)
    {
        // startAddress (CoreLib's ThreadEntryPoint) is unused: threads start at
        // ThreadNative.EntryPointStub and the scheduler's InvokeCurrentThreadStart
        // runs CoreLib's StartThread with the GCHandle<Thread> parameter itself.
        _ = startAddress;

        if (!SchedulerManager.IsRunning)
        {
            // Same behavior as before the scheduler existed: report success,
            // the thread simply never runs.
            return 1;
        }

        nuint size = ((nuint)stackSize + (StackSizeAlignment - 1)) & ~(StackSizeAlignment - 1);
        if (size < MinStackSize)
        {
            size = MinStackSize;
        }

        using (InternalCpu.DisableInterruptsScope())
        {
            // Create scheduler thread with SchedulerThreadFlags.Managed set.
            // SchedulerManager.InvokeCurrentThreadStart evaluates it to
            // call the managed startup or not.
            SchedulerThread thread = new SchedulerThread
            {
                Id = SchedulerManager.AllocateThreadId(),
                CpuId = 0,
                State = SchedulerThreadState.Created,
                Flags = SchedulerThreadFlags.Managed,
                // CoreLib's handle in `parameter` lives only until the thread
                // reports itself started; the mechanism keeps its own for the
                // thread's whole life, so a kill can reach the managed side.
                _managedThread = new GCHandle<SysThread>(GCHandle<SysThread>.FromIntPtr(parameter).Target)
            };

            nuint entryPoint = (nuint)(delegate* unmanaged<IntPtr, void>)&ThreadNative.EntryPointStub;
#if ARCH_X64
            ushort cs = (ushort)GetCurrentCodeSelector();
            thread.InitializeStack(entryPoint, cs, (nuint)parameter, size);
#elif ARCH_ARM64
            // ARM64: no code selector needed, use 0.
            thread.InitializeStack(entryPoint, 0, (nuint)parameter, size);
#endif
            SchedulerManager.CreateThread(0, thread);
            SchedulerManager.ReadyThread(0, thread);

            Serial.WriteString("[libSystemNative] Thread ");
            Serial.WriteNumber(thread.Id);
            Serial.WriteString(" scheduled, stack ");
            Serial.WriteNumber((ulong)size);
            Serial.WriteString(" bytes\n");
        }

        return 1;
    }

    // =========================================================================
    // libSystem.Native exports for the BCL assemblies Cosmos does not plug:
    // System.Net.Sockets, NetworkInformation and Primitives (socket addresses),
    // System.IO.MemoryMappedFiles, NativeLibrary. A kernel referencing them
    // (often through System.Net.Http, which XmlReader's URL resolver brings)
    // failed to link. Socket addresses work, in Linux's sockaddr layout (the
    // BCL is linux-x64's); the rest has no kernel service behind it and fails
    // as upstream's PAL does when the OS lacks it (pal_networking.c,
    // pal_io.c, pal_dynamicload.c).
    // =========================================================================

    // CoreLib's Interop.Error (PAL error codes, Interop.Errors.cs).
    private const int ErrorSuccess = 0;
    private const int ErrorEafnosupport = 0x10005;
    private const int ErrorEbadf = 0x10008;
    private const int ErrorEfault = 0x10015;
    private const int ErrorEinval = 0x1001C;
    private const int ErrorEnotsup = 0x1003D;

    // Address families: the PAL's (System.Net.Sockets.AddressFamily) and Linux's.
    private const int PalAfUnspec = 0;
    private const int PalAfUnix = 1;
    private const int PalAfInet = 2;
    private const int PalAfInet6 = 23;
    private const ushort LinuxAfUnspec = 0;
    private const ushort LinuxAfUnix = 1;
    private const ushort LinuxAfInet = 2;
    private const ushort LinuxAfInet6 = 10;

    // Linux sizes: sockaddr_in, sockaddr_in6, sockaddr_un, sockaddr_storage.
    private const int SockaddrInSize = 16;
    private const int SockaddrIn6Size = 28;
    private const int SockaddrUnSize = 110;
    private const int SockaddrStorageSize = 128;

    // --------------- socket addresses (Linux sockaddr layout) ---------------
    // sa_family: ushort at 0 (host order). sockaddr_in: port at 2 (network
    // order), address at 4. sockaddr_in6: port at 2, flowinfo at 4, address
    // at 8 (16 bytes), scope id at 24.

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_GetSocketAddressSizes")]
    internal static int SystemNative_GetSocketAddressSizes(int* ipv4SocketAddressSize, int* ipv6SocketAddressSize, int* udsSocketAddressSize, int* maxSocketAddressSize)
    {
        if (ipv4SocketAddressSize == null || ipv6SocketAddressSize == null || udsSocketAddressSize == null || maxSocketAddressSize == null)
        {
            return ErrorEfault;
        }

        *ipv4SocketAddressSize = SockaddrInSize;
        *ipv6SocketAddressSize = SockaddrIn6Size;
        *udsSocketAddressSize = SockaddrUnSize;
        *maxSocketAddressSize = SockaddrStorageSize;
        return ErrorSuccess;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_GetMaximumAddressSize")]
    internal static int SystemNative_GetMaximumAddressSize()
    {
        return SockaddrStorageSize;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_GetDomainSocketSizes")]
    internal static void SystemNative_GetDomainSocketSizes(int* pathOffset, int* pathSize, int* addressSize)
    {
        // sockaddr_un: sun_family (2 bytes), then sun_path[108].
        *pathOffset = 2;
        *pathSize = 108;
        *addressSize = SockaddrUnSize;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_GetAddressFamily")]
    internal static int SystemNative_GetAddressFamily(byte* socketAddress, int socketAddressLen, int* addressFamily)
    {
        if (socketAddress == null || addressFamily == null || socketAddressLen < 2)
        {
            return ErrorEfault;
        }

        switch (*(ushort*)socketAddress)
        {
            case LinuxAfUnspec:
                *addressFamily = PalAfUnspec;
                return ErrorSuccess;
            case LinuxAfUnix:
                *addressFamily = PalAfUnix;
                return ErrorSuccess;
            case LinuxAfInet:
                *addressFamily = PalAfInet;
                return ErrorSuccess;
            case LinuxAfInet6:
                *addressFamily = PalAfInet6;
                return ErrorSuccess;
            default:
                return ErrorEafnosupport;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_SetAddressFamily")]
    internal static int SystemNative_SetAddressFamily(byte* socketAddress, int socketAddressLen, int addressFamily)
    {
        if (socketAddress == null || socketAddressLen < 2)
        {
            return ErrorEfault;
        }

        ushort family;
        switch (addressFamily)
        {
            case PalAfUnspec:
                family = LinuxAfUnspec;
                break;
            case PalAfUnix:
                family = LinuxAfUnix;
                break;
            case PalAfInet:
                family = LinuxAfInet;
                break;
            case PalAfInet6:
                family = LinuxAfInet6;
                break;
            default:
                return ErrorEafnosupport;
        }

        *(ushort*)socketAddress = family;
        return ErrorSuccess;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_GetPort")]
    internal static int SystemNative_GetPort(byte* socketAddress, int socketAddressLen, ushort* port)
    {
        if (socketAddress == null || port == null || socketAddressLen < 4)
        {
            return ErrorEfault;
        }

        ushort family = *(ushort*)socketAddress;
        if (family != LinuxAfInet && family != LinuxAfInet6)
        {
            return ErrorEafnosupport;
        }

        *port = (ushort)(socketAddress[2] << 8 | socketAddress[3]);
        return ErrorSuccess;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_SetPort")]
    internal static int SystemNative_SetPort(byte* socketAddress, int socketAddressLen, ushort port)
    {
        if (socketAddress == null || socketAddressLen < 4)
        {
            return ErrorEfault;
        }

        ushort family = *(ushort*)socketAddress;
        if (family != LinuxAfInet && family != LinuxAfInet6)
        {
            return ErrorEafnosupport;
        }

        socketAddress[2] = (byte)(port >> 8);
        socketAddress[3] = (byte)port;
        return ErrorSuccess;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_GetIPv4Address")]
    internal static int SystemNative_GetIPv4Address(byte* socketAddress, int socketAddressLen, uint* address)
    {
        if (socketAddress == null || address == null || socketAddressLen < SockaddrInSize)
        {
            return ErrorEfault;
        }

        if (*(ushort*)socketAddress != LinuxAfInet)
        {
            return ErrorEinval;
        }

        // The bytes as they are (network order), as upstream copies sin_addr.
        *address = *(uint*)(socketAddress + 4);
        return ErrorSuccess;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_SetIPv4Address")]
    internal static int SystemNative_SetIPv4Address(byte* socketAddress, int socketAddressLen, uint address)
    {
        if (socketAddress == null || socketAddressLen < SockaddrInSize)
        {
            return ErrorEfault;
        }

        *(ushort*)socketAddress = LinuxAfInet;
        *(uint*)(socketAddress + 4) = address;
        return ErrorSuccess;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_GetIPv6Address")]
    internal static int SystemNative_GetIPv6Address(byte* socketAddress, int socketAddressLen, byte* address, int addressLen, uint* scopeId)
    {
        if (socketAddress == null || address == null || scopeId == null || socketAddressLen < SockaddrIn6Size || addressLen < 16)
        {
            return ErrorEfault;
        }

        if (*(ushort*)socketAddress != LinuxAfInet6)
        {
            return ErrorEinval;
        }

        for (int i = 0; i < 16; i++)
        {
            address[i] = socketAddress[8 + i];
        }

        *scopeId = *(uint*)(socketAddress + 24);
        return ErrorSuccess;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_PlatformSupportsDualModeIPv4PacketInfo")]
    internal static int SystemNative_PlatformSupportsDualModeIPv4PacketInfo()
    {
        return 0;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_InterfaceNameToIndex")]
    internal static uint SystemNative_InterfaceNameToIndex(byte* utf8NullTerminatedName)
    {
        // if_nametoindex: 0 for an unknown interface.
        return 0;
    }

    // --------------- BSD sockets: none ---------------
    // Creating one fails (EAFNOSUPPORT, as for an unsupported family), so no
    // socket handle exists for the others, which report EBADF.

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_Socket")]
    internal static int SystemNative_Socket(int addressFamily, int socketType, int protocolType, nint* socket)
    {
        if (socket != null)
        {
            *socket = -1;
        }

        return ErrorEafnosupport;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_Connect")]
    internal static int SystemNative_Connect(nint socket, byte* socketAddress, int socketAddressLen) => ErrorEbadf;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_Connectx")]
    internal static int SystemNative_Connectx(nint socket, byte* socketAddress, int socketAddressLen, byte* buffer, int bufferLen, int enableTfo, int* sent)
    {
        if (sent != null)
        {
            *sent = 0;
        }

        return ErrorEbadf;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_GetBytesAvailable")]
    internal static int SystemNative_GetBytesAvailable(nint socket, int* available)
    {
        if (available != null)
        {
            *available = 0;
        }

        return ErrorEbadf;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_GetPeerName")]
    internal static int SystemNative_GetPeerName(nint socket, byte* socketAddress, int* socketAddressLen) => ErrorEbadf;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_GetSockName")]
    internal static int SystemNative_GetSockName(nint socket, byte* socketAddress, int* socketAddressLen) => ErrorEbadf;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_GetSocketErrorOption")]
    internal static int SystemNative_GetSocketErrorOption(nint socket, int* socketError) => ErrorEbadf;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_GetSocketType")]
    internal static int SystemNative_GetSocketType(nint socket, int* addressFamily, int* socketType, int* protocolType, int* isListening) => ErrorEbadf;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_GetSockOpt")]
    internal static int SystemNative_GetSockOpt(nint socket, int optionLevel, int optionName, byte* optionValue, int* optionLen) => ErrorEbadf;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_SetSockOpt")]
    internal static int SystemNative_SetSockOpt(nint socket, int optionLevel, int optionName, byte* optionValue, int optionLen) => ErrorEbadf;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_GetLingerOption")]
    internal static int SystemNative_GetLingerOption(nint socket, void* option) => ErrorEbadf;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_SetLingerOption")]
    internal static int SystemNative_SetLingerOption(nint socket, void* option) => ErrorEbadf;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_SetSendTimeout")]
    internal static int SystemNative_SetSendTimeout(nint socket, int millisecondsTimeout) => ErrorEbadf;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_SetReceiveTimeout")]
    internal static int SystemNative_SetReceiveTimeout(nint socket, int millisecondsTimeout) => ErrorEbadf;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_GetIPv4MulticastOption")]
    internal static int SystemNative_GetIPv4MulticastOption(nint socket, int multicastOption, void* option) => ErrorEbadf;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_SetIPv4MulticastOption")]
    internal static int SystemNative_SetIPv4MulticastOption(nint socket, int multicastOption, void* option) => ErrorEbadf;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_GetIPv6MulticastOption")]
    internal static int SystemNative_GetIPv6MulticastOption(nint socket, int multicastOption, void* option) => ErrorEbadf;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_Receive")]
    internal static int SystemNative_Receive(nint socket, byte* buffer, int bufferLen, int flags, int* received)
    {
        if (received != null)
        {
            *received = 0;
        }

        return ErrorEbadf;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_Send")]
    internal static int SystemNative_Send(nint socket, byte* buffer, int bufferLen, int flags, int* sent)
    {
        if (sent != null)
        {
            *sent = 0;
        }

        return ErrorEbadf;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_ReceiveMessage")]
    internal static int SystemNative_ReceiveMessage(nint socket, void* messageHeader, int flags, long* received)
    {
        if (received != null)
        {
            *received = 0;
        }

        return ErrorEbadf;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_SendMessage")]
    internal static int SystemNative_SendMessage(nint socket, void* messageHeader, int flags, long* sent)
    {
        if (sent != null)
        {
            *sent = 0;
        }

        return ErrorEbadf;
    }

    // fcntl on a socket or a memory-mapped file's descriptor: -1, as for a bad descriptor.

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_FcntlSetIsNonBlocking")]
    internal static int SystemNative_FcntlSetIsNonBlocking(nint fd, int isNonBlocking) => -1;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_FcntlGetIsNonBlocking")]
    internal static int SystemNative_FcntlGetIsNonBlocking(nint fd, int* isNonBlocking) => -1;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_FcntlSetFD")]
    internal static int SystemNative_FcntlSetFD(nint fd, int flags) => -1;

    // The socket event engine (epoll): none.

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_CreateSocketEventPort")]
    internal static int SystemNative_CreateSocketEventPort(nint* port)
    {
        if (port != null)
        {
            *port = -1;
        }

        return ErrorEnotsup;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_CloseSocketEventPort")]
    internal static int SystemNative_CloseSocketEventPort(nint port) => ErrorSuccess;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_CreateSocketEventBuffer")]
    internal static int SystemNative_CreateSocketEventBuffer(int count, void** buffer)
    {
        if (buffer != null)
        {
            *buffer = null;
        }

        return ErrorEnotsup;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_FreeSocketEventBuffer")]
    internal static int SystemNative_FreeSocketEventBuffer(void* buffer) => ErrorSuccess;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_TryChangeSocketEventRegistration")]
    internal static int SystemNative_TryChangeSocketEventRegistration(nint port, nint socket, int currentEvents, int newEvents, nint data) => ErrorEnotsup;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_WaitForSocketEvents")]
    internal static int SystemNative_WaitForSocketEvents(nint port, void* buffer, int* count)
    {
        if (count != null)
        {
            *count = 0;
        }

        return ErrorEnotsup;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_Poll")]
    internal static int SystemNative_Poll(void* pollEvents, uint eventCount, int timeout, uint* triggered)
    {
        if (triggered != null)
        {
            *triggered = 0;
        }

        return ErrorEnotsup;
    }

    // --------------- network interfaces: none known ---------------

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_GetNetworkInterfaces")]
    internal static int SystemNative_GetNetworkInterfaces(int* count, void** interfaces, int* addressCount, void** addresses)
    {
        // Success with no interface: NetworkInterface.GetAllNetworkInterfaces is empty.
        *count = 0;
        *interfaces = null;
        *addressCount = 0;
        *addresses = null;
        return 0;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_CreateNetworkChangeListenerSocket")]
    internal static int SystemNative_CreateNetworkChangeListenerSocket(nint* socket)
    {
        if (socket != null)
        {
            *socket = -1;
        }

        return ErrorEnotsup;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_ReadEvents")]
    internal static int SystemNative_ReadEvents(nint socket, void* onNetworkChange) => ErrorEnotsup;

    // --------------- memory-mapped files and shared memory: none ---------------

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_MMap")]
    internal static nint SystemNative_MMap(nint address, ulong length, int protections, int flags, nint fd, long offset)
    {
        return -1; // MAP_FAILED
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_MUnmap")]
    internal static int SystemNative_MUnmap(nint address, ulong length) => -1;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_MSync")]
    internal static int SystemNative_MSync(nint address, ulong length, int flags) => -1;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_MAdvise")]
    internal static int SystemNative_MAdvise(nint address, ulong length, int advice) => -1;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_ShmOpen")]
    internal static nint SystemNative_ShmOpen(byte* name, int flags, int mode) => -1;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_ShmUnlink")]
    internal static int SystemNative_ShmUnlink(byte* name) => -1;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_MemfdCreate")]
    internal static nint SystemNative_MemfdCreate(byte* name, int isReadonly) => -1;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_IsMemfdSupported")]
    internal static int SystemNative_IsMemfdSupported() => 0;

    /// <summary>
    /// sysconf for the PAL's two names: SysConfName_CLK_TCK (1) and
    /// SysConfName_PAGESIZE (2); -1 for another.
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "SystemNative_SysConf")]
    internal static long SystemNative_SysConf(int name)
    {
        switch (name)
        {
            case 1:
                return 100;
            case 2:
                return 4096;
            default:
                return -1;
        }
    }

    // --------------- dynamic libraries: none (one kernel image) ---------------
    // NativeLibrary.Load then throws DllNotFoundException.

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_LoadLibrary")]
    internal static nint SystemNative_LoadLibrary(byte* filename) => 0;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_GetLoadLibraryError")]
    internal static nint SystemNative_GetLoadLibraryError() => 0;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_GetProcAddress")]
    internal static nint SystemNative_GetProcAddress(nint handle, byte* symbol) => 0;

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_FreeLibrary")]
    internal static void SystemNative_FreeLibrary(nint handle)
    {
    }

    /// <summary>
    /// uname's release for Environment.OSVersion: "0.0.0", from NativeMemory
    /// (Cosmos's heap), which CoreLib's marshaller frees it to, like upstream's
    /// strdup.
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "SystemNative_GetUnixRelease")]
    internal static byte* SystemNative_GetUnixRelease()
    {
        byte* release = (byte*)NativeMemory.Alloc(6);
        release[0] = (byte)'0';
        release[1] = (byte)'.';
        release[2] = (byte)'0';
        release[3] = (byte)'.';
        release[4] = (byte)'0';
        release[5] = 0;
        return release;
    }

    private static ulong GetMonotonicNs()
    {
        long ticks = Stopwatch.GetTimestamp();
        long freq = Stopwatch.Frequency;
        if (freq <= 0)
        {
            return 0;
        }
        ulong t = (ulong)ticks;
        ulong f = (ulong)freq;
        // Split mul/div: ticks * 1e9 overflows in ~147 s on a 62.5 MHz ARM64 timer.
        return (t / f) * 1_000_000_000UL + ((t % f) * 1_000_000_000UL) / f;
    }
}
