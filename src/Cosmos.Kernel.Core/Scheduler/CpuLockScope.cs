// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// Holds one CPU's queue lock, with interrupts masked on the calling CPU, for the lifetime of
/// the scope; disposing it releases the lock and restores the previous interrupt state.
/// Obtained from <see cref="SchedulerManager.LockCpu"/>. What a caller takes around a walk of
/// a CPU's run structure through the diagnostics hooks, or around a tuning setter of its own:
/// the policy hooks run under this lock, taken by the mechanism, and never take it themselves.
/// </summary>
[Experimental(Experimentals.SchedulerSeamDiagId)]
public ref struct CpuLockScope
{
    // A struct's parameterless constructor cannot be hidden, so default(CpuLockScope) is
    // reachable from any consumer and holds nothing. Only a scope handed out by LockCpu has a
    // lock to release and flags to restore.
    private readonly bool _taken;
    private IrqLockScope _scope;

    internal CpuLockScope(IrqLockScope scope)
    {
        _taken = true;
        _scope = scope;
    }

    /// <summary>
    /// Releases the queue lock and restores the interrupt state captured when the scope was
    /// taken. Does nothing on a default-constructed scope, which locked nothing.
    /// </summary>
    public void Dispose()
    {
        if (_taken)
        {
            _scope.Dispose();
        }
    }
}
