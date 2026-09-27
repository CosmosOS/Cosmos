// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Rings;
using Cosmos.Kernel.HAL.Drivers.Usb;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Pipes;

/// <summary>
/// An open interrupt IN endpoint. Keeps one Normal TRB queued per buffer
/// so the controller always has somewhere to put the next report, and
/// tracks the endpoint's recovery after a transfer error. Everything but
/// its construction runs under the controller's event lock, in interrupt
/// context or on a thread draining the events.
/// </summary>
internal sealed class InterruptPipe
{
    /// <summary>Transfers kept in flight; a keyboard needs one, a few absorb bursts between interrupts.</summary>
    private const int MaxBuffers = 8;

    private readonly UsbReportHandler _handler;
    private readonly DmaBuffer _buffers;
    private readonly int _bufferSize;
    private readonly int _bufferCount;

    internal byte EndpointId { get; }
    internal byte EndpointAddress { get; }
    internal ProducerRing Ring { get; }
    internal InterruptPipeState State { get; set; }

    /// <summary>Command TRB of the recovery step in flight, 0 when none is.</summary>
    internal ulong PendingCommand { get; set; }

    /// <summary>Errors since the last good transfer; recovery gives up past a limit.</summary>
    internal int ConsecutiveErrors { get; set; }

    /// <summary>Buffer of the transfer that failed, re-queued when the endpoint turns out not to be halted.</summary>
    internal ulong FailedBuffer { get; set; }

    /// <summary>The failure was a STALL, so the device halted its endpoint too.</summary>
    internal bool StalledByDevice { get; set; }

    private InterruptPipe(byte endpointId, byte endpointAddress, int transferSize, UsbReportHandler handler, DmaBuffer buffers, ProducerRing ring)
    {
        EndpointId = endpointId;
        EndpointAddress = endpointAddress;
        _handler = handler;
        _bufferSize = Math.Clamp(transferSize, 1, XhciMemory.PageSize);
        _bufferCount = Math.Min(MaxBuffers, XhciMemory.PageSize / _bufferSize);
        _buffers = buffers;
        Ring = ring;
    }

    /// <summary>Allocates the pipe's buffers and ring. Thread context.</summary>
    /// <param name="endpointId">Device Context Index of the endpoint.</param>
    /// <param name="endpointAddress">bEndpointAddress, used to clear a device-side halt.</param>
    /// <param name="transferSize">Bytes one service interval can deliver (Max ESIT Payload).</param>
    /// <param name="handler">Receives every completed transfer.</param>
    /// <param name="memory">Where the pages come from.</param>
    /// <exception cref="InvalidOperationException">No DMA memory the controller can reach is free; nothing is kept.</exception>
    internal static InterruptPipe Create(byte endpointId, byte endpointAddress, int transferSize, UsbReportHandler handler, XhciMemory memory)
    {
        DmaBuffer buffers = memory.Allocate(1);
        try
        {
            return new InterruptPipe(endpointId, endpointAddress, transferSize, handler, buffers, new ProducerRing(memory));
        }
        catch (InvalidOperationException)
        {
            memory.Free(buffers);
            throw;
        }
    }

    /// <summary>Queues every buffer. The caller holds the ring lock and rings the doorbell.</summary>
    internal void QueueAll()
    {
        for (int i = 0; i < _bufferCount; i++)
        {
            Queue(_buffers.DeviceAddress + ((ulong)i * (ulong)_bufferSize));
        }
    }

    /// <summary>Queues one buffer. The caller holds the ring lock and rings the doorbell.</summary>
    internal void Queue(ulong bufferAddress) =>
        Ring.Enqueue(bufferAddress, (uint)_bufferSize,
            Trb.TypeField(TrbType.Normal) | Trb.InterruptOnCompletion | Trb.InterruptOnShortPacket);

    /// <summary>Finds the buffer of the TRB a Transfer Event points to.</summary>
    internal bool TryGetBuffer(ulong trbAddress, out ulong bufferAddress)
    {
        bufferAddress = 0;
        if (!Ring.TryRead(trbAddress, out Trb trb))
        {
            return false;
        }

        ulong end = _buffers.DeviceAddress + ((ulong)_bufferCount * (ulong)_bufferSize);
        if (trb.Parameter < _buffers.DeviceAddress || trb.Parameter >= end)
        {
            return false;
        }

        bufferAddress = trb.Parameter;
        return true;
    }

    /// <summary>Hands a completed buffer to the handler.</summary>
    /// <param name="bufferAddress">A buffer <see cref="TryGetBuffer"/> returned.</param>
    /// <param name="residualLength">Bytes the controller did not fill.</param>
    internal void Deliver(ulong bufferAddress, uint residualLength)
    {
        int length = _bufferSize - (int)Math.Min(residualLength, (uint)_bufferSize);
        int offset = (int)(bufferAddress - _buffers.DeviceAddress);
        _handler(_buffers.Span.Slice(offset, length));
    }

    /// <summary>Gives the pipe's pages back, once the controller no longer references them.</summary>
    internal void Free(XhciMemory memory)
    {
        memory.Free(_buffers);
        Ring.Free(memory);
    }
}
