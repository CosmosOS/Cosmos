// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.Drivers.Usb;

/// <summary>
/// A device a <see cref="UsbHostController"/> addressed, as the controller
/// sees it: the controller derives from this class to keep its per-device
/// state and implements the transfers. The kit's USB core reads the
/// device's descriptors, selects its configuration and runs the class
/// drivers through these members only, so a class driver works unchanged on
/// any host controller. The core calls the transfer members from thread
/// context, except <see cref="SubmitControlTransfer"/>, and runs at most one
/// transfer or reset on a bulk endpoint at a time. A controller overrides
/// them as <c>protected override</c>.
/// </summary>
[Experimental(Experimentals.DriverKitDiagId)]
public abstract class UsbHostDevice
{
    /// <summary>Written by whichever thread learns the device is gone, read by whichever thread waits on a transfer.</summary>
    private volatile bool _disconnected;

    /// <summary>
    /// True once the device left the bus or is being released: every
    /// transfer then fails with <see cref="UsbTransferStatus.Disconnected"/>,
    /// and one already waiting stops waiting. Any context.
    /// </summary>
    public bool IsDisconnected => _disconnected;

    /// <summary>
    /// Makes the device's transfers fail from now on. The kit calls it before
    /// the class drivers let go of a device that left, and the controller
    /// before it frees one, or as soon as it sees the device's port go dark.
    /// Any context: it only sets a flag.
    /// </summary>
    protected internal void MarkDisconnected() => _disconnected = true;

    /// <summary>
    /// Runs a control transfer on the default pipe and waits for it. For a
    /// device-to-host request the device's data lands in
    /// <paramref name="data"/>; otherwise <paramref name="data"/> is sent.
    /// Thread context.
    /// </summary>
    /// <param name="setup">The request; its Length is the data stage size, at most 4096 bytes.</param>
    /// <param name="data">At least <see cref="UsbSetupPacket.Length"/> bytes.</param>
    /// <param name="transferred">
    /// Bytes the data stage moved when the transfer succeeds, 0 otherwise:
    /// fewer than asked for when a device-to-host request ended with a short
    /// packet, which is how a device answers with less.
    /// </param>
    /// <returns>How the transfer ended; <see cref="UsbTransferStatus.Disconnected"/> once the device is gone.</returns>
    protected internal abstract UsbTransferStatus ControlTransfer(UsbSetupPacket setup, Span<byte> data, out int transferred);

    /// <summary>
    /// Queues a host-to-device control transfer and returns without waiting
    /// for it, such as a keyboard's LED report. Any context, interrupt
    /// handlers included: it allocates nothing and takes no lock but an
    /// IRQ-safe one.
    /// </summary>
    /// <param name="setup">A host-to-device request; its Length is the data stage size.</param>
    /// <param name="data">At least <see cref="UsbSetupPacket.Length"/> bytes, copied before the call returns.</param>
    /// <returns>False when the transfer could not be queued.</returns>
    protected internal abstract bool SubmitControlTransfer(UsbSetupPacket setup, ReadOnlySpan<byte> data);

    /// <summary>
    /// Opens an interrupt IN endpoint and keeps transfers queued on it for as
    /// long as the device lives, handing every completed one to
    /// <paramref name="handler"/>, in interrupt context or from a thread
    /// draining the controller's events with interrupts masked. The endpoint
    /// cannot be closed. Thread context.
    /// </summary>
    /// <param name="endpoint">An interrupt IN endpoint of one of the device's interfaces.</param>
    /// <param name="handler">Receives each report; see <see cref="UsbReportHandler"/> for what it may do.</param>
    /// <returns>False when the endpoint is not an interrupt IN one or the controller could not open it.</returns>
    protected internal abstract bool OpenInterruptPipe(UsbEndpointInfo endpoint, UsbReportHandler handler);

    /// <summary>
    /// Adds a bulk endpoint to the device's configuration, ready for
    /// <see cref="BulkIn"/> or <see cref="BulkOut"/>. Opening one already
    /// open succeeds. The endpoint cannot be closed. Thread context.
    /// </summary>
    /// <param name="endpoint">A bulk endpoint of one of the device's interfaces.</param>
    /// <returns>False when the endpoint is not a bulk one or the controller could not open it.</returns>
    protected internal abstract bool OpenBulkEndpoint(UsbEndpointInfo endpoint);

    /// <summary>
    /// Reads from an open bulk IN endpoint and waits. The transfer ends early
    /// when the device sends a short packet, which is how it says it has
    /// nothing more. Thread context.
    /// </summary>
    /// <param name="endpoint">A bulk IN endpoint opened with <see cref="OpenBulkEndpoint"/>.</param>
    /// <param name="data">Receives the data; its length is the most the transfer reads.</param>
    /// <param name="transferred">Bytes received, set on failure too.</param>
    /// <returns>How the transfer ended.</returns>
    protected internal abstract UsbTransferStatus BulkIn(UsbEndpointInfo endpoint, Span<byte> data, out int transferred);

    /// <summary>Writes <paramref name="data"/> to an open bulk OUT endpoint and waits. Same rules as <see cref="BulkIn"/>.</summary>
    /// <param name="endpoint">A bulk OUT endpoint opened with <see cref="OpenBulkEndpoint"/>.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="transferred">Bytes the device accepted, set on failure too.</param>
    /// <returns>How the transfer ended.</returns>
    protected internal abstract UsbTransferStatus BulkOut(UsbEndpointInfo endpoint, ReadOnlySpan<byte> data, out int transferred);

    /// <summary>
    /// Returns the host side of an open bulk endpoint to its initial state:
    /// nothing queued and the data toggle back to DATA0. The half of clearing
    /// a halt the device does not do: the kit sends the device
    /// CLEAR_FEATURE(ENDPOINT_HALT) first. Thread context.
    /// </summary>
    /// <param name="endpoint">A bulk endpoint opened with <see cref="OpenBulkEndpoint"/>.</param>
    /// <returns>False when the endpoint is not open or the controller could not reset it.</returns>
    protected internal abstract bool ResetEndpoint(UsbEndpointInfo endpoint);

    /// <summary>
    /// Tells the controller this device is a hub, so it can route
    /// transactions to the devices behind it. The kit's hub driver calls it
    /// once, before it powers the hub's ports. Thread context.
    /// </summary>
    /// <param name="portCount">bNbrPorts from the hub descriptor.</param>
    /// <param name="thinkTime">TT think time from wHubCharacteristics bits 6:5, 0 for a SuperSpeed hub.</param>
    /// <returns>False when the controller refused the configuration.</returns>
    protected internal abstract bool ConfigureAsHub(byte portCount, byte thinkTime);
}
