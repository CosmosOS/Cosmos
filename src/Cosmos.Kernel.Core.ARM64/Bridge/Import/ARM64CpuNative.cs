using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.ARM64.Bridge;

/// <summary>
/// Native CPU operations for ARM64.
/// </summary>
internal static partial class ARM64CpuNative
{
    [LibraryImport("*", EntryPoint = "_native_arm64_read_ttbr1_el1")]
    [SuppressGCTransition]
    public static partial ulong ReadTtbr1();

    [LibraryImport("*", EntryPoint = "_native_arm64_write_ttbr1_el1")]
    [SuppressGCTransition]
    public static partial void WriteTtbr1(ulong value);

    [LibraryImport("*", EntryPoint = "_native_arm64_read_mair_el1")]
    [SuppressGCTransition]
    public static partial ulong ReadMair();

    [LibraryImport("*", EntryPoint = "_native_arm64_tlbi_vale1")]
    [SuppressGCTransition]
    public static partial void InvalidateTlb(ulong vaShifted);

    [LibraryImport("*", EntryPoint = "_native_arm64_dsb_isb")]
    [SuppressGCTransition]
    public static partial void DsbIsb();
}
