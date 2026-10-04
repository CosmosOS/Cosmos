// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>
/// The access object of a USB interface node, the one thing a class driver
/// sees: the interface's identity and endpoints, control transfers on the
/// device's default pipe (serialized across the device's interfaces), the
/// pipes it opens on the ledger of its binding, bulk transfers through them,
/// and, for a hub driver, the enumeration core for the hub's ports. One per
/// interface node. Every member is thread context unless its summary says
/// otherwise: a probe, a work item, a driver thread or a ring caller, never
/// an interrupt handler; called from one (a <see cref="UsbReportHandler"/>
/// on a controller that delivers reports in interrupt context), a member
/// stops with an exception naming itself in a synthetic dispatch, or a
/// panic in a real interrupt, as the binding's members do.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class UsbAccess
{
    /// <summary>The longest data stage a control transfer may carry: one host DMA page.</summary>
    public const int MaxControlTransferLength = 4096;

    /// <summary>ENDPOINT_HALT feature selector (USB 2.0 table 9-6).</summary>
    internal const ushort EndpointHaltFeature = 0;

    private readonly UsbDeviceState _device;
    private readonly UsbInterfaceInfo _interface;

    internal UsbAccess(UsbDeviceState device, UsbInterfaceInfo usbInterface)
    {
        _device = device;
        _interface = usbInterface;
    }

    /// <summary>bInterfaceNumber.</summary>
    public byte InterfaceNumber => _interface.Number;

    /// <summary>bInterfaceClass.</summary>
    public byte InterfaceClass => _interface.Class;

    /// <summary>bInterfaceSubClass.</summary>
    public byte InterfaceSubclass => _interface.Subclass;

    /// <summary>bInterfaceProtocol.</summary>
    public byte InterfaceProtocol => _interface.Protocol;

    /// <summary>The endpoints of alternate setting 0, in descriptor order.</summary>
    public IReadOnlyList<UsbEndpoint> Endpoints => _interface.Endpoints;

    /// <summary>The speed the device was attached at.</summary>
    public UsbSpeed Speed => _device.Device.Speed;

    /// <summary>bMaxPacketSize0 of the device's default pipe.</summary>
    public ushort MaxPacketSize0 => _device.Device.MaxPacketSize0;

    /// <summary>Number of hubs between the device and its root port.</summary>
    public int HubDepth => _device.Device.HubDepth;

    /// <summary>The dotted port chain from the controller down to the device.</summary>
    public string PortPath => _device.PortPath;

    /// <summary>The whole configuration descriptor as read, for a class driver that parses more than the kit does.</summary>
    public ReadOnlySpan<byte> Configuration => _device.Configuration;

    /// <summary>True once the device left the bus. Any context.</summary>
    public bool IsDisconnected => _device.Device.IsDisconnected;

    /// <summary>The first endpoint of that type and direction, or null.</summary>
    /// <param name="type">The transfer type.</param>
    /// <param name="isIn">True for an IN endpoint.</param>
    public UsbEndpoint? FindEndpoint(UsbEndpointType type, bool isIn)
    {
        List<UsbEndpoint> endpoints = _interface.Endpoints;
        for (int i = 0; i < endpoints.Count; i++)
        {
            if (endpoints[i].Type == type && endpoints[i].IsIn == isIn)
            {
                return endpoints[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Runs a control transfer on the device's default pipe and waits for
    /// it: Disconnected at once on a disconnected device; otherwise
    /// serialized with the device's other interfaces and run by the host.
    /// </summary>
    /// <param name="setup">The request; its Length is the data stage size.</param>
    /// <param name="data">The OUT data or the IN buffer, at least <see cref="UsbSetupPacket.Length"/> bytes.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="data"/> is shorter than the setup's Length, or the Length exceeds <see cref="MaxControlTransferLength"/>.</exception>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public UsbTransferStatus ControlTransfer(UsbSetupPacket setup, Span<byte> data)
    {
        InterruptContextGuard.ThrowIfInHandler(nameof(ControlTransfer));
        if (setup.Length > MaxControlTransferLength)
        {
            throw new ArgumentOutOfRangeException(nameof(setup), setup.Length, "The data stage exceeds MaxControlTransferLength.");
        }

        if (data.Length < setup.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(data), data.Length, "The buffer is shorter than the setup's Length.");
        }

        if (_device.Device.IsDisconnected)
        {
            return UsbTransferStatus.Disconnected;
        }

        return _device.ControlTransfer(setup, data);
    }

    /// <summary>A device-to-host control request; the data stage is <paramref name="data"/>.Length bytes.</summary>
    /// <param name="requestType">The type and recipient; the direction is added.</param>
    /// <param name="request">bRequest.</param>
    /// <param name="value">wValue.</param>
    /// <param name="index">wIndex.</param>
    /// <param name="data">Receives the data stage.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="data"/> is longer than <see cref="MaxControlTransferLength"/>.</exception>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public UsbTransferStatus ControlIn(UsbRequestType requestType, byte request, ushort value, ushort index, Span<byte> data)
    {
        // Checked before the cast to the setup's ushort Length, which a
        // longer span would wrap.
        ArgumentOutOfRangeException.ThrowIfGreaterThan(data.Length, MaxControlTransferLength, nameof(data));
        return ControlTransfer(new UsbSetupPacket(requestType | UsbRequestType.DeviceToHost, request, value, index, (ushort)data.Length), data);
    }

    /// <summary>A host-to-device control request with no data stage.</summary>
    /// <param name="requestType">The type and recipient.</param>
    /// <param name="request">bRequest.</param>
    /// <param name="value">wValue.</param>
    /// <param name="index">wIndex.</param>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public UsbTransferStatus ControlOut(UsbRequestType requestType, byte request, ushort value, ushort index) =>
        ControlTransfer(new UsbSetupPacket(requestType, request, value, index, 0), Span<byte>.Empty);

    /// <summary>
    /// A host-to-device control request whose data stage is a copy of
    /// <paramref name="data"/>, in a buffer the kit owns for the call (a
    /// probe may allocate).
    /// </summary>
    /// <param name="requestType">The type and recipient.</param>
    /// <param name="request">bRequest.</param>
    /// <param name="value">wValue.</param>
    /// <param name="index">wIndex.</param>
    /// <param name="data">The data stage.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="data"/> is longer than <see cref="MaxControlTransferLength"/>.</exception>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public UsbTransferStatus ControlOut(UsbRequestType requestType, byte request, ushort value, ushort index, ReadOnlySpan<byte> data)
    {
        // Checked before the copy and the cast to the setup's ushort Length.
        ArgumentOutOfRangeException.ThrowIfGreaterThan(data.Length, MaxControlTransferLength, nameof(data));
        byte[] buffer = new byte[data.Length];
        data.CopyTo(buffer);
        return ControlTransfer(new UsbSetupPacket(requestType, request, value, index, (ushort)data.Length), buffer);
    }

    /// <summary>GET_DESCRIPTOR of a standard descriptor into <paramref name="buffer"/>.</summary>
    /// <param name="type">The descriptor type.</param>
    /// <param name="index">The descriptor index.</param>
    /// <param name="buffer">Receives the descriptor; its length is the most read.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="buffer"/> is longer than <see cref="MaxControlTransferLength"/>.</exception>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public UsbTransferStatus GetDescriptor(UsbDescriptorType type, byte index, Span<byte> buffer)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(buffer.Length, MaxControlTransferLength, nameof(buffer));
        return ControlIn(UsbRequestType.Standard | UsbRequestType.Device, (byte)UsbStandardRequest.GetDescriptor, (ushort)(((byte)type << 8) | index), 0, buffer);
    }

    /// <summary>
    /// Opens an interrupt IN endpoint: the host keeps transfers queued on it
    /// and hands every completed one to <paramref name="handler"/> until the
    /// pipe is closed. The pipe goes on <paramref name="binding"/>'s ledger
    /// and is closed in its unwind, or earlier through <see cref="ClosePipe"/>.
    /// </summary>
    /// <param name="binding">The driver's binding on this node.</param>
    /// <param name="endpoint">One of <see cref="Endpoints"/>, an interrupt IN endpoint.</param>
    /// <param name="handler">Receives each report; see <see cref="UsbReportHandler"/> for its contract.</param>
    /// <param name="pipe">The pipe, when opened.</param>
    /// <returns>False with <paramref name="pipe"/> null when the device is disconnected or the host refused.</returns>
    /// <exception cref="ArgumentException"><paramref name="binding"/> is bound to another node, or <paramref name="endpoint"/> is not one of this interface's interrupt IN endpoints.</exception>
    /// <exception cref="InvalidOperationException">The binding is being torn down (the pipe is closed again first), or the caller is an interrupt handler.</exception>
    public bool OpenInterruptPipe(DeviceBinding binding, UsbEndpoint endpoint, UsbReportHandler handler, [NotNullWhen(true)] out UsbPipe? pipe)
    {
        InterruptContextGuard.ThrowIfInHandler(nameof(OpenInterruptPipe));
        ThrowIfNotOwner(binding);
        ThrowIfNotOurs(endpoint);
        if (endpoint.Type != UsbEndpointType.Interrupt || !endpoint.IsIn)
        {
            throw new ArgumentException("The endpoint is not an interrupt IN endpoint.", nameof(endpoint));
        }

        ArgumentNullException.ThrowIfNull(handler);
        pipe = null;
        if (_device.Device.IsDisconnected)
        {
            return false;
        }

        UsbPipe? opened = _device.Device.OpenInterruptPipe(endpoint, handler);
        if (opened is null)
        {
            return false;
        }

        binding.RecordPipe(new UsbPipeResource(_device.Device, opened, binding), nameof(OpenInterruptPipe));
        pipe = opened;
        return true;
    }

    /// <summary>
    /// Opens a bulk endpoint, either direction, for <see cref="BulkIn"/> or
    /// <see cref="BulkOut"/>. The pipe goes on <paramref name="binding"/>'s
    /// ledger and is closed in its unwind, or earlier through
    /// <see cref="ClosePipe"/>.
    /// </summary>
    /// <param name="binding">The driver's binding on this node.</param>
    /// <param name="endpoint">One of <see cref="Endpoints"/>, a bulk endpoint.</param>
    /// <param name="pipe">The pipe, when opened.</param>
    /// <returns>False with <paramref name="pipe"/> null when the device is disconnected or the host refused.</returns>
    /// <exception cref="ArgumentException"><paramref name="binding"/> is bound to another node, or <paramref name="endpoint"/> is not one of this interface's bulk endpoints.</exception>
    /// <exception cref="InvalidOperationException">The binding is being torn down (the pipe is closed again first), or the caller is an interrupt handler.</exception>
    public bool OpenBulkPipe(DeviceBinding binding, UsbEndpoint endpoint, [NotNullWhen(true)] out UsbPipe? pipe)
    {
        InterruptContextGuard.ThrowIfInHandler(nameof(OpenBulkPipe));
        ThrowIfNotOwner(binding);
        ThrowIfNotOurs(endpoint);
        if (endpoint.Type != UsbEndpointType.Bulk)
        {
            throw new ArgumentException("The endpoint is not a bulk endpoint.", nameof(endpoint));
        }

        pipe = null;
        if (_device.Device.IsDisconnected)
        {
            return false;
        }

        UsbPipe? opened = _device.Device.OpenBulkPipe(endpoint);
        if (opened is null)
        {
            return false;
        }

        binding.RecordPipe(new UsbPipeResource(_device.Device, opened, binding), nameof(OpenBulkPipe));
        pipe = opened;
        return true;
    }

    /// <summary>
    /// Closes a pipe before the unwind would: takes it off the binding's
    /// ledger and closes it on the host unless the host already did. A
    /// pipe already closed is taken off the ledger and otherwise ignored.
    /// </summary>
    /// <param name="binding">The binding that opened the pipe.</param>
    /// <param name="pipe">A pipe from <see cref="OpenInterruptPipe"/> or <see cref="OpenBulkPipe"/>.</param>
    /// <exception cref="ArgumentException"><paramref name="pipe"/> was not opened through <paramref name="binding"/>.</exception>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public void ClosePipe(DeviceBinding binding, UsbPipe pipe)
    {
        InterruptContextGuard.ThrowIfInHandler(nameof(ClosePipe));
        ArgumentNullException.ThrowIfNull(binding);
        ArgumentNullException.ThrowIfNull(pipe);
        UsbPipeResource record = binding.FindPipe(pipe) ?? throw new ArgumentException("The pipe was not opened through this binding.", nameof(pipe));
        binding.RemovePipe(record);
        record.Release();
    }

    /// <summary>
    /// Reads from a bulk IN pipe and waits; ends early on a short packet.
    /// One caller per pipe at a time is the driver's rule.
    /// </summary>
    /// <param name="pipe">A bulk IN pipe of this interface.</param>
    /// <param name="data">Receives the data; its length is the most the transfer reads.</param>
    /// <param name="transferred">Bytes received, set on failure too.</param>
    /// <returns>Error with 0 transferred on a closed pipe; Disconnected on a disconnected device; the host's status otherwise.</returns>
    /// <exception cref="ArgumentException"><paramref name="pipe"/>'s endpoint is not one of this interface's.</exception>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public UsbTransferStatus BulkIn(UsbPipe pipe, Span<byte> data, out int transferred)
    {
        InterruptContextGuard.ThrowIfInHandler(nameof(BulkIn));
        ThrowIfNotOurs(pipe);
        if (pipe.IsClosed)
        {
            transferred = 0;
            return UsbTransferStatus.Error;
        }

        if (_device.Device.IsDisconnected)
        {
            transferred = 0;
            return UsbTransferStatus.Disconnected;
        }

        return _device.Device.BulkIn(pipe, data, out transferred);
    }

    /// <summary>
    /// Writes <paramref name="data"/> to a bulk OUT pipe and waits; ends
    /// early on a short packet. One caller per pipe at a time is the
    /// driver's rule.
    /// </summary>
    /// <param name="pipe">A bulk OUT pipe of this interface.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="transferred">Bytes the device accepted, set on failure too.</param>
    /// <returns>Error with 0 transferred on a closed pipe; Disconnected on a disconnected device; the host's status otherwise.</returns>
    /// <exception cref="ArgumentException"><paramref name="pipe"/>'s endpoint is not one of this interface's.</exception>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public UsbTransferStatus BulkOut(UsbPipe pipe, ReadOnlySpan<byte> data, out int transferred)
    {
        InterruptContextGuard.ThrowIfInHandler(nameof(BulkOut));
        ThrowIfNotOurs(pipe);
        if (pipe.IsClosed)
        {
            transferred = 0;
            return UsbTransferStatus.Error;
        }

        if (_device.Device.IsDisconnected)
        {
            transferred = 0;
            return UsbTransferStatus.Disconnected;
        }

        return _device.Device.BulkOut(pipe, data, out transferred);
    }

    /// <summary>
    /// Clears a halted endpoint on both sides (USB 2.0 section 9.4.5): the
    /// device through CLEAR_FEATURE(ENDPOINT_HALT), which also restarts its
    /// data toggle, then the host side so both toggles agree again.
    /// </summary>
    /// <param name="pipe">The halted pipe.</param>
    /// <returns>Error when the host-side reset fails; the control transfer's status otherwise.</returns>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public UsbTransferStatus ClearHalt(UsbPipe pipe)
    {
        InterruptContextGuard.ThrowIfInHandler(nameof(ClearHalt));
        ArgumentNullException.ThrowIfNull(pipe);
        UsbTransferStatus status = ControlOut(UsbRequestType.Standard | UsbRequestType.Endpoint, (byte)UsbStandardRequest.ClearFeature, EndpointHaltFeature, pipe.Endpoint.Address);
        if (!_device.Device.ResetEndpoint(pipe))
        {
            return UsbTransferStatus.Error;
        }

        return status;
    }

    /// <summary>Tells the host this device is a hub, so it can route transactions to the devices behind it.</summary>
    /// <param name="portCount">bNbrPorts from the hub descriptor.</param>
    /// <param name="thinkTime">TT think time from wHubCharacteristics bits 6:5 (high-speed hubs only).</param>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public bool ConfigureAsHub(byte portCount, byte thinkTime)
    {
        InterruptContextGuard.ThrowIfInHandler(nameof(ConfigureAsHub));
        return _device.Device.ConfigureAsHub(portCount, thinkTime);
    }

    /// <summary>
    /// Attaches the device on <paramref name="port"/> of this hub: the
    /// enumeration core with this interface's device as the parent hub and
    /// <paramref name="binding"/> as the owner of the published nodes.
    /// Thread context: the hub driver's probe (the children are then
    /// offered after the probe, as the publish from the worker queues them)
    /// or its driver thread (the children are bound before this returns).
    /// </summary>
    /// <param name="binding">The hub driver's binding on this node.</param>
    /// <param name="port">The 1-based port number on the hub.</param>
    /// <param name="speed">The speed the port reported.</param>
    /// <returns>False when enumeration failed, or when the binding began detaching before the nodes were published; logged through the binding either way.</returns>
    /// <exception cref="ArgumentException"><paramref name="binding"/> is bound to another node.</exception>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public bool AttachChild(DeviceBinding binding, byte port, UsbSpeed speed)
    {
        InterruptContextGuard.ThrowIfInHandler(nameof(AttachChild));
        ThrowIfNotOwner(binding);
        return UsbEnumeration.Attach(_device.Bus, binding, _device, port, speed);
    }

    /// <summary>
    /// Detaches the device on <paramref name="port"/> of this hub: no device
    /// on that port returns; else the detach half of the enumeration core.
    /// Thread context, the hub's driver thread; it returns once every
    /// interface node of the device and of its subtree was torn down and
    /// every device released, or at once when <paramref name="binding"/> is
    /// itself being torn down: its teardown then finishes the nodes and the
    /// enclosing detach releases the devices.
    /// </summary>
    /// <param name="binding">The hub driver's binding on this node.</param>
    /// <param name="port">The 1-based port number on the hub.</param>
    /// <exception cref="ArgumentException"><paramref name="binding"/> is bound to another node.</exception>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public void DetachChild(DeviceBinding binding, byte port)
    {
        InterruptContextGuard.ThrowIfInHandler(nameof(DetachChild));
        ThrowIfNotOwner(binding);
        UsbDeviceState? child = _device.Bus.Find(_device, port);
        if (child is null)
        {
            return;
        }

        UsbEnumeration.Detach(_device.Bus, binding, child);
    }

    private void ThrowIfNotOwner(DeviceBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);
        if (!ReferenceEquals(binding.Node.AccessObject, this))
        {
            throw new ArgumentException("The binding is bound to another node.", nameof(binding));
        }
    }

    private void ThrowIfNotOurs(UsbEndpoint endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        if (!Owns(endpoint))
        {
            throw new ArgumentException("The endpoint is not one of this interface's.", nameof(endpoint));
        }
    }

    private void ThrowIfNotOurs(UsbPipe pipe)
    {
        ArgumentNullException.ThrowIfNull(pipe);
        if (!Owns(pipe.Endpoint))
        {
            throw new ArgumentException("The pipe is not one of this interface's.", nameof(pipe));
        }
    }

    /// <summary>True when <paramref name="endpoint"/> is one of this interface's, by reference.</summary>
    private bool Owns(UsbEndpoint endpoint)
    {
        List<UsbEndpoint> endpoints = _interface.Endpoints;
        for (int i = 0; i < endpoints.Count; i++)
        {
            if (ReferenceEquals(endpoints[i], endpoint))
            {
                return true;
            }
        }

        return false;
    }
}
