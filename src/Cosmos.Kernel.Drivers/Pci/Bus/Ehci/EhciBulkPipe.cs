// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;
using Cosmos.Kernel.HAL.DriverKit.Threading;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ehci;

/// <summary>
/// An open bulk endpoint. Its transfers are synchronous: the data goes
/// through the 64 KiB bounce buffer of its memory set one qTD of at most
/// five pages at a time (the bounce is 64 KiB aligned, so every chunk
/// starts on a page boundary), and the drain records the completion here
/// for the thread waiting on it. The two qTD slots rotate: the transfer
/// goes in the dummy the queue head's overlay points at, and the other
/// slot becomes the new dummy. A new object every open over a pooled
/// <see cref="EhciPipeMemory"/>. <see cref="Busy"/>, <see cref="Dummy"/>
/// and <see cref="PendingTransfer"/> are written under the controller's
/// lock; the completion fields by the drain before <see cref="Completed"/>.
/// </summary>
internal sealed class EhciBulkPipe : UsbPipe
{
    /// <summary>Set by the drain once the pending qTD retired, after the token; cleared by the issuer under the lock. Read and written with <c>Volatile</c>.</summary>
    public bool Completed;

    /// <summary>Creates the pipe over its memory set. Thread context, the host's open path.</summary>
    /// <param name="endpoint">The bulk endpoint, either direction.</param>
    /// <param name="memory">The pooled queue head, qTD slots, bounce buffer and event, cleared.</param>
    /// <param name="endpointId">The endpoint's id, <c>number * 2 + IN</c>.</param>
    public EhciBulkPipe(UsbEndpoint endpoint, EhciPipeMemory memory, byte endpointId)
        : base(endpoint)
    {
        Memory = memory;
        EndpointId = endpointId;
    }

    /// <summary>The pooled queue head, qTD slots, bounce buffer and event.</summary>
    public EhciPipeMemory Memory { get; }

    /// <summary>The endpoint's id.</summary>
    public byte EndpointId { get; }

    /// <summary>True while a transfer, a reset or a close owns the pipe. Under the lock.</summary>
    public bool Busy { get; set; }

    /// <summary>The event the transfer's completion signals.</summary>
    public DeviceEvent Event => Memory.Event!;

    /// <summary>Index in the memory set of the inactive dummy qTD the overlay points at. Under the lock.</summary>
    public int Dummy { get; set; }

    /// <summary>True while a qTD is queued and a thread waits for it; the drain checks the slot at <see cref="TransferSlot"/>. Under the lock or in the drain.</summary>
    public bool PendingTransfer { get; set; }

    /// <summary>Index in the memory set of the qTD in flight.</summary>
    public int TransferSlot { get; set; }

    /// <summary>The token of the retired qTD; written before <see cref="Completed"/>.</summary>
    public uint CompletionToken { get; set; }

    /// <summary>Marks the pipe closed for good; the host's close path, under the lock.</summary>
    internal void MarkClosed()
    {
        IsClosed = true;
    }
}
