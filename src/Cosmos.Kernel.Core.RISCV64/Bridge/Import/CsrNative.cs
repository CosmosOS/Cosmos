using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.RISCV64.Bridge;

/// <summary>
/// The RISC-V control and status registers the kernel reads and writes from
/// managed code (Cosmos.Kernel.Native.RISCV64, Devices/Timer.s and CPU/PageTable.s).
/// </summary>
internal static partial class CsrNative
{
    /// <summary>The time CSR: ticks at the platform's timebase frequency.</summary>
    [LibraryImport("*", EntryPoint = "_native_riscv64_read_time")]
    [SuppressGCTransition]
    public static partial ulong ReadTime();

    /// <summary>Writes stimecmp. Only on a CPU with the Sstc extension.</summary>
    [LibraryImport("*", EntryPoint = "_native_riscv64_write_stimecmp")]
    [SuppressGCTransition]
    public static partial void WriteStimecmp(ulong value);

    /// <summary>Reads sie, the supervisor interrupt-enable register.</summary>
    [LibraryImport("*", EntryPoint = "_native_riscv64_read_sie")]
    [SuppressGCTransition]
    public static partial ulong ReadSie();

    /// <summary>Sets the given sie bits.</summary>
    [LibraryImport("*", EntryPoint = "_native_riscv64_set_sie")]
    [SuppressGCTransition]
    public static partial void SetSie(ulong bits);

    /// <summary>Clears the given sie bits.</summary>
    [LibraryImport("*", EntryPoint = "_native_riscv64_clear_sie")]
    [SuppressGCTransition]
    public static partial void ClearSie(ulong bits);

    /// <summary>Reads sip, the supervisor interrupt-pending register.</summary>
    [LibraryImport("*", EntryPoint = "_native_riscv64_read_sip")]
    [SuppressGCTransition]
    public static partial ulong ReadSip();

    /// <summary>Sets the given sip bits: S-mode may set SSIP only.</summary>
    [LibraryImport("*", EntryPoint = "_native_riscv64_set_sip")]
    [SuppressGCTransition]
    public static partial void SetSip(ulong bits);

    /// <summary>Clears the given sip bits: S-mode may clear SSIP only.</summary>
    [LibraryImport("*", EntryPoint = "_native_riscv64_clear_sip")]
    [SuppressGCTransition]
    public static partial void ClearSip(ulong bits);

    /// <summary>Reads satp: the translation mode and the root page table.</summary>
    [LibraryImport("*", EntryPoint = "_native_riscv64_read_satp")]
    [SuppressGCTransition]
    public static partial ulong ReadSatp();

    /// <summary>sfence.vma for every address: orders page table stores before later translations.</summary>
    [LibraryImport("*", EntryPoint = "_native_riscv64_sfence_vma_all")]
    [SuppressGCTransition]
    public static partial void SfenceVmaAll();

    /// <summary>sfence.vma for one virtual address, every address space.</summary>
    [LibraryImport("*", EntryPoint = "_native_riscv64_sfence_vma")]
    [SuppressGCTransition]
    public static partial void SfenceVma(ulong virtualAddress);

    /// <summary>A full fence, device accesses included.</summary>
    [LibraryImport("*", EntryPoint = "_native_riscv64_io_fence")]
    [SuppressGCTransition]
    public static partial void IoFence();
}
