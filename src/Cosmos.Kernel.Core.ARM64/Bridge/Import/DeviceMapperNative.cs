using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.ARM64.Bridge;

internal static partial class DeviceMapperNative
{
    [LibraryImport("*", EntryPoint = "_native_arm64_read_ttbr1_el1")]
    [SuppressGCTransition]
    public static partial ulong ReadTtbr1();

    [LibraryImport("*", EntryPoint = "_native_arm64_read_mair_el1")]
    [SuppressGCTransition]
    public static partial ulong ReadMair();

    [LibraryImport("*", EntryPoint = "_native_arm64_tlbi_vale1")]
    [SuppressGCTransition]
    public static partial void FlushTlb(ulong vaShifted);

    [LibraryImport("*", EntryPoint = "_native_arm64_va_to_pa")]
    [SuppressGCTransition]
    public static partial ulong VirtToPhys(ulong va);

    [LibraryImport("*", EntryPoint = "_native_arm64_spare_l2_table_addr")]
    [SuppressGCTransition]
    public static partial ulong GetSpareL2TableAddr();

    [LibraryImport("*", EntryPoint = "_native_arm64_dsb_isb")]
    [SuppressGCTransition]
    public static partial void DsbIsb();
}
