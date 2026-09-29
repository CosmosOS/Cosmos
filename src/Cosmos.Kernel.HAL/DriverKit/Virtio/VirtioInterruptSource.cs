// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.DriverKit.Virtio;

/// <summary>
/// One virtual interrupt of a virtio node: the configuration change or one
/// queue, multiplexed over the transport's interrupt entries by the
/// <see cref="VirtioAccess"/>. The source has an entry from the moment the
/// access assigned one (the configuration source at the handshake start, a
/// queue source when its queue was created) and none (-1) after a reset; a
/// connect is refused without one. The transport's entry is shared with the
/// other sources of the node, so a mask drops deliveries at the kit, not at
/// the controller. Connection and mask state live under one IRQ-safe lock;
/// the trampoline is copied under it and invoked after it is released
/// (its fault path masks the handle, which comes back to the same lock).
/// </summary>
internal sealed class VirtioInterruptSource : InterruptSource
{
    /// <summary>The queue index that stands for the configuration change source.</summary>
    private const int ConfigKind = -1;

    /// <summary>The entry of a source nothing is assigned to.</summary>
    internal const int NoEntry = -1;

    private readonly int _kind;
    private volatile int _entry = NoEntry;
    private SchedSpinLock _lock;
    private InterruptTrampoline? _trampoline;
    private bool _masked;

    /// <summary>A source of the given kind: <see cref="ConfigKind"/> or a queue index.</summary>
    private VirtioInterruptSource(int kind)
    {
        _kind = kind;
    }

    /// <summary>The transport entry this source is raised on, or <see cref="NoEntry"/>. Any context.</summary>
    internal int Entry => _entry;

    /// <summary>The configuration change source of a node.</summary>
    internal static VirtioInterruptSource ForConfig() => new(ConfigKind);

    /// <summary>The source of queue <paramref name="index"/>.</summary>
    /// <param name="index">The queue index.</param>
    internal static VirtioInterruptSource ForQueue(int index) => new(index);

    /// <inheritdoc/>
    public override string Describe() => _kind == ConfigKind ? "config" : $"queue {_kind}";

    /// <summary>Gives the source an entry, or takes it away with <see cref="NoEntry"/>. Thread context.</summary>
    /// <param name="entry">The transport entry, or <see cref="NoEntry"/>.</param>
    internal void AssignEntry(int entry) => _entry = entry;

    /// <summary>
    /// The access's guard for a trampoline a declined probe left behind:
    /// masks the source so a raise between the handshake restart and the
    /// unwind is dropped at the kit. The unwind then disconnects the
    /// trampoline, and the next connect clears the mask. Thread context.
    /// </summary>
    internal void Quiet()
    {
        using (_lock.AcquireIrqSafe())
        {
            _masked = true;
        }
    }

    /// <summary>
    /// Delivers one interrupt to the connected handler: nothing while
    /// masked or unconnected. Interrupt context; allocation-free. The
    /// trampoline is copied under the lock and invoked after it is
    /// released: its fault path masks the handle, which comes back to this
    /// lock, and the kit's spin lock is not reentrant.
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
            if (_entry == NoEntry || _trampoline is not null)
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
