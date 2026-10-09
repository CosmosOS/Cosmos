using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.CPU;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// Masks maskable interrupts on the current CPU for the lifetime of the
/// scope and restores the previous interrupt state on dispose. Obtained
/// from <see cref="SchedulerManager.MaskInterrupts"/>; use it in a
/// <see langword="using"/> statement around scheduler entry points the
/// manager does not already guard (diagnostics reads, tuning setters), so
/// the timer tick cannot observe a half-mutated run structure.
/// </summary>
[Experimental(Experimentals.SchedulerSeamDiagId)]
public ref struct InterruptMaskScope
{
    // A struct's parameterless constructor cannot be hidden, so
    // default(InterruptMaskScope) is reachable from any consumer and carries
    // a zeroed InterruptScope. Disposing that would restore flags nobody
    // saved - RFLAGS 0 clears IF on x64, DAIF 0 unmasks on ARM64 - inside
    // whatever critical section the caller happens to be in. Only a scope
    // handed out by MaskInterrupts() has anything to restore.
    private readonly bool _taken;
    private InternalCpu.InterruptScope _scope;

    internal InterruptMaskScope(InternalCpu.InterruptScope scope)
    {
        _taken = true;
        _scope = scope;
    }

    /// <summary>
    /// Restores the interrupt state captured when the scope was taken. Does
    /// nothing on a default-constructed scope, which masked nothing.
    /// </summary>
    public void Dispose()
    {
        if (_taken)
        {
            _scope.Dispose();
        }
    }
}
