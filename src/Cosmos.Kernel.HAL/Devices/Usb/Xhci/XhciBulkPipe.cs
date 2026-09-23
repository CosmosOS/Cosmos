// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using SchedMutex = Cosmos.Kernel.Core.Scheduler.Mutex;

namespace Cosmos.Kernel.HAL.Devices.Usb.Xhci;

/// <summary>
/// An open bulk endpoint. Its transfers are synchronous: the data goes
/// through a DMA bounce buffer one Normal TRB at a time, and the event
/// handler records the completion here for the thread waiting on it.
/// </summary>
internal sealed unsafe class XhciBulkPipe
{
    /// <summary>Bounce buffer size in pages; a transfer larger than the part of it one TRB can cover is split.</summary>
    private const int BufferPages = 16;

    /// <summary>A TRB's data buffer must not cross a 64 KiB boundary (xHCI 1.2 §6.4.1.1).</summary>
    private const ulong TrbBufferBoundary = 64 * 1024;

    private readonly byte* _pages;

    /// <param name="endpointId">Device Context Index of the endpoint.</param>
    /// <param name="endpoint">The endpoint descriptor.</param>
    public XhciBulkPipe(byte endpointId, UsbEndpoint endpoint)
    {
        EndpointId = endpointId;
        Endpoint = endpoint;
        _pages = XhciDma.AllocPages(BufferPages, out ulong pagesAddress);

        // The pages are page aligned but not 64 KiB aligned, so they may
        // straddle one boundary: one TRB covers the longer side of it. When
        // they are aligned, the boundary is their end and they all count.
        ulong end = pagesAddress + ((ulong)BufferPages * (ulong)XhciDma.PageSize);
        ulong boundary = ((pagesAddress / TrbBufferBoundary) + 1) * TrbBufferBoundary;
        bool afterBoundary = end - boundary > boundary - pagesAddress;
        ulong start = afterBoundary ? boundary : pagesAddress;
        Buffer = _pages + (start - pagesAddress);
        BufferAddress = start;
        BufferLength = (int)((afterBoundary ? end : boundary) - start);
        Ring = new XhciRing();
    }

    public byte EndpointId { get; }
    public UsbEndpoint Endpoint { get; }
    public XhciRing Ring { get; }

    /// <summary>The part of the bounce buffer one TRB covers: whole pages, so a multiple of any packet size.</summary>
    public byte* Buffer { get; }

    public ulong BufferAddress { get; }
    public int BufferLength { get; }

    /// <summary>Serializes the transfers of this pipe, which share the buffer and the completion state below.</summary>
    public SchedMutex Mutex { get; } = new();

    // Completion of the transfer in flight, under the controller's event lock.

    /// <summary>The TRB of the transfer in flight, 0 when none is.</summary>
    public ulong PendingTrb { get; set; }

    public bool Completed { get; set; }
    public XhciCompletionCode CompletionCode { get; set; }

    /// <summary>Bytes of the pending TRB the controller did not transfer.</summary>
    public uint ResidualLength { get; set; }

    public void Free()
    {
        XhciDma.Free(_pages);
        Ring.Free();
    }
}
