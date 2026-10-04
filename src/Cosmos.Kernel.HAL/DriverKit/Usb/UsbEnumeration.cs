// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>
/// The enumeration core, the body of <see cref="UsbBus.Attach"/>,
/// <see cref="UsbBus.Detach"/>, <see cref="UsbAccess.AttachChild"/> and
/// <see cref="UsbAccess.DetachChild"/>. Attach addresses the device on a
/// port through the host, reads its descriptors, selects its first
/// configuration and publishes its interface nodes beneath the owner's
/// node; Detach marks the device and its subtree disconnected, retracts
/// every interface node and releases the devices through the host. All
/// thread context: a probe, the host's hot-plug thread or a hub's driver
/// thread, never the worker's teardown. Every log line goes through the
/// owner's binding.
/// </summary>
internal static class UsbEnumeration
{
    internal const int DeviceDescriptorLength = 18;
    internal const int ConfigurationHeaderLength = 9;

    /// <summary>Upper bound on the configuration descriptor set read: one host DMA page.</summary>
    internal const int MaxConfigurationLength = 4096;

    // Device descriptor field offsets (USB 2.0 section 9.6.1).
    private const int DeviceClassOffset = 4;
    private const int DeviceSubclassOffset = 5;
    private const int DeviceProtocolOffset = 6;
    private const int VendorIdOffset = 8;
    private const int ProductIdOffset = 10;

    // Configuration descriptor field offsets (USB 2.0 section 9.6.3).
    private const int TotalLengthOffset = 2;
    private const int ConfigurationValueOffset = 5;

    // Interface descriptor (USB 2.0 section 9.6.5).
    private const int InterfaceDescriptorLength = 9;
    private const int InterfaceNumberOffset = 2;
    private const int AlternateSettingOffset = 3;
    private const int InterfaceClassOffset = 5;
    private const int InterfaceSubclassOffset = 6;
    private const int InterfaceProtocolOffset = 7;

    // Endpoint descriptor (USB 2.0 section 9.6.6).
    private const int EndpointDescriptorLength = 7;
    private const int EndpointAddressOffset = 2;
    private const int EndpointAttributesOffset = 3;
    private const int EndpointMaxPacketSizeOffset = 4;
    private const int EndpointIntervalOffset = 6;

    // SuperSpeed Endpoint Companion descriptor (USB 3.2 section 9.6.7).
    private const int CompanionDescriptorLength = 6;
    private const int CompanionMaxBurstOffset = 2;

    /// <summary>Every descriptor starts with bLength then bDescriptorType.</summary>
    private const int DescriptorHeaderLength = 2;

    /// <summary>
    /// Attaches the device on <paramref name="port"/> of <paramref name="parent"/>
    /// (null: a root port of <paramref name="bus"/>) and publishes one node
    /// per interface beneath <paramref name="owner"/>'s node. Thread context.
    /// </summary>
    /// <param name="bus">The bus.</param>
    /// <param name="owner">The host or hub binding that owns the published nodes.</param>
    /// <param name="parent">The hub the port belongs to, or null for a root port.</param>
    /// <param name="port">The 1-based port number.</param>
    /// <param name="speed">The speed the port reported.</param>
    /// <returns>False when a device of this owner is already on the port, when any step failed, or when the owner began detaching before its nodes were published; logged either way.</returns>
    internal static bool Attach(UsbBus bus, DeviceBinding owner, UsbDeviceState? parent, byte port, UsbSpeed speed)
    {
        string path = parent is null ? $"{bus.Ordinal}-{port}" : $"{parent.PortPath}.{port}";

        // 1. The port is free, or holds the stale state of a hub probe that
        //    attached its children and then failed: its unwind retracted the
        //    nodes and arbitration offered the hub to the next candidate,
        //    whose attach lands here. Anything else on the port is the
        //    caller's bug: the thread paths detach before they attach.
        UsbDeviceState? existing = bus.Find(parent, port);
        if (existing is not null)
        {
            if (ReferenceEquals(existing.Owner, owner) || !IsStale(existing))
            {
                owner.Log($"usb {path}: port already attached");
                return false;
            }

            Release(bus, owner, existing, hostPresent: true);
            owner.Log($"usb {path}: released a stale device");
        }

        // 2. The host gives the device its address and default pipe.
        UsbDevice? device = bus.Host.AddressDevice(parent?.Device, port, speed);
        if (device is null)
        {
            owner.Log($"usb {path}: the host could not address the device");
            return false;
        }

        // 3. The device descriptor: the five identity fields.
        Span<byte> deviceDescriptor = stackalloc byte[DeviceDescriptorLength];
        if (GetDescriptor(device, UsbDescriptorType.Device, deviceDescriptor) != UsbTransferStatus.Success)
        {
            owner.Log($"usb {path}: could not read the device descriptor");
            bus.Host.ReleaseDevice(device, hostPresent: true);
            return false;
        }

        device.DeviceClass = deviceDescriptor[DeviceClassOffset];
        device.DeviceSubclass = deviceDescriptor[DeviceSubclassOffset];
        device.DeviceProtocol = deviceDescriptor[DeviceProtocolOffset];
        device.VendorId = ReadUInt16(deviceDescriptor, VendorIdOffset);
        device.ProductId = ReadUInt16(deviceDescriptor, ProductIdOffset);

        // 4. The first configuration: its header for the total length, then
        //    the whole set, parsed for alternate setting 0 of each interface.
        Span<byte> header = stackalloc byte[ConfigurationHeaderLength];
        if (GetDescriptor(device, UsbDescriptorType.Configuration, header) != UsbTransferStatus.Success)
        {
            owner.Log($"usb {path}: could not read the configuration descriptor");
            bus.Host.ReleaseDevice(device, hostPresent: true);
            return false;
        }

        int totalLength = Math.Min((int)ReadUInt16(header, TotalLengthOffset), MaxConfigurationLength);
        if (totalLength < ConfigurationHeaderLength)
        {
            owner.Log($"usb {path}: could not read the configuration descriptor");
            bus.Host.ReleaseDevice(device, hostPresent: true);
            return false;
        }

        byte[] configuration = new byte[totalLength];
        if (GetDescriptor(device, UsbDescriptorType.Configuration, configuration) != UsbTransferStatus.Success)
        {
            owner.Log($"usb {path}: could not read the configuration descriptor");
            bus.Host.ReleaseDevice(device, hostPresent: true);
            return false;
        }

        device.ConfigurationValue = configuration[ConfigurationValueOffset];
        UsbInterfaceInfo[] interfaces = ParseConfiguration(configuration);

        // 5. SET_CONFIGURATION, a zero-length OUT.
        UsbSetupPacket setConfiguration = new(UsbRequestType.HostToDevice | UsbRequestType.Standard | UsbRequestType.Device,
            (byte)UsbStandardRequest.SetConfiguration, device.ConfigurationValue, 0, 0);
        if (device.ControlTransfer(setConfiguration, Span<byte>.Empty) != UsbTransferStatus.Success)
        {
            owner.Log($"usb {path}: SET_CONFIGURATION failed");
            bus.Host.ReleaseDevice(device, hostPresent: true);
            return false;
        }

        // 6. The device is attached.
        owner.Log($"usb {path}: {device.VendorId:x4}:{device.ProductId:x4} {SpeedName(speed)}, {interfaces.Length} interface(s)");

        // 7. The state goes in the tree, then one node per interface.
        UsbDeviceState state = new(device, bus, parent, port, path, configuration, interfaces, owner);
        bus.Link(state);
        if (interfaces.Length == 0)
        {
            owner.Log($"usb {path}: no interface");
            return true;
        }

        List<DeviceNode> nodes = new(interfaces.Length);
        try
        {
            for (int i = 0; i < interfaces.Length; i++)
            {
                UsbInterfaceInfo info = interfaces[i];
                UsbIdentity identity = new(path, info.Number, device.VendorId, device.ProductId, device.DeviceClass, device.DeviceSubclass,
                    device.DeviceProtocol, info.Class, info.Subclass, info.Protocol, speed, device.ConfigurationValue);
                nodes.Add(owner.PublishChild(identity, [], [], new UsbAccess(state, info)));
            }
        }
        catch (InvalidOperationException exception)
        {
            // The owner began detaching between the caller's check and the
            // publish. The state stays linked, marked disconnected: the
            // owner's teardown tears down the nodes published so far, and
            // its ReleaseAll or the enclosing root Detach releases the device.
            device.MarkDisconnected();
            owner.Log($"usb {path}: {exception.Message}");
            return false;
        }
        finally
        {
            state.Nodes = nodes.ToArray();
        }

        return true;
    }

    /// <summary>
    /// Detaches <paramref name="state"/>: marks its device and every device
    /// below it disconnected (so a thread waiting on a transfer anywhere in
    /// the branch stops now, before any teardown runs), retracts every
    /// interface node not yet retracted (the hub nodes' own children go
    /// first, inside their teardown), releases the devices deepest first
    /// and unlinks the state. Thread context, the owner's driver thread,
    /// never the worker: it returns once every node's binding was torn down.
    /// When the owner is itself being torn down, a retraction's wait is
    /// released early with the node still bound; the detach then stops
    /// there and leaves the state linked and marked disconnected, so the
    /// owner's teardown finishes the nodes and the enclosing root detach or
    /// the bus's <see cref="UsbBus.ReleaseAll"/> releases the device exactly
    /// once, after every node was torn down.
    /// </summary>
    /// <param name="bus">The bus.</param>
    /// <param name="owner">The binding whose node the interface nodes hang off.</param>
    /// <param name="state">The device to detach.</param>
    internal static void Detach(UsbBus bus, DeviceBinding owner, UsbDeviceState state)
    {
        // 1. Every transfer in the branch returns Disconnected from here on.
        MarkDisconnected(bus, state);

        // 2. The interface nodes, each torn down before the next. A node
        //    still bound afterwards means the wait was released early
        //    because the owner is detaching: its teardown tears the node
        //    down and the enclosing release frees the device, never here.
        DeviceNode[] nodes = state.Nodes;
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].State != NodeState.Retracted)
            {
                owner.RetractChild(nodes[i], hardwarePresent: false);
                if (nodes[i].State != NodeState.Retracted)
                {
                    return;
                }
            }
        }

        // 3 and 4. The devices, leaves first; then the state leaves the tree.
        ReleaseSubtree(bus, state, hostPresent: true);
        bus.Unlink(state);
        owner.Log($"usb {state.PortPath}: disconnected");
    }

    /// <summary>
    /// The detach without the retraction, steps 1, 3 and 4: for a stale
    /// state whose nodes are already retracted, and for the bus's
    /// <see cref="UsbBus.ReleaseAll"/> after the kit's child step tore the
    /// nodes down. Thread context.
    /// </summary>
    /// <param name="bus">The bus.</param>
    /// <param name="owner">The binding that logs.</param>
    /// <param name="state">The device to release.</param>
    /// <param name="hostPresent">Whether the controller is still there to be told.</param>
    internal static void Release(UsbBus bus, DeviceBinding owner, UsbDeviceState state, bool hostPresent)
    {
        MarkDisconnected(bus, state);
        ReleaseSubtree(bus, state, hostPresent);
        bus.Unlink(state);
        owner.Log($"usb {state.PortPath}: disconnected");
    }

    /// <summary>A state left by another owner whose every node is retracted, or that has none.</summary>
    private static bool IsStale(UsbDeviceState state)
    {
        DeviceNode[] nodes = state.Nodes;
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].State != NodeState.Retracted)
            {
                return false;
            }
        }

        return true;
    }

    private static void MarkDisconnected(UsbBus bus, UsbDeviceState state)
    {
        state.Device.MarkDisconnected();
        UsbDeviceState[] children = bus.ChildrenOf(state);
        for (int i = 0; i < children.Length; i++)
        {
            MarkDisconnected(bus, children[i]);
        }
    }

    /// <summary>Releases every device below <paramref name="state"/>, leaves first, then the state's own.</summary>
    private static void ReleaseSubtree(UsbBus bus, UsbDeviceState state, bool hostPresent)
    {
        UsbDeviceState[] children = bus.ChildrenOf(state);
        for (int i = 0; i < children.Length; i++)
        {
            ReleaseSubtree(bus, children[i], hostPresent);
        }

        bus.Host.ReleaseDevice(state.Device, hostPresent);
    }

    private static UsbTransferStatus GetDescriptor(UsbDevice device, UsbDescriptorType type, Span<byte> buffer)
    {
        UsbSetupPacket setup = new(UsbRequestType.DeviceToHost | UsbRequestType.Standard | UsbRequestType.Device,
            (byte)UsbStandardRequest.GetDescriptor, (ushort)((byte)type << 8), 0, (ushort)buffer.Length);
        return device.ControlTransfer(setup, buffer);
    }

    /// <summary>
    /// Walks the descriptor set of a configuration. Only alternate setting 0
    /// of each interface is recorded: it is the one active after
    /// SET_CONFIGURATION, and the kit never selects another. Another
    /// alternate setting suspends endpoint collection until the next
    /// interface descriptor; a SuperSpeed companion sets the last endpoint's
    /// MaxBurst; everything else is skipped and no string descriptor is read.
    /// </summary>
    private static UsbInterfaceInfo[] ParseConfiguration(ReadOnlySpan<byte> configuration)
    {
        List<UsbInterfaceInfo> interfaces = [];
        UsbInterfaceInfo? current = null;
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
                    current = new UsbInterfaceInfo(
                        descriptor[InterfaceNumberOffset],
                        descriptor[InterfaceClassOffset],
                        descriptor[InterfaceSubclassOffset],
                        descriptor[InterfaceProtocolOffset]);
                    interfaces.Add(current);
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

        return interfaces.ToArray();
    }

    private static string SpeedName(UsbSpeed speed) => speed switch
    {
        UsbSpeed.Low => "low-speed",
        UsbSpeed.Full => "full-speed",
        UsbSpeed.High => "high-speed",
        UsbSpeed.Super => "SuperSpeed",
        UsbSpeed.SuperPlus => "SuperSpeedPlus",
        _ => "unknown speed",
    };

    private static ushort ReadUInt16(ReadOnlySpan<byte> data, int offset) =>
        (ushort)(data[offset] | (data[offset + 1] << 8));
}
