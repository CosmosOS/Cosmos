// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Bridge;

/// <summary>
/// Native imports for the kernel random generator (Security/KernelRandom.cs):
/// the CPU's random number instructions and the cycle counter its jitter
/// samples are timed with, all in CPU/Random.s of the native packages.
/// </summary>
/// <remarks>
/// The counter is read through its own serialized routine rather than the
/// plain reads the arch bridges (X64CpuNative, GenericTimerNative) import,
/// which Core could not reach anyway: a jitter sample must time exactly the
/// work between two reads. Never call a random instruction import before
/// <see cref="Security.HardwareRandom"/> has seen it advertised: it faults
/// where the CPU lacks it.
/// </remarks>
internal static unsafe partial class RandomNative
{
#if ARCH_X64
    /// <summary>RDRAND: 1 with a word at <paramref name="value"/>, 0 after 10 failed attempts.</summary>
    [LibraryImport("*", EntryPoint = "_native_cpu_rdrand64")]
    [SuppressGCTransition]
    public static partial int RdRand64(ulong* value);

    /// <summary>RDSEED: 1 with a word at <paramref name="value"/>, 0 after 100 failed attempts.</summary>
    [LibraryImport("*", EntryPoint = "_native_cpu_rdseed64")]
    [SuppressGCTransition]
    public static partial int RdSeed64(ulong* value);

    /// <summary>The time stamp counter, RDTSC between two LFENCEs.</summary>
    [LibraryImport("*", EntryPoint = "_native_cpu_rdtsc_ordered")]
    [SuppressGCTransition]
    public static partial ulong ReadCounter();
#elif ARCH_ARM64
    /// <summary>ID_AA64ISAR0_EL1; FEAT_RNG is present when bits 63:60 are not zero.</summary>
    [LibraryImport("*", EntryPoint = "_native_cpu_read_id_aa64isar0")]
    [SuppressGCTransition]
    public static partial ulong ReadIdAa64Isar0();

    /// <summary>RNDR: 1 with a word at <paramref name="value"/>, 0 after 10 failed reads.</summary>
    [LibraryImport("*", EntryPoint = "_native_cpu_rndr64")]
    [SuppressGCTransition]
    public static partial int Rndr64(ulong* value);

    /// <summary>RNDRRS: 1 with a word at <paramref name="value"/>, 0 after 100 failed reads.</summary>
    [LibraryImport("*", EntryPoint = "_native_cpu_rndrrs64")]
    [SuppressGCTransition]
    public static partial int Rndrrs64(ulong* value);

    /// <summary>The generic timer's physical count, CNTPCT_EL0 between two ISBs.</summary>
    [LibraryImport("*", EntryPoint = "_native_cpu_cntpct_ordered")]
    [SuppressGCTransition]
    public static partial ulong ReadCounter();
#endif
}
