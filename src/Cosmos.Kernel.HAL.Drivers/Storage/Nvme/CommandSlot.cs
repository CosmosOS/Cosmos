// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Storage.Nvme;

/// <summary>
/// One I/O command slot of an <see cref="NvmeController"/>: the command
/// identifier its commands carry, the page their data moves through, and
/// the completion the I/O completion queue reported for the last one. Each
/// slot owns its page, so commands on different slots, from different
/// namespaces or threads, are in flight at once without trampling one
/// shared page.
/// </summary>
internal sealed class CommandSlot
{
    // Written by whoever drains the completion queue, under the
    // controller's lock, and read by the thread waiting for the command
    // without it: the status first, then the flag, which publishes it.
    private uint _status;
    private volatile bool _done;

    /// <summary>The command identifier of the slot's commands, which is also its index.</summary>
    internal ushort CommandId { get; }

    /// <summary>The one page a command on the slot reads into or writes from.</summary>
    internal DmaBuffer BounceBuffer { get; }

    /// <summary>
    /// Signalled for each completion of the slot drained while the
    /// controller's interrupts are message-signaled. A signal can be left
    /// over from an earlier command, so a waiter goes by
    /// <see cref="IsDone"/>, never by the signal alone.
    /// </summary>
    internal DeviceEvent Completed { get; }

    /// <summary>
    /// True while a caller holds the slot, and for good once it is
    /// quarantined. Guarded by the controller's lock.
    /// </summary>
    internal bool InUse { get; set; }

    /// <summary>True once the completion queue reported the slot's current command.</summary>
    internal bool IsDone => _done;

    /// <summary>The status the completion reported, 0 for success. Read once <see cref="IsDone"/> is true.</summary>
    internal uint Status => _status;

    /// <summary>Creates slot <paramref name="commandId"/> over <paramref name="bounceBuffer"/>.</summary>
    internal CommandSlot(ushort commandId, DmaBuffer bounceBuffer, DeviceEvent completed)
    {
        CommandId = commandId;
        BounceBuffer = bounceBuffer;
        Completed = completed;
    }

    /// <summary>Forgets the previous command's completion, before a new one is submitted on the slot.</summary>
    internal void Reset()
    {
        _done = false;
        _status = 0;
    }

    /// <summary>Records the completion of the slot's command. Any context: it allocates nothing.</summary>
    internal void Complete(uint status)
    {
        _status = status;
        _done = true;
    }
}
