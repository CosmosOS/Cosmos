// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Devices.Usb;

/// <summary>
/// A device addressed on a USB bus. The host controller that addressed it
/// derives from this type to carry its own per-device state and implements
/// the transfer primitives; class drivers only ever program against this
/// type, so they work unchanged on any host controller.
/// </summary>
internal abstract class UsbDevice
{
    private const int DeviceDescriptorLength = 18;
    private const int ConfigurationHeaderLength = 9;

    /// <summary>Upper bound on the configuration descriptor set read: one host-controller DMA page.</summary>
    private const int MaxConfigurationLength = 4096;

    // Device descriptor field offsets (USB 2.0 §9.6.1).
    private const int DeviceClassOffset = 4;
    private const int DeviceSubclassOffset = 5;
    private const int DeviceProtocolOffset = 6;
    private const int VendorIdOffset = 8;
    private const int ProductIdOffset = 10;

    // Configuration descriptor field offsets (USB 2.0 §9.6.3).
    private const int TotalLengthOffset = 2;
    private const int ConfigurationValueOffset = 5;

    // Interface descriptor (USB 2.0 §9.6.5).
    private const int InterfaceDescriptorLength = 9;
    private const int InterfaceNumberOffset = 2;
    private const int AlternateSettingOffset = 3;
    private const int InterfaceClassOffset = 5;
    private const int InterfaceSubclassOffset = 6;
    private const int InterfaceProtocolOffset = 7;

    // Endpoint descriptor (USB 2.0 §9.6.6).
    private const int EndpointDescriptorLength = 7;
    private const int EndpointAddressOffset = 2;
    private const int EndpointAttributesOffset = 3;
    private const int EndpointMaxPacketSizeOffset = 4;
    private const int EndpointIntervalOffset = 6;

    // SuperSpeed Endpoint Companion descriptor (USB 3.2 §9.6.7).
    private const int CompanionDescriptorLength = 6;
    private const int CompanionMaxBurstOffset = 2;

    /// <summary>ENDPOINT_HALT feature selector (USB 2.0 table 9-6).</summary>
    internal const ushort EndpointHaltFeature = 0;

    /// <summary>Every descriptor starts with bLength then bDescriptorType.</summary>
    private const int DescriptorHeaderLength = 2;

    /// <summary>Written by the hot-plug thread, read by whichever thread waits on a transfer.</summary>
    private volatile bool _disconnected;

    public UsbHostController HostController { get; }

    /// <summary>The hub this device is attached to, or null for a device on a root port.</summary>
    public UsbDevice? Parent { get; }

    /// <summary>Port number on <see cref="Parent"/>, or the root port number when there is no parent.</summary>
    public byte PortNumber { get; }

    /// <summary>The root hub port the device's branch of the tree hangs off.</summary>
    public byte RootPortNumber { get; }

    /// <summary>Number of hubs between this device and its root port.</summary>
    public int HubDepth { get; }

    public UsbSpeed Speed { get; }

    /// <summary>bMaxPacketSize0 in bytes, set by the host controller while addressing the device.</summary>
    public ushort MaxPacketSize0 { get; protected set; }

    public ushort VendorId { get; private set; }
    public ushort ProductId { get; private set; }
    public byte DeviceClass { get; private set; }
    public byte DeviceSubclass { get; private set; }
    public byte DeviceProtocol { get; private set; }

    /// <summary>bConfigurationValue of the first configuration, the one this stack selects.</summary>
    public byte ConfigurationValue { get; private set; }

    public List<UsbInterface> Interfaces { get; } = [];

    /// <summary>
    /// The device left the bus, or is being released. From then on every
    /// transfer fails with <see cref="UsbTransferStatus.Disconnected"/>, and
    /// one already waiting stops waiting.
    /// </summary>
    public bool IsDisconnected => _disconnected;

    protected UsbDevice(UsbHostController hostController, UsbDevice? parent, byte portNumber, UsbSpeed speed)
    {
        HostController = hostController;
        Parent = parent;
        PortNumber = portNumber;
        Speed = speed;
        RootPortNumber = parent?.RootPortNumber ?? portNumber;
        HubDepth = parent is null ? 0 : parent.HubDepth + 1;
    }

    /// <summary>
    /// Makes the device's transfers fail from now on. <see cref="UsbManager"/>
    /// calls it before the class drivers let go of a device that left, and
    /// the host controller before it frees one.
    /// </summary>
    internal void MarkDisconnected() => _disconnected = true;

    /// <summary>
    /// Runs a control transfer on the default pipe and waits for it. For a
    /// device-to-host request the device's data lands in
    /// <paramref name="data"/>; otherwise <paramref name="data"/> is sent.
    /// Only call from thread context: it polls for the completion.
    /// </summary>
    /// <param name="setup">The request; its Length is the data stage size.</param>
    /// <param name="data">At least <see cref="UsbSetupPacket.Length"/> bytes.</param>
    public abstract UsbTransferStatus ControlTransfer(UsbSetupPacket setup, Span<byte> data);

    /// <summary>
    /// Queues a host-to-device control transfer and returns without waiting
    /// for it: the form usable from interrupt context (e.g. keyboard LEDs).
    /// </summary>
    /// <returns><see langword="false"/> when the transfer could not be queued.</returns>
    public abstract bool SubmitControlTransfer(UsbSetupPacket setup, ReadOnlySpan<byte> data);

    /// <summary>
    /// Opens an interrupt IN endpoint and keeps transfers queued on it for
    /// as long as the device lives, handing every completed one to
    /// <paramref name="handler"/>.
    /// </summary>
    public abstract bool OpenInterruptPipe(UsbEndpoint endpoint, UsbInterruptHandler handler);

    /// <summary>
    /// Adds a bulk endpoint to the device's configuration, ready for
    /// <see cref="BulkIn"/> or <see cref="BulkOut"/>.
    /// </summary>
    public abstract bool OpenBulkEndpoint(UsbEndpoint endpoint);

    /// <summary>
    /// Reads from an open bulk IN endpoint and waits. The transfer ends
    /// early when the device sends a short packet, which is how it says it
    /// has nothing more. Only call from thread context, one transfer per
    /// endpoint at a time.
    /// </summary>
    /// <param name="endpoint">A bulk IN endpoint opened with <see cref="OpenBulkEndpoint"/>.</param>
    /// <param name="data">Receives the data; its length is the most the transfer reads.</param>
    /// <param name="transferred">Bytes received, set on failure too.</param>
    public abstract UsbTransferStatus BulkIn(UsbEndpoint endpoint, Span<byte> data, out int transferred);

    /// <summary>
    /// Writes <paramref name="data"/> to an open bulk OUT endpoint and waits.
    /// Same rules as <see cref="BulkIn"/>.
    /// </summary>
    /// <param name="endpoint">A bulk OUT endpoint opened with <see cref="OpenBulkEndpoint"/>.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="transferred">Bytes the device accepted, set on failure too.</param>
    public abstract UsbTransferStatus BulkOut(UsbEndpoint endpoint, ReadOnlySpan<byte> data, out int transferred);

    /// <summary>
    /// Returns the host side of an open bulk endpoint to its initial state:
    /// nothing queued and the data toggle back to DATA0. A failed transfer
    /// already leaves the host side able to run the next one; this is the
    /// half of <see cref="ClearHalt"/> the device does not do.
    /// </summary>
    public abstract bool ResetEndpoint(UsbEndpoint endpoint);

    /// <summary>
    /// Tells the host controller this device is a hub, so it can route
    /// transactions to the devices behind it.
    /// </summary>
    /// <param name="portCount">bNbrPorts from the hub descriptor.</param>
    /// <param name="thinkTime">TT think time from wHubCharacteristics bits 6:5 (high-speed hubs only).</param>
    public abstract bool ConfigureAsHub(byte portCount, byte thinkTime);

    /// <summary>Device-to-host control request; the data stage is <paramref name="data"/>.Length bytes.</summary>
    public UsbTransferStatus ControlIn(UsbRequestType requestType, byte request, ushort value, ushort index, Span<byte> data) =>
        ControlTransfer(new UsbSetupPacket(requestType | UsbRequestType.DeviceToHost, request, value, index, (ushort)data.Length), data);

    /// <summary>Host-to-device control request with no data stage.</summary>
    public UsbTransferStatus ControlOut(UsbRequestType requestType, byte request, ushort value, ushort index) =>
        ControlTransfer(new UsbSetupPacket(requestType, request, value, index, 0), []);

    /// <summary>
    /// Clears a halted endpoint on both sides (USB 2.0 §9.4.5): the device
    /// through CLEAR_FEATURE(ENDPOINT_HALT), which also restarts its data
    /// toggle, then the host through <see cref="ResetEndpoint"/> so both
    /// toggles agree again.
    /// </summary>
    public UsbTransferStatus ClearHalt(UsbEndpoint endpoint)
    {
        UsbTransferStatus status = ControlOut(UsbRequestType.Standard | UsbRequestType.Endpoint,
            (byte)UsbStandardRequest.ClearFeature, EndpointHaltFeature, endpoint.Address);
        if (status == UsbTransferStatus.Success && !ResetEndpoint(endpoint))
        {
            return UsbTransferStatus.Error;
        }

        return status;
    }

    public UsbTransferStatus GetDescriptor(UsbDescriptorType type, byte index, Span<byte> buffer) =>
        ControlIn(UsbRequestType.Standard | UsbRequestType.Device, (byte)UsbStandardRequest.GetDescriptor,
            (ushort)(((byte)type << 8) | index), 0, buffer);

    /// <summary>
    /// Reads the device descriptor and the first configuration, filling the
    /// identity properties and <see cref="Interfaces"/>.
    /// </summary>
    internal bool ReadDescriptors()
    {
        Span<byte> device = stackalloc byte[DeviceDescriptorLength];
        if (GetDescriptor(UsbDescriptorType.Device, 0, device) != UsbTransferStatus.Success)
        {
            return false;
        }

        DeviceClass = device[DeviceClassOffset];
        DeviceSubclass = device[DeviceSubclassOffset];
        DeviceProtocol = device[DeviceProtocolOffset];
        VendorId = ReadUInt16(device, VendorIdOffset);
        ProductId = ReadUInt16(device, ProductIdOffset);

        Span<byte> header = stackalloc byte[ConfigurationHeaderLength];
        if (GetDescriptor(UsbDescriptorType.Configuration, 0, header) != UsbTransferStatus.Success)
        {
            return false;
        }

        ConfigurationValue = header[ConfigurationValueOffset];
        int totalLength = Math.Min((int)ReadUInt16(header, TotalLengthOffset), MaxConfigurationLength);
        if (totalLength < ConfigurationHeaderLength)
        {
            return false;
        }

        byte[] configuration = new byte[totalLength];
        if (GetDescriptor(UsbDescriptorType.Configuration, 0, configuration) != UsbTransferStatus.Success)
        {
            return false;
        }

        ParseConfiguration(configuration);
        return true;
    }

    internal UsbTransferStatus SetConfiguration() =>
        ControlOut(UsbRequestType.Standard | UsbRequestType.Device, (byte)UsbStandardRequest.SetConfiguration, ConfigurationValue, 0);

    /// <summary>
    /// Walks the descriptor set of a configuration. Only alternate setting 0
    /// of each interface is recorded: it is the one active after
    /// SET_CONFIGURATION, and this stack never selects another.
    /// </summary>
    private void ParseConfiguration(ReadOnlySpan<byte> configuration)
    {
        UsbInterface? current = null;
        UsbEndpoint? lastEndpoint = null;
        int offset = 0;
        while (offset + DescriptorHeaderLength <= configuration.Length)
        {
            int length = configuration[offset];
            if (length < DescriptorHeaderLength || offset + length > configuration.Length)
            {
                break;
            }

            ReadOnlySpan<byte> descriptor = configuration.Slice(offset, length);
            UsbDescriptorType type = (UsbDescriptorType)descriptor[1];
            if (type == UsbDescriptorType.Interface && length >= InterfaceDescriptorLength)
            {
                current = null;
                lastEndpoint = null;
                if (descriptor[AlternateSettingOffset] == 0)
                {
                    current = new UsbInterface(
                        descriptor[InterfaceNumberOffset],
                        descriptor[InterfaceClassOffset],
                        descriptor[InterfaceSubclassOffset],
                        descriptor[InterfaceProtocolOffset]);
                    Interfaces.Add(current);
                }
            }
            else if (type == UsbDescriptorType.Endpoint && length >= EndpointDescriptorLength && current is not null)
            {
                lastEndpoint = new UsbEndpoint(
                    descriptor[EndpointAddressOffset],
                    descriptor[EndpointAttributesOffset],
                    ReadUInt16(descriptor, EndpointMaxPacketSizeOffset),
                    descriptor[EndpointIntervalOffset]);
                current.Endpoints.Add(lastEndpoint);
            }
            else if (type == UsbDescriptorType.SuperSpeedEndpointCompanion && length >= CompanionDescriptorLength && lastEndpoint is not null)
            {
                // The companion follows the endpoint descriptor it completes.
                lastEndpoint.MaxBurst = descriptor[CompanionMaxBurstOffset];
            }

            offset += length;
        }
    }

    private static ushort ReadUInt16(ReadOnlySpan<byte> data, int offset) =>
        (ushort)(data[offset] | (data[offset + 1] << 8));
}
