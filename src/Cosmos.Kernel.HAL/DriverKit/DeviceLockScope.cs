// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.Scheduler;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// The held state of a <see cref="DeviceLock"/>, returned by
/// <see cref="DeviceLock.Acquire"/>: a <c>using</c> binds to it, and
/// <see cref="Dispose"/> releases the lock then restores the interrupt
/// state the acquire found. Any context; disposed once, in the context
/// that acquired. A default instance holds nothing and disposes to nothing.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public ref struct DeviceLockScope
{
    private IrqLockScope _scope;
    private readonly bool _held;

    internal DeviceLockScope(IrqLockScope scope)
    {
        _scope = scope;
        _held = true;
    }

    /// <summary>Releases the lock and restores the interrupt state. Any context; a second call does nothing.</summary>
    public void Dispose()
    {
        if (_held)
        {
            _scope.Dispose();
        }
    }
}
