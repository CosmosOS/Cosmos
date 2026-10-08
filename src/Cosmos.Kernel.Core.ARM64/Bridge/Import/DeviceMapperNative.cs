using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.ARM64.Bridge;

internal static partial class DeviceMapperNative
{
    [LibraryImport("*", EntryPoint = "_native_arm64_read_ttbr0_el1")]
    [SuppressGCTransition]
    public static partial ulong ReadTtbr0();

    [LibraryImport("*", EntryPoint = "_native_arm64_read_ttbr1_el1")]
    [SuppressGCTransition]
    public static partial ulong ReadTtbr1();

    [LibraryImport("*", EntryPoint = "_native_arm64_read_tcr_el1")]
    [SuppressGCTransition]
    public static partial ulong ReadTcr();

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

    /// <summary>
    /// Clean and invalidate the data-cache line covering <paramref name="address"/>
    /// to the point of coherency, then DSB SY and ISB.
    /// </summary>
    /// <remarks>
    /// Required after writing a page-table descriptor. The tables live in
    /// cacheable memory, so a descriptor written with ordinary stores can still
    /// be stale from the translation walk's point of view; the Arm ARM
    /// translation-table update sequence requires the cache maintenance before
    /// the TLBI that publishes it. Skipping it produces a mapping the software
    /// walk can read but the MMU cannot use — AT returns 0 and a write through
    /// the new alias traps.
    /// <para>
    /// This symbol was exported by the native assembly from the start but had no
    /// managed caller until the Pager needed it.
    /// </para>
    /// </remarks>
    [LibraryImport("*", EntryPoint = "_native_arm64_dc_civac")]
    [SuppressGCTransition]
    public static partial void CleanCacheLine(ulong address);
}
