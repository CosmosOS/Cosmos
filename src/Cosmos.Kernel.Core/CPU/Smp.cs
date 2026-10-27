// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Boot.Limine;

namespace Cosmos.Kernel.Core.CPU;

/// <summary>
/// The CPUs of the machine, as the bootloader reports them (SMP phase 2, CosmosOS/Cosmos#3339).
/// Limine's MP response lists every CPU with the identity its interrupt controller knows it by
/// (the Local APIC id on x64, the MPIDR affinity on ARM64) and names the one that booted; the
/// list, not the MADT, is the truth about how many CPUs exist, since it is what the bootloader
/// could start, and a machine without ACPI has one too. The boot CPU is index 0 wherever the
/// list puts it; the others take indices 1 onward in the list's order, which is the order the
/// scheduler's per-CPU states and idle threads follow.
/// </summary>
internal static unsafe class Smp
{
    /// <summary>
    /// The CPUs the bootloader reports, the boot CPU included: 1 when it did not answer the MP
    /// request (a loader without the feature, or a boot it could not enumerate the CPUs on),
    /// and never more than the per-CPU table holds.
    /// </summary>
    internal static uint CpuCount
    {
        get
        {
            ulong count = BootloaderCpuCount;
            if (count == 0)
            {
                return 1;
            }

            return count > (ulong)PerCpuBlock.MaxCpus ? (uint)PerCpuBlock.MaxCpus : (uint)count;
        }
    }

    /// <summary>The MP response's count, 0 when there is no response.</summary>
    private static ulong BootloaderCpuCount
    {
        get
        {
#if ARCH_ARM64
            LimineMpResponseArm64* response = (LimineMpResponseArm64*)Limine.Mp.Response;
#else
            LimineMpResponseX64* response = (LimineMpResponseX64*)Limine.Mp.Response;
#endif
            return response == null ? 0 : response->CpuCount;
        }
    }
}
