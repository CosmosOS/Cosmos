using System.Diagnostics;
using Cosmos.Build.API.Attributes;
#if ARCH_X64
using Cosmos.Kernel.Core.X64.Bridge;
using Cosmos.Kernel.Core.X64.Cpu;
#elif ARCH_ARM64
using Cosmos.Kernel.Core.ARM64.Bridge;
#elif ARCH_RISCV64
using Cosmos.Kernel.Core.RISCV64.Cpu;
#endif

namespace Cosmos.Kernel.Plugs.System.Diagnostics;

/// <summary>
/// Plug for System.Diagnostics.Stopwatch to provide timestamp functionality.
/// Uses TSC (Time Stamp Counter) on x64, the ARM64 generic timer on ARM64 and
/// the time CSR on RISC-V.
/// Native imports live in Cosmos.Kernel.Core.X64/Bridge/Import/X64CpuNative.cs and
/// Cosmos.Kernel.Core.ARM64/Bridge/Import/GenericTimerNative.cs.
/// </summary>
[Plug(typeof(Stopwatch))]
public static class StopwatchPlug
{
#if ARCH_X64
    /// <summary>
    /// Gets the current timestamp using TSC.
    /// </summary>
    [PlugMember]
    public static long GetTimestamp()
    {
        return (long)X64CpuNative.ReadTsc();
    }

    /// <summary>
    /// Gets the TSC frequency in ticks per second.
    /// Called during Stopwatch class static initialization.
    /// </summary>
    [PlugMember]
    public static long GetFrequency()
    {
        return X64CpuOps.TscFrequency;
    }

    /// <summary>
    /// Gets the TSC frequency in ticks per second (field access plug).
    /// </summary>
    [PlugMember("get_Frequency")]
    public static long get_Frequency()
    {
        return X64CpuOps.TscFrequency;
    }

    /// <summary>
    /// Gets whether the timer is high resolution (TSC is high resolution).
    /// </summary>
    [PlugMember("get_IsHighResolution")]
    public static bool get_IsHighResolution()
    {
        return true;
    }
#elif ARCH_ARM64
    /// <summary>
    /// Gets the current timestamp using the ARM64 generic timer counter (cntpct_el0).
    /// </summary>
    [PlugMember]
    public static long GetTimestamp()
    {
        return (long)GenericTimerNative.GetCounter();
    }

    /// <summary>
    /// Gets the ARM64 generic timer frequency in ticks per second (cntfrq_el0).
    /// </summary>
    [PlugMember]
    public static long GetFrequency()
    {
        return (long)GenericTimerNative.GetFrequency();
    }

    /// <summary>
    /// Gets the ARM64 generic timer frequency in ticks per second (cntfrq_el0).
    /// </summary>
    [PlugMember("get_Frequency")]
    public static long get_Frequency()
    {
        return (long)GenericTimerNative.GetFrequency();
    }

    /// <summary>
    /// Gets whether the timer is high resolution.
    /// </summary>
    [PlugMember("get_IsHighResolution")]
    public static bool get_IsHighResolution()
    {
        return true;
    }
#elif ARCH_RISCV64
    /// <summary>
    /// Gets the current timestamp from the RISC-V time CSR.
    /// </summary>
    [PlugMember]
    public static long GetTimestamp()
    {
        return (long)Timebase.ReadCounter();
    }

    /// <summary>
    /// Gets the time CSR frequency in ticks per second (the device tree's timebase-frequency).
    /// </summary>
    [PlugMember]
    public static long GetFrequency()
    {
        return (long)Timebase.Frequency;
    }

    /// <summary>
    /// Gets the time CSR frequency in ticks per second (field access plug).
    /// </summary>
    [PlugMember("get_Frequency")]
    public static long get_Frequency()
    {
        return (long)Timebase.Frequency;
    }

    /// <summary>
    /// Gets whether the timer is high resolution: the 10 MHz of QEMU virt is
    /// the 100 ns DateTime tick.
    /// </summary>
    [PlugMember("get_IsHighResolution")]
    public static bool get_IsHighResolution()
    {
        return true;
    }
#endif
}
