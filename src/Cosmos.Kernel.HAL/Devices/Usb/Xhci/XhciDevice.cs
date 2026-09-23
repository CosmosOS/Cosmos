// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Devices.Usb.Xhci;

/// <summary>
/// A device slot of an <see cref="XhciController"/>: the controller's view
/// of a <see cref="UsbDevice"/>. Owns the slot's input and output device
/// contexts, the default control endpoint's ring and data buffers, and the
/// interrupt and bulk pipes opened on it. Its route string and Transaction
/// Translator fields are derived from the parent hub at construction.
/// </summary>
internal sealed unsafe class XhciDevice : UsbDevice
{
    /// <summary>Device Context Index of the default control endpoint.</summary>
    public const byte ControlEndpointId = 1;

    /// <summary>Highest Device Context Index (endpoint 15 IN).</summary>
    private const int MaxEndpointId = 31;

    /// <summary>An input context holds the Input Control Context plus a device context of 32 entries.</summary>
    private const int InputContextEntries = MaxEndpointId + 2;

    /// <summary>A route string holds one 4-bit port number per tier (USB 3.2 §8.9).</summary>
    private const int RouteStringPortBits = 4;
    private const byte MaxRoutablePort = 15;

    // Default bMaxPacketSize0 before the device descriptor is read: 8 is
    // valid for every low/full-speed device, and high and SuperSpeed fix
    // theirs (USB 2.0 §5.5.3, USB 3.2 §9.6.1).
    private const ushort DefaultMaxPacketSizeLowFull = 8;
    private const ushort DefaultMaxPacketSizeHigh = 64;
    private const ushort DefaultMaxPacketSizeSuper = 512;

    private readonly XhciController _controller;
    private readonly int _contextSize;
    private readonly XhciInterruptPipe?[] _pipes = new XhciInterruptPipe?[MaxEndpointId + 1];
    private readonly XhciBulkPipe?[] _bulkPipes = new XhciBulkPipe?[MaxEndpointId + 1];

    public XhciDevice(XhciController controller, byte slotId, XhciDevice? parent, byte port, UsbSpeed speed, int contextSize)
        : base(controller, parent, port, speed)
    {
        _controller = controller;
        _contextSize = contextSize;
        SlotId = slotId;

        if (parent is not null)
        {
            RouteString = parent.RouteString | ((uint)Math.Min(port, MaxRoutablePort) << (RouteStringPortBits * parent.HubDepth));

            // A low/full-speed device behind a high-speed hub talks through
            // that hub's Transaction Translator; deeper in a full-speed
            // subtree it inherits the TT its parent hub already uses.
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

        OutputContext = XhciDma.AllocPages(1, out ulong outputContextAddress);
        OutputContextAddress = outputContextAddress;
        InputContext = XhciDma.AllocPages(1, out ulong inputContextAddress);
        InputContextAddress = inputContextAddress;
        ControlBuffer = XhciDma.AllocPages(1, out ulong controlBufferAddress);
        ControlBufferAddress = controlBufferAddress;
        AsyncControlBuffer = XhciDma.AllocPages(1, out ulong asyncControlBufferAddress);
        AsyncControlBufferAddress = asyncControlBufferAddress;
        ControlRing = new XhciRing();
    }

    public byte SlotId { get; }

    /// <summary>Route string of the Slot Context: the hub port at each tier below the root port.</summary>
    public uint RouteString { get; }

    /// <summary>Slot of the high-speed hub whose TT serves this device, 0 when none does.</summary>
    public byte TtHubSlotId { get; }

    /// <summary>Port of that hub the device's subtree hangs off.</summary>
    public byte TtPortNumber { get; }

    public byte* OutputContext { get; }
    public ulong OutputContextAddress { get; }
    public byte* InputContext { get; }
    public ulong InputContextAddress { get; }
    public XhciRing ControlRing { get; }

    /// <summary>Data stage buffer of synchronous control transfers.</summary>
    public byte* ControlBuffer { get; }
    public ulong ControlBufferAddress { get; }

    /// <summary>Data stage buffer of fire-and-forget control transfers, kept apart so they never race a synchronous one.</summary>
    public byte* AsyncControlBuffer { get; }
    public ulong AsyncControlBufferAddress { get; }

    /// <summary>A fire-and-forget control transfer failed and left the default endpoint halted.</summary>
    public bool ControlEndpointHalted { get; set; }

    public uint* InputControlContext => (uint*)InputContext;
    public uint* InputSlotContext => (uint*)(InputContext + _contextSize);
    public uint* OutputSlotContext => (uint*)OutputContext;

    public uint* InputEndpointContext(byte endpointId) => (uint*)(InputContext + ((endpointId + 1) * _contextSize));

    public uint* OutputEndpointContext(byte endpointId) => (uint*)(OutputContext + (endpointId * _contextSize));

    /// <summary>Device Context Index of an endpoint: its number x 2, plus 1 for IN (xHCI 1.2 §4.5.1).</summary>
    public static byte EndpointId(UsbEndpoint endpoint) => (byte)((endpoint.Number * 2) + (endpoint.IsIn ? 1 : 0));

    public void ClearInputContext() => new Span<byte>(InputContext, InputContextEntries * _contextSize).Clear();

    public void SetMaxPacketSize0(ushort maxPacketSize) => MaxPacketSize0 = maxPacketSize;

    public XhciInterruptPipe? GetPipe(byte endpointId) => endpointId <= MaxEndpointId ? _pipes[endpointId] : null;

    public void AddPipe(XhciInterruptPipe pipe) => _pipes[pipe.EndpointId] = pipe;

    public XhciBulkPipe? GetBulkPipe(byte endpointId) => endpointId <= MaxEndpointId ? _bulkPipes[endpointId] : null;

    public void AddBulkPipe(XhciBulkPipe pipe) => _bulkPipes[pipe.EndpointId] = pipe;

    /// <summary>The pipe whose recovery step is the command at <paramref name="commandAddress"/>, if any.</summary>
    public XhciInterruptPipe? FindPipeByCommand(ulong commandAddress)
    {
        foreach (XhciInterruptPipe? pipe in _pipes)
        {
            if (pipe is not null && pipe.PendingCommand == commandAddress)
            {
                return pipe;
            }
        }

        return null;
    }

    public override UsbTransferStatus ControlTransfer(UsbSetupPacket setup, Span<byte> data) =>
        _controller.ControlTransfer(this, setup, data);

    public override bool SubmitControlTransfer(UsbSetupPacket setup, ReadOnlySpan<byte> data) =>
        _controller.SubmitControlTransfer(this, setup, data);

    public override bool OpenInterruptPipe(UsbEndpoint endpoint, UsbInterruptHandler handler) =>
        _controller.OpenInterruptPipe(this, endpoint, handler);

    public override bool OpenBulkEndpoint(UsbEndpoint endpoint) =>
        _controller.OpenBulkPipe(this, endpoint);

    public override UsbTransferStatus BulkIn(UsbEndpoint endpoint, Span<byte> data, out int transferred) =>
        _controller.BulkIn(this, endpoint, data, out transferred);

    public override UsbTransferStatus BulkOut(UsbEndpoint endpoint, ReadOnlySpan<byte> data, out int transferred) =>
        _controller.BulkOut(this, endpoint, data, out transferred);

    public override bool ResetEndpoint(UsbEndpoint endpoint) =>
        _controller.ResetBulkPipe(this, endpoint);

    public override bool ConfigureAsHub(byte portCount, byte thinkTime) =>
        _controller.ConfigureHub(this, portCount, thinkTime);

    /// <summary>Frees the slot's DMA memory once the controller no longer references it.</summary>
    public void Free()
    {
        foreach (XhciInterruptPipe? pipe in _pipes)
        {
            pipe?.Free();
        }

        foreach (XhciBulkPipe? pipe in _bulkPipes)
        {
            pipe?.Free();
        }

        ControlRing.Free();
        XhciDma.Free(OutputContext);
        XhciDma.Free(InputContext);
        XhciDma.Free(ControlBuffer);
        XhciDma.Free(AsyncControlBuffer);
    }
}
