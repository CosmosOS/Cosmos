// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Boot.Limine;

/// <summary>
/// Limine MP (multiprocessor) request. Its presence makes the bootloader start every
/// application processor and park it, spinning on its <c>goto_address</c>; the response lists
/// the CPUs, the boot CPU included, with the identity their interrupt controller knows them by.
/// The response and the per-CPU entry are laid out differently on x86-64 and AArch64, so
/// <see cref="Response"/> is untyped here and read through <see cref="LimineMpResponseX64"/> or
/// <see cref="LimineMpResponseArm64"/> by the architecture-specific build of the kernel.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct LimineMpRequest(ulong flags)
{
    /// <summary>Request flag bit 0, x86-64 only: enable x2APIC if the hardware has it.</summary>
    public const ulong X2ApicFlag = 1;

    public readonly LimineID ID = new(0x95a67b819a1b857e, 0xa0b61b723b6a73e0);
    public readonly ulong Revision = 0;
    public readonly void* Response;

    /// <summary>The request's flags; <see cref="X2ApicFlag"/> is the only one defined.</summary>
    public readonly ulong Flags = flags;
}

/// <summary>The MP response on x86-64.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct LimineMpResponseX64
{
    public readonly ulong Revision;
    /// <summary>Bit 0: x2APIC was enabled.</summary>
    public readonly uint Flags;
    /// <summary>The Local APIC id of the boot CPU.</summary>
    public readonly uint BspLapicId;
    /// <summary>How many CPUs there are, the boot CPU included.</summary>
    public readonly ulong CpuCount;
    /// <summary><see cref="CpuCount"/> pointers to the CPUs' entries.</summary>
    public readonly LimineMpInfoX64** Cpus;
}

/// <summary>
/// One CPU on x86-64. The kernel writes <see cref="ExtraArgument"/>, then <see cref="GotoAddress"/>
/// with a release store: the parked CPU jumps there with a pointer to this entry in <c>rdi</c>,
/// on a stack of its own, in the state the boot CPU had at entry.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct LimineMpInfoX64
{
    /// <summary>The ACPI processor UID the MADT gives this CPU.</summary>
    public uint ProcessorId;
    /// <summary>The Local APIC id the MADT gives this CPU.</summary>
    public uint LapicId;
    /// <summary>The bootloader's; the parked CPU uses it, the kernel leaves it alone.</summary>
    public ulong Reserved;
    /// <summary>Where the parked CPU goes once this is written; unused for the boot CPU's entry.</summary>
    public nuint GotoAddress;
    /// <summary>Free for the kernel: the CPU reads it where it lands.</summary>
    public ulong ExtraArgument;
}

/// <summary>The MP response on AArch64.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct LimineMpResponseArm64
{
    public readonly ulong Revision;
    /// <summary>Always zero.</summary>
    public readonly ulong Flags;
    /// <summary>The MPIDR of the boot CPU: its affinity fields, every other bit masked off.</summary>
    public readonly ulong BspMpidr;
    /// <summary>How many CPUs there are, the boot CPU included.</summary>
    public readonly ulong CpuCount;
    /// <summary><see cref="CpuCount"/> pointers to the CPUs' entries.</summary>
    public readonly LimineMpInfoArm64** Cpus;
}

/// <summary>
/// One CPU on AArch64. The kernel writes <see cref="ExtraArgument"/>, then <see cref="GotoAddress"/>
/// with a release store: the parked CPU jumps there with a pointer to this entry in <c>x0</c>,
/// on a stack of its own, in the state the boot CPU had at entry.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct LimineMpInfoArm64
{
    /// <summary>The ACPI processor UID the MADT gives this CPU; 0 without ACPI.</summary>
    public uint ProcessorId;
    /// <summary>The bootloader's.</summary>
    public uint Reserved1;
    /// <summary>The MPIDR of this CPU: its affinity fields, every other bit masked off.</summary>
    public ulong Mpidr;
    /// <summary>The bootloader's; the parked CPU uses it, the kernel leaves it alone.</summary>
    public ulong Reserved;
    /// <summary>Where the parked CPU goes once this is written; unused for the boot CPU's entry.</summary>
    public nuint GotoAddress;
    /// <summary>Free for the kernel: the CPU reads it where it lands.</summary>
    public ulong ExtraArgument;
}
