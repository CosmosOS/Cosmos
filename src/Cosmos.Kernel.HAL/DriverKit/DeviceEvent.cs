// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.Scheduler;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// A counted signal between a handler and a thread, created through
/// <see cref="DeviceBinding.CreateEvent"/>. A handler signals it; a thread
/// waits through <see cref="DeviceBinding.Wait"/>, which is the only way to
/// wait, so a handler holding the event cannot block on it. Teardown cancels
/// it, and every waiter then returns false, at once and forever.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class DeviceEvent
{
    private readonly InterruptEvent _event = new();
    private volatile bool _cancelled;

    internal DeviceEvent()
    {
    }

    /// <summary>True once the binding that created the event is being torn down.</summary>
    public bool IsCancelled => _cancelled;

    /// <summary>Signals the event, waking one waiter or latching for the next. Allocation-free; any context.</summary>
    public void Signal() => _event.Signal();

    /// <summary>
    /// Waits for a signal, up to <paramref name="timeoutMilliseconds"/>.
    /// Thread context only, through the binding.
    /// </summary>
    /// <param name="timeoutMilliseconds">Longest time to wait.</param>
    /// <returns>True when a signal was consumed; false on timeout or cancellation.</returns>
    internal bool Wait(uint timeoutMilliseconds)
    {
        if (_cancelled)
        {
            return false;
        }

        bool signaled = _event.Wait(timeoutMilliseconds);
        if (_cancelled)
        {
            // The cancel wakes one waiter at a time; pass it on so every
            // thread parked here leaves before the binding joins them.
            _event.Signal();
            return false;
        }

        return signaled;
    }

    /// <summary>Makes every waiter, present and future, return false. Teardown only.</summary>
    internal void Cancel()
    {
        _cancelled = true;
        _event.Signal();
    }
}
