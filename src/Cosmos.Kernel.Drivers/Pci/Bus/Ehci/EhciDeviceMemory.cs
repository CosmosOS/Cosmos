// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Resources;
using Cosmos.Kernel.HAL.DriverKit.Threading;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ehci;

/// <summary>
/// The DMA memory one addressed device needs, and the event its control
/// transfers wait on: the default control endpoint's queue head, four
/// transfer descriptor slots that rotate through SETUP, DATA, STATUS and
/// the dummy the overlay idles on, one slot holding the 8-byte SETUP
/// packet, and a bounce page for the data stage, so a 4096-byte stage fits
/// in one page. Allocated once on the host's binding, never freed early
/// (the kit frees every DMA page in the binding's teardown alone), and
/// pooled by <see cref="EhciState"/>: a released set goes back to the free
/// list and the next address takes it, with a fresh <see cref="EhciDevice"/>
/// over it. A set whose queue head the controller may still hold after an
/// unacknowledged doorbell is retired instead of pooled.
/// </summary>
internal sealed class EhciDeviceMemory
{
    /// <summary>Takes the slots, the page and the event the host allocated. Thread context.</summary>
    /// <param name="queueHead">The control endpoint's queue head slot.</param>
    /// <param name="transfers">Four transfer descriptor slots.</param>
    /// <param name="setup">The slot the SETUP packet is written into.</param>
    /// <param name="buffer">One page: the data stage bounce buffer.</param>
    /// <param name="controlEvent">The event the control transfer completion signals.</param>
    public EhciDeviceMemory(EhciQueueHeadSlot queueHead, EhciDescriptorSlot[] transfers, EhciDescriptorSlot setup, DmaBuffer buffer, DeviceEvent controlEvent)
    {
        QueueHead = queueHead;
        Transfers = transfers;
        Setup = setup;
        Buffer = buffer;
        ControlEvent = controlEvent;
    }

    /// <summary>The default control endpoint's queue head.</summary>
    public EhciQueueHeadSlot QueueHead { get; }

    /// <summary>The rotating transfer descriptor slots: a chain of up to three stages followed by the dummy.</summary>
    public EhciDescriptorSlot[] Transfers { get; }

    /// <summary>The slot holding the SETUP packet, outside the bounce page.</summary>
    public EhciDescriptorSlot Setup { get; }

    /// <summary>The bounce page of a control transfer's data stage.</summary>
    public DmaBuffer Buffer { get; }

    /// <summary>The event a control transfer's completion signals, created once per set.</summary>
    public DeviceEvent ControlEvent { get; }

    /// <summary>Index in <see cref="Transfers"/> of the inactive dummy the queue head's overlay points at. Under the lock.</summary>
    public int DummyIndex { get; set; }

    /// <summary>True once the controller may still hold the queue head (an unacknowledged doorbell): the set is never pooled again. Under the lock.</summary>
    public bool Retired { get; set; }

    /// <summary>Zeroes every descriptor slot and the bounce page and starts the rotation at slot 0, for a set taken from the pool. Thread context, before the device is addressed.</summary>
    public void Clear()
    {
        QueueHead.Slot.Clear();
        for (int i = 0; i < Transfers.Length; i++)
        {
            Transfers[i].Clear();
        }

        Setup.Clear();
        Buffer.Span.Clear();
        DummyIndex = 0;
    }
}
