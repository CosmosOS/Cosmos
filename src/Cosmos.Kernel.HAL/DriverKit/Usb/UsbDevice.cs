// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>
/// One USB function behind one port: the host created it when it addressed
/// the port (an xHCI slot) and derives from it to carry its per-device
/// state and the transfer primitives; the enumeration core fills the
/// identity it read from the descriptors. A host implements the protected
/// members; the kit calls them through the internal ones, so a class driver
/// holding a reference can reach none of them and goes through its
/// <see cref="UsbAccess"/> instead.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public abstract class UsbDevice
{
    /// <summary>Written by the host's port change handler or the kit's detach, read by whichever thread waits on a transfer.</summary>
    private volatile bool _disconnected;

    /// <summary>Creates the kit half of a device the host addressed.</summary>
    /// <param name="host">The host controller that addressed it.</param>
    /// <param name="parent">The hub it hangs off, or null for a device on a root port.</param>
    /// <param name="portNumber">The 1-based port number on <paramref name="parent"/>, or the root port number.</param>
    /// <param name="speed">The speed the port reported.</param>
    protected UsbDevice(UsbHostController host, UsbDevice? parent, byte portNumber, UsbSpeed speed)
    {
        ArgumentNullException.ThrowIfNull(host);
        Host = host;
        Parent = parent;
        PortNumber = portNumber;
        Speed = speed;
        RootPortNumber = parent?.RootPortNumber ?? portNumber;
        HubDepth = parent is null ? 0 : parent.HubDepth + 1;
    }

    /// <summary>The host controller that addressed the device.</summary>
    public UsbHostController Host { get; }

    /// <summary>The hub this device is attached to, or null for a device on a root port.</summary>
    public UsbDevice? Parent { get; }

    /// <summary>Port number on <see cref="Parent"/>, or the root port number when there is no parent.</summary>
    public byte PortNumber { get; }

    /// <summary>The root hub port the device's branch of the tree hangs off.</summary>
    public byte RootPortNumber { get; }

    /// <summary>Number of hubs between this device and its root port.</summary>
    public int HubDepth { get; }

    /// <summary>The speed the device was attached at.</summary>
    public UsbSpeed Speed { get; }

    /// <summary>bMaxPacketSize0 in bytes, set by the host while addressing the device.</summary>
    public ushort MaxPacketSize0 { get; protected set; }

    /// <summary>idVendor, written by the enumeration core.</summary>
    public ushort VendorId { get; internal set; }

    /// <summary>idProduct, written by the enumeration core.</summary>
    public ushort ProductId { get; internal set; }

    /// <summary>bDeviceClass, written by the enumeration core.</summary>
    public byte DeviceClass { get; internal set; }

    /// <summary>bDeviceSubClass, written by the enumeration core.</summary>
    public byte DeviceSubclass { get; internal set; }

    /// <summary>bDeviceProtocol, written by the enumeration core.</summary>
    public byte DeviceProtocol { get; internal set; }

    /// <summary>bConfigurationValue of the first configuration, the one the kit selects; written by the enumeration core.</summary>
    public byte ConfigurationValue { get; internal set; }

    /// <summary>
    /// True once the device left the bus, or is being released. From then on
    /// every transfer returns <see cref="UsbTransferStatus.Disconnected"/>
    /// and one already waiting stops waiting. Any context.
    /// </summary>
    public bool IsDisconnected => _disconnected;

    /// <summary>
    /// Any context, allocation-free: every transfer to the device returns
    /// Disconnected from here on and one already waiting stops waiting. The
    /// host calls it from its port change handler when a root port loses its
    /// connection; the kit calls it for a whole subtree at detach.
    /// </summary>
    public void MarkDisconnected()
    {
        if (!_disconnected)
        {
            _disconnected = true;
            OnDisconnectedCore();
        }
    }

    /// <summary>
    /// Any context, allocation-free, under no lock: called by
    /// <see cref="MarkDisconnected"/> on the first transition so the host
    /// wakes every thread waiting on a transfer or command of this device;
    /// the waiter re-reads <see cref="IsDisconnected"/> and returns
    /// Disconnected.
    /// </summary>
    protected abstract void OnDisconnectedCore();

    /// <summary>
    /// Runs a control transfer on the default pipe and waits for it. Thread
    /// context; the data span is the OUT data or the IN buffer; one control
    /// transfer per device at a time (the kit serializes them).
    /// </summary>
    /// <param name="setup">The request; its Length is the data stage size.</param>
    /// <param name="data">At least <see cref="UsbSetupPacket.Length"/> bytes.</param>
    protected abstract UsbTransferStatus ControlTransferCore(UsbSetupPacket setup, Span<byte> data);

    /// <summary>
    /// Configures an interrupt IN endpoint and keeps transfers queued on it
    /// until the pipe is closed, handing every completed one to
    /// <paramref name="handler"/>. Thread context.
    /// </summary>
    /// <param name="endpoint">An interrupt IN endpoint of this device.</param>
    /// <param name="handler">Receives each report.</param>
    /// <returns>The pipe; null when the controller refused.</returns>
    protected abstract UsbPipe? OpenInterruptPipeCore(UsbEndpoint endpoint, UsbReportHandler handler);

    /// <summary>Configures a bulk endpoint, either direction. Thread context.</summary>
    /// <param name="endpoint">A bulk endpoint of this device.</param>
    /// <returns>The pipe; null when the controller refused.</returns>
    protected abstract UsbPipe? OpenBulkPipeCore(UsbEndpoint endpoint);

    /// <summary>
    /// Stops the endpoint, drops its context and marks the pipe closed;
    /// on a disconnected device only the state is dropped. Thread context.
    /// </summary>
    /// <param name="pipe">A pipe this device opened.</param>
    protected abstract void ClosePipeCore(UsbPipe pipe);

    /// <summary>
    /// Reads from an open bulk IN pipe and waits. Thread context, one
    /// transfer per pipe at a time (the caller serializes); ends early on a
    /// short packet.
    /// </summary>
    /// <param name="pipe">A bulk IN pipe of this device.</param>
    /// <param name="data">Receives the data; its length is the most the transfer reads.</param>
    /// <param name="transferred">Bytes received, set on failure too.</param>
    protected abstract UsbTransferStatus BulkInCore(UsbPipe pipe, Span<byte> data, out int transferred);

    /// <summary>
    /// Writes <paramref name="data"/> to an open bulk OUT pipe and waits.
    /// Thread context, one transfer per pipe at a time (the caller
    /// serializes); ends early on a short packet.
    /// </summary>
    /// <param name="pipe">A bulk OUT pipe of this device.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="transferred">Bytes the device accepted, set on failure too.</param>
    protected abstract UsbTransferStatus BulkOutCore(UsbPipe pipe, ReadOnlySpan<byte> data, out int transferred);

    /// <summary>
    /// The host half of CLEAR_FEATURE(ENDPOINT_HALT): returns the host side
    /// of the pipe to its initial state, nothing queued and the data toggle
    /// back to DATA0. Thread context.
    /// </summary>
    /// <param name="pipe">A pipe of this device.</param>
    protected abstract bool ResetEndpointCore(UsbPipe pipe);

    /// <summary>
    /// Tells the host controller this device is a hub, so it can route
    /// transactions to the devices behind it. Thread context.
    /// </summary>
    /// <param name="portCount">bNbrPorts from the hub descriptor.</param>
    /// <param name="thinkTime">TT think time from wHubCharacteristics bits 6:5 (high-speed hubs only).</param>
    protected abstract bool ConfigureAsHubCore(byte portCount, byte thinkTime);

    /// <summary>Kit side of <see cref="ControlTransferCore"/>; called by the enumeration core and the access object.</summary>
    internal UsbTransferStatus ControlTransfer(UsbSetupPacket setup, Span<byte> data) => ControlTransferCore(setup, data);

    /// <summary>Kit side of <see cref="OpenInterruptPipeCore"/>; called by the access object.</summary>
    internal UsbPipe? OpenInterruptPipe(UsbEndpoint endpoint, UsbReportHandler handler) => OpenInterruptPipeCore(endpoint, handler);

    /// <summary>Kit side of <see cref="OpenBulkPipeCore"/>; called by the access object.</summary>
    internal UsbPipe? OpenBulkPipe(UsbEndpoint endpoint) => OpenBulkPipeCore(endpoint);

    /// <summary>Kit side of <see cref="ClosePipeCore"/>; called by the pipe's ledger entry.</summary>
    internal void ClosePipe(UsbPipe pipe) => ClosePipeCore(pipe);

    /// <summary>Kit side of <see cref="BulkInCore"/>; called by the access object.</summary>
    internal UsbTransferStatus BulkIn(UsbPipe pipe, Span<byte> data, out int transferred) => BulkInCore(pipe, data, out transferred);

    /// <summary>Kit side of <see cref="BulkOutCore"/>; called by the access object.</summary>
    internal UsbTransferStatus BulkOut(UsbPipe pipe, ReadOnlySpan<byte> data, out int transferred) => BulkOutCore(pipe, data, out transferred);

    /// <summary>Kit side of <see cref="ResetEndpointCore"/>; called by the access object.</summary>
    internal bool ResetEndpoint(UsbPipe pipe) => ResetEndpointCore(pipe);

    /// <summary>Kit side of <see cref="ConfigureAsHubCore"/>; called by the access object.</summary>
    internal bool ConfigureAsHub(byte portCount, byte thinkTime) => ConfigureAsHubCore(portCount, thinkTime);
}
