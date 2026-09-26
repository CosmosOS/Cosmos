// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.Scheduler;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.Drivers;

/// <summary>
/// The one lock a driver may take in its interrupt handler, and the one to
/// guard state its handler shares with thread context: entering masks
/// interrupts on this CPU, then spins until the lock is free, so the
/// handler can never interrupt a holder on the same CPU and wait for it
/// forever. Hold it briefly, and never across a wait. Not re-entrant.
/// </summary>
/// <example>
/// <code>
/// using (_lock.EnterScope())
/// {
///     _pending++;
/// }
/// </code>
/// </example>
internal sealed class IrqSafeLock
{
    // Not readonly: SpinLock is a mutable struct, and a readonly field would
    // hand every call a defensive copy that no one else sees locked.
    private SchedSpinLock _lock;

    /// <summary>
    /// Masks interrupts and takes the lock. Disposing the returned scope
    /// releases the lock, then puts the interrupt mask back as it was.
    /// </summary>
    /// <returns>The held lock, to dispose once, typically through <c>using</c>.</returns>
    public Scope EnterScope() => new(_lock.AcquireIrqSafe());

    /// <summary>
    /// The held lock, returned by <see cref="EnterScope"/>. A ref struct, so
    /// it cannot outlive the method that took the lock.
    /// </summary>
    public ref struct Scope
    {
        private IrqLockScope _held;

        /// <summary>False for a <c>default</c> scope, which holds nothing to release.</summary>
        private readonly bool _entered;

        internal Scope(IrqLockScope held)
        {
            _held = held;
            _entered = true;
        }

        /// <summary>
        /// Releases the lock, then restores the interrupt mask as it was
        /// before <see cref="EnterScope"/>. A second call does nothing.
        /// </summary>
        public void Dispose()
        {
            if (_entered)
            {
                _held.Dispose();
            }
        }
    }
}
