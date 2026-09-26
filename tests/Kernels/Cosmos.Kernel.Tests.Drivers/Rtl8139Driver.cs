// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;
using Cosmos.Kernel.HAL.Drivers;
using Cosmos.Kernel.HAL.Drivers.Pci;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// A driver for the Realtek RTL8139 (10ec:8139), written against the driver
/// kit's seam alone, after the design's sample driver. Its registers go
/// through memory BAR 1, so the same code runs on x64 and ARM64; its DMA
/// buffers sit below 4 GiB, since the chip takes 32-bit bus addresses; and
/// it has no MSI-X, so the kit polls its interrupt handler from the timer.
/// The handler hands received frames to a work item, which delivers them to
/// the network stack through the link Probe published, and the stack sends
/// through <see cref="Transmit"/>. The rtl8139 cells check that the link
/// reaches the network manager and carries a DHCP exchange.
/// </summary>
/// <remarks>
/// Registered with a class match, after the suite's other Ethernet class
/// drivers, rather than with the device match a real driver would use: it
/// is then offered the RTL8139 after all of them, so the ranking cell still
/// sees them in order, and it declines every other NIC.
/// </remarks>
internal sealed class Rtl8139Driver : PciDriver
{
    /// <summary>The registration's name, and the owner the RTL8139 gets.</summary>
    public const string Name = "rtl8139";

    /// <summary>Realtek's vendor ID.</summary>
    public const ushort VendorId = 0x10EC;

    /// <summary>The RTL8139's device ID.</summary>
    public const ushort DeviceId = 0x8139;

    private const int RegisterBar = 1;
    private const ulong ThirtyTwoBitBus = uint.MaxValue;

    /// <summary>Config offset of the Command register.</summary>
    private const ushort CommandOffset = 0x04;

    // Registers in BAR 1 (RTL8139 datasheet, section 6).
    private const ulong MacRegister = 0x00;
    private const ulong TransmitStatus0 = 0x10;
    private const ulong TransmitAddress0 = 0x20;
    private const ulong ReceiveBufferStart = 0x30;
    private const ulong CommandRegister = 0x37;
    private const ulong ReceiveReadPointer = 0x38;
    private const ulong InterruptMask = 0x3C;
    private const ulong InterruptStatus = 0x3E;
    private const ulong ReceiveConfig = 0x44;
    private const ulong MediaStatus = 0x58;

    private const byte CommandReset = 0x10;
    private const byte CommandReceiveEnable = 0x08;
    private const byte CommandTransmitEnable = 0x04;
    private const byte CommandBufferEmpty = 0x01;
    private const ushort InterruptReceiveOk = 0x0001;
    private const ushort InterruptReceiveError = 0x0002;
    private const ushort InterruptTransmitOk = 0x0004;
    private const ushort InterruptTransmitError = 0x0008;
    private const ushort InterruptReceiveOverflow = 0x0010;
    private const ushort ReceiveCauses = InterruptReceiveOk | InterruptReceiveError | InterruptReceiveOverflow;

    /// <summary>Accept every frame: to any physical address, multicast and broadcast.</summary>
    private const uint ReceiveAcceptAll = 0x0F;

    /// <summary>
    /// WRAP: the chip writes a frame that runs past the end of the ring on
    /// past it, into the slack after the ring, rather than wrapping it to
    /// the start, so every frame is contiguous in the buffer.
    /// </summary>
    private const uint ReceiveWrap = 0x80;

    /// <summary>OWN in a transmit status register: set after reset, and again once the chip copied the slot.</summary>
    private const uint TransmitHostOwns = 0x2000;

    private const ushort ReceiveStatusOk = 0x0001;

    /// <summary>LINKB in the media status register: set while the link is down.</summary>
    private const byte MediaLinkDown = 0x04;

    /// <summary>The ring size RCR's buffer length field selects when left at 0.</summary>
    private const int ReceiveRingLength = 8192;

    /// <summary>The ring, the 16 bytes the datasheet adds, and room for the longest frame WRAP writes past the end.</summary>
    private const int ReceiveBufferLength = ReceiveRingLength + 16 + 1536;

    private const int TransmitSlotLength = 2048;
    private const int TransmitSlotCount = 4;
    private const int MinimumFrameLength = 60;

    /// <summary>60 bytes plus the CRC: the chip pads shorter frames.</summary>
    private const int MinimumReceivedLength = 64;

    private const int MaximumReceivedLength = 1522;

    /// <summary>Bytes of the header the chip writes in front of each received frame: status, then length.</summary>
    private const int ReceiveHeaderLength = 4;

    /// <summary>The CRC the chip leaves at the end of each received frame, which the stack does not want.</summary>
    private const int CrcLength = 4;

    /// <summary>CAPR lags the next frame's offset by 16 bytes.</summary>
    private const int ReadPointerLag = 16;

    private const int ResetPolls = 1000;

    // Written from the interrupt handler, the driver-work thread and the
    // stack's sender, read by the cells: fields behind Volatile reads.
    private static int s_handlerCalls;
    private static int s_receiveInterrupts;
    private static int s_receiveWorkRuns;
    private static uint s_receiveWorkThreadId;
    private static int s_framesDelivered;
    private static int s_badFrames;
    private static int s_transmits;

    // Set in Probe. The kit arms the interrupt and runs the work item only
    // after Probe returns Bound; the null checks below are for the compiler.
    private MmioRegion? _registers;
    private DmaBuffer? _receiveBuffer;
    private DmaBuffer? _transmitBuffer;
    private DeviceWorkItem? _receiveWork;
    private NetworkLink? _link;
    private int _receiveOffset;
    private int _nextTransmitSlot;

    /// <summary>True once Probe ran on the RTL8139.</summary>
    public static bool Probed { get; private set; }

    /// <summary>The Command register as Probe found it, after the Ethernet class drivers declined.</summary>
    public static ushort CommandAtProbe { get; private set; }

    /// <summary>What TryRequestInterrupts answered.</summary>
    public static bool InterruptsGranted { get; private set; }

    /// <summary>The MAC address Probe read from the chip and published the link with; null until then.</summary>
    public static MACAddress? Address { get; private set; }

    /// <summary>What the media status register said about the link when Probe read it.</summary>
    public static bool LinkUpAtProbe { get; private set; }

    /// <summary>Why Probe failed, when it did; null otherwise.</summary>
    public static string? ProbeFailure { get; private set; }

    /// <summary>Calls of the handler: polled, one per timer tick once Bound.</summary>
    public static int HandlerCalls => Volatile.Read(ref s_handlerCalls);

    /// <summary>Handler calls that found a receive cause in the interrupt status, and scheduled the receive work item.</summary>
    public static int ReceiveInterrupts => Volatile.Read(ref s_receiveInterrupts);

    /// <summary>Runs of the receive work item.</summary>
    public static int ReceiveWorkRuns => Volatile.Read(ref s_receiveWorkRuns);

    /// <summary>The scheduler's ID for the thread the receive work item last ran on.</summary>
    public static uint ReceiveWorkThreadId => Volatile.Read(ref s_receiveWorkThreadId);

    /// <summary>Frames the work item delivered to the stack.</summary>
    public static int FramesDelivered => Volatile.Read(ref s_framesDelivered);

    /// <summary>Ring entries whose header the work item could not accept, which stop the drain.</summary>
    public static int BadFrames => Volatile.Read(ref s_badFrames);

    /// <summary>Frames the chip took from the stack's sends.</summary>
    public static int Transmits => Volatile.Read(ref s_transmits);

    /// <inheritdoc />
    // protected internal, not protected: this assembly sees the HAL's
    // internals, so the override must keep the base's full accessibility.
    protected internal override ProbeResult Probe(PciDeviceContext context)
    {
        ProbeLog.Record(Name);
        PciFunction function = context.Function;
        if (function.VendorId != VendorId || function.DeviceId != DeviceId)
        {
            return ProbeResult.Declined;
        }

        Probed = true;
        CommandAtProbe = function.ReadConfig16(CommandOffset);

        if (!context.TryMapBar(RegisterBar, out MmioRegion? registers))
        {
            return Fail(context, "BAR 1 did not map");
        }

        registers.Write8(CommandRegister, CommandReset);
        int polls = 0;
        while ((registers.Read8(CommandRegister) & CommandReset) != 0)
        {
            if (++polls == ResetPolls)
            {
                return Fail(context, "the reset did not complete");
            }

            context.Delay(TimeSpan.FromMicroseconds(10));
        }

        if (!context.TryAllocateDma(ReceiveBufferLength, ThirtyTwoBitBus, out DmaBuffer? receiveBuffer)
            || !context.TryAllocateDma(TransmitSlotCount * TransmitSlotLength, ThirtyTwoBitBus, out DmaBuffer? transmitBuffer))
        {
            return Fail(context, "no DMA memory below 4 GiB");
        }

        if (!context.TryCreateWorkItem(DrainReceiveRing, out DeviceWorkItem? receiveWork))
        {
            return Fail(context, "no work item for the receive path");
        }

        InterruptsGranted = context.TryRequestInterrupts(OnInterrupt);
        if (!InterruptsGranted)
        {
            return Fail(context, "no MSI-X and no ticking timer to poll with");
        }

        // Only once the chip is reset: it must not master into the buffers
        // with state firmware left behind.
        context.EnableBusMastering();
        registers.Write32(ReceiveBufferStart, (uint)receiveBuffer.DeviceAddress);
        for (int slot = 0; slot < TransmitSlotCount; slot++)
        {
            registers.Write32(TransmitAddress0 + (ulong)(slot * sizeof(uint)),
                (uint)transmitBuffer.DeviceAddress + (uint)(slot * TransmitSlotLength));
        }

        // RCR before the receiver is enabled: writing it also starts the
        // chip's ring at the buffer's first byte, where _receiveOffset starts.
        registers.Write32(ReceiveConfig, ReceiveAcceptAll | ReceiveWrap);
        registers.Write8(CommandRegister, CommandReceiveEnable | CommandTransmitEnable);
        registers.Write16(InterruptMask, ReceiveCauses | InterruptTransmitOk | InterruptTransmitError);

        _registers = registers;
        _receiveBuffer = receiveBuffer;
        _transmitBuffer = transmitBuffer;
        _receiveWork = receiveWork;

        byte[] mac = new byte[6];
        for (int i = 0; i < mac.Length; i++)
        {
            mac[i] = registers.Read8(MacRegister + (ulong)i);
        }

        MACAddress address = new(mac);
        Address = address;

        // The network manager receives the link right after this returns Bound.
        _link = context.PublishNetworkLink(address, Transmit);
        LinkUpAtProbe = (registers.Read8(MediaStatus) & MediaLinkDown) == 0;
        _link.SetLinkState(LinkUpAtProbe);
        return ProbeResult.Bound;
    }

    private static ProbeResult Fail(PciDeviceContext context, string reason)
    {
        ProbeFailure = reason;
        context.WriteLog(reason);
        return ProbeResult.Failed;
    }

    /// <summary>
    /// The interrupt handler. Interrupt context: no allocation, no throw, no
    /// string. Polled, it runs on every tick, so it reads the chip's own
    /// status to see whether there is anything to do, acknowledges it, and
    /// leaves the receive ring to the work item.
    /// </summary>
    private void OnInterrupt(int vector)
    {
        s_handlerCalls++;
        if (_registers is not { } registers)
        {
            return;
        }

        ushort status = registers.Read16(InterruptStatus);
        if (status == 0)
        {
            return;
        }

        // Write 1 to clear.
        registers.Write16(InterruptStatus, status);
        if ((status & ReceiveCauses) != 0)
        {
            s_receiveInterrupts++;
            _receiveWork?.Schedule();
        }
    }

    /// <summary>
    /// The receive work item: driver-work thread, where it may allocate.
    /// Hands every frame in the ring to the stack, which copies it.
    /// </summary>
    private void DrainReceiveRing()
    {
        if (KernelState.TryGetCurrentThread(out uint threadId, out _))
        {
            Volatile.Write(ref s_receiveWorkThreadId, threadId);
        }

        Interlocked.Increment(ref s_receiveWorkRuns);
        if (_registers is not { } registers || _receiveBuffer is not { } buffer || _link is not { } link)
        {
            return;
        }

        Span<byte> ring = buffer.Span;

        // Each Command read is followed by a DMA read barrier, so the ring
        // loads below see what the chip wrote before it cleared BUFE.
        while ((registers.Read8(CommandRegister) & CommandBufferEmpty) == 0)
        {
            ushort status = BinaryPrimitives.ReadUInt16LittleEndian(ring[_receiveOffset..]);
            int length = BinaryPrimitives.ReadUInt16LittleEndian(ring[(_receiveOffset + 2)..]);
            if ((status & ReceiveStatusOk) == 0 || length < MinimumReceivedLength || length > MaximumReceivedLength)
            {
                // A production driver resets the receiver here; the cells
                // report the count instead.
                Interlocked.Increment(ref s_badFrames);
                break;
            }

            link.Deliver(ring.Slice(_receiveOffset + ReceiveHeaderLength, length - CrcLength));
            Interlocked.Increment(ref s_framesDelivered);

            _receiveOffset = (_receiveOffset + ReceiveHeaderLength + length + 3) & ~3;
            if (_receiveOffset >= ReceiveRingLength)
            {
                _receiveOffset -= ReceiveRingLength;
            }

            registers.Write16(ReceiveReadPointer, (ushort)(_receiveOffset - ReadPointerLag));
        }
    }

    /// <summary>
    /// The link's transmit handler. Interrupts masked, one call at a time:
    /// the kit serializes it, so the slot rotation needs no lock.
    /// </summary>
    private bool Transmit(ReadOnlySpan<byte> frame)
    {
        if (_registers is not { } registers || _transmitBuffer is not { } buffer || frame.Length > TransmitSlotLength)
        {
            return false;
        }

        int slot = _nextTransmitSlot;
        ulong status = TransmitStatus0 + (ulong)(slot * sizeof(uint));
        if ((registers.Read32(status) & TransmitHostOwns) == 0)
        {
            // The chip is still copying this slot.
            return false;
        }

        Span<byte> data = buffer.Span.Slice(slot * TransmitSlotLength, TransmitSlotLength);
        frame.CopyTo(data);
        int length = Math.Max(frame.Length, MinimumFrameLength);
        data[frame.Length..length].Clear();

        // OWN = 0 starts the copy. The register write's DMA write barrier
        // orders it after the stores above.
        registers.Write32(status, (uint)length);
        _nextTransmitSlot = (slot + 1) % TransmitSlotCount;
        Interlocked.Increment(ref s_transmits);
        return true;
    }
}
