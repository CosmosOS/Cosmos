// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.DriverKit.Platform;

/// <summary>
/// One line of the platform's interrupt controller that a root platform
/// node delivers (a virtio-mmio slot's SPI on the virt machine). The
/// machine description names the line and the <see cref="PlatformLineRouting"/>
/// that programs the controller; the source only keeps the connection
/// state, under one IRQ-safe lock, and forwards to the routing. The
/// trampoline is stored before the routing is asked, so a level line that
/// fires the moment it is enabled finds it, and dropped again when the
/// routing refuses, so the next candidate can request the source and a
/// disconnect never reaches the routing for a line it did not grant. The
/// adapter from the platform's handler shape to the kit's trampoline is
/// allocated once, at the first connect, in thread context.
/// </summary>
internal sealed class PlatformLineInterruptSource : InterruptSource
{
    private readonly uint _line;
    private readonly PlatformLineRouting _routing;
    private SchedSpinLock _lock;
    private InterruptTrampoline? _trampoline;
    private InterruptManager.IrqDelegate? _adapter;
    private volatile bool _connected;

    /// <summary>Creates the source for one line. Thread context.</summary>
    /// <param name="line">The controller's id of the line.</param>
    /// <param name="routing">The machine description's routing for the controller.</param>
    internal PlatformLineInterruptSource(uint line, PlatformLineRouting routing)
    {
        _line = line;
        _routing = routing;
    }

    /// <inheritdoc/>
    public override string Describe() => $"line {_line}";

    /// <inheritdoc/>
    protected override bool TryConnectCore(InterruptTrampoline trampoline)
    {
        if (!InterruptManager.IsControllerInitialized)
        {
            return false;
        }

        _adapter ??= HandleIrq;
        using (_lock.AcquireIrqSafe())
        {
            if (_connected)
            {
                return false;
            }

            _trampoline = trampoline;
            _connected = true;
        }

        if (_routing.TryConnect(_line, _adapter))
        {
            return true;
        }

        using (_lock.AcquireIrqSafe())
        {
            _trampoline = null;
            _connected = false;
        }

        return false;
    }

    /// <inheritdoc/>
    protected override void MaskCore()
    {
        if (_connected)
        {
            _routing.Mask(_line);
        }
    }

    /// <inheritdoc/>
    protected override void UnmaskCore()
    {
        if (_connected)
        {
            _routing.Unmask(_line);
        }
    }

    /// <inheritdoc/>
    protected override void DisconnectCore()
    {
        if (!_connected)
        {
            return;
        }

        _routing.Disconnect(_line);
        using (_lock.AcquireIrqSafe())
        {
            _trampoline = null;
            _connected = false;
        }
    }

    /// <summary>
    /// The platform's handler: copies the trampoline under the lock and
    /// runs it after the lock is released, never under it (the
    /// trampoline's fault path masks the handle, which reaches this
    /// source's lock again). Interrupt context; allocation-free.
    /// </summary>
    private void HandleIrq(ref IRQContext context)
    {
        InterruptTrampoline? trampoline;
        using (_lock.AcquireIrqSafe())
        {
            trampoline = _trampoline;
        }

        trampoline?.Invoke(synthetic: false);
    }
}
