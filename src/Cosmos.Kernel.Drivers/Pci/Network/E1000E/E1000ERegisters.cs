// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Pci.Network.E1000E;

/// <summary>
/// The register map of the Intel 82574 (E1000E) family as the driver and
/// its state use it: byte offsets into the BAR0 window, the bits of the
/// registers they program, and the descriptor command and status bits.
/// Constants only, no execution context.
/// </summary>
internal static class E1000ERegisters
{
    /// <summary>Device Control (CTRL).</summary>
    public const ulong Control = 0x0000;

    /// <summary>Device Status (STATUS).</summary>
    public const ulong Status = 0x0008;

    /// <summary>EEPROM/Flash Control (EECD).</summary>
    public const ulong EepromControl = 0x0010;

    /// <summary>EEPROM Read (EERD).</summary>
    public const ulong EepromRead = 0x0014;

    /// <summary>Interrupt Cause Read (ICR): a read clears it.</summary>
    public const ulong InterruptCauseRead = 0x00C0;

    /// <summary>Interrupt Mask Set/Read (IMS).</summary>
    public const ulong InterruptMaskSet = 0x00D0;

    /// <summary>Interrupt Mask Clear (IMC).</summary>
    public const ulong InterruptMaskClear = 0x00D8;

    /// <summary>Receive Control (RCTL).</summary>
    public const ulong ReceiveControl = 0x0100;

    /// <summary>Transmit Control (TCTL).</summary>
    public const ulong TransmitControl = 0x0400;

    /// <summary>Transmit Inter Packet Gap (TIPG).</summary>
    public const ulong TransmitInterPacketGap = 0x0410;

    /// <summary>Receive Descriptor Base Address Low (RDBAL).</summary>
    public const ulong ReceiveDescriptorBaseLow = 0x2800;

    /// <summary>Receive Descriptor Base Address High (RDBAH).</summary>
    public const ulong ReceiveDescriptorBaseHigh = 0x2804;

    /// <summary>Receive Descriptor Length (RDLEN), in bytes.</summary>
    public const ulong ReceiveDescriptorLength = 0x2808;

    /// <summary>Receive Descriptor Head (RDH).</summary>
    public const ulong ReceiveDescriptorHead = 0x2810;

    /// <summary>Receive Descriptor Tail (RDT).</summary>
    public const ulong ReceiveDescriptorTail = 0x2818;

    /// <summary>Transmit Descriptor Base Address Low (TDBAL).</summary>
    public const ulong TransmitDescriptorBaseLow = 0x3800;

    /// <summary>Transmit Descriptor Base Address High (TDBAH).</summary>
    public const ulong TransmitDescriptorBaseHigh = 0x3804;

    /// <summary>Transmit Descriptor Length (TDLEN), in bytes.</summary>
    public const ulong TransmitDescriptorLength = 0x3808;

    /// <summary>Transmit Descriptor Head (TDH).</summary>
    public const ulong TransmitDescriptorHead = 0x3810;

    /// <summary>Transmit Descriptor Tail (TDT).</summary>
    public const ulong TransmitDescriptorTail = 0x3818;

    /// <summary>Multicast Table Array (MTA): <see cref="MulticastTableEntries"/> dwords.</summary>
    public const ulong MulticastTableArray = 0x5200;

    /// <summary>Number of dwords in the multicast table array.</summary>
    public const int MulticastTableEntries = 128;

    /// <summary>Bytes per multicast table entry.</summary>
    public const int MulticastTableEntryBytes = 4;

    /// <summary>Receive Address Low 0 (RAL0): the first four bytes of the station address.</summary>
    public const ulong ReceiveAddressLow0 = 0x5400;

    /// <summary>Receive Address High 0 (RAH0): the last two bytes of the station address and the valid bit.</summary>
    public const ulong ReceiveAddressHigh0 = 0x5404;

    /// <summary>CTRL bit 6: Set Link Up.</summary>
    public const uint ControlSetLinkUp = 1u << 6;

    /// <summary>CTRL bit 26: Device Reset, self-clearing.</summary>
    public const uint ControlReset = 1u << 26;

    /// <summary>STATUS bit 1: Link Up.</summary>
    public const uint StatusLinkUp = 1u << 1;

    /// <summary>EECD bit 9: the auto-read of the NVM after a reset is done.</summary>
    public const uint EepromControlAutoReadDone = 1u << 9;

    /// <summary>EERD bit 0: start a read.</summary>
    public const uint EepromReadStart = 1u << 0;

    /// <summary>EERD bit 1: the read is done (82574 layout).</summary>
    public const uint EepromReadDone = 1u << 1;

    /// <summary>EERD bits 15:2: the word address (82574 layout).</summary>
    public const int EepromReadAddressShift = 2;

    /// <summary>EERD bits 31:16: the word read.</summary>
    public const int EepromReadDataShift = 16;

    /// <summary>RAH bits 15:0: the address bytes the register carries.</summary>
    public const uint ReceiveAddressHighBytesMask = 0xFFFF;

    /// <summary>RAH bit 31: Address Valid.</summary>
    public const uint ReceiveAddressValid = 1u << 31;

    /// <summary>ICR/IMS bit 2: Link Status Change.</summary>
    public const uint InterruptLinkStatusChange = 1u << 2;

    /// <summary>ICR/IMS bit 4: Receive Descriptor Minimum Threshold reached.</summary>
    public const uint InterruptReceiveDescriptorMinimumThreshold = 1u << 4;

    /// <summary>ICR/IMS bit 7: Receiver Timer, a frame arrived.</summary>
    public const uint InterruptReceiveTimer = 1u << 7;

    /// <summary>Every interrupt cause, for IMC.</summary>
    public const uint AllInterrupts = 0xFFFFFFFF;

    /// <summary>RCTL bit 1: Receiver Enable.</summary>
    public const uint ReceiveControlEnable = 1u << 1;

    /// <summary>RCTL bit 4: Multicast Promiscuous, so IPv6 solicited-node groups pass the empty multicast table.</summary>
    public const uint ReceiveControlMulticastPromiscuous = 1u << 4;

    /// <summary>RCTL bit 15: Broadcast Accept Mode.</summary>
    public const uint ReceiveControlBroadcastAccept = 1u << 15;

    /// <summary>RCTL bits 17:16 at 0: 2048-byte receive buffers.</summary>
    public const uint ReceiveControlBufferSize2048 = 0u << 16;

    /// <summary>RCTL bit 26: Strip Ethernet CRC from the received frame.</summary>
    public const uint ReceiveControlStripCrc = 1u << 26;

    /// <summary>TCTL bit 1: Transmitter Enable.</summary>
    public const uint TransmitControlEnable = 1u << 1;

    /// <summary>TCTL bit 3: Pad Short Packets.</summary>
    public const uint TransmitControlPadShortPackets = 1u << 3;

    /// <summary>TCTL bits 11:4: Collision Threshold.</summary>
    public const int TransmitControlCollisionThresholdShift = 4;

    /// <summary>TCTL bits 21:12: Collision Distance.</summary>
    public const int TransmitControlCollisionDistanceShift = 12;

    /// <summary>The collision threshold the driver programs: 15 retries.</summary>
    public const uint CollisionThreshold = 15;

    /// <summary>The collision distance the driver programs: 64 bytes, full duplex.</summary>
    public const uint CollisionDistance = 64;

    /// <summary>TIPG as the driver programs it: IPGT 10, IPGR1 8, IPGR2 6, the IEEE 802.3 defaults.</summary>
    public const uint InterPacketGap = 10 | (8 << 10) | (6 << 20);

    /// <summary>Transmit descriptor command bit 0: End Of Packet.</summary>
    public const byte TransmitCommandEndOfPacket = 1 << 0;

    /// <summary>Transmit descriptor command bit 1: Insert FCS.</summary>
    public const byte TransmitCommandInsertFcs = 1 << 1;

    /// <summary>Transmit descriptor command bit 3: Report Status.</summary>
    public const byte TransmitCommandReportStatus = 1 << 3;

    /// <summary>Receive descriptor status bit 0: Descriptor Done.</summary>
    public const byte ReceiveStatusDescriptorDone = 1 << 0;

    /// <summary>Receive descriptor status bit 1: End Of Packet.</summary>
    public const byte ReceiveStatusEndOfPacket = 1 << 1;
}
