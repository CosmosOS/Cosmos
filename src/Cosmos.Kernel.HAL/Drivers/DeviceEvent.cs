// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.HAL.Drivers.Engine;

namespace Cosmos.Kernel.HAL.Drivers;

/// <summary>
/// Hands a signal from a driver's interrupt handler to a thread that waits
/// for it, such as a work item waiting for a command to complete. Signals
/// are counted: each <see cref="Signal"/> lets one <see cref="Wait"/>
/// return, whether it came before that Wait or during it. Created during
/// Probe through <see cref="DeviceContext.CreateEvent"/> and owned by the
/// binding: once the binding attempt is declined or fails, or the USB
/// device it was created for leaves the bus, every Wait returns false.
/// </summary>
internal sealed class DeviceEvent
{
    private readonly DeviceContext _context;
    private readonly InterruptEvent _event = new();

    /// <summary>Set when the binding attempt is torn down or its USB device left; every Wait returns false from then on.</summary>
    private volatile bool _cancelled;

    internal DeviceEvent(DeviceContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Signals the event: wakes one waiting thread, or lets the next
    /// <see cref="Wait"/> return at once. IRQ-safe: it allocates nothing and
    /// takes an IRQ-safe lock only, so an interrupt handler may call it.
    /// </summary>
    public void Signal() => _event.Signal();

    /// <summary>
    /// Blocks until the event is signalled, then consumes the signal.
    /// Thread context only. On the idle thread, which is the thread that
    /// boots the kernel and runs it, it polls with interrupts on instead of
    /// blocking, since the idle thread cannot block.
    /// </summary>
    /// <returns>
    /// True when a signal was consumed. False once the binding attempt that
    /// created the event was declined or failed, or its USB device left the
    /// bus, without waiting.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The Probe that created the event is still running. Its interrupts are
    /// armed only once Probe returns Bound, so no signal could come.
    /// </exception>
    public bool Wait()
    {
        if (_context.State == DeviceContextState.Probing)
        {
            throw new InvalidOperationException("A DeviceEvent cannot be waited on during the Probe that created it: the binding's interrupts are armed only after Probe returns Bound.");
        }

        if (_cancelled)
        {
            return false;
        }

        _event.Wait();
        if (_cancelled)
        {
            // Cancel signals once. Passing that wake-up on lets the next
            // thread waiting behind this one see the cancellation too.
            _event.Signal();
            return false;
        }

        return true;
    }

    /// <summary>
    /// Makes every <see cref="Wait"/>, running or to come, return false.
    /// Called when the binding attempt that created the event is torn down,
    /// and when its USB device leaves the bus.
    /// </summary>
    internal void Cancel()
    {
        _cancelled = true;
        _event.Signal();
    }
}
