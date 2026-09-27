// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Pipes;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Rings;
using Cosmos.Kernel.HAL.Drivers.Usb;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci;

/// <summary>
/// A device slot of an <see cref="XhciController"/>: the controller's view
/// of a device it addressed. Owns the slot's input and output device
/// contexts, the default control endpoint's ring and data buffers, and the
/// interrupt and bulk pipes opened on it. Its route string and Transaction
/// Translator fields are derived from the parent hub at construction.
/// </summary>
internal sealed class XhciDevice : UsbHostDevice
{
    /// <summary>Device Context Index of the default control endpoint.</summary>
    internal const byte ControlEndpointId = 1;

    /// <summary>Highest Device Context Index (endpoint 15 IN).</summary>
    private const int MaxEndpointId = 31;

    /// <summary>An input context holds the Input Control Context plus a device context of 32 entries.</summary>
    private const int InputContextEntries = MaxEndpointId + 2;

    /// <summary>Pages a slot holds besides its control ring: the output and input contexts and the two control data buffers.</summary>
    private const int SlotPageCount = 4;

    /// <summary>bEndpointAddress bits 3:0 hold the endpoint number.</summary>
    private const byte EndpointNumberMask = 0x0F;

    /// <summary>A route string holds one 4-bit port number per tier (USB 3.2 §8.9).</summary>
    private const int RouteStringPortBits = 4;
    private const byte MaxRoutablePort = 15;

    // Default bMaxPacketSize0 before the device descriptor is read: 8 is
    // valid for every low/full-speed device, and high and SuperSpeed fix
    // theirs (USB 2.0 §5.5.3, USB 3.2 §9.6.1).
    private const ushort DefaultMaxPacketSizeLowFull = 8;
    private const ushort DefaultMaxPacketSizeHigh = 64;
    private const ushort DefaultMaxPacketSizeSuper = 512;

    private readonly int _contextSize;
    private readonly InterruptPipe?[] _pipes = new InterruptPipe?[MaxEndpointId + 1];
    private readonly BulkPipe?[] _bulkPipes = new BulkPipe?[MaxEndpointId + 1];
    private readonly DmaBuffer _outputContext;
    private readonly DmaBuffer _inputContext;
    private readonly DmaBuffer _controlBuffer;
    private readonly DmaBuffer _asyncControlBuffer;

    /// <summary>Bulk transfers and resets running on the device, which a release waits out.</summary>
    private int _bulkTransfers;

    /// <summary>Set from interrupt context when a fire-and-forget control transfer fails, cleared by the recovery.</summary>
    private volatile bool _controlEndpointHalted;

    internal XhciController Controller { get; }
    internal byte SlotId { get; }

    /// <summary>The hub the device hangs off, null on a root port.</summary>
    internal XhciDevice? Parent { get; }

    /// <summary>The root port the device's branch of the tree hangs off.</summary>
    internal byte RootPort { get; }

    /// <summary>Number of hubs between the device and its root port.</summary>
    internal int HubDepth { get; }

    internal UsbSpeed Speed { get; }

    /// <summary>Route string of the Slot Context: the hub port at each tier below the root port.</summary>
    internal uint RouteString { get; }

    /// <summary>Slot of the high-speed hub whose TT serves this device, 0 when none does.</summary>
    internal byte TtHubSlotId { get; }

    /// <summary>Port of that hub the device's subtree hangs off.</summary>
    internal byte TtPortNumber { get; }

    /// <summary>bMaxPacketSize0 in bytes: a default for the speed until the device descriptor gives the real one.</summary>
    internal ushort MaxPacketSize0 { get; private set; }

    internal ulong OutputContextAddress => _outputContext.DeviceAddress;
    internal ulong InputContextAddress => _inputContext.DeviceAddress;
    internal ProducerRing ControlRing { get; }

    /// <summary>Data stage buffer of synchronous control transfers.</summary>
    internal Span<byte> ControlBuffer => _controlBuffer.Span;
    internal ulong ControlBufferAddress => _controlBuffer.DeviceAddress;

    /// <summary>Data stage buffer of fire-and-forget control transfers, kept apart so they never race a synchronous one.</summary>
    internal Span<byte> AsyncControlBuffer => _asyncControlBuffer.Span;
    internal ulong AsyncControlBufferAddress => _asyncControlBuffer.DeviceAddress;

    /// <summary>A fire-and-forget control transfer failed and left the default endpoint halted.</summary>
    internal bool ControlEndpointHalted
    {
        get => _controlEndpointHalted;
        set => _controlEndpointHalted = value;
    }

    /// <summary>True while a bulk transfer or reset runs on the device.</summary>
    internal bool HasBulkTransfers => Volatile.Read(ref _bulkTransfers) != 0;

    internal Span<uint> InputControlContext => InputContext(0);
    internal Span<uint> InputSlotContext => InputContext(1);
    internal Span<uint> OutputSlotContext => MemoryMarshal.Cast<byte, uint>(_outputContext.Span[.._contextSize]);

    private XhciDevice(XhciController controller, byte slotId, XhciDevice? parent, byte port, UsbSpeed speed, int contextSize,
        DmaBuffer outputContext, DmaBuffer inputContext, DmaBuffer controlBuffer, DmaBuffer asyncControlBuffer, ProducerRing controlRing)
    {
        Controller = controller;
        SlotId = slotId;
        Parent = parent;
        Speed = speed;
        _contextSize = contextSize;
        _outputContext = outputContext;
        _inputContext = inputContext;
        _controlBuffer = controlBuffer;
        _asyncControlBuffer = asyncControlBuffer;
        ControlRing = controlRing;
        RootPort = parent?.RootPort ?? port;
        HubDepth = parent is null ? 0 : parent.HubDepth + 1;

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
    }

    /// <summary>
    /// Allocates the pages of an enabled slot: its two device contexts, the
    /// default control endpoint's ring and its two data buffers. Thread
    /// context.
    /// </summary>
    /// <exception cref="InvalidOperationException">No DMA memory the controller can reach is free; what was allocated is given back.</exception>
    internal static XhciDevice Create(XhciController controller, XhciMemory memory, byte slotId, XhciDevice? parent, byte port,
        UsbSpeed speed, int contextSize)
    {
        List<DmaBuffer> allocated = new(SlotPageCount);
        try
        {
            for (int i = 0; i < SlotPageCount; i++)
            {
                allocated.Add(memory.Allocate(1));
            }

            return new XhciDevice(controller, slotId, parent, port, speed, contextSize,
                allocated[0], allocated[1], allocated[2], allocated[3], new ProducerRing(memory));
        }
        catch (InvalidOperationException)
        {
            for (int i = 0; i < allocated.Count; i++)
            {
                memory.Free(allocated[i]);
            }

            throw;
        }
    }

    /// <summary>Device Context Index of an endpoint: its number x 2, plus 1 for IN (xHCI 1.2 §4.5.1).</summary>
    internal static byte EndpointId(UsbEndpointInfo endpoint) =>
        (byte)(((endpoint.Address & EndpointNumberMask) * 2) + (endpoint.Direction == UsbDirection.In ? 1 : 0));

    internal Span<uint> InputEndpointContext(byte endpointId) => InputContext(endpointId + 1);

    internal Span<uint> OutputEndpointContext(byte endpointId) =>
        MemoryMarshal.Cast<byte, uint>(_outputContext.Span.Slice(endpointId * _contextSize, _contextSize));

    internal void ClearInputContext() => _inputContext.Span[..(InputContextEntries * _contextSize)].Clear();

    internal void SetMaxPacketSize0(ushort maxPacketSize) => MaxPacketSize0 = maxPacketSize;

    /// <summary>Makes the device's transfers fail from now on: it left the bus, or is being released.</summary>
    internal void MarkGone() => MarkDisconnected();

    internal InterruptPipe? GetPipe(byte endpointId) => endpointId <= MaxEndpointId ? _pipes[endpointId] : null;

    internal void AddPipe(InterruptPipe pipe) => _pipes[pipe.EndpointId] = pipe;

    internal BulkPipe? GetBulkPipe(byte endpointId) => endpointId <= MaxEndpointId ? _bulkPipes[endpointId] : null;

    internal void AddBulkPipe(BulkPipe pipe) => _bulkPipes[pipe.EndpointId] = pipe;

    /// <summary>Counts a bulk transfer or reset in, before it checks whether the device is gone.</summary>
    internal void EnterBulkTransfer() => Interlocked.Increment(ref _bulkTransfers);

    internal void ExitBulkTransfer() => Interlocked.Decrement(ref _bulkTransfers);

    /// <summary>The pipe whose recovery step is the command at <paramref name="commandAddress"/>, if any.</summary>
    internal InterruptPipe? FindPipeByCommand(ulong commandAddress)
    {
        foreach (InterruptPipe? pipe in _pipes)
        {
            if (pipe is not null && pipe.PendingCommand == commandAddress)
            {
                return pipe;
            }
        }

        return null;
    }

    /// <summary>Gives the slot's pages back once the controller no longer references them. Thread context.</summary>
    internal void Free(XhciMemory memory)
    {
        foreach (InterruptPipe? pipe in _pipes)
        {
            pipe?.Free(memory);
        }

        foreach (BulkPipe? pipe in _bulkPipes)
        {
            pipe?.Free(memory);
        }

        ControlRing.Free(memory);
        memory.Free(_outputContext);
        memory.Free(_inputContext);
        memory.Free(_controlBuffer);
        memory.Free(_asyncControlBuffer);
    }

    /// <inheritdoc />
    protected override UsbTransferStatus ControlTransfer(UsbSetupPacket setup, Span<byte> data, out int transferred) =>
        Controller.ControlTransfer(this, setup, data, out transferred);

    /// <inheritdoc />
    protected override bool SubmitControlTransfer(UsbSetupPacket setup, ReadOnlySpan<byte> data) =>
        Controller.SubmitControlTransfer(this, setup, data);

    /// <inheritdoc />
    protected override bool OpenInterruptPipe(UsbEndpointInfo endpoint, UsbReportHandler handler) =>
        Controller.OpenInterruptPipe(this, endpoint, handler);

    /// <inheritdoc />
    protected override bool OpenBulkEndpoint(UsbEndpointInfo endpoint) =>
        Controller.OpenBulkPipe(this, endpoint);

    /// <inheritdoc />
    protected override UsbTransferStatus BulkIn(UsbEndpointInfo endpoint, Span<byte> data, out int transferred) =>
        Controller.BulkIn(this, endpoint, data, out transferred);

    /// <inheritdoc />
    protected override UsbTransferStatus BulkOut(UsbEndpointInfo endpoint, ReadOnlySpan<byte> data, out int transferred) =>
        Controller.BulkOut(this, endpoint, data, out transferred);

    /// <inheritdoc />
    protected override bool ResetEndpoint(UsbEndpointInfo endpoint) =>
        Controller.ResetBulkPipe(this, endpoint);

    /// <inheritdoc />
    protected override bool ConfigureAsHub(byte portCount, byte thinkTime) =>
        Controller.ConfigureHub(this, portCount, thinkTime);

    private Span<uint> InputContext(int index) =>
        MemoryMarshal.Cast<byte, uint>(_inputContext.Span.Slice(index * _contextSize, _contextSize));
}
