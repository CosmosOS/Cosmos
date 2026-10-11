using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.ARM64.Bridge;

internal static partial class GICv3Native
{
    [LibraryImport("*", EntryPoint = "_native_arm64_gicv3_read_icc_iar1_el1")]
    [SuppressGCTransition]
    public static partial uint ReadIar1();

    [LibraryImport("*", EntryPoint = "_native_arm64_gicv3_write_icc_eoir1_el1")]
    [SuppressGCTransition]
    public static partial void WriteEoir1(uint intId);

    [LibraryImport("*", EntryPoint = "_native_arm64_gicv3_read_icc_hppir1_el1")]
    [SuppressGCTransition]
    public static partial uint ReadHppir1();

    [LibraryImport("*", EntryPoint = "_native_arm64_gicv3_write_icc_pmr_el1")]
    [SuppressGCTransition]
    public static partial void WritePmr(uint priority);

    [LibraryImport("*", EntryPoint = "_native_arm64_gicv3_read_icc_pmr_el1")]
    [SuppressGCTransition]
    public static partial uint ReadPmr();

    [LibraryImport("*", EntryPoint = "_native_arm64_gicv3_write_icc_bpr1_el1")]
    [SuppressGCTransition]
    public static partial void WriteBpr1(uint value);

    [LibraryImport("*", EntryPoint = "_native_arm64_gicv3_write_icc_ctlr_el1")]
    [SuppressGCTransition]
    public static partial void WriteCtlr(uint value);

    [LibraryImport("*", EntryPoint = "_native_arm64_gicv3_read_icc_ctlr_el1")]
    [SuppressGCTransition]
    public static partial uint ReadCtlr();

    [LibraryImport("*", EntryPoint = "_native_arm64_gicv3_write_icc_igrpen1_el1")]
    [SuppressGCTransition]
    public static partial void WriteIgrpen1(uint value);

    [LibraryImport("*", EntryPoint = "_native_arm64_gicv3_read_icc_igrpen1_el1")]
    [SuppressGCTransition]
    public static partial uint ReadIgrpen1();

    [LibraryImport("*", EntryPoint = "_native_arm64_gicv3_write_icc_sgi1r_el1")]
    [SuppressGCTransition]
    public static partial void WriteSgi1r(ulong value);

    [LibraryImport("*", EntryPoint = "_native_arm64_gicv3_read_icc_sre_el1")]
    [SuppressGCTransition]
    public static partial uint ReadSre();

    [LibraryImport("*", EntryPoint = "_native_arm64_gicv3_write_icc_sre_el1")]
    [SuppressGCTransition]
    public static partial void WriteSre(uint value);

    [LibraryImport("*", EntryPoint = "_native_arm64_read_mpidr_el1")]
    [SuppressGCTransition]
    public static partial ulong ReadMpidr();
}
