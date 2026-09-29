// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.DriverKit.Pci;

/// <summary>
/// One message interrupt of a PCI function: entry <c>index</c> of the
/// MSI-X table the function advertises, programmed through the function's
/// <see cref="PciMessageTable"/>. Connection state lives under one
/// IRQ-safe lock; the adapter from the platform's handler shape to the
/// kit's trampoline is allocated once, at the first connect, in thread
/// context. The trampoline is stored before the table is asked, so a
/// message that arrives as soon as the entry is unmasked finds it, and
/// dropped again when the table refuses.
/// </summary>
internal sealed class PciMessageInterruptSource : InterruptSource
{
    private readonly PciMessageTable _table;
    private readonly int _index;
    private SchedSpinLock _lock;
    private InterruptTrampoline? _trampoline;
    private InterruptManager.IrqDelegate? _adapter;
    private volatile bool _connected;

    /// <summary>The source for entry <paramref name="index"/> of <paramref name="table"/>.</summary>
    /// <param name="table">The function's table.</param>
    /// <param name="index">The entry, 0 to <see cref="PciMessageTable.EntryCount"/> - 1.</param>
    internal PciMessageInterruptSource(PciMessageTable table, int index)
    {
        _table = table;
        _index = index;
    }

    /// <inheritdoc/>
    public override string Describe() => $"message {_index} of {_table.EntryCount}";

    /// <inheritdoc/>
    protected override bool TryConnectCore(InterruptTrampoline trampoline)
    {
        _adapter ??= HandleMessage;
        using (_lock.AcquireIrqSafe())
        {
            if (_connected)
            {
                return false;
            }

            _trampoline = trampoline;
            _connected = true;
        }

        if (_table.TryConnect(_index, _adapter))
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
            _table.Mask(_index);
        }
    }

    /// <inheritdoc/>
    protected override void UnmaskCore()
    {
        if (_connected)
        {
            _table.Unmask(_index);
        }
    }

    /// <inheritdoc/>
    protected override void DisconnectCore()
    {
        if (!_connected)
        {
            return;
        }

        _table.Disconnect(_index);
        using (_lock.AcquireIrqSafe())
        {
            _trampoline = null;
            _connected = false;
        }
    }

    /// <summary>The platform's handler: copies the trampoline under the lock and runs it after. Interrupt context; allocation-free.</summary>
    private void HandleMessage(ref IRQContext context)
    {
        InterruptTrampoline? trampoline;
        using (_lock.AcquireIrqSafe())
        {
            trampoline = _trampoline;
        }

        trampoline?.Invoke(synthetic: false);
    }
}
