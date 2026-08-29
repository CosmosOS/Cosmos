using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL;

namespace Cosmos.Kernel.System;

/// <summary>
/// User-facing power-management API. Routes through the platform HAL.
/// </summary>
public static class Power
{
    /// <summary>
    /// Park the CPU. Returns when an interrupt wakes it.
    /// </summary>
    public static void Halt()
    {
        if (PlatformHAL.CpuOps != null)
        {
            PlatformHAL.CpuOps.Halt();
        }
        else
        {
            while (true) { }
        }
    }

    /// <summary>
    /// Restart the machine. Does not return on success; falls back to <see cref="Halt"/>
    /// if power ops are unavailable.
    /// </summary>
    [DoesNotReturn]
    public static void Reboot()
    {
        PlatformHAL.PowerOps?.Reboot();
        while (true)
        {
            Halt();
        }
    }

    /// <summary>
    /// Power off the machine. Does not return on success; falls back to <see cref="Halt"/>
    /// if power ops are unavailable.
    /// </summary>
    [DoesNotReturn]
    public static void Shutdown()
    {
        PlatformHAL.PowerOps?.Shutdown();
        while (true)
        {
            Halt();
        }
    }
}
