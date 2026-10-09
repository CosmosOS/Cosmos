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
}
