// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// An open interrupt IN endpoint. Keeps one TD queued per buffer on its ED
/// in the interrupt tree so the controller always has somewhere to put
/// the next report, hands every completed one to its
/// <see cref="UsbReportHandler"/>, and tracks the endpoint's recovery
/// after a transfer error. A new object every open over a pooled
/// <see cref="OhciPipeMemory"/>: the endpoint, the handler and the index
/// are fixed at construction and <see cref="UsbPipe.IsClosed"/> is
/// one-way. Its fields are written under the controller's lock or in its
/// drain.
/// </summary>
internal sealed class OhciInterruptPipe : UsbPipe
{
    private readonly UsbReportHandler _handler;
    private readonly int _bufferSize;
    private readonly int _bufferCount;

    /// <summary>Creates the pipe over its memory set. Thread context, the host's open path.</summary>
    /// <param name="endpoint">The interrupt IN endpoint.</param>
    /// <param name="handler">Receives every completed transfer.</param>
    /// <param name="memory">The pooled ED, TD slots and buffer page, reset.</param>
    /// <param name="index">The endpoint's index in the device's pipe tables.</param>
    /// <param name="intervalExponent">The interval list the ED sits in: period 2^n frames.</param>
    public OhciInterruptPipe(UsbEndpoint endpoint, UsbReportHandler handler, OhciPipeMemory memory, byte index, int intervalExponent)
        : base(endpoint)
    {
        _handler = handler;
        Memory = memory;
        Index = index;
        IntervalExponent = intervalExponent;

        // One buffer holds what one service interval can deliver; at full
        // speed that is one packet.
        int transferSize = endpoint.MaxPacketSize * (endpoint.AdditionalTransactions + 1);
        _bufferSize = Math.Clamp(transferSize, 1, memory.BufferLength);
        _bufferCount = Math.Min(OhciProtocol.MaxBuffers, memory.BufferLength / _bufferSize);
        TdBuffer = new int[memory.TdSlots.Length];
        for (int i = 0; i < TdBuffer.Length; i++)
        {
            TdBuffer[i] = -1;
        }
    }

    /// <summary>The pooled ED, TD slots and buffer page.</summary>
    public OhciPipeMemory Memory { get; }

    /// <summary>The endpoint's index in the device's pipe tables.</summary>
    public byte Index { get; }

    /// <summary>The interval list the ED sits in: period 2^n frames.</summary>
    public int IntervalExponent { get; }

    /// <summary>Bytes one service interval can deliver: the size of every buffer and of every TD.</summary>
    public int TransferSize => _bufferSize;

    /// <summary>How many buffers, and so TDs, the pipe keeps queued.</summary>
    public int BufferCount => _bufferCount;

    /// <summary>Where the pipe is in its lifecycle.</summary>
    public OhciPipeState State { get; set; }

    /// <summary>True while a recovery or a close owns the pipe's descriptors. Under the lock.</summary>
    public bool Busy { get; set; }

    /// <summary>Errors since the last good transfer; recovery gives up past a limit.</summary>
    public int ConsecutiveErrors { get; set; }

    /// <summary>The failure was a STALL, so the device halted its endpoint too and needs CLEAR_FEATURE.</summary>
    public bool StalledByDevice { get; set; }

    /// <summary>The buffer each TD slot of the set carries, -1 for the dummy and the free ones.</summary>
    public int[] TdBuffer { get; }

    /// <summary>Marks the pipe closed for good; the host's close path, under the lock.</summary>
    internal void MarkClosed()
    {
        IsClosed = true;
    }

    /// <summary>Physical address of buffer <paramref name="bufferIndex"/>. Any context.</summary>
    public uint BufferAddress(int bufferIndex)
    {
        return (uint)(Memory.Buffer.PhysicalAddress + ((ulong)bufferIndex * (ulong)_bufferSize));
    }

    /// <summary>Hands a completed buffer to the handler. Any context, allocation-free; the span is valid for the call.</summary>
    /// <param name="bufferIndex">The buffer the retired TD carried.</param>
    /// <param name="length">Bytes the device sent.</param>
    public void Deliver(int bufferIndex, int length)
    {
        int offset = bufferIndex * _bufferSize;
        _handler(Memory.Buffer.Span.Slice(offset, Math.Clamp(length, 0, _bufferSize)));
    }
}
