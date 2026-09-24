// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Devices.Usb.Xhci;

/// <summary>
/// An open interrupt IN endpoint. Keeps one Normal TRB queued per buffer
/// so the controller always has somewhere to put the next report, and
/// tracks the endpoint's recovery after a transfer error.
/// </summary>
internal sealed unsafe class XhciInterruptPipe
{
    /// <summary>Transfers kept in flight; a keyboard needs one, a few absorb bursts between interrupts.</summary>
    private const int MaxBuffers = 8;

    private readonly UsbInterruptHandler _handler;
    private readonly byte* _buffers;
    private readonly ulong _buffersAddress;
    private readonly int _bufferSize;
    private readonly int _bufferCount;

    public byte EndpointId { get; }
    public byte EndpointAddress { get; }
    public XhciRing Ring { get; }
    public XhciPipeState State { get; set; }

    /// <summary>Command TRB of the recovery step in flight, 0 when none is.</summary>
    public ulong PendingCommand { get; set; }

    /// <summary>Errors since the last good transfer; recovery gives up past a limit.</summary>
    public int ConsecutiveErrors { get; set; }

    /// <summary>Buffer of the transfer that failed, re-queued when the endpoint turns out not to be halted.</summary>
    public ulong FailedBuffer { get; set; }

    /// <summary>The failure was a STALL, so the device halted its endpoint too.</summary>
    public bool StalledByDevice { get; set; }

    /// <param name="endpointId">Device Context Index of the endpoint.</param>
    /// <param name="endpointAddress">bEndpointAddress, used to clear a device-side halt.</param>
    /// <param name="transferSize">Bytes one service interval can deliver (Max ESIT Payload).</param>
    /// <param name="handler">Receives every completed transfer.</param>
    public XhciInterruptPipe(byte endpointId, byte endpointAddress, int transferSize, UsbInterruptHandler handler)
    {
        EndpointId = endpointId;
        EndpointAddress = endpointAddress;
        _handler = handler;
        _bufferSize = Math.Clamp(transferSize, 1, XhciDma.PageSize);
        _bufferCount = Math.Min(MaxBuffers, XhciDma.PageSize / _bufferSize);
        _buffers = XhciDma.AllocPages(1, out _buffersAddress);
        Ring = new XhciRing();
    }

    /// <summary>Queues every buffer. The caller holds the ring lock and rings the doorbell.</summary>
    public void QueueAll()
    {
        for (int i = 0; i < _bufferCount; i++)
        {
            Queue(_buffersAddress + ((ulong)i * (ulong)_bufferSize));
        }
    }

    /// <summary>Queues one buffer. The caller holds the ring lock and rings the doorbell.</summary>
    public void Queue(ulong bufferAddress) =>
        Ring.Enqueue(bufferAddress, (uint)_bufferSize,
            XhciTrb.TypeField(XhciTrbType.Normal) | XhciTrb.InterruptOnCompletion | XhciTrb.InterruptOnShortPacket);

    /// <summary>Finds the buffer of the TRB a Transfer Event points to.</summary>
    public bool TryGetBuffer(ulong trbAddress, out ulong bufferAddress)
    {
        bufferAddress = 0;
        if (!Ring.TryRead(trbAddress, out XhciTrb trb))
        {
            return false;
        }

        ulong end = _buffersAddress + ((ulong)_bufferCount * (ulong)_bufferSize);
        if (trb.Parameter < _buffersAddress || trb.Parameter >= end)
        {
            return false;
        }

        bufferAddress = trb.Parameter;
        return true;
    }

    /// <summary>Hands a completed buffer to the handler.</summary>
    /// <param name="bufferAddress">A buffer <see cref="TryGetBuffer"/> returned.</param>
    /// <param name="residualLength">Bytes the controller did not fill.</param>
    public void Deliver(ulong bufferAddress, uint residualLength)
    {
        int length = _bufferSize - (int)Math.Min(residualLength, (uint)_bufferSize);
        _handler(new ReadOnlySpan<byte>(_buffers + (bufferAddress - _buffersAddress), length));
    }

    public void Free()
    {
        XhciDma.Free(_buffers);
        Ring.Free();
    }
}

/// <summary>Where an interrupt pipe is in its lifecycle.</summary>
internal enum XhciPipeState
{
    Running,

    /// <summary>A Reset Endpoint command is in flight after a transfer error.</summary>
    ResettingEndpoint,

    /// <summary>A Set TR Dequeue Pointer command is in flight, moving past the abandoned transfers.</summary>
    SettingDequeuePointer,

    /// <summary>Recovery gave up; no transfers are queued any more.</summary>
    Stopped
}
