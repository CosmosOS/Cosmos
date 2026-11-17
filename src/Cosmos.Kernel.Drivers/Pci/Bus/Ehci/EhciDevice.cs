// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;
using Cosmos.Kernel.HAL.DriverKit.Threading;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ehci;

/// <summary>
/// An addressed device of an <see cref="EhciState"/>: the controller's
/// view of a <see cref="UsbDevice"/>. Owns the device's memory set (its
/// control queue head, qTD slots, SETUP slot and bounce page), the control
/// transfer wait state, the hub facts and the pipes opened on it, indexed
/// by endpoint id. Its transaction translator fields are derived from the
/// parent hub at construction. One object per address, dropped at release:
/// the memory set is pooled, the object never is. Every <c>Core</c> member
/// forwards to the method of <see cref="EhciState"/> that takes the device;
/// the execution context is the one <see cref="UsbDevice"/> states for it.
/// </summary>
internal sealed class EhciDevice : UsbDevice
{
    /// <summary>Endpoint id of the default control endpoint, number 0 OUT.</summary>
    public const byte ControlEndpointId = 0;

    // Default bMaxPacketSize0 before the device descriptor is read: 8 is
    // valid for every low and full-speed device, and high speed fixes it at
    // 64 (USB 2.0 section 5.5.3).
    private const ushort DefaultMaxPacketSizeLowFull = 8;
    private const ushort DefaultMaxPacketSizeHigh = 64;

    /// <summary>Set by the drain once the control transfer's chain completed, after <see cref="ControlStatus"/> and <see cref="ControlRemaining"/>; cleared by the issuer under the lock. Read and written with <c>Volatile</c>.</summary>
    public bool ControlCompleted;

    /// <summary>Creates the device over its memory set. Thread context, the host's address path.</summary>
    /// <param name="controller">The host controller addressing it.</param>
    /// <param name="parent">The hub the device hangs off, or null for a root port.</param>
    /// <param name="port">The 1-based port number on the parent or the controller.</param>
    /// <param name="speed">The speed the port reported.</param>
    /// <param name="address">The USB address the device will answer at.</param>
    /// <param name="memory">The device's pooled memory set, cleared.</param>
    public EhciDevice(EhciState controller, EhciDevice? parent, byte port, UsbSpeed speed, byte address, EhciDeviceMemory memory)
        : base(controller, parent, port, speed)
    {
        Controller = controller;
        Address = address;
        Memory = memory;

        // A low or full-speed device talks through the transaction
        // translator of the nearest high-speed hub above it; the port is
        // the one of that hub's child on the path down to this device.
        if (speed is UsbSpeed.Low or UsbSpeed.Full && parent is not null)
        {
            EhciDevice hub = parent;
            byte hubPort = port;
            while (hub.Speed != UsbSpeed.High && hub.Parent is EhciDevice above)
            {
                hubPort = hub.PortNumber;
                hub = above;
            }

            if (hub.Speed == UsbSpeed.High)
            {
                TtHubAddress = hub.Address;
                TtPort = hubPort;
            }
        }

        MaxPacketSize0 = speed == UsbSpeed.High ? DefaultMaxPacketSizeHigh : DefaultMaxPacketSizeLowFull;
    }

    /// <summary>The host controller, typed.</summary>
    internal EhciState Controller { get; }

    /// <summary>The USB address, 1 to 127.</summary>
    public byte Address { get; }

    /// <summary>Address of the high-speed hub whose transaction translator serves this device, 0 when none does (a high-speed device, or a root port device).</summary>
    public byte TtHubAddress { get; }

    /// <summary>Port of that hub the device's subtree hangs off.</summary>
    public byte TtPort { get; }

    /// <summary>The device's pooled memory set.</summary>
    public EhciDeviceMemory Memory { get; }

    /// <summary>The event a control transfer's completion signals.</summary>
    public DeviceEvent ControlEvent => Memory.ControlEvent;

    /// <summary>True while a thread's control transfer owns the control queue head. Under the lock.</summary>
    public bool ControlBusy { get; set; }

    /// <summary>A control transfer failed and left the control queue head halted; the next transfer quiesces it first. Written by the issuing thread.</summary>
    public bool ControlHalted { get; set; }

    /// <summary>Count of qTDs in the control chain in flight (2 or 3), 0 when no thread waits. Under the lock or in the drain.</summary>
    public int ControlPendingCount { get; set; }

    /// <summary>Index in the memory set of the chain's first qTD, the SETUP stage. Under the lock or in the drain.</summary>
    public int ControlFirstSlot { get; set; }

    /// <summary>True when the chain in flight carries a DATA stage, the slot after the first. Under the lock or in the drain.</summary>
    public bool ControlHasData { get; set; }

    /// <summary>The whole token of the qTD that ended the transfer, its status byte and CERR read by the status mapping; written before <see cref="ControlCompleted"/>.</summary>
    public uint ControlStatus { get; set; }

    /// <summary>Bytes the DATA stage left untransferred, 0 without one; written before <see cref="ControlCompleted"/>.</summary>
    public int ControlRemaining { get; set; }

    /// <summary>True once the hub driver registered the device as a hub.</summary>
    public bool IsHub { get; set; }

    /// <summary>bNbrPorts of a hub, 0 otherwise.</summary>
    public byte PortCount { get; set; }

    /// <summary>TT think time of a hub, unused by the controller.</summary>
    public byte ThinkTime { get; set; }

    /// <summary>The open interrupt pipes by endpoint id. Written under the lock; read by the drain without it.</summary>
    public EhciInterruptPipe?[] InterruptPipes { get; } = new EhciInterruptPipe?[EhciProtocol.MaxEndpointId + 1];

    /// <summary>The open bulk pipes by endpoint id. Written under the lock; read by the drain and by <see cref="OnDisconnectedCore"/> without it.</summary>
    public EhciBulkPipe?[] BulkPipes { get; } = new EhciBulkPipe?[EhciProtocol.MaxEndpointId + 1];

    /// <summary>Endpoint id of an endpoint: its number times 2, plus 1 for IN.</summary>
    /// <param name="endpoint">The endpoint.</param>
    public static byte EndpointId(UsbEndpoint endpoint)
    {
        return (byte)((endpoint.Number * 2) + (endpoint.IsIn ? 1 : 0));
    }

    /// <summary>Records bMaxPacketSize0 once the device descriptor gave it and the queue head took it. Thread context, the host's address path.</summary>
    /// <param name="value">The packet size in bytes.</param>
    internal void SetMaxPacketSize0(ushort value)
    {
        MaxPacketSize0 = value;
    }

    /// <inheritdoc/>
    protected override void OnDisconnectedCore()
    {
        // Any context, allocation-free, no lock: the handler's port change
        // path and the hot-plug thread's detach reach it. A waiter parked
        // in the binding's wait wakes, re-reads IsDisconnected and returns.
        // The array is read whole: its writers hold the lock, which
        // disables interrupts; a stale entry signals a pooled event, which
        // the next waiter's loop absorbs.
        ControlEvent.Signal();
        EhciBulkPipe?[] pipes = BulkPipes;
        for (int i = 0; i < pipes.Length; i++)
        {
            pipes[i]?.Event.Signal();
        }
    }

    /// <inheritdoc/>
    protected override UsbTransferStatus ControlTransferCore(UsbSetupPacket setup, Span<byte> data)
    {
        return Controller.ControlTransfer(this, setup, data);
    }

    /// <inheritdoc/>
    protected override UsbPipe? OpenInterruptPipeCore(UsbEndpoint endpoint, UsbReportHandler handler)
    {
        return Controller.OpenInterruptPipe(this, endpoint, handler);
    }

    /// <inheritdoc/>
    protected override UsbPipe? OpenBulkPipeCore(UsbEndpoint endpoint)
    {
        return Controller.OpenBulkPipe(this, endpoint);
    }

    /// <inheritdoc/>
    protected override void ClosePipeCore(UsbPipe pipe)
    {
        Controller.ClosePipe(this, pipe);
    }

    /// <inheritdoc/>
    protected override UsbTransferStatus BulkInCore(UsbPipe pipe, Span<byte> data, out int transferred)
    {
        return Controller.BulkIn(this, pipe, data, out transferred);
    }

    /// <inheritdoc/>
    protected override UsbTransferStatus BulkOutCore(UsbPipe pipe, ReadOnlySpan<byte> data, out int transferred)
    {
        return Controller.BulkOut(this, pipe, data, out transferred);
    }

    /// <inheritdoc/>
    protected override bool ResetEndpointCore(UsbPipe pipe)
    {
        return Controller.ResetEndpoint(this, pipe);
    }

    /// <inheritdoc/>
    protected override bool ConfigureAsHubCore(byte portCount, byte thinkTime)
    {
        return Controller.ConfigureAsHub(this, portCount, thinkTime);
    }
}
