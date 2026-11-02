using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.CPU;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// Simple spinlock for SMP synchronization.
///
/// <para>The plain <see cref="Acquire"/> / <see cref="Release"/> pair is
/// only safe for locks that are never taken from interrupt context. If an
/// ISR can acquire the same lock as mainline code (e.g. a wait/signal
/// pair where the ISR signals), use <see cref="AcquireIrqSafe"/>: a single
/// CPU holding the plain lock when the ISR fires will spin forever waiting
/// for itself.</para>
///
/// <para>Nothing allocates while holding one, whichever form took it: a
/// holder that starts a collection stops every other CPU, and a CPU that
/// spins on this lock with interrupts masked cannot take the stop interrupt.
/// The spin answers the stop in its place (<see cref="CpuStop.Safepoint"/>),
/// so a waiter never holds a collection up; a holder of one of the three
/// locks a collection takes (the heap lock, the handle store's chain lock,
/// the page allocator's) would, so those are taken through
/// <see cref="AcquireCollectorLock"/>, which counts them per CPU
/// (<see cref="PerCpuBlock.CollectorLockDepth"/>): the safepoint acknowledges
/// at zero only, and <see cref="CpuStop.Enter"/> at a nonzero count panics.
/// The scheduler's locks are taken in one order, queue locks by ascending
/// CPU index, then the registry lock, then the stop, then the heap lock, and
/// two queue locks at once only by a migration (<see cref="SchedulerManager"/>).</para>
/// </summary>
internal struct SpinLock
{
    private int _locked;

    public void Acquire()
    {
        while (Interlocked.CompareExchange(ref _locked, 1, 0) != 0)
        {
            // Bounded by the holder's release; a stop that lands meanwhile is
            // answered from here, interrupts staying as they are.
            CpuStop.Safepoint();
        }
    }

    public void Release()
    {
        Interlocked.Exchange(ref _locked, 0);
    }

    public bool TryAcquire()
    {
        return Interlocked.CompareExchange(ref _locked, 1, 0) == 0;
    }

    /// <summary>
    /// Disable interrupts and acquire the lock as a single scope. Dispose
    /// releases the lock then restores the prior interrupt state. Use
    /// whenever the same lock can be taken from both mainline and ISR
    /// context — without IRQ-safe acquisition, a single CPU holding the
    /// lock when its ISR fires will deadlock against itself.
    /// </summary>
    [UnscopedRef]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IrqLockScope AcquireIrqSafe()
    {
        return new IrqLockScope(ref this);
    }

    /// <summary>
    /// <see cref="AcquireIrqSafe"/> for one of the locks a collection takes: the
    /// hold is counted in the CPU's <see cref="PerCpuBlock.CollectorLockDepth"/>,
    /// which keeps a CPU from acknowledging a stop while it holds the lock and
    /// makes an allocation under it a panic rather than a hang.
    /// </summary>
    [UnscopedRef]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CollectorLockScope AcquireCollectorLock()
    {
        return new CollectorLockScope(ref this);
    }

    public readonly bool IsLocked => _locked != 0;
}

/// <summary>
/// RAII scope returned by <see cref="SpinLock.AcquireCollectorLock"/>: an
/// <see cref="IrqLockScope"/> whose hold is counted in the CPU's
/// <see cref="PerCpuBlock.CollectorLockDepth"/>. The count goes up once the
/// lock is held and down before it is released, so the spin in between runs
/// at the outer depth: a CPU waiting for its first collector lock answers a
/// stop, one waiting for an inner lock while holding an outer one does not.
/// </summary>
internal ref struct CollectorLockScope
{
    private IrqLockScope _scope;
    private bool _disposed;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal CollectorLockScope(ref SpinLock spinLock)
    {
        _scope = new IrqLockScope(ref spinLock);
        _disposed = false;
        PerCpuBlock.Current.CollectorLockDepth++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        PerCpuBlock.Current.CollectorLockDepth--;
        _scope.Dispose();
    }
}

/// <summary>
/// RAII scope returned by <see cref="SpinLock.AcquireIrqSafe"/>. Disables
/// interrupts and acquires the lock on creation; on dispose, releases the
/// lock then restores the prior interrupt state. Dispose order matters:
/// the lock must be released before interrupts are re-enabled so an ISR
/// firing immediately after restore never sees the lock held.
/// </summary>
internal ref struct IrqLockScope
{
    private InternalCpu.InterruptScope _irq;
    private readonly ref SpinLock _lock;
    private bool _disposed;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal IrqLockScope(ref SpinLock spinLock)
    {
        _irq = new InternalCpu.InterruptScope();
        _lock = ref spinLock;
        _disposed = false;
        spinLock.Acquire();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        _lock.Release();
        _irq.Dispose();
    }
}
