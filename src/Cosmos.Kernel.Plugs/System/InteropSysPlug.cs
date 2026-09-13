// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Diagnostics;
using Cosmos.Build.API.Attributes;
using Cosmos.Kernel.System;
using Cosmos.Kernel.System.Diagnostics;
using Monitor = Cosmos.Kernel.Core.Scheduler.Monitor;
using System.Runtime.InteropServices;
#if ARCH_X64
using Cosmos.Kernel.HAL.X64.Devices.Clock;
#elif ARCH_ARM64
using Cosmos.Kernel.HAL.ARM64.Devices.Clock;
#endif


namespace Cosmos.Kernel.Plugs.System;

/// <summary>
/// Plug for Interop+Sys class to provide OS interop functions for bare-metal kernel.
/// </summary>
[Plug("Interop/Sys")]
public static class InteropSysPlug
{
    // Simple LFSR-based pseudo-random number generator state
    private static ulong s_randomState = 0x853c49e6748fea9bUL;

    /// <summary>
    /// Provides non-cryptographic random bytes for Random.Shared seeding.
    /// Uses a simple XorShift algorithm since we don't have OS randomness.
    /// </summary>
    [PlugMember]
    public static unsafe void GetNonCryptographicallySecureRandomBytes(byte* buffer, int length)
    {
        // Mix in some entropy from the timer/tick count
        ulong state = s_randomState;
        state ^= (ulong)Stopwatch.GetTimestamp();

        for (int i = 0; i < length; i++)
        {
            // XorShift64 algorithm
            state ^= state << 13;
            state ^= state >> 7;
            state ^= state << 17;
            buffer[i] = (byte)(state & 0xFF);
        }

        s_randomState = state;
    }

    /// <summary>
    /// Provides cryptographically secure random bytes.
    /// In a real kernel, this would use hardware RNG (RDRAND) if available.
    /// </summary>
    [PlugMember]
    public static unsafe void GetCryptographicallySecureRandomBytes(byte* buffer, int length)
    {
        // For now, use the same non-crypto implementation
        // TODO: Use RDRAND instruction if available
        GetNonCryptographicallySecureRandomBytes(buffer, length);
    }

    [PlugMember]
    public static long GetLowResolutionTimestamp()
    {

        if (KernelFeatures.Timer)
        {
            if (RTC.Instance is null)
            {
                return 0;
            }

            return RTC.Instance.GetElapsedTicks() / TimeSpan.TicksPerMillisecond;
        }
        else
        {
            return 0;
        }
    }

    [PlugMember]
    public static int GetErrNo()
    {
        // Always return 0 (no error) for now
        return 0;
    }

    [PlugMember]
    public static void SetErrNo(int value)
    {
        // No-op for now
    }

    /// <summary>
    /// dlopen replacement. There is no dynamic library loading on bare metal, so
    /// every load fails. This is what turns a call to an unplugged
    /// libSystem.Native P/Invoke into a catchable <see cref="DllNotFoundException"/>:
    /// without it, the lazy P/Invoke resolver recurses through its own unplugged
    /// P/Invokes (LoadLibrary, GetProcessPath, ...) until the stack overflows and
    /// the kernel triple-faults.
    /// </summary>
    [PlugMember]
    internal static IntPtr LoadLibrary(string filename)
    {
        return IntPtr.Zero;
    }

    /// <summary>
    /// The kernel image is the process. A fixed path keeps
    /// <c>AppContext.BaseDirectory</c> (used by the P/Invoke resolver's library
    /// search, among others) from re-entering an unresolvable P/Invoke.
    /// </summary>
    [PlugMember]
    internal static string? GetProcessPath()
    {
        return "/kernel.elf";
    }

    [PlugMember]
    internal static IntPtr LowLevelMonitor_Create()
    {
        Monitor monitor = new();
        var gchandle = new GCHandle<Monitor>(monitor);

        return GCHandle<Monitor>.ToIntPtr(gchandle);
    }

    [PlugMember]
    internal static void LowLevelMonitor_Destroy(IntPtr monitor)
    {
        var gchandle = GCHandle<Monitor>.FromIntPtr(monitor);
        gchandle.Target.Dispose();
        gchandle.Dispose();
    }

    [PlugMember]
    internal static void LowLevelMonitor_Acquire(IntPtr monitor)
    {
        Log.Write("[LowLevelMonitor] Acquire BEGIN\n");
        var gchandle = GCHandle<Monitor>.FromIntPtr(monitor);
        gchandle.Target.Acquire();
        Log.Write("[LowLevelMonitor] Acquire END\n");
    }

    [PlugMember]
    internal static void LowLevelMonitor_Release(IntPtr monitor)
    {
        Log.Write("[LowLevelMonitor] Release\n");
        var gchandle = GCHandle<Monitor>.FromIntPtr(monitor);
        gchandle.Target.Release();
    }

    [PlugMember]
    internal static void LowLevelMonitor_Wait(IntPtr monitor)
    {
        Log.Write("[LowLevelMonitor] Wait BEGIN\n");
        var gchandle = GCHandle<Monitor>.FromIntPtr(monitor);
        gchandle.Target.Wait();
        Log.Write("[LowLevelMonitor] Wait END\n");
    }

    [PlugMember]
    internal static bool LowLevelMonitor_TimedWait(IntPtr monitor, int timeoutMilliseconds)
    {
        Log.Write("[LowLevelMonitor] LowLevelMonitor_TimedWait: BEGIN, timeout=");
        Log.WriteNumber(timeoutMilliseconds);
        Log.Write("ms\n");
        var mon = GCHandle<Monitor>.FromIntPtr(monitor).Target;

        if (timeoutMilliseconds < 0)
        {
            mon.Wait();
            return true;
        }

        mon.Wait(timeoutMilliseconds);

        return true;
    }

    [PlugMember]
    internal static void LowLevelMonitor_Signal_Release(IntPtr monitor)
    {
        Log.Write("[LowLevelMonitor] Signal_Release\n");
        var gchandle = GCHandle<Monitor>.FromIntPtr(monitor);
        gchandle.Target.Signal();
    }

}
