// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.Bridge;

namespace Cosmos.Kernel.Core.CPU;

/// <summary>
/// Low-level CPU operations that can be used by Core components like the heap.
/// Native imports live in Bridge/Import/CpuNative.cs.
/// </summary>
internal static class InternalCpu
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DisableInterrupts() => CpuNative.DisableInterrupts();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EnableInterrupts() => CpuNative.EnableInterrupts();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Halt() => CpuNative.Halt();

    /// <summary>
    /// Creates a scope that disables interrupts and automatically re-enables them on dispose.
    /// Usage: using (InternalCpu.DisableInterruptsScope()) { ... }
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static InterruptScope DisableInterruptsScope()
    {
        return new InterruptScope();
    }

    /// <summary>
    /// A disposable scope that disables interrupts on creation and restores
    /// the prior interrupt-enable state on dispose. Critically, this saves
    /// the full RFLAGS / DAIF on entry and restores it on exit, so nested
    /// scopes are correct: an inner scope disposing inside an outer
    /// disabled region does NOT prematurely re-enable interrupts.
    /// </summary>
    public ref struct InterruptScope
    {
        private bool _disposed;
        private ulong _savedFlags;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InterruptScope()
        {
            _disposed = false;
            _savedFlags = CpuNative.SaveIrqAndDisable();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                CpuNative.RestoreIrq(_savedFlags);
            }
        }
    }
}
