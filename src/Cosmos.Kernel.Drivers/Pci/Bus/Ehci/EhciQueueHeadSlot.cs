// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ehci;

/// <summary>
/// A queue head the driver built in a descriptor slot, with the software
/// side the schedule needs to unlink it again: which list it is on, so the
/// predecessor can be found. One object per queue head of a memory set,
/// created with the set and kept with it. <see cref="ListIndex"/> is
/// written under the controller's lock.
/// </summary>
internal sealed class EhciQueueHeadSlot
{
    /// <summary>The list index of a queue head that is on no list.</summary>
    public const int NoList = -1;

    /// <summary>The list index of the async schedule; the periodic lists take the interval head's index, 0 to 7.</summary>
    public const int AsyncList = EhciProtocol.IntervalHeadCount;

    /// <summary>Wraps a descriptor slot holding a queue head. Thread context.</summary>
    /// <param name="slot">The slot, zeroed or holding a queue head the driver wrote.</param>
    public EhciQueueHeadSlot(EhciDescriptorSlot slot)
    {
        Slot = slot;
    }

    /// <summary>The descriptor slot.</summary>
    public EhciDescriptorSlot Slot { get; }

    /// <summary>Physical address of the queue head, 32-byte aligned.</summary>
    public ulong PhysicalAddress => Slot.PhysicalAddress;

    /// <summary>The queue head's address as a horizontal link pointer: the 32-bit address with Typ 01b.</summary>
    public uint LinkPointer => (uint)Slot.PhysicalAddress | EhciProtocol.LinkTypeQueueHead;

    /// <summary>The queue head itself.</summary>
    public ref EhciQueueHead QueueHead => ref Slot.QueueHead;

    /// <summary>The list the queue head is linked into: <see cref="AsyncList"/>, an interval head's index, or <see cref="NoList"/>. Under the lock.</summary>
    public int ListIndex { get; set; } = NoList;

    /// <summary>True while the queue head is in one of the controller's lists. Under the lock.</summary>
    public bool IsLinked => ListIndex != NoList;
}
