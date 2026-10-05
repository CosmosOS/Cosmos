// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.DriverKit.Ps2;

/// <summary>
/// The one interrupt of a Ps2 node: raised by its <see cref="Ps2Access"/>
/// for every byte of the port's stream, whether the controller driver took
/// the byte from its interrupt handler, from a periodic drain or from a
/// poll inside an exchange. Always connectable: the controller driver
/// raises it whatever its own wake mechanism is. The controller's line
/// serves both ports and is never masked for one, so a mask drops
/// deliveries at the kit. Connection and mask state live under one
/// IRQ-safe lock; the trampoline is copied under it and invoked after it
/// is released (its fault path masks the handle, which comes back to the
/// same lock).
/// </summary>
internal sealed class Ps2InterruptSource : InterruptSource
{
    private readonly Ps2Port _port;
    private SchedSpinLock _lock;
    private InterruptTrampoline? _trampoline;
    private bool _masked;

    /// <summary>The source of one port. Thread context.</summary>
    /// <param name="port">The port.</param>
    internal Ps2InterruptSource(Ps2Port port)
    {
        _port = port;
    }

    /// <inheritdoc/>
    public override string Describe() => _port == Ps2Port.Keyboard ? "port kbd" : "port aux";

    /// <summary>
    /// Delivers one interrupt to the connected handler: nothing while
    /// masked or unconnected. Called by the access with interrupts
    /// disabled, from the controller driver's handler or from a polled
    /// delivery; allocation-free. The trampoline is copied under the lock
    /// and invoked after it is released: its fault path masks the handle,
    /// which comes back to this lock, and the kit's spin lock is not
    /// reentrant.
    /// </summary>
    internal void Raise()
    {
        InterruptTrampoline? trampoline;
        using (_lock.AcquireIrqSafe())
        {
            if (_masked || _trampoline is null)
            {
                return;
            }

            trampoline = _trampoline;
        }

        trampoline.Invoke(synthetic: false);
    }

    /// <inheritdoc/>
    protected override bool TryConnectCore(InterruptTrampoline trampoline)
    {
        using (_lock.AcquireIrqSafe())
        {
            if (_trampoline is not null)
            {
                return false;
            }

            _trampoline = trampoline;
            _masked = false;
            return true;
        }
    }

    /// <inheritdoc/>
    protected override void MaskCore()
    {
        using (_lock.AcquireIrqSafe())
        {
            _masked = true;
        }
    }

    /// <inheritdoc/>
    protected override void UnmaskCore()
    {
        using (_lock.AcquireIrqSafe())
        {
            _masked = false;
        }
    }

    /// <inheritdoc/>
    protected override void DisconnectCore()
    {
        using (_lock.AcquireIrqSafe())
        {
            _masked = true;
            _trampoline = null;
        }
    }
}
