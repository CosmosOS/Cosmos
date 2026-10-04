// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Usb;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// A device slot of an <see cref="XhciState"/>: the controller's view of a
/// <see cref="UsbDevice"/>. Owns the slot's memory set (its contexts, its
/// control ring and bounce page), the control transfer wait state, the
/// recovery hand-off of its interrupt pipes and the pipes opened on it,
/// indexed by Device Context Index. Its route string and Transaction
/// Translator fields are derived from the parent hub at construction. One
/// object per Address Device, dropped at release: the memory set is pooled,
/// the object never is. Every <c>Core</c> member forwards to the method of
/// <see cref="XhciState"/> that takes the slot; the execution context is
/// the one <see cref="UsbDevice"/> states for it.
/// </summary>
internal sealed class XhciSlot : UsbDevice
{
    /// <summary>Device Context Index of the default control endpoint.</summary>
    public const byte ControlEndpointId = 1;

    /// <summary>Highest Device Context Index (endpoint 15 IN).</summary>
    public const byte MaxEndpointId = 31;

    /// <summary>An input context holds the Input Control Context plus a device context of 32 entries.</summary>
    public const int InputContextEntries = MaxEndpointId + 2;

    /// <summary>A route string holds one 4-bit port number per tier (USB 3.2 section 8.9).</summary>
    private const int RouteStringPortBits = 4;
    private const byte MaxRoutablePort = 15;

    // Default bMaxPacketSize0 before the device descriptor is read: 8 is
    // valid for every low and full-speed device, and high and SuperSpeed fix
    // theirs (USB 2.0 section 5.5.3, USB 3.2 section 9.6.1).
    private const ushort DefaultMaxPacketSizeLowFull = 8;
    private const ushort DefaultMaxPacketSizeHigh = 64;
    private const ushort DefaultMaxPacketSizeSuper = 512;

    /// <summary>Set by the handler or the polled drain once the control transfer's Status Stage completed, after <see cref="ControlCode"/>; cleared by the issuer under the lock. Read and written with <c>Volatile</c>.</summary>
    public bool ControlCompleted;

    /// <summary>Creates the slot over an addressed device's memory set. Thread context, the host's address path.</summary>
    /// <param name="controller">The host controller that enabled the slot.</param>
    /// <param name="parent">The hub the device hangs off, or null for a root port.</param>
    /// <param name="port">The 1-based port number on the parent or the controller.</param>
    /// <param name="speed">The speed the port reported.</param>
    /// <param name="slotId">The slot id Enable Slot returned.</param>
    /// <param name="memory">The slot's pooled memory set, cleared.</param>
    public XhciSlot(XhciState controller, XhciSlot? parent, byte port, UsbSpeed speed, byte slotId, XhciSlotMemory memory)
        : base(controller, parent, port, speed)
    {
        Controller = controller;
        SlotId = slotId;
        Memory = memory;

        if (parent is not null)
        {
            RouteString = parent.RouteString | ((uint)Math.Min(port, MaxRoutablePort) << (RouteStringPortBits * parent.HubDepth));

            // A low or full-speed device behind a high-speed hub talks
            // through that hub's Transaction Translator; deeper in a
            // full-speed subtree it inherits the TT its parent already uses.
            if (speed is UsbSpeed.Low or UsbSpeed.Full)
            {
                if (parent.Speed == UsbSpeed.High)
                {
                    TtHubSlotId = parent.SlotId;
                    TtPortNumber = port;
                }
                else
                {
                    TtHubSlotId = parent.TtHubSlotId;
                    TtPortNumber = parent.TtPortNumber;
                }
            }
        }

        MaxPacketSize0 = speed switch
        {
            UsbSpeed.High => DefaultMaxPacketSizeHigh,
            UsbSpeed.Super or UsbSpeed.SuperPlus => DefaultMaxPacketSizeSuper,
            _ => DefaultMaxPacketSizeLowFull
        };
    }

    /// <summary>The host controller, typed.</summary>
    internal XhciState Controller { get; }

    /// <summary>The slot id, 1 to MaxSlots.</summary>
    public byte SlotId { get; }

    /// <summary>Route string of the Slot Context: the hub port at each tier below the root port.</summary>
    public uint RouteString { get; }

    /// <summary>Slot of the high-speed hub whose TT serves this device, 0 when none does.</summary>
    public byte TtHubSlotId { get; }

    /// <summary>Port of that hub the device's subtree hangs off.</summary>
    public byte TtPortNumber { get; }

    /// <summary>The slot's pooled memory set.</summary>
    public XhciSlotMemory Memory { get; }

    /// <summary>A control transfer failed and left the default endpoint halted on the controller; the next transfer recovers it first. Written by the handler and the issuing thread.</summary>
    public bool ControlEndpointHalted { get; set; }

    /// <summary>The event a control transfer's completion signals.</summary>
    public DeviceEvent ControlEvent => Memory.ControlEvent;

    /// <summary>The completion code of the last control transfer; written before <see cref="ControlCompleted"/>.</summary>
    public XhciCompletionCode ControlCode { get; set; }

    /// <summary>The Status Stage address of the thread's control transfer in flight; 0 when no thread waits. Under the lock or in the handler.</summary>
    public ulong ControlStatusTrb { get; set; }

    /// <summary>True while a thread's control transfer, or a recovery's CLEAR_FEATURE, owns the control ring. Under the lock or in the handler.</summary>
    public bool ControlBusy { get; set; }

    /// <summary>The Status Stage address of the CLEAR_FEATURE a recovery put on the control ring; 0 when none. Under the lock or in the handler.</summary>
    public ulong RecoveryStatusTrb { get; set; }

    /// <summary>The interrupt pipe that CLEAR_FEATURE is for, set together with <see cref="RecoveryStatusTrb"/>. Under the lock or in the handler.</summary>
    public XhciInterruptPipe? RecoveringPipe { get; set; }

    /// <summary>True while a caller writes the slot's input context and runs the Configure Endpoint or Evaluate Context that reads it; claimed through the host's input claim, since a ring thread's endpoint reset and the worker's pipe open or close may meet on one slot. Under the lock.</summary>
    public bool InputBusy { get; set; }

    /// <summary>Pipe memory sets closed on the disconnected slot, by DCI, held back until Disable Slot ran: until then the controller's endpoint contexts still name their rings, and a pooled ring taken by another open would be fetched by the released endpoint. The release returns them to the pools. Under the lock.</summary>
    public XhciPipeMemory?[] RetiredPipeMemory { get; } = new XhciPipeMemory?[MaxEndpointId + 1];

    /// <summary>The open interrupt pipes by DCI. Written under the lock; read by the handler without it.</summary>
    public XhciInterruptPipe?[] InterruptPipes { get; } = new XhciInterruptPipe?[MaxEndpointId + 1];

    /// <summary>The open bulk pipes by DCI. Written under the lock; read by the handler and by <see cref="OnDisconnectedCore"/> without it.</summary>
    public XhciBulkPipe?[] BulkPipes { get; } = new XhciBulkPipe?[MaxEndpointId + 1];

    /// <summary>Device Context Index of an endpoint: its number times 2, plus 1 for IN (xHCI 1.2 section 4.5.1).</summary>
    /// <param name="endpoint">The endpoint.</param>
    public static byte EndpointId(UsbEndpoint endpoint) => (byte)((endpoint.Number * 2) + (endpoint.IsIn ? 1 : 0));

    /// <summary>Records bMaxPacketSize0 once the device descriptor gave it and Evaluate Context took it. Thread context, the host's address path.</summary>
    /// <param name="value">The packet size in bytes.</param>
    internal void SetMaxPacketSize0(ushort value) => MaxPacketSize0 = value;

    /// <summary>The highest DCI with an open pipe, or 1 when none; <paramref name="excluding"/> is left out. Under the lock.</summary>
    /// <param name="excluding">A DCI being closed, or 0.</param>
    internal byte HighestOpenEndpointId(byte excluding)
    {
        for (int dci = MaxEndpointId; dci > ControlEndpointId; dci--)
        {
            if (dci != excluding && (InterruptPipes[dci] is not null || BulkPipes[dci] is not null))
            {
                return (byte)dci;
            }
        }

        return ControlEndpointId;
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
        XhciBulkPipe?[] pipes = BulkPipes;
        for (int i = 0; i < pipes.Length; i++)
        {
            pipes[i]?.Event.Signal();
        }
    }

    /// <inheritdoc/>
    protected override UsbTransferStatus ControlTransferCore(UsbSetupPacket setup, Span<byte> data) =>
        Controller.ControlTransfer(this, setup, data);

    /// <inheritdoc/>
    protected override UsbPipe? OpenInterruptPipeCore(UsbEndpoint endpoint, UsbReportHandler handler) =>
        Controller.OpenInterruptPipe(this, endpoint, handler);

    /// <inheritdoc/>
    protected override UsbPipe? OpenBulkPipeCore(UsbEndpoint endpoint) =>
        Controller.OpenBulkPipe(this, endpoint);

    /// <inheritdoc/>
    protected override void ClosePipeCore(UsbPipe pipe) =>
        Controller.ClosePipe(this, pipe);

    /// <inheritdoc/>
    protected override UsbTransferStatus BulkInCore(UsbPipe pipe, Span<byte> data, out int transferred) =>
        Controller.BulkIn(this, pipe, data, out transferred);

    /// <inheritdoc/>
    protected override UsbTransferStatus BulkOutCore(UsbPipe pipe, ReadOnlySpan<byte> data, out int transferred) =>
        Controller.BulkOut(this, pipe, data, out transferred);

    /// <inheritdoc/>
    protected override bool ResetEndpointCore(UsbPipe pipe) =>
        Controller.ResetEndpoint(this, pipe);

    /// <inheritdoc/>
    protected override bool ConfigureAsHubCore(byte portCount, byte thinkTime) =>
        Controller.ConfigureAsHub(this, portCount, thinkTime);
}
