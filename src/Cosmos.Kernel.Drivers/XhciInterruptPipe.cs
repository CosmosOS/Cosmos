// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Usb;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// An open interrupt IN endpoint. Keeps one Normal TRB queued per buffer so
/// the controller always has somewhere to put the next report, hands every
/// completed one to its <see cref="UsbReportHandler"/>, and tracks the
/// endpoint's recovery after a transfer error. A new object every open over
/// a pooled <see cref="XhciPipeMemory"/>: the endpoint, the handler and the
/// DCI are fixed at construction and <see cref="UsbPipe.IsClosed"/> is
/// one-way. Its fields are written under the controller's lock or in its
/// handler.
/// </summary>
internal sealed class XhciInterruptPipe : UsbPipe
{
    private readonly UsbReportHandler _handler;
    private readonly int _bufferSize;
    private readonly int _bufferCount;

    /// <summary>Creates the pipe over its memory set. Thread context, the host's open path.</summary>
    /// <param name="endpoint">The interrupt IN endpoint.</param>
    /// <param name="handler">Receives every completed transfer.</param>
    /// <param name="memory">The pooled ring and buffer page, the ring reset.</param>
    /// <param name="dci">The endpoint's Device Context Index.</param>
    public XhciInterruptPipe(UsbEndpoint endpoint, UsbReportHandler handler, XhciPipeMemory memory, byte dci)
        : base(endpoint)
    {
        _handler = handler;
        Memory = memory;
        EndpointId = dci;

        // For a high-speed periodic endpoint, Max Burst is the number of
        // additional transactions per microframe (xHCI 1.2 section 6.2.3.4):
        // one buffer holds what one service interval can deliver.
        int transferSize = endpoint.MaxPacketSize * (endpoint.AdditionalTransactions + 1);
        _bufferSize = Math.Clamp(transferSize, 1, memory.BufferLength);
        _bufferCount = Math.Min(XhciProtocol.MaxBuffers, memory.BufferLength / _bufferSize);
    }

    /// <summary>The pooled ring and buffer page.</summary>
    public XhciPipeMemory Memory { get; }

    /// <summary>The endpoint's Device Context Index.</summary>
    public byte EndpointId { get; }

    /// <summary>Bytes one service interval can deliver: the size of every buffer and of every TRB.</summary>
    public int TransferSize => _bufferSize;

    /// <summary>Where the pipe is in its lifecycle.</summary>
    public XhciPipeState State { get; set; }

    /// <summary>Command TRB of the recovery step in flight, 0 when none is.</summary>
    public ulong PendingCommand { get; set; }

    /// <summary>Errors since the last good transfer; recovery gives up past a limit.</summary>
    public int ConsecutiveErrors { get; set; }

    /// <summary>Buffer of the transfer that failed, re-queued when the endpoint turns out not to be halted.</summary>
    public ulong FailedBuffer { get; set; }

    /// <summary>The failure was a STALL, so the device halted its endpoint too and needs CLEAR_FEATURE.</summary>
    public bool StalledByDevice { get; set; }

    /// <summary>The recovery reached its CLEAR_FEATURE step while another owner held the slot's control ring; the owner's hand-off check sends it.</summary>
    public bool PendingClearHalt { get; set; }

    /// <summary>Marks the pipe closed for good; the host's close path, under the lock.</summary>
    internal void MarkClosed() => IsClosed = true;

    /// <summary>Queues every buffer. Under the lock or in the handler; the caller rings the doorbell.</summary>
    public void QueueAll()
    {
        for (int i = 0; i < _bufferCount; i++)
        {
            Queue(Memory.Buffer.PhysicalAddress + ((ulong)i * (ulong)_bufferSize));
        }
    }

    /// <summary>Queues one buffer as a Normal TRB that interrupts on completion and on a short packet. Under the lock or in the handler; the caller rings the doorbell.</summary>
    /// <param name="bufferAddress">A buffer's physical address.</param>
    public void Queue(ulong bufferAddress) =>
        Memory.Ring.Enqueue(bufferAddress, (uint)_bufferSize,
            XhciTrb.TypeField(XhciTrbType.Normal) | XhciTrb.InterruptOnCompletion | XhciTrb.InterruptOnShortPacket);

    /// <summary>Finds the buffer of the TRB a Transfer Event points to. Any context, allocation-free.</summary>
    /// <param name="trbAddress">The event's TRB pointer.</param>
    /// <param name="bufferAddress">The buffer that TRB named.</param>
    public bool TryGetBuffer(ulong trbAddress, out ulong bufferAddress)
    {
        bufferAddress = 0;
        if (!Memory.Ring.TryRead(trbAddress, out XhciTrb trb))
        {
            return false;
        }

        ulong start = Memory.Buffer.PhysicalAddress;
        ulong end = start + ((ulong)_bufferCount * (ulong)_bufferSize);
        if (trb.Parameter < start || trb.Parameter >= end)
        {
            return false;
        }

        bufferAddress = trb.Parameter;
        return true;
    }

    /// <summary>Hands a completed buffer to the handler. Any context, allocation-free; the span is valid for the call.</summary>
    /// <param name="bufferAddress">A buffer <see cref="TryGetBuffer"/> returned.</param>
    /// <param name="residualLength">Bytes the controller did not fill.</param>
    public void Deliver(ulong bufferAddress, uint residualLength)
    {
        int length = _bufferSize - (int)Math.Min(residualLength, (uint)_bufferSize);
        int offset = (int)(bufferAddress - Memory.Buffer.PhysicalAddress);
        _handler(Memory.Buffer.Span.Slice(offset, length));
    }
}
