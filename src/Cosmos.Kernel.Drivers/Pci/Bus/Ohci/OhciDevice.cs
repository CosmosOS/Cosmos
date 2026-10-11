// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;
using Cosmos.Kernel.HAL.DriverKit.Threading;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// An addressed device of an <see cref="OhciState"/>: the controller's
/// view of a <see cref="UsbDevice"/>. Owns the device's memory set (its
/// control ED and TDs, its SETUP slot and bounce page), the control
/// transfer wait state and the pipes opened on it, indexed by endpoint
/// (number times two, plus one for IN). One object per address path,
/// dropped at release: the memory set is pooled, the object never is.
/// Every <c>Core</c> member forwards to the method of
/// <see cref="OhciState"/> that takes the device; the execution context is
/// the one <see cref="UsbDevice"/> states for it. A full-speed host needs
/// no routing fields: no transaction translator sits between it and a
/// device.
/// </summary>
internal sealed class OhciDevice : UsbDevice
{
    /// <summary>Set by the drain once the control transfer's STATUS TD retired, or any stage failed, after <see cref="ControlCode"/> and <see cref="ControlRemaining"/>; cleared by the issuer under the lock. Read and written with <c>Volatile</c>.</summary>
    public bool ControlCompleted;

    /// <summary>Creates the device over its memory set. Thread context, the host's address path.</summary>
    /// <param name="controller">The host controller addressing it.</param>
    /// <param name="parent">The hub the device hangs off, or null for a root port.</param>
    /// <param name="port">The 1-based port number on the parent or the controller.</param>
    /// <param name="speed">The speed the port reported.</param>
    /// <param name="address">The USB address the device will answer at.</param>
    /// <param name="memory">The device's pooled memory set, reset.</param>
    public OhciDevice(OhciState controller, OhciDevice? parent, byte port, UsbSpeed speed, byte address, OhciDeviceMemory memory)
        : base(controller, parent, port, speed)
    {
        Controller = controller;
        Address = address;
        Memory = memory;
        MaxPacketSize0 = OhciProtocol.DefaultMaxPacketSize0;
        ControlSetupIndex = -1;
        ControlDataIndex = -1;
        ControlStatusIndex = -1;
    }

    /// <summary>The host controller, typed.</summary>
    internal OhciState Controller { get; }

    /// <summary>The USB address, 1 to 127.</summary>
    public byte Address { get; }

    /// <summary>The device's pooled memory set.</summary>
    public OhciDeviceMemory Memory { get; }

    /// <summary>A control transfer failed and left the control ED halted (H set); the next transfer quiesces it first. Written by the drain and the issuing thread.</summary>
    public bool ControlHalted { get; set; }

    /// <summary>The event a control transfer's completion signals.</summary>
    public DeviceEvent ControlEvent => Memory.ControlEvent;

    /// <summary>The condition code of the TD that finished the last control transfer; written before <see cref="ControlCompleted"/>.</summary>
    public OhciConditionCode ControlCode { get; set; }

    /// <summary>Bytes of the DATA stage the device did not move, from the TD's final CBP; written before <see cref="ControlCompleted"/>.</summary>
    public int ControlRemaining { get; set; }

    /// <summary>True while a thread's control transfer is on the ED and waits; the drain completes it. Under the lock or in the drain.</summary>
    public bool ControlPending { get; set; }

    /// <summary>TD slot index of the pending SETUP stage, -1 when none. Under the lock or in the drain.</summary>
    public int ControlSetupIndex { get; set; }

    /// <summary>TD slot index of the pending DATA stage, -1 without one. Under the lock or in the drain.</summary>
    public int ControlDataIndex { get; set; }

    /// <summary>TD slot index of the pending STATUS stage, -1 when none. Under the lock or in the drain.</summary>
    public int ControlStatusIndex { get; set; }

    /// <summary>True while a thread's control transfer owns the control ED. Under the lock.</summary>
    public bool ControlBusy { get; set; }

    /// <summary>The device is a hub, as <see cref="ConfigureAsHubCore"/> recorded.</summary>
    public bool IsHub { get; private set; }

    /// <summary>bNbrPorts of a hub.</summary>
    public byte PortCount { get; private set; }

    /// <summary>TT think time of a hub; unused on a full-speed host.</summary>
    public byte ThinkTime { get; private set; }

    /// <summary>The open interrupt pipes by endpoint index. Written under the lock; read by the drain without it.</summary>
    public OhciInterruptPipe?[] InterruptPipes { get; } = new OhciInterruptPipe?[OhciProtocol.MaxEndpointIndex + 1];

    /// <summary>The open bulk pipes by endpoint index. Written under the lock; read by the drain and by <see cref="OnDisconnectedCore"/> without it.</summary>
    public OhciBulkPipe?[] BulkPipes { get; } = new OhciBulkPipe?[OhciProtocol.MaxEndpointIndex + 1];

    /// <summary>Index of an endpoint in the pipe tables: its number times 2, plus 1 for IN.</summary>
    /// <param name="endpoint">The endpoint.</param>
    public static byte EndpointIndex(UsbEndpoint endpoint)
    {
        return (byte)((endpoint.Number * 2) + (endpoint.IsIn ? 1 : 0));
    }

    /// <summary>Records bMaxPacketSize0 once the device descriptor gave it and the ED took it. Thread context, the host's address path.</summary>
    /// <param name="value">The packet size in bytes.</param>
    internal void SetMaxPacketSize0(ushort value)
    {
        MaxPacketSize0 = value;
    }

    /// <summary>Records what the hub driver read from the hub descriptor. Thread context.</summary>
    internal void SetHub(byte portCount, byte thinkTime)
    {
        IsHub = true;
        PortCount = portCount;
        ThinkTime = thinkTime;
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
        OhciBulkPipe?[] pipes = BulkPipes;
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
