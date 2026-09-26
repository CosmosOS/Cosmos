// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.Devices.Usb;
using Cosmos.Kernel.HAL.Drivers.Engine;

namespace Cosmos.Kernel.HAL.Drivers.Usb;

/// <summary>
/// A USB driver's handle on the interface it was offered: what the device
/// and the interface declare, control requests on the device's default
/// pipe, and, during Probe only, the interface's interrupt IN and bulk
/// endpoints. The host controller cannot close an endpoint once opened,
/// and an interrupt IN pipe starts transferring at once, so opening one
/// commits the interface: if the probe then declines or fails, the kit
/// offers the interface to no other driver. When an attempt is declined or
/// fails, the kit disarms its report handlers, drops its publications and
/// work items, cancels its events, and invalidates its bulk pipes and the
/// context, in that order. When the device of a bound interface leaves the
/// bus, the kit marks the context not present and disarms its report
/// handlers, withdraws its publications from the managers, drops its work
/// items, cancels its events and waits for a work item still running, calls
/// the driver's Remove, and ends the binding: from then on every transfer
/// answers <see cref="UsbTransferStatus.Disconnected"/>.
/// </summary>
internal sealed class UsbDeviceContext : DeviceContext
{
    /// <summary>
    /// Most bytes one control request moves in this version: the host
    /// controller stages the data through one 4 KiB page. Every standard
    /// descriptor, and every class request a boot device needs, fits.
    /// </summary>
    private const int MaximumControlDataLength = 4096;

    private readonly UsbDevice _device;
    private readonly UsbInterface _interface;

    // What the attempt opened. The trampolines are armed on Bound and
    // disarmed by teardown or unplug; the pipes are invalidated by teardown.
    // Null until the first one: most attempts open one endpoint at most.
    private List<UsbReportTrampoline>? _reportTrampolines;
    private List<byte>? _interruptEndpoints;
    private List<UsbBulkPipe>? _bulkPipes;

    /// <summary>The device the interface belongs to, with every interface of its active configuration.</summary>
    public UsbDeviceInfo Device { get; }

    /// <summary>The interface on offer, one of <see cref="UsbDeviceInfo.Interfaces"/>.</summary>
    public UsbInterfaceInfo Interface { get; }

    /// <summary>
    /// True once the attempt opened an endpoint, interrupt or bulk. The host
    /// controller cannot close one, so a declined or failed attempt that
    /// opened one ends the offering of the interface.
    /// </summary>
    internal bool OpenedPipe { get; private set; }

    /// <summary>
    /// The driver instance the registration's factory created for this
    /// attempt, whose Remove the kit calls when a bound interface's device
    /// leaves the bus; null until the factory returned.
    /// </summary>
    internal UsbDriver? Driver { get; set; }

    /// <summary>Builds the context of one binding attempt on <paramref name="usbInterface"/> of <paramref name="device"/>.</summary>
    /// <param name="driverName">The candidate registration's name.</param>
    /// <param name="path">The interface's path, <c>usb/bus-ports:configuration.interface</c>.</param>
    /// <param name="device">The configured device.</param>
    /// <param name="usbInterface">The interface on offer, which no driver owns.</param>
    internal UsbDeviceContext(string driverName, string path, UsbDevice device, UsbInterface usbInterface)
        : base(driverName, path)
    {
        _device = device;
        _interface = usbInterface;
        Device = new UsbDeviceInfo(device);
        Interface = Device.Describe(usbInterface);
    }

    /// <summary>
    /// Runs a device-to-host control request on the device's default pipe
    /// and waits for it. Thread context only.
    /// </summary>
    /// <param name="kind">Who defines the request.</param>
    /// <param name="recipient">What the request is addressed to.</param>
    /// <param name="request">bRequest.</param>
    /// <param name="value">wValue.</param>
    /// <param name="index">wIndex: for an interface or endpoint request, its number or address.</param>
    /// <param name="data">Receives the data stage; its length is wLength, at most 4096 bytes.</param>
    /// <returns>
    /// The status, and how many bytes the device sent, at the start of
    /// <paramref name="data"/>: fewer than asked for when it answered with
    /// less. <see cref="UsbTransferStatus.Disconnected"/> once the device
    /// left the bus.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="data"/> is longer than 4096 bytes, or
    /// <paramref name="kind"/> or <paramref name="recipient"/> is not a
    /// defined value.
    /// </exception>
    /// <exception cref="InvalidOperationException">The binding attempt was declined or failed.</exception>
    public UsbTransferResult ControlIn(UsbRequestKind kind, UsbRecipient recipient, byte request, ushort value, ushort index, Span<byte> data)
    {
        ThrowIfTornDown();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(data.Length, MaximumControlDataLength, nameof(data));
        UsbRequestType requestType = RequestType(kind, recipient) | UsbRequestType.DeviceToHost;
        if (!IsPresent)
        {
            return new UsbTransferResult(UsbTransferStatus.Disconnected, 0);
        }

        UsbTransferStatus status = _device.ControlTransfer(
            new UsbSetupPacket(requestType, request, value, index, (ushort)data.Length), data, out int transferred);
        return new UsbTransferResult(status, transferred);
    }

    /// <summary>
    /// Runs a host-to-device control request with no data stage on the
    /// device's default pipe and waits for it. Thread context only.
    /// </summary>
    /// <param name="kind">Who defines the request.</param>
    /// <param name="recipient">What the request is addressed to.</param>
    /// <param name="request">bRequest.</param>
    /// <param name="value">wValue.</param>
    /// <param name="index">wIndex: for an interface or endpoint request, its number or address.</param>
    /// <returns>The status; <see cref="UsbTransferStatus.Disconnected"/> once the device left the bus.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="kind"/> or <paramref name="recipient"/> is not a defined value.</exception>
    /// <exception cref="InvalidOperationException">The binding attempt was declined or failed.</exception>
    public UsbTransferStatus ControlOut(UsbRequestKind kind, UsbRecipient recipient, byte request, ushort value, ushort index) =>
        ControlOut(kind, recipient, request, value, index, ReadOnlySpan<byte>.Empty);

    /// <summary>
    /// Runs a host-to-device control request on the device's default pipe,
    /// sending <paramref name="data"/> as its data stage, and waits for it.
    /// Thread context only.
    /// </summary>
    /// <param name="kind">Who defines the request.</param>
    /// <param name="recipient">What the request is addressed to.</param>
    /// <param name="request">bRequest.</param>
    /// <param name="value">wValue.</param>
    /// <param name="index">wIndex: for an interface or endpoint request, its number or address.</param>
    /// <param name="data">The data stage; its length is wLength, at most 4096 bytes.</param>
    /// <returns>The status; <see cref="UsbTransferStatus.Disconnected"/> once the device left the bus.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="data"/> is longer than 4096 bytes, or
    /// <paramref name="kind"/> or <paramref name="recipient"/> is not a
    /// defined value.
    /// </exception>
    /// <exception cref="InvalidOperationException">The binding attempt was declined or failed.</exception>
    public UsbTransferStatus ControlOut(UsbRequestKind kind, UsbRecipient recipient, byte request, ushort value, ushort index, ReadOnlySpan<byte> data)
    {
        ThrowIfTornDown();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(data.Length, MaximumControlDataLength, nameof(data));
        UsbRequestType requestType = RequestType(kind, recipient);
        if (!IsPresent)
        {
            return UsbTransferStatus.Disconnected;
        }

        // The host controller takes one span for both directions and only
        // reads it for a host-to-device request, so the caller's read-only
        // data is never written.
        Span<byte> stage = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(data), data.Length);
        return _device.ControlTransfer(new UsbSetupPacket(requestType, request, value, index, (ushort)data.Length), stage);
    }

    /// <summary>
    /// Opens an interrupt IN endpoint of this interface and hands every
    /// report the device sends on it to <paramref name="handler"/>, from the
    /// moment Probe returns Bound. The endpoint starts transferring at once,
    /// so reports arrive during Probe too; those, and any once the attempt
    /// is declined or fails, are dropped. An attempt that opened an endpoint
    /// and does not bind leaves the interface without a driver. Probe only.
    /// </summary>
    /// <param name="endpoint">An interrupt IN endpoint of <see cref="Interface"/>, as <see cref="UsbInterfaceInfo.TryFindEndpoint"/> returns it.</param>
    /// <param name="handler">Called in interrupt context for every report; see <see cref="UsbReportHandler"/> for what it may do.</param>
    /// <returns>
    /// False when <paramref name="endpoint"/> is not an interrupt IN
    /// endpoint of this interface, or when the host controller refused to
    /// open it.
    /// </returns>
    /// <exception cref="InvalidOperationException">Called outside the driver's Probe, or the endpoint is already open.</exception>
    public bool OpenInterruptIn(UsbEndpointInfo endpoint, UsbReportHandler handler)
    {
        ThrowIfNotProbing(nameof(OpenInterruptIn));
        ArgumentNullException.ThrowIfNull(handler);

        UsbEndpoint? found = FindEndpoint(endpoint.Address);
        if (found is null || found.Type != UsbEndpointType.Interrupt || !found.IsIn)
        {
            WriteLog($"endpoint 0x{endpoint.Address:X2} is not an interrupt IN endpoint of interface {_interface.Number}");
            return false;
        }

        // Opening it again would have the host controller reconfigure a
        // running endpoint under the first handler.
        if (IsInterruptEndpointOpen(found.Address))
        {
            throw new InvalidOperationException($"Interrupt endpoint 0x{found.Address:X2} is already open.");
        }

        UsbReportTrampoline trampoline = new(handler, DriverName, Path);
        if (!_device.OpenInterruptPipe(found, trampoline.OnReport))
        {
            WriteLog($"the host controller refused to open interrupt endpoint 0x{found.Address:X2}");
            return false;
        }

        OpenedPipe = true;
        (_reportTrampolines ??= []).Add(trampoline);
        (_interruptEndpoints ??= []).Add(found.Address);
        return true;
    }

    /// <summary>
    /// Opens a bulk endpoint of this interface, IN or OUT. Asking again for
    /// the same endpoint returns the same pipe. An attempt that opened an
    /// endpoint and does not bind leaves the interface without a driver.
    /// Probe only.
    /// </summary>
    /// <param name="endpoint">A bulk endpoint of <see cref="Interface"/>, as <see cref="UsbInterfaceInfo.TryFindEndpoint"/> returns it.</param>
    /// <param name="pipe">The open pipe when the call returns true.</param>
    /// <returns>
    /// False when <paramref name="endpoint"/> is not a bulk endpoint of this
    /// interface, or when the host controller refused to open it.
    /// </returns>
    /// <exception cref="InvalidOperationException">Called outside the driver's Probe.</exception>
    public bool TryOpenBulk(UsbEndpointInfo endpoint, [NotNullWhen(true)] out UsbBulkPipe? pipe)
    {
        ThrowIfNotProbing(nameof(TryOpenBulk));

        pipe = null;
        UsbEndpoint? found = FindEndpoint(endpoint.Address);
        if (found is null || found.Type != UsbEndpointType.Bulk)
        {
            WriteLog($"endpoint 0x{endpoint.Address:X2} is not a bulk endpoint of interface {_interface.Number}");
            return false;
        }

        if (_bulkPipes is { } opened)
        {
            for (int i = 0; i < opened.Count; i++)
            {
                if (opened[i].EndpointAddress == found.Address)
                {
                    pipe = opened[i];
                    return true;
                }
            }
        }

        if (!_device.OpenBulkEndpoint(found))
        {
            WriteLog($"the host controller refused to open bulk endpoint 0x{found.Address:X2}");
            return false;
        }

        OpenedPipe = true;
        pipe = new UsbBulkPipe(_device, found);
        (_bulkPipes ??= []).Add(pipe);
        return true;
    }

    /// <summary>
    /// Releases what a declined or failed attempt holds, and makes every
    /// later use of the context and its bulk pipes throw. The endpoints it
    /// opened stay open, since the host controller cannot close them: a
    /// report on an interrupt endpoint stops at its disarmed trampoline, and
    /// the caller offers the interface to no other driver.
    /// </summary>
    internal void TearDown()
    {
        State = DeviceContextState.TornDown;

        // 1. Disarm: no report reaches the driver's handler from here on.
        DisarmReports();

        // 2. What Probe published never reaches a manager, work scheduled
        // during Probe never runs, and a thread waiting on one of the
        // attempt's events is told it is over.
        DropQueuedAndCancelEvents();

        // 3. The pipes last, like the regions of a PCI attempt: a driver
        // still holding one gets an exception instead of moving data on an
        // interface that is no longer its own.
        if (_bulkPipes is { } pipes)
        {
            for (int i = 0; i < pipes.Count; i++)
            {
                pipes[i].Invalidate();
            }
        }
    }

    /// <summary>
    /// The first three steps of the unplug teardown, for a bound interface
    /// whose device left the bus, on the hot-plug thread and before the host
    /// controller frees the device's pipes. The kit then calls the driver's
    /// Remove, and <see cref="MarkRemoved"/> ends the binding.
    /// </summary>
    internal void Unplug()
    {
        // 1. Not present: every transfer answers Disconnected from here on,
        // and no report reaches the driver's handlers any more, including
        // one the host controller delivers before it frees the pipe.
        MarkNotPresent();
        DisarmReports();

        // 2. The mouse and the network link the driver published leave
        // their managers, silenced first.
        WithdrawPublications();

        // 3. No work item of the binding starts again and no Wait on its
        // events blocks; one already running may still be using the device's
        // state, so Remove waits for it, for a while.
        DropWorkAndCancelEvents();
        WaitForRunningWorkItem();
    }

    /// <summary>
    /// The last step of the unplug teardown, once the driver's Remove
    /// returned: the binding is over. What the context still offers answers
    /// as for a device that is gone: transfers, through the context or its
    /// bulk pipes, return <see cref="UsbTransferStatus.Disconnected"/>, and
    /// the Probe-only members throw.
    /// </summary>
    internal void MarkRemoved() => State = DeviceContextState.Removed;

    /// <summary>Lets the reports of the endpoints Probe opened through to the driver's handlers, now that Probe returned Bound.</summary>
    private protected override void ArmInterrupts()
    {
        if (_reportTrampolines is not { } trampolines)
        {
            return;
        }

        for (int i = 0; i < trampolines.Count; i++)
        {
            trampolines[i].Arm();
        }
    }

    private void DisarmReports()
    {
        if (_reportTrampolines is not { } trampolines)
        {
            return;
        }

        for (int i = 0; i < trampolines.Count; i++)
        {
            trampolines[i].Disarm();
        }
    }

    private bool IsInterruptEndpointOpen(byte address)
    {
        if (_interruptEndpoints is not { } opened)
        {
            return false;
        }

        for (int i = 0; i < opened.Count; i++)
        {
            if (opened[i] == address)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The endpoint of this interface at <paramref name="address"/>. A
    /// driver names an endpoint by the <see cref="UsbEndpointInfo"/> it was
    /// given, and the address is what identifies it within the interface.
    /// </summary>
    private UsbEndpoint? FindEndpoint(byte address)
    {
        List<UsbEndpoint> endpoints = _interface.Endpoints;
        for (int i = 0; i < endpoints.Count; i++)
        {
            if (endpoints[i].Address == address)
            {
                return endpoints[i];
            }
        }

        return null;
    }

    /// <summary>bmRequestType's type and recipient fields for a request of <paramref name="kind"/> to <paramref name="recipient"/>.</summary>
    private static UsbRequestType RequestType(UsbRequestKind kind, UsbRecipient recipient)
    {
        UsbRequestType type = kind switch
        {
            UsbRequestKind.Standard => UsbRequestType.Standard,
            UsbRequestKind.Class => UsbRequestType.Class,
            UsbRequestKind.Vendor => UsbRequestType.Vendor,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

        UsbRequestType target = recipient switch
        {
            UsbRecipient.Device => UsbRequestType.Device,
            UsbRecipient.Interface => UsbRequestType.Interface,
            UsbRecipient.Endpoint => UsbRequestType.Endpoint,
            UsbRecipient.Other => UsbRequestType.Other,
            _ => throw new ArgumentOutOfRangeException(nameof(recipient))
        };

        return type | target;
    }
}
