// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Xhci;

/// <summary>
/// The DMA memory one open pipe needs: its transfer ring and its data
/// buffer (one page split into report buffers for an interrupt pipe, the
/// 64 KiB bounce of a bulk pipe), plus the completion event of a bulk pipe.
/// Allocated once on the host's binding, never freed early, and pooled by
/// <see cref="XhciState"/> per kind: a closed pipe's set goes back to its
/// free list and the next open takes it, resets the ring and puts a fresh
/// <see cref="XhciInterruptPipe"/> or <see cref="XhciBulkPipe"/> over it.
/// </summary>
internal sealed class XhciPipeMemory
{
    /// <summary>Takes the pages and the event the host allocated. Thread context.</summary>
    /// <param name="ring">One page: the transfer ring.</param>
    /// <param name="buffer">The data buffer: one page for an interrupt pipe, 64 KiB for a bulk pipe.</param>
    /// <param name="deviceEvent">The completion event of a bulk pipe; null for an interrupt pipe.</param>
    public XhciPipeMemory(DmaBuffer ring, DmaBuffer buffer, DeviceEvent? deviceEvent)
    {
        Ring = new XhciRing(ring);
        Buffer = buffer;
        BufferLength = buffer.Length;
        Event = deviceEvent;
    }

    /// <summary>The transfer ring; reset by every open.</summary>
    public XhciRing Ring { get; }

    /// <summary>The data buffer.</summary>
    public DmaBuffer Buffer { get; }

    /// <summary>Bytes in <see cref="Buffer"/>.</summary>
    public int BufferLength { get; }

    /// <summary>The event a bulk transfer's completion signals; null on an interrupt pipe's set.</summary>
    public DeviceEvent? Event { get; }
}
