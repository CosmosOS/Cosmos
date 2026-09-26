// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Buffers.Binary;
using Cosmos.Kernel.HAL.Drivers;
using Cosmos.Kernel.HAL.Drivers.Pci;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace DevKernel.Drivers;

/// <summary>
/// Realtek RTL8139 (10ec:8139), one instance per bound function, written
/// against the driver kit alone. Its registers go through memory BAR 1, so
/// the same code runs on x64 and ARM64; its DMA buffers sit below 4 GiB,
/// since the chip takes 32-bit bus addresses; and it has no MSI-X, so the kit
/// calls <see cref="OnInterrupt"/> from the timer interrupt, about every
/// 55 ms on x64 and 10 ms on ARM64. The handler hands received frames to a
/// work item, which delivers them to the network stack through the link
/// Probe published; the stack sends through <see cref="Transmit"/>.
/// </summary>
/// <remarks>
/// The driver has no teardown code: a PCI function is never unbound in this
/// version, and when Probe fails the kit restores the Command register,
/// which turns bus mastering off, before it frees the DMA buffers.
/// </remarks>
internal sealed class Rtl8139Driver : PciDriver
{
    /// <summary>The registration's name, and the owner the RTL8139 gets.</summary>
    public const string Name = "rtl8139";

    private const ushort RealtekVendorId = 0x10EC;
    private const ushort Rtl8139DeviceId = 0x8139;
    private const int RegisterBar = 1;
    private const ulong ThirtyTwoBitBus = uint.MaxValue;

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
    /// into the slack after it, rather than wrapping it to the start, so
    /// every frame is contiguous in the buffer.
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

    // Set in Probe. The kit arms the interrupt and runs the work item only
    // after Probe returns Bound; the null checks below are for the compiler.
    private MmioRegion? _registers;
    private DmaBuffer? _receiveBuffer;
    private DmaBuffer? _transmitBuffer;
    private DeviceWorkItem? _receiveWork;
    private NetworkLink? _link;
    private int _receiveOffset;
    private int _nextTransmitSlot;

    /// <summary>The registration the kernel passes to DriverManager.Register: the RTL8139 by vendor and device ID.</summary>
    /// <returns>A registration named <see cref="Name"/>.</returns>
    public static PciDriverRegistration CreateRegistration() =>
        new(Name, static () => new Rtl8139Driver(), PciMatch.Device(RealtekVendorId, Rtl8139DeviceId));

    /// <inheritdoc />
    protected override ProbeResult Probe(PciDeviceContext context)
    {
        if (!context.TryMapBar(RegisterBar, out MmioRegion? registers))
        {
            return ProbeResult.Declined;
        }

        registers.Write8(CommandRegister, CommandReset);
        int polls = 0;
        while ((registers.Read8(CommandRegister) & CommandReset) != 0)
        {
            if (++polls == ResetPolls)
            {
                context.WriteLog("the reset did not complete");
                return ProbeResult.Failed;
            }

            context.Delay(TimeSpan.FromMicroseconds(10));
        }

        if (!context.TryAllocateDma(ReceiveBufferLength, ThirtyTwoBitBus, out DmaBuffer? receiveBuffer)
            || !context.TryAllocateDma(TransmitSlotCount * TransmitSlotLength, ThirtyTwoBitBus, out DmaBuffer? transmitBuffer))
        {
            context.WriteLog("no DMA memory below 4 GiB");
            return ProbeResult.Failed;
        }

        if (!context.TryCreateWorkItem(DrainReceiveRing, out DeviceWorkItem? receiveWork))
        {
            context.WriteLog("needs the scheduler for its receive path");
            return ProbeResult.Failed;
        }

        if (!context.TryRequestInterrupts(OnInterrupt))
        {
            context.WriteLog("no MSI-X and no ticking timer to poll with");
            return ProbeResult.Failed;
        }

        registers.Write32(ReceiveBufferStart, (uint)receiveBuffer.DeviceAddress);
        for (int slot = 0; slot < TransmitSlotCount; slot++)
        {
            registers.Write32(TransmitAddress0 + (ulong)(slot * sizeof(uint)),
                (uint)transmitBuffer.DeviceAddress + (uint)(slot * TransmitSlotLength));
        }

        // Only once the chip is reset and knows where its buffers are, so it
        // can never master through an address firmware left behind; and
        // before the receiver and transmitter are enabled below, which is
        // when it starts to.
        context.EnableBusMastering();

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

        // The network manager receives the link right after this returns Bound.
        _link = context.PublishNetworkLink(new MACAddress(mac), Transmit);
        _link.SetLinkState((registers.Read8(MediaStatus) & MediaLinkDown) == 0);
        return ProbeResult.Bound;
    }

    /// <summary>
    /// The interrupt handler. Interrupt context: no allocation, no throw, no
    /// string, no lock. Polled, it runs on every tick, so it reads the chip's
    /// own status to see whether there is anything to do, acknowledges it,
    /// and leaves the receive ring to the work item.
    /// </summary>
    private void OnInterrupt(int vector)
    {
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
            _receiveWork?.Schedule();
        }
    }

    /// <summary>
    /// The receive work item: driver-work thread, where it may allocate.
    /// Hands every frame in the ring to the stack, which copies it.
    /// </summary>
    private void DrainReceiveRing()
    {
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
                // A production driver resets the receiver here.
                break;
            }

            link.Deliver(ring.Slice(_receiveOffset + ReceiveHeaderLength, length - CrcLength));

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
        return true;
    }
}
