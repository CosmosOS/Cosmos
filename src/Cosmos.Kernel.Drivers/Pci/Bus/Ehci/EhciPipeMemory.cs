// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Resources;
using Cosmos.Kernel.HAL.DriverKit.Threading;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ehci;

/// <summary>
/// The DMA memory one open pipe needs: its queue head, its transfer
/// descriptor slots (one per buffer plus the dummy for an interrupt pipe,
/// the transfer and the dummy for a bulk pipe) and its data buffer (one
/// page split into report buffers for an interrupt pipe, the 64 KiB bounce
/// of a bulk pipe), plus the completion event of a bulk pipe. Allocated
/// once on the host's binding, never freed early, and pooled by
/// <see cref="EhciState"/> per kind: a closed pipe's set goes back to its
/// free list and the next open takes it, clears it and puts a fresh
/// <see cref="EhciInterruptPipe"/> or <see cref="EhciBulkPipe"/> over it.
/// A set whose queue head the controller may still hold after an
/// unacknowledged doorbell is retired instead of pooled.
/// </summary>
internal sealed class EhciPipeMemory
{
    /// <summary>Takes the slots, the buffer and the event the host allocated. Thread context.</summary>
    /// <param name="queueHead">The endpoint's queue head slot.</param>
    /// <param name="transfers">The transfer descriptor slots.</param>
    /// <param name="buffer">The data buffer: one page for an interrupt pipe, 64 KiB for a bulk pipe.</param>
    /// <param name="deviceEvent">The completion event of a bulk pipe; null for an interrupt pipe.</param>
    public EhciPipeMemory(EhciQueueHeadSlot queueHead, EhciDescriptorSlot[] transfers, DmaBuffer buffer, DeviceEvent? deviceEvent)
    {
        QueueHead = queueHead;
        Transfers = transfers;
        Buffer = buffer;
        BufferLength = buffer.Length;
        Event = deviceEvent;
    }

    /// <summary>The endpoint's queue head.</summary>
    public EhciQueueHeadSlot QueueHead { get; }

    /// <summary>The transfer descriptor slots.</summary>
    public EhciDescriptorSlot[] Transfers { get; }

    /// <summary>The data buffer.</summary>
    public DmaBuffer Buffer { get; }

    /// <summary>Bytes in <see cref="Buffer"/>.</summary>
    public int BufferLength { get; }

    /// <summary>The event a bulk transfer's completion signals; null on an interrupt pipe's set.</summary>
    public DeviceEvent? Event { get; }

    /// <summary>True for a bulk pipe's set, which carries an event.</summary>
    public bool IsBulk => Event is not null;

    /// <summary>True once the controller may still hold the queue head (an unacknowledged doorbell): the set is never pooled again. Under the lock.</summary>
    public bool Retired { get; set; }

    /// <summary>Zeroes every descriptor slot, for a set taken from the pool. Thread context, before the endpoint is built.</summary>
    public void Clear()
    {
        QueueHead.Slot.Clear();
        for (int i = 0; i < Transfers.Length; i++)
        {
            Transfers[i].Clear();
        }
    }
}
