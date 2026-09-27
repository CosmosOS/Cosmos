// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.E1000e.Registers;

/// <summary>
/// The registers of an Intel gigabit Ethernet controller of the E1000E
/// family, in the mapped BAR 0 (82574 datasheet §8): the control and status
/// pair, the interrupt block, the two descriptor rings' base, length, head
/// and tail, the receive address filter and the multicast table.
/// </summary>
/// <remarks>
/// Every access goes through <see cref="MmioRegion"/>, which bounds-checks
/// the offset and carries the DMA barriers: a ring's descriptors are stores
/// to ordinary memory, and the tail write that hands them over has to be
/// seen after them.
/// </remarks>
internal sealed class ControllerRegisters
{
    /// <summary>CTRL: Device Control.</summary>
    private const ulong ControlOffset = 0x0000;

    /// <summary>STATUS: Device Status.</summary>
    private const ulong StatusOffset = 0x0008;

    /// <summary>EERD: EEPROM Read.</summary>
    private const ulong EepromReadOffset = 0x0014;

    /// <summary>CTRL_EXT: Extended Device Control.</summary>
    private const ulong ExtendedControlOffset = 0x0018;

    /// <summary>ICR: Interrupt Cause Read, read to acknowledge the interrupt.</summary>
    private const ulong InterruptCauseOffset = 0x00C0;

    /// <summary>IMS: Interrupt Mask Set/Read.</summary>
    private const ulong InterruptMaskSetOffset = 0x00D0;

    /// <summary>IMC: Interrupt Mask Clear.</summary>
    private const ulong InterruptMaskClearOffset = 0x00D8;

    /// <summary>EIAC: Extended Interrupt Auto Clear.</summary>
    private const ulong InterruptAutoClearOffset = 0x00DC;

    /// <summary>IVAR: Interrupt Vector Allocation, which MSI-X vector each cause is sent on.</summary>
    private const ulong InterruptVectorAllocationOffset = 0x00E4;

    /// <summary>RCTL: Receive Control.</summary>
    private const ulong ReceiveControlOffset = 0x0100;

    /// <summary>TCTL: Transmit Control.</summary>
    private const ulong TransmitControlOffset = 0x0400;

    /// <summary>TIPG: Transmit Inter Packet Gap.</summary>
    private const ulong InterPacketGapOffset = 0x0410;

    // RDBAL, RDBAH, RDLEN, RDH, RDT: the receive ring, where it is, how long
    // it is, and the two ends the device and the driver each own.
    private const ulong ReceiveRingBaseLowOffset = 0x2800;
    private const ulong ReceiveRingBaseHighOffset = 0x2804;
    private const ulong ReceiveRingLengthOffset = 0x2808;
    private const ulong ReceiveHeadOffset = 0x2810;
    private const ulong ReceiveTailOffset = 0x2818;

    // TDBAL, TDBAH, TDLEN, TDH, TDT: the transmit ring, the same way round.
    private const ulong TransmitRingBaseLowOffset = 0x3800;
    private const ulong TransmitRingBaseHighOffset = 0x3804;
    private const ulong TransmitRingLengthOffset = 0x3808;
    private const ulong TransmitHeadOffset = 0x3810;
    private const ulong TransmitTailOffset = 0x3818;

    /// <summary>MTA: the first of the multicast table's entries.</summary>
    private const ulong MulticastTableOffset = 0x5200;

    /// <summary>RAL0 and RAH0: the first receive address filter, the device's own.</summary>
    private const ulong ReceiveAddressLowOffset = 0x5400;
    private const ulong ReceiveAddressHighOffset = 0x5404;

    /// <summary>Entries of the multicast table, one bit of the hash each.</summary>
    private const uint MulticastTableEntries = 128;

    /// <summary>
    /// Bytes of BAR 0 the offsets above reach into. A controller whose BAR is
    /// shorter is one this driver would throw on halfway through bring-up.
    /// </summary>
    private const ulong RequiredLength = ReceiveAddressHighOffset + sizeof(uint);

    /// <summary>CTRL.SLU: bring the link up rather than leave it to the PHY's strapping.</summary>
    private const uint ControlSetLinkUp = 1 << 6;

    /// <summary>CTRL.RST: the device reset, which the device clears once it is over.</summary>
    private const uint ControlReset = 1u << 26;

    /// <summary>STATUS.LU: the link has a carrier.</summary>
    private const uint StatusLinkUp = 1 << 1;

    /// <summary>RAH0.AV: the filter holds an address to compare against.</summary>
    private const uint ReceiveAddressValid = 1u << 31;

    /// <summary>EERD.START, and EERD.DONE once the word read back.</summary>
    private const uint EepromStart = 1 << 0;
    private const uint EepromDone = 1 << 4;

    /// <summary>Bit position of EERD's word address, and of the word it returns.</summary>
    private const int EepromAddressShift = 8;
    private const int EepromDataShift = 16;

    /// <summary>Reads of EERD before an EEPROM word is given up on; the device answers in microseconds.</summary>
    private const int EepromPolls = 1000;

    /// <summary>CTRL_EXT.IAME: reading ICR auto-masks the causes. Off, so the mask stays where it was put.</summary>
    private const uint ExtendedControlInterruptAckAutoMask = 1u << 27;

    /// <summary>CTRL_EXT.PBA_CLR: reading ICR clears the MSI-X pending bit, so the next message is sent.</summary>
    private const uint ExtendedControlPendingBitClear = 1u << 31;

    /// <summary>RCTL.EN, .MPE, .BAM, .SECRC: the receiver, multicast promiscuous, broadcast, and strip the CRC.</summary>
    private const uint ReceiveEnable = 1 << 1;
    private const uint ReceiveMulticastPromiscuous = 1 << 4;
    private const uint ReceiveBroadcastAccept = 1 << 15;
    private const uint ReceiveStripCrc = 1u << 26;

    /// <summary>RCTL.BSIZE = 0: 2048-byte receive buffers, which is the field's reset value.</summary>
    private const uint ReceiveBufferSize2048 = 0;

    /// <summary>TCTL.EN and .PSP: the transmitter, and pad a frame shorter than 64 bytes in hardware.</summary>
    private const uint TransmitEnable = 1 << 1;
    private const uint TransmitPadShortPackets = 1 << 3;

    /// <summary>TCTL.CT and .COLD: 15 retries and a 64-byte collision distance, the full-duplex defaults.</summary>
    private const uint TransmitCollisionThreshold = 15u << 4;
    private const uint TransmitCollisionDistance = 64u << 12;

    /// <summary>TIPG: IPGT 10, IPGR1 8, IPGR2 6, the IEEE 802.3 gaps.</summary>
    private const uint InterPacketGap = 10 | (8u << 10) | (6u << 20);

    /// <summary>IVAR's per-cause valid bit; the three bits under it are the MSI-X vector.</summary>
    private const uint VectorAllocationValid = 0x8;

    /// <summary>Bit position of IVAR's transmit queue 0 field; receive queue 0 is at the bottom.</summary>
    private const int VectorAllocationTransmitShift = 8;

    /// <summary>IVAR bit 31: report a transmit interrupt on every descriptor write-back.</summary>
    private const uint VectorAllocationTransmitEveryWriteBack = 1u << 31;

    /// <summary>ICR/IMS/EIAC bits: receive queue 0 and transmit queue 0.</summary>
    private const uint CauseReceiveQueue0 = 1 << 20;
    private const uint CauseTransmitQueue0 = 1 << 22;

    /// <summary>
    /// The causes this driver runs on, and the whole of what it puts in IMS.
    /// Both are queue causes, which is what lets EIAC clear them: a cause
    /// outside EIAC's range, a link status change among them, would stay set
    /// in ICR, since reading ICR does not clear it while MSI-X is on, and the
    /// device would raise a message for it again as soon as the mask went
    /// back. The link is read from the status register on every drain
    /// instead, which is where the driver would act on it anyway.
    /// </summary>
    private const uint EnabledCauses = CauseReceiveQueue0 | CauseTransmitQueue0;

    /// <summary>Every cause, which IMC clears in one write.</summary>
    private const uint AllCauses = 0xFFFF_FFFF;

    private readonly MmioRegion _registers;

    /// <summary>True while the device reports a carrier.</summary>
    internal bool LinkUp => (_registers.Read32(StatusOffset) & StatusLinkUp) != 0;

    /// <summary>True once the device has finished the reset <see cref="BeginReset"/> started.</summary>
    internal bool ResetComplete => (_registers.Read32(ControlOffset) & ControlReset) == 0;

    /// <summary>RAL0: the low four bytes of the address the receiver filters on.</summary>
    internal uint ReceiveAddressLow => _registers.Read32(ReceiveAddressLowOffset);

    /// <summary>RAH0: the high two bytes, under the validity and select fields above them.</summary>
    internal uint ReceiveAddressHigh => _registers.Read32(ReceiveAddressHighOffset);

    /// <summary>TDH: the transmit descriptor the device is on, which bounds what the driver may fill.</summary>
    internal uint TransmitHead => _registers.Read32(TransmitHeadOffset);

    /// <summary>RDT: one past the last receive descriptor the device may write into.</summary>
    internal uint ReceiveTail
    {
        set => _registers.Write32(ReceiveTailOffset, value);
    }

    /// <summary>TDT: one past the last transmit descriptor the driver has filled.</summary>
    internal uint TransmitTail
    {
        set => _registers.Write32(TransmitTailOffset, value);
    }

    /// <summary>Views the registers of a controller whose BAR 0 is <paramref name="registers"/>.</summary>
    internal ControllerRegisters(MmioRegion registers) => _registers = registers;

    /// <summary>True when <paramref name="registers"/> is long enough to hold every register this driver uses.</summary>
    internal static bool Covers(MmioRegion registers) => registers.Length >= RequiredLength;

    /// <summary>Masks every interrupt cause, whatever the reset or firmware left enabled.</summary>
    internal void MaskAllInterrupts() => _registers.Write32(InterruptMaskClearOffset, AllCauses);

    /// <summary>Starts the device reset; the caller waits on <see cref="ResetComplete"/>.</summary>
    internal void BeginReset() => _registers.Write32(ControlOffset, _registers.Read32(ControlOffset) | ControlReset);

    /// <summary>Tells the PHY to bring the link up, which the device reports through <see cref="LinkUp"/>.</summary>
    internal void SetLinkUp() => _registers.Write32(ControlOffset, _registers.Read32(ControlOffset) | ControlSetLinkUp);

    /// <summary>Clears the multicast table, so no group is accepted by a hash firmware left behind.</summary>
    internal void ClearMulticastTable()
    {
        for (uint entry = 0; entry < MulticastTableEntries; entry++)
        {
            _registers.Write32(MulticastTableOffset + entry * sizeof(uint), 0);
        }
    }

    /// <summary>
    /// Programs the receive address filter with the device's own address and
    /// marks it valid, so the receiver accepts what is addressed to it.
    /// </summary>
    /// <param name="low">The low four bytes, in the order they go on the wire.</param>
    /// <param name="high">The high two bytes; the validity bit is added here.</param>
    internal void SetReceiveAddress(uint low, uint high)
    {
        _registers.Write32(ReceiveAddressLowOffset, low);
        _registers.Write32(ReceiveAddressHighOffset, high | ReceiveAddressValid);
    }

    /// <summary>
    /// Reads one word of the EEPROM through EERD. The device answers in
    /// microseconds, so the wait is a bounded spin rather than a delay.
    /// </summary>
    /// <param name="address">The word's address, 0 to 2 for the MAC address.</param>
    /// <param name="value">The word read back when the call returns true.</param>
    /// <returns>False when the device never reported the read done, which means it has no EEPROM to read.</returns>
    internal bool TryReadEepromWord(ushort address, out ushort value)
    {
        _registers.Write32(EepromReadOffset, ((uint)address << EepromAddressShift) | EepromStart);

        for (int poll = 0; poll < EepromPolls; poll++)
        {
            uint read = _registers.Read32(EepromReadOffset);
            if ((read & EepromDone) != 0)
            {
                value = (ushort)(read >> EepromDataShift);
                return true;
            }
        }

        value = 0;
        return false;
    }

    /// <summary>
    /// Points the device at the receive ring and empties it: the head and the
    /// tail both at 0, which the caller moves once the descriptors are posted.
    /// </summary>
    /// <param name="address">The ring, as the device addresses it.</param>
    /// <param name="length">Bytes of the ring, a multiple of 128.</param>
    internal void SetReceiveRing(ulong address, uint length)
    {
        _registers.Write32(ReceiveRingBaseLowOffset, (uint)address);
        _registers.Write32(ReceiveRingBaseHighOffset, (uint)(address >> 32));
        _registers.Write32(ReceiveRingLengthOffset, length);
        _registers.Write32(ReceiveHeadOffset, 0);
        _registers.Write32(ReceiveTailOffset, 0);
    }

    /// <summary>Points the device at the transmit ring and empties it, as <see cref="SetReceiveRing"/> does.</summary>
    /// <param name="address">The ring, as the device addresses it.</param>
    /// <param name="length">Bytes of the ring, a multiple of 128.</param>
    internal void SetTransmitRing(ulong address, uint length)
    {
        _registers.Write32(TransmitRingBaseLowOffset, (uint)address);
        _registers.Write32(TransmitRingBaseHighOffset, (uint)(address >> 32));
        _registers.Write32(TransmitRingLengthOffset, length);
        _registers.Write32(TransmitHeadOffset, 0);
        _registers.Write32(TransmitTailOffset, 0);
    }

    /// <summary>
    /// Starts the receiver on 2048-byte buffers, accepting what is addressed
    /// to the filter, broadcast, and every multicast group: IPv6 Neighbour
    /// Discovery arrives on solicited-node groups, which the empty multicast
    /// table would otherwise filter out. The device strips the CRC, so a
    /// descriptor's length is the frame the stack wants.
    /// </summary>
    internal void EnableReceiver() =>
        _registers.Write32(ReceiveControlOffset,
            ReceiveEnable | ReceiveBroadcastAccept | ReceiveMulticastPromiscuous | ReceiveBufferSize2048 | ReceiveStripCrc);

    /// <summary>Sets the inter-packet gaps and starts the transmitter, which pads short frames itself.</summary>
    internal void EnableTransmitter()
    {
        _registers.Write32(InterPacketGapOffset, InterPacketGap);
        _registers.Write32(TransmitControlOffset,
            TransmitEnable | TransmitPadShortPackets | TransmitCollisionThreshold | TransmitCollisionDistance);
    }

    /// <summary>
    /// Routes both queues to MSI-X vector 0, the one entry the kit programs,
    /// and enables them. Programmed whether or not the kit granted MSI-X: the
    /// device consults IVAR in MSI-X mode only, so on the polled path this
    /// leaves nothing but the causes latched in ICR, which is all the handler
    /// needs either way.
    /// </summary>
    internal void EnableInterrupts()
    {
        _registers.Write32(InterruptVectorAllocationOffset,
            VectorAllocationValid
            | (VectorAllocationValid << VectorAllocationTransmitShift)
            | VectorAllocationTransmitEveryWriteBack);

        // Reading ICR has to clear the pending bit, or a message raised while
        // the kit still had the MSI-X entry masked would keep the device from
        // sending the next one; and it must not mask the causes with it,
        // since only EIAC below is meant to.
        uint extended = _registers.Read32(ExtendedControlOffset);
        extended &= ~ExtendedControlInterruptAckAutoMask;
        extended |= ExtendedControlPendingBitClear;
        _registers.Write32(ExtendedControlOffset, extended);

        // The causes clear themselves as the message goes out, so the handler
        // never has to acknowledge a frame it is about to read.
        _registers.Write32(InterruptAutoClearOffset, EnabledCauses);
        _registers.Write32(InterruptMaskSetOffset, EnabledCauses);
    }

    /// <summary>
    /// Acknowledges the interrupt and puts the mask back. Interrupt context.
    /// Reading ICR clears the MSI-X pending bit behind the causes; what they
    /// say is discarded, since the handler looks at the ring whichever one
    /// fired, and on the polled path it runs with none of them set. Writing
    /// IMS again is what keeps the next message coming: the device takes a
    /// cause out of the mask as it sends the message for it, and leaves it
    /// out until the driver asks for it back.
    /// </summary>
    internal void AcknowledgeAndRearm()
    {
        _ = _registers.Read32(InterruptCauseOffset);
        _registers.Write32(InterruptMaskSetOffset, EnabledCauses);
    }
}
