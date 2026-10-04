// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>
/// An opened endpoint: an interrupt IN pipe that delivers reports to a
/// <see cref="UsbReportHandler"/>, or a bulk pipe (IN or OUT) the driver
/// transfers through <see cref="UsbAccess.BulkIn"/> and
/// <see cref="UsbAccess.BulkOut"/>. The host creates it and derives its
/// own pipe kinds from it; the kit records it on the ledger of the binding
/// that opened it and closes it in the unwind, or earlier through
/// <see cref="UsbAccess.ClosePipe"/>. Any context for the two getters.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public abstract class UsbPipe
{
    /// <summary>Creates a pipe over an endpoint.</summary>
    /// <param name="endpoint">The endpoint the pipe was opened on.</param>
    protected UsbPipe(UsbEndpoint endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        Endpoint = endpoint;
    }

    /// <summary>The endpoint the pipe was opened on.</summary>
    public UsbEndpoint Endpoint { get; }

    /// <summary>True once the host closed the pipe; the host is the only writer, the kit only reads it.</summary>
    public bool IsClosed { get; protected set; }
}
