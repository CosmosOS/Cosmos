// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Virtio.Rings;
using Cosmos.Kernel.HAL.Drivers.Pci;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Virtio.Net;

/// <summary>
/// One virtio network device (virtio 1.2 §5.1): brings it up over the
/// transport, keeps its receive and transmit queues, and carries frames
/// between them and the link it published to the kernel's network stack.
/// </summary>
/// <remarks>
/// <para>
/// Each queue is reached from one context only, so neither needs a lock.
/// The receive queue belongs to the work item, which the interrupt handler
/// schedules and the kit runs on its driver-work thread, where delivering a
/// frame may allocate. The transmit queue belongs to the transmit handler
/// and to the interrupt handler, which reclaims the slots the device has
/// finished with; the kit calls the transmit handler with interrupts masked
/// and one call at a time, so on this single CPU the two cannot overlap.
/// </para>
/// <para>
/// Both directions carry one descriptor per frame, out of one DMA buffer per
/// queue divided into equal slots, so a slot's index is its descriptor's.
/// One allocation per direction rather than one per slot: the kit allocates
/// whole pages, and a 2 KiB slot of its own would waste half of each.
/// </para>
/// </remarks>
internal sealed class VirtioNetController
{
    // virtio-net feature bits (virtio 1.2 §5.1.3).

    /// <summary>VIRTIO_NET_F_MAC: the device configuration carries the MAC address to use.</summary>
    private const ulong FeatureMac = 1UL << 5;

    /// <summary>VIRTIO_NET_F_STATUS: the device configuration carries the link status.</summary>
    private const ulong FeatureStatus = 1UL << 16;

    // virtio_net_config (virtio 1.2 §5.1.4): mac[6], then status.
    private const ulong MacConfigOffset = 0;
    private const ulong StatusConfigOffset = 6;

    /// <summary>VIRTIO_NET_S_LINK_UP in the configuration's status field.</summary>
    private const ushort StatusLinkUp = 1;

    /// <summary>
    /// The virtio-net header in front of every frame, in both directions.
    /// Ten bytes, and with VIRTIO_F_VERSION_1 always the two of
    /// <c>num_buffers</c> behind them (virtio 1.2 §5.1.6.1), which is why the
    /// driver requires that feature rather than sizing the header at runtime.
    /// </summary>
    private const int HeaderLength = 12;

    /// <summary>The receive queue, queue 0 of every virtio-net device.</summary>
    private const ushort ReceiveQueue = 0;

    /// <summary>The transmit queue, queue 1.</summary>
    private const ushort TransmitQueue = 1;

    /// <summary>
    /// Queues this driver sets up, which sizes the transport's doorbell
    /// table: the receive and the transmit queue. A virtio-net device also
    /// has a control queue, which this driver never uses and so never asks
    /// the transport to ring.
    /// </summary>
    internal const int QueueCount = 2;

    /// <summary>
    /// Bytes per slot: the header and the longest Ethernet frame the stack
    /// sends or receives, rounded up. Without VIRTIO_NET_F_MRG_RXBUF, which
    /// the driver does not negotiate, the device writes each received frame
    /// into one slot whole and never splits one across buffers.
    /// </summary>
    private const int SlotLength = 2048;

    /// <summary>The longest frame a slot holds behind the header.</summary>
    private const int MaximumFrameLength = SlotLength - HeaderLength;

    /// <summary>
    /// Descriptors wanted per queue, which the device lowers to what it
    /// offers. 64 slots of 2 KiB is 128 KiB per direction: enough to absorb a
    /// burst between two drains, including the 55 ms between timer ticks
    /// where the kit can only poll the interrupt handler.
    /// </summary>
    private const int PreferredQueueLength = 64;

    /// <summary>Bytes of a MAC address, the length of the configuration's mac field.</summary>
    private const int MacAddressLength = 6;

    // MSI-X capability (PCI 3.0 §6.8.2), read to tell which interrupt path
    // the kit granted: the kit programs the capability, the driver only asks.
    private const byte MsiXCapabilityId = 0x11;
    private const ushort MsiXMessageControlOffset = 0x02;
    private const ushort MsiXEnableBit = 0x8000;

    private readonly PciDeviceContext _context;
    private readonly VirtioPciTransport _transport;

    /// <summary>True once the device agreed to report its link status.</summary>
    private bool _reportsLinkStatus;

    // Set by TryStart, in the order it works through them. The kit runs the
    // work item and the interrupt handler only once Probe returned Bound, so
    // by then they are all set; the null checks below are for the compiler.
    private SplitVirtqueue? _receive;
    private SplitVirtqueue? _transmit;
    private DmaBuffer? _receiveSlots;
    private DmaBuffer? _transmitSlots;
    private DeviceWorkItem? _receiveWork;
    private NetworkLink? _link;

    /// <summary>Takes over a device the transport has located the registers of.</summary>
    /// <param name="context">The binding attempt, which the controller allocates and publishes through.</param>
    /// <param name="transport">The device's registers, before the reset.</param>
    internal VirtioNetController(PciDeviceContext context, VirtioPciTransport transport)
    {
        _context = context;
        _transport = transport;
    }

    /// <summary>
    /// Brings the device up and publishes its link: reset, features, queues,
    /// receive buffers, then DRIVER_OK. Probe only.
    /// </summary>
    /// <returns>
    /// False when the device cannot be driven, which the kit logged: the
    /// caller then tells the device the driver gave up. Nothing is published
    /// on that path.
    /// </returns>
    internal bool TryStart()
    {
        if (!_transport.TryBegin())
        {
            return false;
        }

        ulong wanted = VirtioPciTransport.FeatureVersion1 | FeatureMac | FeatureStatus;
        if (!_transport.TryNegotiateFeatures(wanted, out ulong features))
        {
            return false;
        }

        // The modern header length and the 64-bit ring addresses below both
        // come with VERSION_1; a device offering neither it nor the legacy
        // interface this driver declined is one nothing here can drive.
        if ((features & VirtioPciTransport.FeatureVersion1) == 0)
        {
            _context.WriteLog("the device does not offer VIRTIO_F_VERSION_1");
            return false;
        }

        // Without a MAC from the device there is no address to send from: the
        // stack would come up on 00:00:00:00:00:00 and every reply to its ARP
        // and DHCP would go nowhere. Better to say so than to publish a link
        // that cannot work.
        if ((features & FeatureMac) == 0)
        {
            _context.WriteLog("the device offers no MAC address");
            return false;
        }

        _reportsLinkStatus = (features & FeatureStatus) != 0;

        if (!_context.TryCreateWorkItem(DrainReceiveQueue, out DeviceWorkItem? receiveWork))
        {
            _context.WriteLog("needs the scheduler for its receive path");
            return false;
        }

        // Received frames sit in the used ring until something drains them,
        // so a device with no interrupt path of any kind is one whose receive
        // side would never run. The kit polls the handler from the timer
        // where it cannot route a message, which is slower but works.
        if (!_context.TryRequestInterrupts(OnInterrupt))
        {
            _context.WriteLog("no MSI-X and no ticking timer to poll with");
            return false;
        }

        // Only where the kit actually routed a message: a polled handler is
        // reached from the timer, and a vector the device raised into the
        // masked entry would be one nothing acknowledges.
        if (IsMsiXEnabled(_context.Function))
        {
            _transport.TryUseMessageVector();
        }

        // Before the queues are enabled, and so before DRIVER_OK lets the
        // device fetch a descriptor: bus mastering is what carries its reads
        // and writes, and the kit would otherwise only turn it on once Probe
        // returned Bound.
        _context.EnableBusMastering();

        if (!_transport.TryCreateQueue(ReceiveQueue, PreferredQueueLength, out SplitVirtqueue? receive)
            || !_transport.TryCreateQueue(TransmitQueue, PreferredQueueLength, out SplitVirtqueue? transmit))
        {
            return false;
        }

        if (!_context.TryAllocateDma(receive.Count * SlotLength, ulong.MaxValue, out DmaBuffer? receiveSlots)
            || !_context.TryAllocateDma(transmit.Count * SlotLength, ulong.MaxValue, out DmaBuffer? transmitSlots))
        {
            _context.WriteLog("no DMA memory for the frame buffers");
            return false;
        }

        // Only once everything the two paths need exists, so neither runs
        // against a half-built queue if the device signals early.
        _receive = receive;
        _transmit = transmit;
        _receiveSlots = receiveSlots;
        _transmitSlots = transmitSlots;
        _receiveWork = receiveWork;

        PostReceiveSlots();

        // The device starts here, and only then is there any point ringing
        // its doorbell for the buffers posted above.
        _transport.Finish();
        _transport.Notify(ReceiveQueue);

        _link = _context.PublishNetworkLink(ReadMacAddress(), Transmit);
        RefreshLinkState();
        return true;
    }

    /// <summary>
    /// The interrupt handler. Interrupt context: no allocation, no throw, no
    /// string, no lock. The driver asked the device for one vector, which
    /// carries both queues and configuration changes alike, so any signal
    /// means the same thing: look at the rings. Reclaiming transmit slots
    /// touches no device register and cannot allocate, so it happens here;
    /// receiving hands frames to the stack, so it goes to the work item.
    /// </summary>
    private void OnInterrupt(int vector)
    {
        ReclaimTransmitSlots();
        _receiveWork?.Schedule();
    }

    /// <summary>
    /// The receive path: driver-work thread, where it may allocate. Hands
    /// every frame the device has written to the stack, which copies it, then
    /// posts the slot back and tells the device it is there.
    /// </summary>
    private void DrainReceiveQueue()
    {
        if (_receive is not { } queue || _receiveSlots is not { } slots || _link is not { } link)
        {
            return;
        }

        bool posted = false;
        while (queue.TryTakeUsed(out int descriptor, out int written))
        {
            // A device that reports more than the slot holds is broken;
            // believing it would read past the buffer.
            if (written > SlotLength)
            {
                written = SlotLength;
            }

            if (written > HeaderLength)
            {
                link.Deliver(SlotOf(slots, descriptor).Slice(HeaderLength, written - HeaderLength));
            }

            queue.Describe(descriptor, SlotAddressOf(slots, descriptor), SlotLength, VringDescriptor.DeviceWritable);
            queue.Offer(descriptor);
            posted = true;
        }

        if (posted)
        {
            _transport.Notify(ReceiveQueue);
        }

        // Cheap, and this is the one context that may read the device
        // configuration without holding up an interrupt handler.
        RefreshLinkState();
    }

    /// <summary>
    /// The link's transmit handler. Interrupts masked, one call at a time, so
    /// the queue needs no lock: it copies the frame behind a cleared header
    /// into a free slot and offers it to the device.
    /// </summary>
    /// <returns>False when the frame is too long, or when every slot is still with the device.</returns>
    private bool Transmit(ReadOnlySpan<byte> frame)
    {
        if (_transmit is not { } queue || _transmitSlots is not { } slots || frame.Length > MaximumFrameLength)
        {
            return false;
        }

        // Frames the device has sent since the last interrupt, so a burst
        // does not wait for one to get its slots back.
        ReclaimTransmitSlots();

        if (!queue.TryAllocateDescriptor(out int descriptor))
        {
            return false;
        }

        Span<byte> slot = SlotOf(slots, descriptor);
        slot[..HeaderLength].Clear();
        frame.CopyTo(slot[HeaderLength..]);

        queue.Describe(descriptor, SlotAddressOf(slots, descriptor), HeaderLength + frame.Length,
            VringDescriptor.DeviceReadable);
        queue.Offer(descriptor);
        _transport.Notify(TransmitQueue);
        return true;
    }

    /// <summary>
    /// Takes back the slots the device has finished sending. Any context: it
    /// reads the used ring and writes the free list, both of which belong to
    /// the transmit queue's two callers.
    /// </summary>
    private void ReclaimTransmitSlots()
    {
        if (_transmit is not { } queue)
        {
            return;
        }

        while (queue.TryTakeUsed(out int descriptor, out _))
        {
            queue.FreeDescriptor(descriptor);
        }
    }

    /// <summary>Points every descriptor of the receive queue at its slot and offers it to the device.</summary>
    private void PostReceiveSlots()
    {
        if (_receive is not { } queue || _receiveSlots is not { } slots)
        {
            return;
        }

        while (queue.TryAllocateDescriptor(out int descriptor))
        {
            queue.Describe(descriptor, SlotAddressOf(slots, descriptor), SlotLength, VringDescriptor.DeviceWritable);
            queue.Offer(descriptor);
        }
    }

    /// <summary>Reads the address the device was given, which the stack sends from.</summary>
    private MACAddress ReadMacAddress()
    {
        byte[] address = new byte[MacAddressLength];
        for (int i = 0; i < address.Length; i++)
        {
            address[i] = _transport.ReadDeviceConfig8(MacConfigOffset + (ulong)i);
        }

        return new MACAddress(address);
    }

    /// <summary>
    /// Tells the link whether the device reports a carrier. A device that
    /// does not report one at all counts as up: it is a virtual NIC whose
    /// backend is there or not, and there is nothing to watch.
    /// </summary>
    private void RefreshLinkState()
    {
        if (_link is not { } link)
        {
            return;
        }

        link.SetLinkState(!_reportsLinkStatus
            || (_transport.ReadDeviceConfig16(StatusConfigOffset) & StatusLinkUp) != 0);
    }

    /// <summary>
    /// Whether the kit routed the function's interrupts through MSI-X, read
    /// from the capability it programmed, rather than polled from the timer.
    /// </summary>
    private static bool IsMsiXEnabled(PciFunction function) =>
        function.TryFindCapability(MsiXCapabilityId, out ushort capability)
        && (function.ReadConfig16((ushort)(capability + MsiXMessageControlOffset)) & MsiXEnableBit) != 0;

    /// <summary>The slot descriptor <paramref name="index"/> owns, in the driver's own memory.</summary>
    private static Span<byte> SlotOf(DmaBuffer slots, int index) => slots.Span.Slice(index * SlotLength, SlotLength);

    /// <summary>The same slot, as the device addresses it.</summary>
    private static ulong SlotAddressOf(DmaBuffer slots, int index) =>
        slots.DeviceAddress + (ulong)(index * SlotLength);
}
