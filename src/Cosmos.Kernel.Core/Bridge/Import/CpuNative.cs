using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Bridge;

internal static partial class CpuNative
{
    [LibraryImport("*", EntryPoint = "_native_cpu_disable_interrupts")]
    [SuppressGCTransition]
    public static partial void DisableInterrupts();

    [LibraryImport("*", EntryPoint = "_native_cpu_enable_interrupts")]
    [SuppressGCTransition]
    public static partial void EnableInterrupts();

    [LibraryImport("*", EntryPoint = "_native_cpu_halt")]
    [SuppressGCTransition]
    public static partial void Halt();

    /// <summary>
    /// Captures the current interrupt-enable state (RFLAGS / DAIF), disables interrupts, and returns
    /// the captured state. Pair with <see cref="RestoreIrq"/> — <c>InternalCpu.InterruptScope</c>
    /// uses these so a nested inner dispose does not prematurely re-enable interrupts.
    /// </summary>
    [LibraryImport("*", EntryPoint = "_native_cpu_save_irq_and_disable")]
    [SuppressGCTransition]
    public static partial ulong SaveIrqAndDisable();

    /// <summary>Restores an interrupt-enable state previously returned by <see cref="SaveIrqAndDisable"/>.</summary>
    [LibraryImport("*", EntryPoint = "_native_cpu_restore_irq")]
    [SuppressGCTransition]
    public static partial void RestoreIrq(ulong flags);

    /// <summary>
    /// Reads the current interrupt-enable state (RFLAGS / DAIF) without changing it, for a
    /// caller that only asks the question: no interrupt window is closed and reopened.
    /// </summary>
    [LibraryImport("*", EntryPoint = "_native_cpu_read_irq_flags")]
    [SuppressGCTransition]
    public static partial ulong ReadIrqFlags();

    /// <summary>
    /// Makes <paramref name="block"/> the calling CPU's: loads the per-CPU register,
    /// <c>IA32_GS_BASE</c> on x64 and <c>TPIDR_EL1</c> on ARM64, with its address. Once per CPU,
    /// by <see cref="CPU.PerCpuBlock.InstallBootCpu"/> for the boot CPU.
    /// </summary>
    [LibraryImport("*", EntryPoint = "_native_cpu_set_block")]
    [SuppressGCTransition]
    public static partial void SetCurrentBlock(nuint block);

    /// <summary>
    /// The index of the calling CPU, read from its per-CPU block (<see cref="CPU.PerCpuBlock"/>)
    /// through the per-CPU register: <c>IA32_GS_BASE</c> on x64, <c>TPIDR_EL1</c> on ARM64.
    /// </summary>
    [LibraryImport("*", EntryPoint = "_native_cpu_read_index")]
    [SuppressGCTransition]
    public static partial ulong ReadCpuIndex();

    /// <summary>The calling CPU's per-CPU block (<see cref="CPU.PerCpuBlock"/>).</summary>
    [LibraryImport("*", EntryPoint = "_native_cpu_block_address")]
    [SuppressGCTransition]
    public static partial nuint GetCurrentBlock();

    /// <summary>
    /// The address of the application processor trampoline (<c>CPU/Smp.s</c> of the native
    /// package): what <see cref="CPU.Smp.StartApplicationProcessors"/> writes into a parked
    /// CPU's <c>goto_address</c>. The trampoline loads the per-CPU register from the entry's
    /// <c>extra_argument</c>, puts the CPU in the state <c>kmain</c> gives the boot CPU, and
    /// calls <c>__managed__ap_main</c>.
    /// </summary>
    [LibraryImport("*", EntryPoint = "_native_cpu_ap_trampoline_address")]
    [SuppressGCTransition]
    public static partial nuint GetApTrampolineAddress();
}
