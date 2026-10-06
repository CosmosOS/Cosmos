// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Usb;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Xhci;

/// <summary>
/// An open bulk endpoint. Its transfers are synchronous: the data goes
/// through the 64 KiB bounce buffer of its memory set one Normal TRB at a
/// time (64 KiB aligned, so no TRB ever splits at a 64 KiB boundary), and
/// the event handler records the completion here for the thread waiting on
/// it. A new object every open over a pooled <see cref="XhciPipeMemory"/>.
/// <see cref="Busy"/> and <see cref="PendingTrb"/> are written under the
/// controller's lock; the completion fields by the handler before
/// <see cref="Completed"/>.
/// </summary>
internal sealed class XhciBulkPipe : UsbPipe
{
    /// <summary>Set by the handler or the polled drain once the pending TRB completed, after the code and the residual; cleared by the issuer under the lock. Read and written with <c>Volatile</c>.</summary>
    public bool Completed;

    /// <summary>Creates the pipe over its memory set. Thread context, the host's open path.</summary>
    /// <param name="endpoint">The bulk endpoint, either direction.</param>
    /// <param name="memory">The pooled ring, bounce buffer and event, the ring reset.</param>
    /// <param name="dci">The endpoint's Device Context Index.</param>
    public XhciBulkPipe(UsbEndpoint endpoint, XhciPipeMemory memory, byte dci)
        : base(endpoint)
    {
        Memory = memory;
        EndpointId = dci;
    }

    /// <summary>The pooled ring, bounce buffer and event.</summary>
    public XhciPipeMemory Memory { get; }

    /// <summary>The endpoint's Device Context Index.</summary>
    public byte EndpointId { get; }

    /// <summary>True while a transfer, a reset or a close owns the pipe. Under the lock.</summary>
    public bool Busy { get; set; }

    /// <summary>The event the transfer's completion signals.</summary>
    public DeviceEvent Event => Memory.Event!;

    /// <summary>The completion code of the pending TRB; written before <see cref="Completed"/>.</summary>
    public XhciCompletionCode CompletionCode { get; set; }

    /// <summary>Bytes of the pending TRB the controller did not transfer.</summary>
    public uint ResidualLength { get; set; }

    /// <summary>The TRB of the transfer in flight, 0 when none is. Under the lock.</summary>
    public ulong PendingTrb { get; set; }

    /// <summary>Marks the pipe closed for good; the host's close path, under the lock.</summary>
    internal void MarkClosed() => IsClosed = true;
}
