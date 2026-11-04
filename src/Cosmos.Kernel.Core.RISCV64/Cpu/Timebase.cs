// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.RISCV64.Bridge;

namespace Cosmos.Kernel.Core.RISCV64.Cpu;

/// <summary>
/// The frequency the time CSR counts at. RISC-V has no register that holds
/// it: the device tree's <c>/cpus/timebase-frequency</c> does, and the HAL
/// sets it from there at boot. Until then, and without a tree, it is the
/// 10 MHz of the QEMU virt machine.
/// </summary>
internal static class Timebase
{
    /// <summary>The QEMU virt machine's timebase: 10 MHz.</summary>
    public const ulong QemuVirtFrequency = 10_000_000;

    private static ulong s_frequency = QemuVirtFrequency;

    /// <summary>Ticks of the time CSR per second.</summary>
    public static ulong Frequency => s_frequency;

    /// <summary>Records the timebase the platform description gives; 0 is ignored.</summary>
    /// <param name="frequency">Ticks per second.</param>
    public static void SetFrequency(ulong frequency)
    {
        if (frequency != 0)
        {
            s_frequency = frequency;
        }
    }

    /// <summary>The time CSR.</summary>
    public static ulong ReadCounter() => CsrNative.ReadTime();
}
