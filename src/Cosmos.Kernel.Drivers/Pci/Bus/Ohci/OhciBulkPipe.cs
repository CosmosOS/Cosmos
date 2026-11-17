// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;
using Cosmos.Kernel.HAL.DriverKit.Threading;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// An open bulk endpoint. Its transfers are synchronous: the data goes
/// through the 64 KiB bounce buffer of its memory set one TD of at most
/// two pages at a time, and the drain records the completion here for
/// the thread waiting on it. A new object every open over a pooled
/// <see cref="OhciPipeMemory"/>. <see cref="Busy"/> and
/// <see cref="PendingIndex"/> are written under the controller's lock;
/// the completion fields by the drain before <see cref="Completed"/>.
/// </summary>
internal sealed class OhciBulkPipe : UsbPipe
{
    /// <summary>Set by the drain once the pending TD retired, after the code and the buffer pointer; cleared by the issuer under the lock. Read and written with <c>Volatile</c>.</summary>
    public bool Completed;

    /// <summary>Creates the pipe over its memory set. Thread context, the host's open path.</summary>
    /// <param name="endpoint">The bulk endpoint, either direction.</param>
    /// <param name="memory">The pooled ED, TD slots, bounce buffer and event, reset.</param>
    /// <param name="index">The endpoint's index in the device's pipe tables.</param>
    public OhciBulkPipe(UsbEndpoint endpoint, OhciPipeMemory memory, byte index)
        : base(endpoint)
    {
        Memory = memory;
        Index = index;
        PendingIndex = -1;
    }

    /// <summary>The pooled ED, TD slots, bounce buffer and event.</summary>
    public OhciPipeMemory Memory { get; }

    /// <summary>The endpoint's index in the device's pipe tables.</summary>
    public byte Index { get; }

    /// <summary>True while a transfer, a reset or a close owns the pipe. Under the lock.</summary>
    public bool Busy { get; set; }

    /// <summary>The event the transfer's completion signals.</summary>
    public DeviceEvent Event => Memory.Event ?? throw new InvalidOperationException("The bulk set carries no event.");

    /// <summary>The condition code of the pending TD; written before <see cref="Completed"/>.</summary>
    public OhciConditionCode CompletionCode { get; set; }

    /// <summary>The pending TD's final CBP: 0 when the buffer was done, else the first byte not moved.</summary>
    public uint CompletionBufferPointer { get; set; }

    /// <summary>TD slot index of the transfer in flight, -1 when none is. Under the lock.</summary>
    public int PendingIndex { get; set; }

    /// <summary>Marks the pipe closed for good; the host's close path, under the lock.</summary>
    internal void MarkClosed()
    {
        IsClosed = true;
    }
}
