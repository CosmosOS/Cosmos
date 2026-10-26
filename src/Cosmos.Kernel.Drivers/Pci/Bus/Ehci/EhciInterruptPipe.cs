// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ehci;

/// <summary>
/// An open interrupt IN endpoint. Keeps one qTD queued per buffer on its
/// periodic queue head so the controller always has somewhere to put the
/// next report, hands every completed one to its
/// <see cref="UsbReportHandler"/>, and records the error that sends it to
/// the hot-plug thread for recovery. The qTD slots form a linear chain
/// ended by an inactive dummy: <see cref="Head"/> is the slot the
/// controller completes next and <see cref="Dummy"/> the one it idles on;
/// a completed slot becomes the new dummy and the old dummy is refilled
/// with the completed buffer, so the chain rotates through the slots. A
/// new object every open over a pooled <see cref="EhciPipeMemory"/>: the
/// endpoint, the handler and the id are fixed at construction and
/// <see cref="UsbPipe.IsClosed"/> is one-way. Its fields are written under
/// the controller's lock or in its drain.
/// </summary>
internal sealed class EhciInterruptPipe : UsbPipe
{
    private readonly UsbReportHandler _handler;
    private readonly int _bufferSize;
    private readonly int _bufferCount;
    private readonly int[] _slotBuffers;

    /// <summary>Creates the pipe over its memory set. Thread context, the host's open path.</summary>
    /// <param name="device">The device the endpoint belongs to.</param>
    /// <param name="endpoint">The interrupt IN endpoint.</param>
    /// <param name="handler">Receives every completed transfer.</param>
    /// <param name="memory">The pooled queue head, qTD slots and buffer page, cleared.</param>
    /// <param name="endpointId">The endpoint's id, <c>number * 2 + 1</c>.</param>
    public EhciInterruptPipe(EhciDevice device, UsbEndpoint endpoint, UsbReportHandler handler, EhciPipeMemory memory, byte endpointId)
        : base(endpoint)
    {
        _handler = handler;
        Device = device;
        Memory = memory;
        EndpointId = endpointId;

        // For a high-speed periodic endpoint, one service interval delivers
        // up to AdditionalTransactions + 1 packets, at most the Mult the
        // queue head carries: one buffer holds them all.
        int transactions = Math.Min(endpoint.AdditionalTransactions + 1, (int)EhciProtocol.QhMultMax);
        int transferSize = endpoint.MaxPacketSize * transactions;
        _bufferSize = Math.Clamp(transferSize, 1, memory.BufferLength);
        _bufferCount = Math.Min(EhciProtocol.MaxBuffers, memory.BufferLength / _bufferSize);
        _slotBuffers = new int[memory.Transfers.Length];
    }

    /// <summary>The device the pipe was opened on.</summary>
    public EhciDevice Device { get; }

    /// <summary>The pooled queue head, qTD slots and buffer page.</summary>
    public EhciPipeMemory Memory { get; }

    /// <summary>The endpoint's id.</summary>
    public byte EndpointId { get; }

    /// <summary>Bytes one service interval can deliver: the size of every buffer and of every qTD.</summary>
    public int TransferSize => _bufferSize;

    /// <summary>Buffers, and so qTDs, kept in flight.</summary>
    public int BufferCount => _bufferCount;

    /// <summary>Where the pipe is in its lifecycle.</summary>
    public EhciPipeState State { get; set; }

    /// <summary>The interval head the queue head is linked behind, 0 to 7: a period of 2^interval frames, fixed at open.</summary>
    public int Interval { get; set; }

    /// <summary>Index in the memory set of the qTD slot the controller completes next.</summary>
    public int Head { get; set; }

    /// <summary>Index in the memory set of the inactive dummy qTD the chain ends with.</summary>
    public int Dummy { get; set; }

    /// <summary>Errors since the last good transfer; recovery gives up past a limit.</summary>
    public int ConsecutiveErrors { get; set; }

    /// <summary>The failure was a STALL, so the device halted its endpoint too and needs CLEAR_FEATURE.</summary>
    public bool StalledByDevice { get; set; }

    /// <summary>Marks the pipe closed for good; the host's close path, under the lock.</summary>
    internal void MarkClosed()
    {
        IsClosed = true;
    }

    /// <summary>The buffer index the qTD in <paramref name="slot"/> carries.</summary>
    /// <param name="slot">A qTD slot index of the memory set.</param>
    public int BufferOf(int slot)
    {
        return _slotBuffers[slot];
    }

    /// <summary>Records which buffer the qTD in <paramref name="slot"/> carries.</summary>
    /// <param name="slot">A qTD slot index of the memory set.</param>
    /// <param name="buffer">A buffer index below <see cref="BufferCount"/>.</param>
    public void SetBufferOf(int slot, int buffer)
    {
        _slotBuffers[slot] = buffer;
    }

    /// <summary>Physical address of a buffer in the page.</summary>
    /// <param name="buffer">A buffer index below <see cref="BufferCount"/>.</param>
    public ulong BufferAddress(int buffer)
    {
        return Memory.Buffer.PhysicalAddress + ((ulong)buffer * (ulong)_bufferSize);
    }

    /// <summary>Hands a completed buffer to the handler. Any context, allocation-free; the span is valid for the call.</summary>
    /// <param name="buffer">A buffer index below <see cref="BufferCount"/>.</param>
    /// <param name="length">Bytes the device sent.</param>
    public void Deliver(int buffer, int length)
    {
        _handler(Memory.Buffer.Span.Slice(buffer * _bufferSize, length));
    }
}
