// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Usb;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Rings;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Pipes;

/// <summary>
/// An open bulk endpoint. Its transfers are synchronous: the data goes
/// through a DMA bounce buffer one Normal TRB at a time, and the event
/// handler records the completion here for the thread waiting on it. The
/// kit's USB core runs one transfer or reset on it at a time, which is what
/// lets them share the buffer and the completion state.
/// </summary>
internal sealed class BulkPipe
{
    /// <summary>Bounce buffer size in pages; a transfer larger than the part of it one TRB can cover is split.</summary>
    private const int BufferPages = 16;

    /// <summary>A TRB's data buffer must not cross a 64 KiB boundary (xHCI 1.2 §6.4.1.1).</summary>
    private const ulong TrbBufferBoundary = 64 * 1024;

    private readonly DmaBuffer _pages;

    /// <summary>Offset in <see cref="_pages"/> of the part one TRB covers.</summary>
    private readonly int _bufferOffset;

    internal byte EndpointId { get; }
    internal UsbEndpointInfo Endpoint { get; }
    internal ProducerRing Ring { get; }

    /// <summary>The part of the bounce buffer one TRB covers: whole pages, so a multiple of any packet size.</summary>
    internal Span<byte> Buffer => _pages.Span.Slice(_bufferOffset, BufferLength);

    internal ulong BufferAddress { get; }
    internal int BufferLength { get; }

    // Completion of the transfer in flight, under the controller's event lock.

    /// <summary>The TRB of the transfer in flight, 0 when none is.</summary>
    internal ulong PendingTrb { get; set; }

    internal bool Completed { get; set; }
    internal CompletionCode CompletionCode { get; set; }

    /// <summary>Bytes of the pending TRB the controller did not transfer.</summary>
    internal uint ResidualLength { get; set; }

    private BulkPipe(byte endpointId, UsbEndpointInfo endpoint, DmaBuffer pages, ProducerRing ring)
    {
        EndpointId = endpointId;
        Endpoint = endpoint;
        _pages = pages;
        Ring = ring;

        // The pages are page aligned but not 64 KiB aligned, so they may
        // straddle one boundary: one TRB covers the longer side of it. When
        // they are aligned, the boundary is their end and they all count.
        ulong start = pages.DeviceAddress;
        ulong end = start + ((ulong)BufferPages * XhciMemory.PageSize);
        ulong boundary = ((start / TrbBufferBoundary) + 1) * TrbBufferBoundary;
        bool afterBoundary = end - boundary > boundary - start;
        ulong bufferStart = afterBoundary ? boundary : start;
        _bufferOffset = (int)(bufferStart - start);
        BufferAddress = bufferStart;
        BufferLength = (int)((afterBoundary ? end : boundary) - bufferStart);
    }

    /// <summary>Allocates the pipe's bounce buffer and ring. Thread context.</summary>
    /// <param name="endpointId">Device Context Index of the endpoint.</param>
    /// <param name="endpoint">The endpoint descriptor.</param>
    /// <param name="memory">Where the pages come from.</param>
    /// <exception cref="InvalidOperationException">No DMA memory the controller can reach is free; nothing is kept.</exception>
    internal static BulkPipe Create(byte endpointId, UsbEndpointInfo endpoint, XhciMemory memory)
    {
        DmaBuffer pages = memory.Allocate(BufferPages);
        try
        {
            return new BulkPipe(endpointId, endpoint, pages, new ProducerRing(memory));
        }
        catch (InvalidOperationException)
        {
            memory.Free(pages);
            throw;
        }
    }

    /// <summary>Gives the pipe's pages back, once the controller no longer references them.</summary>
    internal void Free(XhciMemory memory)
    {
        memory.Free(_pages);
        Ring.Free(memory);
    }
}
