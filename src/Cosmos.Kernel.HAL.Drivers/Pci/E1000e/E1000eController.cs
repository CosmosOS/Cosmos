// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Pci;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.E1000e.Registers;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.E1000e.Rings;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.E1000e;

/// <summary>
/// One Intel gigabit Ethernet controller of the E1000E family: resets it,
/// finds the address it is known by, builds its two descriptor rings and
/// carries frames between them and the link it published to the kernel's
/// network stack.
/// </summary>
/// <remarks>
/// <para>
/// Each ring is reached from one context only, so neither needs a lock. The
/// receive ring belongs to the work item, which the interrupt handler
/// schedules and the kit runs on its driver-work thread, where delivering a
/// frame may allocate. The transmit ring belongs to the transmit handler,
/// which the kit calls with interrupts masked and one call at a time; the
/// interrupt handler never touches it, since the device's own head register
/// says which descriptors are free again.
/// </para>
/// <para>
/// Both directions carry one descriptor per frame, out of one DMA buffer per
/// ring divided into equal slots, so a slot's index is its descriptor's. Four
/// allocations rather than one per slot: the kit allocates whole pages, and a
/// 2 KiB slot of its own would waste half of each.
/// </para>
/// </remarks>
internal sealed class E1000eController
{
    /// <summary>
    /// Descriptors per ring. The count has to be a multiple of eight, since
    /// the device fetches descriptors in cache lines, and 32 slots of 2 KiB
    /// per direction is enough to absorb a burst between two drains, the
    /// 55 ms between timer ticks where the kit can only poll the handler
    /// included.
    /// </summary>
    private const int DescriptorCount = 32;

    /// <summary>
    /// Bytes per slot, which is the receive buffer size the receiver is
    /// configured for: the longest Ethernet frame the stack sends or receives
    /// fits in one, so the device never splits a frame across two.
    /// </summary>
    private const int SlotLength = 2048;

    /// <summary>Bytes of a MAC address.</summary>
    private const int MacAddressLength = 6;

    /// <summary>EEPROM words holding the address, one per two of its bytes.</summary>
    private const int MacAddressWords = MacAddressLength / 2;

    /// <summary>The address bytes RAH0 carries, under the validity and select fields above them.</summary>
    private const uint ReceiveAddressHighBytes = 0xFFFF;

    /// <summary>Reads of CTRL before a reset is given up on, one every 10 µs; the device takes under a millisecond.</summary>
    private const int ResetPolls = 1000;

    private readonly PciDeviceContext _context;
    private readonly ControllerRegisters _registers;

    // Set by TryStart, in the order it works through them. The kit runs the
    // work item and the interrupt handler only once Probe returned Bound, so
    // by then they are all set; the null checks below are for the compiler.
    private DmaBuffer? _receiveRing;
    private DmaBuffer? _receiveSlots;
    private DmaBuffer? _transmitRing;
    private DmaBuffer? _transmitSlots;
    private DeviceWorkItem? _receiveWork;
    private NetworkLink? _link;

    /// <summary>The receive descriptor the work item looks at next, which only it moves.</summary>
    private int _receiveIndex;

    /// <summary>The transmit descriptor the next frame goes in, which is also what TDT holds.</summary>
    private int _transmitIndex;

    /// <summary>Takes over a controller whose registers the driver has mapped.</summary>
    /// <param name="context">The binding attempt, which the controller allocates and publishes through.</param>
    /// <param name="registers">BAR 0, before the reset.</param>
    internal E1000eController(PciDeviceContext context, ControllerRegisters registers)
    {
        _context = context;
        _registers = registers;
    }

    /// <summary>
    /// Brings the controller up and publishes its link: reset, address, rings,
    /// then the engines and the interrupts. Probe only.
    /// </summary>
    /// <returns>
    /// False when the controller cannot be driven, which the kit logged.
    /// Nothing is published on that path, and the kit turns bus mastering
    /// back off before it frees the rings.
    /// </returns>
    internal bool TryStart()
    {
        if (!TryReset() || !TryReadMacAddress(out MACAddress address))
        {
            return false;
        }

        if (!_context.TryCreateWorkItem(DrainReceiveRing, out DeviceWorkItem? receiveWork))
        {
            _context.WriteLog("needs the scheduler for its receive path");
            return false;
        }

        // Received frames sit in the ring until something drains them, so a
        // controller with no interrupt path of any kind is one whose receive
        // side would never run. The kit polls the handler from the timer
        // where it cannot route a message, which is slower but works.
        if (!_context.TryRequestInterrupts(OnInterrupt))
        {
            _context.WriteLog("no MSI-X and no ticking timer to poll with");
            return false;
        }

        if (!_context.TryAllocateDma(DescriptorCount * ReceiveDescriptor.Size, ulong.MaxValue, out DmaBuffer? receiveRing)
            || !_context.TryAllocateDma(DescriptorCount * SlotLength, ulong.MaxValue, out DmaBuffer? receiveSlots)
            || !_context.TryAllocateDma(DescriptorCount * TransmitDescriptor.Size, ulong.MaxValue, out DmaBuffer? transmitRing)
            || !_context.TryAllocateDma(DescriptorCount * SlotLength, ulong.MaxValue, out DmaBuffer? transmitSlots))
        {
            _context.WriteLog("no DMA memory for the descriptor rings and their frames");
            return false;
        }

        // Only once everything the two paths need exists, so neither runs
        // against a half-built ring if the device signals early.
        _receiveRing = receiveRing;
        _receiveSlots = receiveSlots;
        _transmitRing = transmitRing;
        _transmitSlots = transmitSlots;
        _receiveWork = receiveWork;

        // The receiver below asks for every multicast group, so the table only
        // has to be free of whatever hash firmware left in it.
        _registers.ClearMulticastTable();

        for (int index = 0; index < DescriptorCount; index++)
        {
            ReceiveDescriptor.Post(ReceiveDescriptorAt(receiveRing, index), SlotAddressOf(receiveSlots, index));
        }

        _registers.SetReceiveRing(receiveRing.DeviceAddress, DescriptorCount * ReceiveDescriptor.Size);
        _registers.SetTransmitRing(transmitRing.DeviceAddress, DescriptorCount * TransmitDescriptor.Size);

        // Only once the device is reset and knows where its rings are, so it
        // can never master through an address firmware left behind; and before
        // the engines below, which is when it starts to.
        _context.EnableBusMastering();

        _registers.EnableReceiver();
        _registers.EnableTransmitter();

        // Every descriptor but the last is the device's now: the tail is one
        // past the last it may write, so the descriptor it points at is the
        // hole that tells it where the driver has got to.
        _registers.ReceiveTail = DescriptorCount - 1;

        _registers.EnableInterrupts();
        _registers.SetLinkUp();

        _link = _context.PublishNetworkLink(address, Transmit);
        RefreshLinkState();
        return true;
    }

    /// <summary>
    /// The interrupt handler. Interrupt context: no allocation, no throw, no
    /// string, no lock. The driver asked for one vector, which both queues
    /// share, so any signal means the same thing: look at the ring.
    /// Acknowledging is what lets the device raise the next one.
    /// </summary>
    private void OnInterrupt(int vector)
    {
        _registers.AcknowledgeAndRearm();
        _receiveWork?.Schedule();
    }

    /// <summary>
    /// The receive path: driver-work thread, where it may allocate. Hands
    /// every frame the device has written to the stack, which copies it, then
    /// posts the descriptor back and moves the tail to say so.
    /// </summary>
    private void DrainReceiveRing()
    {
        if (_receiveRing is not { } ring || _receiveSlots is not { } slots || _link is not { } link)
        {
            return;
        }

        int posted = -1;
        while (true)
        {
            Span<byte> descriptor = ReceiveDescriptorAt(ring, _receiveIndex);
            byte status = ReceiveDescriptor.ReadStatus(descriptor);
            if ((status & ReceiveDescriptor.Done) == 0)
            {
                break;
            }

            // A frame the device split across descriptors, or one it flagged,
            // is dropped rather than delivered: the slot is as long as the
            // longest frame the receiver accepts, so neither should happen,
            // and a length past its end would read into the next slot.
            int length = ReceiveDescriptor.ReadLength(descriptor);
            if ((status & ReceiveDescriptor.EndOfPacket) != 0 && ReceiveDescriptor.ReadErrors(descriptor) == 0
                && length > 0 && length <= SlotLength)
            {
                link.Deliver(SlotOf(slots, _receiveIndex)[..length]);
            }

            // Clears the status with it, which is what hands the descriptor
            // back; the tail write below is what tells the device it is there.
            ReceiveDescriptor.Post(descriptor, SlotAddressOf(slots, _receiveIndex));
            posted = _receiveIndex;
            _receiveIndex = Next(_receiveIndex);
        }

        if (posted >= 0)
        {
            _registers.ReceiveTail = (uint)posted;
        }

        // A register read and a call the link takes from anywhere, on the one
        // thread that is not holding up an interrupt handler.
        RefreshLinkState();
    }

    /// <summary>
    /// The link's transmit handler. Interrupts masked, one call at a time, so
    /// the ring needs no lock: it copies the frame into a free slot and moves
    /// the tail on to it. Frames shorter than 64 bytes are padded by the
    /// device, which the transmitter was configured for.
    /// </summary>
    /// <returns>False when the frame is too long, or when every descriptor is still with the device.</returns>
    private bool Transmit(ReadOnlySpan<byte> frame)
    {
        if (_transmitRing is not { } ring || _transmitSlots is not { } slots
            || frame.Length == 0 || frame.Length > SlotLength)
        {
            return false;
        }

        // The driver fills from the tail up to the device's head, and a tail
        // that caught the head up would read to the device as a ring with
        // nothing in it, so one descriptor is always left unused: that is what
        // keeps a full ring from looking like an empty one.
        int next = Next(_transmitIndex);
        if (next == (int)_registers.TransmitHead)
        {
            return false;
        }

        frame.CopyTo(SlotOf(slots, _transmitIndex));
        TransmitDescriptor.Send(TransmitDescriptorAt(ring, _transmitIndex),
            SlotAddressOf(slots, _transmitIndex), frame.Length);

        _transmitIndex = next;
        _registers.TransmitTail = (uint)_transmitIndex;
        return true;
    }

    /// <summary>
    /// Resets the device, with every interrupt cause masked either side of it:
    /// before, so one firmware left enabled cannot fire into a vector nobody
    /// owns; after, because the reset puts the mask back to its default.
    /// </summary>
    /// <returns>False when the device never reported the reset done.</returns>
    private bool TryReset()
    {
        _registers.MaskAllInterrupts();
        _registers.BeginReset();

        int polls = 0;
        while (!_registers.ResetComplete)
        {
            if (++polls == ResetPolls)
            {
                _context.WriteLog("the reset did not complete");
                return false;
            }

            _context.Delay(TimeSpan.FromMicroseconds(10));
        }

        _registers.MaskAllInterrupts();
        return true;
    }

    /// <summary>
    /// Reads the address the controller is known by: the receive address
    /// filter, which the reset reloads from the EEPROM on most of the family,
    /// and the EEPROM itself on a part that leaves the filter empty, which is
    /// then programmed with it so the receiver filters on it.
    /// </summary>
    /// <param name="address">The address when the call returns true.</param>
    /// <returns>
    /// False when neither holds one. Without an address there is nothing to
    /// send from: the stack would come up on 00:00:00:00:00:00 and every reply
    /// to its ARP and DHCP would go nowhere, so this says so rather than
    /// publishing a link that cannot work.
    /// </returns>
    private bool TryReadMacAddress(out MACAddress address)
    {
        address = MACAddress.None;

        uint low = _registers.ReceiveAddressLow;
        uint high = _registers.ReceiveAddressHigh & ReceiveAddressHighBytes;
        if (low != 0 || high != 0)
        {
            address = new MACAddress(AddressBytes(low, high));
            return true;
        }

        byte[] bytes = new byte[MacAddressLength];
        for (int word = 0; word < MacAddressWords; word++)
        {
            if (!_registers.TryReadEepromWord((ushort)word, out ushort value))
            {
                _context.WriteLog("no address in the receive filter and no EEPROM to read one from");
                return false;
            }

            bytes[word * 2] = (byte)value;
            bytes[word * 2 + 1] = (byte)(value >> 8);
        }

        low = bytes[0] | ((uint)bytes[1] << 8) | ((uint)bytes[2] << 16) | ((uint)bytes[3] << 24);
        high = bytes[4] | ((uint)bytes[5] << 8);
        _registers.SetReceiveAddress(low, high);

        address = new MACAddress(bytes);
        return true;
    }

    /// <summary>Tells the link whether the device reports a carrier.</summary>
    private void RefreshLinkState() => _link?.SetLinkState(_registers.LinkUp);

    /// <summary>The descriptor after <paramref name="index"/>, wrapping at the end of either ring.</summary>
    private static int Next(int index) => index + 1 == DescriptorCount ? 0 : index + 1;

    /// <summary>Descriptor <paramref name="index"/> of the receive ring, in the driver's own memory.</summary>
    private static Span<byte> ReceiveDescriptorAt(DmaBuffer ring, int index) =>
        ring.Span.Slice(index * ReceiveDescriptor.Size, ReceiveDescriptor.Size);

    /// <summary>Descriptor <paramref name="index"/> of the transmit ring, in the driver's own memory.</summary>
    private static Span<byte> TransmitDescriptorAt(DmaBuffer ring, int index) =>
        ring.Span.Slice(index * TransmitDescriptor.Size, TransmitDescriptor.Size);

    /// <summary>The slot descriptor <paramref name="index"/> owns, in the driver's own memory.</summary>
    private static Span<byte> SlotOf(DmaBuffer slots, int index) => slots.Span.Slice(index * SlotLength, SlotLength);

    /// <summary>The same slot, as the device addresses it.</summary>
    private static ulong SlotAddressOf(DmaBuffer slots, int index) => slots.DeviceAddress + (ulong)(index * SlotLength);

    /// <summary>The six address bytes the receive address filter holds, in the order they go on the wire.</summary>
    private static byte[] AddressBytes(uint low, uint high) =>
    [
        (byte)low, (byte)(low >> 8), (byte)(low >> 16), (byte)(low >> 24),
        (byte)high, (byte)(high >> 8)
    ];
}
