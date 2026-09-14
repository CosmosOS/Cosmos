// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;

namespace Cosmos.Kernel.HAL.Devices.Storage;

/// <summary>
/// AHCI Generic Host Control Registers.
/// </summary>
internal class GenericRegisters
{
    /// <summary>CAP - Host Capabilities register offset (AHCI spec 3.1.1).</summary>
    private const ulong CapabilitiesOffset = 0x00;

    /// <summary>GHC - Global Host Control register offset (AHCI spec 3.1.2).</summary>
    private const ulong GlobalHostControlOffset = 0x04;

    /// <summary>IS - Interrupt Status register offset (AHCI spec 3.1.3).</summary>
    private const ulong InterruptStatusOffset = 0x08;

    /// <summary>PI - Ports Implemented register offset (AHCI spec 3.1.4).</summary>
    private const ulong ImplementedPortsOffset = 0x0C;

    /// <summary>VS - AHCI Version register offset (AHCI spec 3.1.5).</summary>
    private const ulong AhciVersionOffset = 0x10;

    /// <summary>CCC_CTL - Command Completion Coalescing Control register offset (AHCI spec 3.1.6).</summary>
    private const ulong CccControlOffset = 0x14;

    /// <summary>CCC_PORTS - Command Completion Coalescing Ports register offset (AHCI spec 3.1.7).</summary>
    private const ulong CccPortsOffset = 0x18;

    /// <summary>EM_LOC - Enclosure Management Location register offset (AHCI spec 3.1.8).</summary>
    private const ulong EmLocationOffset = 0x1C;

    /// <summary>EM_CTL - Enclosure Management Control register offset (AHCI spec 3.1.9).</summary>
    private const ulong EmControlOffset = 0x20;

    /// <summary>CAP2 - Host Capabilities Extended register offset (AHCI spec 3.1.10).</summary>
    private const ulong ExtendedCapabilitiesOffset = 0x24;

    /// <summary>BOHC - BIOS/OS Handoff Control and Status register offset (AHCI spec 3.1.11).</summary>
    private const ulong BiosHandOffStatusOffset = 0x28;

    private readonly ulong _address;

    public GenericRegisters(ulong address)
    {
        _address = address;
    }

    public uint Capabilities
    {
        get => Native.MMIO.Read32(_address + CapabilitiesOffset);
        set => Native.MMIO.Write32(_address + CapabilitiesOffset, value);
    }

    public uint GlobalHostControl
    {
        get => Native.MMIO.Read32(_address + GlobalHostControlOffset);
        set => Native.MMIO.Write32(_address + GlobalHostControlOffset, value);
    }

    public uint InterruptStatus
    {
        get => Native.MMIO.Read32(_address + InterruptStatusOffset);
        set => Native.MMIO.Write32(_address + InterruptStatusOffset, value);
    }

    public uint ImplementedPorts
    {
        get => Native.MMIO.Read32(_address + ImplementedPortsOffset);
        set => Native.MMIO.Write32(_address + ImplementedPortsOffset, value);
    }

    public uint AhciVersion
    {
        get => Native.MMIO.Read32(_address + AhciVersionOffset);
        set => Native.MMIO.Write32(_address + AhciVersionOffset, value);
    }

    public uint CCC_Control
    {
        get => Native.MMIO.Read32(_address + CccControlOffset);
        set => Native.MMIO.Write32(_address + CccControlOffset, value);
    }

    public uint CCC_Ports
    {
        get => Native.MMIO.Read32(_address + CccPortsOffset);
        set => Native.MMIO.Write32(_address + CccPortsOffset, value);
    }

    public uint EM_Location
    {
        get => Native.MMIO.Read32(_address + EmLocationOffset);
        set => Native.MMIO.Write32(_address + EmLocationOffset, value);
    }

    public uint EM_Control
    {
        get => Native.MMIO.Read32(_address + EmControlOffset);
        set => Native.MMIO.Write32(_address + EmControlOffset, value);
    }

    public uint ExtendedCapabilities
    {
        get => Native.MMIO.Read32(_address + ExtendedCapabilitiesOffset);
        set => Native.MMIO.Write32(_address + ExtendedCapabilitiesOffset, value);
    }

    public uint BIOSHandOffStatus
    {
        get => Native.MMIO.Read32(_address + BiosHandOffStatusOffset);
        set => Native.MMIO.Write32(_address + BiosHandOffStatusOffset, value);
    }
}

/// <summary>
/// AHCI Port Registers. Carries a back-reference to the
/// <see cref="AhciController"/> that owns the port so per-port code (SATA
/// command issue, port reset) can reach controller state without going
/// through globals.
/// </summary>
internal class PortRegisters
{
    /// <summary>Size of one port register bank in bytes; port N starts at 0x100 + N * 0x80 (AHCI spec 3.3).</summary>
    private const uint PortRegisterSpanBytes = 0x80;

    /// <summary>PxCLB - Command List Base Address register offset (AHCI spec 3.3.1).</summary>
    private const ulong ClbOffset = 0x00;

    /// <summary>PxCLBU - Command List Base Address Upper 32-bits register offset (AHCI spec 3.3.2).</summary>
    private const ulong ClbuOffset = 0x04;

    /// <summary>PxFB - FIS Base Address register offset (AHCI spec 3.3.3).</summary>
    private const ulong FbOffset = 0x08;

    /// <summary>PxFBU - FIS Base Address Upper 32-bits register offset (AHCI spec 3.3.4).</summary>
    private const ulong FbuOffset = 0x0C;

    /// <summary>PxIS - Interrupt Status register offset (AHCI spec 3.3.5).</summary>
    private const ulong IsOffset = 0x10;

    /// <summary>PxIE - Interrupt Enable register offset (AHCI spec 3.3.6).</summary>
    private const ulong IeOffset = 0x14;

    /// <summary>PxCMD - Command and Status register offset (AHCI spec 3.3.7).</summary>
    private const ulong CmdOffset = 0x18;

    /// <summary>PxTFD - Task File Data register offset (AHCI spec 3.3.8).</summary>
    private const ulong TfdOffset = 0x20;

    /// <summary>PxSIG - Signature register offset (AHCI spec 3.3.9).</summary>
    private const ulong SigOffset = 0x24;

    /// <summary>PxSSTS - SATA Status register offset (AHCI spec 3.3.10).</summary>
    private const ulong SstsOffset = 0x28;

    /// <summary>PxSCTL - SATA Control register offset (AHCI spec 3.3.11).</summary>
    private const ulong SctlOffset = 0x2C;

    /// <summary>PxSERR - SATA Error register offset (AHCI spec 3.3.12).</summary>
    private const ulong SerrOffset = 0x30;

    /// <summary>PxSACT - SATA Active register offset (AHCI spec 3.3.13).</summary>
    private const ulong SactOffset = 0x34;

    /// <summary>PxCI - Command Issue register offset (AHCI spec 3.3.14).</summary>
    private const ulong CiOffset = 0x38;

    /// <summary>PxSNTF - SATA Notification register offset (AHCI spec 3.3.15).</summary>
    private const ulong SntfOffset = 0x3C;

    /// <summary>PxFBS - FIS-Based Switching Control register offset (AHCI spec 3.3.16).</summary>
    private const ulong FbsOffset = 0x40;

    /// <summary>PxDEVSLP - Device Sleep register offset (AHCI spec 3.3.17).</summary>
    private const ulong DevslpOffset = 0x44;

    // Port register field masks (AHCI spec 3.3.10 / 3.3.11); shared with
    // AhciController and Sata, which operate on this register bank.

    /// <summary>PxSSTS.DET - Device Detection 4-bit field mask (bits 3:0, AHCI spec 3.3.10).</summary>
    internal const uint SstsDetMask = 0x0F;

    /// <summary>PxSSTS.IPM - Interface Power Management field position (bits 11:8, AHCI spec 3.3.10).</summary>
    internal const int SstsIpmShift = 8;

    /// <summary>PxSSTS.IPM - Interface Power Management 4-bit field mask (AHCI spec 3.3.10).</summary>
    internal const uint SstsIpmMask = 0x0F;

    /// <summary>PxSCTL.DET - Device Detection Initialization 4-bit field mask (bits 3:0, AHCI spec 3.3.11).</summary>
    internal const uint SctlDetMask = 0xFU;

    /// <summary>PxSCTL.DET value 1: perform interface communication initialization (COMRESET, AHCI spec 3.3.11).</summary>
    internal const uint SctlDetComreset = 1U;

    /// <summary>All-ones write for RW1C registers (PxSERR/PxIS): clears every latched bit.</summary>
    internal const uint Rw1CClearAll = 0xFFFFFFFFu;

    private readonly ulong _address;
    public uint PortNumber { get; }
    public PortType PortType { get; set; } = PortType.Nothing;
    public bool Active { get; set; }
    public AhciController Controller { get; }

    public PortRegisters(ulong baseAddress, uint portNumber, AhciController controller)
    {
        PortNumber = portNumber;
        _address = baseAddress + PortRegisterSpanBytes * portNumber;
        Active = false;
        Controller = controller;
    }

    /// <summary>Command List Base Address</summary>
    public uint CLB
    {
        get => Native.MMIO.Read32(_address + ClbOffset);
        set => Native.MMIO.Write32(_address + ClbOffset, value);
    }

    /// <summary>Command List Base Address Upper</summary>
    public uint CLBU
    {
        get => Native.MMIO.Read32(_address + ClbuOffset);
        set => Native.MMIO.Write32(_address + ClbuOffset, value);
    }

    /// <summary>FIS Base Address</summary>
    public uint FB
    {
        get => Native.MMIO.Read32(_address + FbOffset);
        set => Native.MMIO.Write32(_address + FbOffset, value);
    }

    /// <summary>FIS Base Address Upper</summary>
    public uint FBU
    {
        get => Native.MMIO.Read32(_address + FbuOffset);
        set => Native.MMIO.Write32(_address + FbuOffset, value);
    }

    /// <summary>Interrupt Status</summary>
    public uint IS
    {
        get => Native.MMIO.Read32(_address + IsOffset);
        set => Native.MMIO.Write32(_address + IsOffset, value);
    }

    /// <summary>Interrupt Enable</summary>
    public uint IE
    {
        get => Native.MMIO.Read32(_address + IeOffset);
        set => Native.MMIO.Write32(_address + IeOffset, value);
    }

    /// <summary>Command</summary>
    public uint CMD
    {
        get => Native.MMIO.Read32(_address + CmdOffset);
        set => Native.MMIO.Write32(_address + CmdOffset, value);
    }

    /// <summary>Task File Data</summary>
    public uint TFD
    {
        get => Native.MMIO.Read32(_address + TfdOffset);
        set => Native.MMIO.Write32(_address + TfdOffset, value);
    }

    /// <summary>Signature</summary>
    public uint SIG
    {
        get => Native.MMIO.Read32(_address + SigOffset);
        set => Native.MMIO.Write32(_address + SigOffset, value);
    }

    /// <summary>SATA Status</summary>
    public uint SSTS
    {
        get => Native.MMIO.Read32(_address + SstsOffset);
        set => Native.MMIO.Write32(_address + SstsOffset, value);
    }

    /// <summary>SATA Control</summary>
    public uint SCTL
    {
        get => Native.MMIO.Read32(_address + SctlOffset);
        set => Native.MMIO.Write32(_address + SctlOffset, value);
    }

    /// <summary>SATA Error</summary>
    public uint SERR
    {
        get => Native.MMIO.Read32(_address + SerrOffset);
        set => Native.MMIO.Write32(_address + SerrOffset, value);
    }

    /// <summary>SATA Active</summary>
    public uint SACT
    {
        get => Native.MMIO.Read32(_address + SactOffset);
        set => Native.MMIO.Write32(_address + SactOffset, value);
    }

    /// <summary>Command Issue</summary>
    public uint CI
    {
        get => Native.MMIO.Read32(_address + CiOffset);
        set => Native.MMIO.Write32(_address + CiOffset, value);
    }

    /// <summary>SATA Notification</summary>
    public uint SNTF
    {
        get => Native.MMIO.Read32(_address + SntfOffset);
        set => Native.MMIO.Write32(_address + SntfOffset, value);
    }

    /// <summary>FIS-Based Switching</summary>
    public uint FBS
    {
        get => Native.MMIO.Read32(_address + FbsOffset);
        set => Native.MMIO.Write32(_address + FbsOffset, value);
    }

    /// <summary>Device Sleep</summary>
    public uint DEVSLP
    {
        get => Native.MMIO.Read32(_address + DevslpOffset);
        set => Native.MMIO.Write32(_address + DevslpOffset, value);
    }
}

/// <summary>
/// HBA Command Header structure.
/// </summary>
internal class HbaCommandHeader
{
    /// <summary>Size of one command header in bytes; slot N starts at CLB + N * 32 (AHCI spec 4.2.2).</summary>
    private const uint CommandHeaderSizeBytes = 32;

    /// <summary>Number of 32-bit dwords in a command header (32 bytes / 4).</summary>
    private const int CommandHeaderDwordCount = 8;

    /// <summary>CFL - Command FIS Length mask, bits 4:0 of the header DW0 (AHCI spec 4.2.2).</summary>
    private const int CommandFisLengthMask = 0x1F;

    /// <summary>A - ATAPI bit position in the header DW0 (AHCI spec 4.2.2).</summary>
    private const int AtapiBitShift = 5;

    /// <summary>W - Write bit position in the header DW0 (AHCI spec 4.2.2).</summary>
    private const int WriteBitShift = 6;

    /// <summary>PRDTL - Physical Region Descriptor Table Length field offset (AHCI spec 4.2.2).</summary>
    private const ulong PrdtlOffset = 0x02;

    /// <summary>PRDBC - Physical Region Descriptor Byte Count field offset (AHCI spec 4.2.2).</summary>
    private const ulong PrdbcOffset = 0x04;

    /// <summary>CTBA - Command Table Descriptor Base Address field offset (AHCI spec 4.2.2).</summary>
    private const ulong CtbaOffset = 0x08;

    /// <summary>CTBAU - Command Table Descriptor Base Address Upper 32-bits field offset (AHCI spec 4.2.2).</summary>
    private const ulong CtbauOffset = 0x0C;

    private readonly ulong _address;

    public HbaCommandHeader(ulong baseAddress, uint slot)
    {
        _address = baseAddress + CommandHeaderSizeBytes * slot;
        // Clear the header
        for (int i = 0; i < CommandHeaderDwordCount; i++)
        {
            Native.MMIO.Write32(_address + (ulong)(i * sizeof(uint)), 0);
        }
    }

    public byte CFL
    {
        get => (byte)(Native.MMIO.Read8(_address) & CommandFisLengthMask);
        set => Native.MMIO.Write8(_address, value);
    }

    public byte Atapi
    {
        get => (byte)((Native.MMIO.Read8(_address) >> AtapiBitShift) & 1);
        set
        {
            byte current = Native.MMIO.Read8(_address);
            Native.MMIO.Write8(_address, (byte)(current | (value << AtapiBitShift)));
        }
    }

    public byte Write
    {
        get => (byte)((Native.MMIO.Read8(_address) >> WriteBitShift) & 1);
        set
        {
            byte current = Native.MMIO.Read8(_address);
            Native.MMIO.Write8(_address, (byte)(current | (value << WriteBitShift)));
        }
    }

    public ushort PRDTL
    {
        get => Native.MMIO.Read16(_address + PrdtlOffset);
        set => Native.MMIO.Write16(_address + PrdtlOffset, value);
    }

    public uint PRDBC
    {
        get => Native.MMIO.Read32(_address + PrdbcOffset);
        set => Native.MMIO.Write32(_address + PrdbcOffset, value);
    }

    public uint CTBA
    {
        get => Native.MMIO.Read32(_address + CtbaOffset);
        set => Native.MMIO.Write32(_address + CtbaOffset, value);
    }

    public uint CTBAU
    {
        get => Native.MMIO.Read32(_address + CtbauOffset);
        set => Native.MMIO.Write32(_address + CtbauOffset, value);
    }
}

/// <summary>
/// HBA Command Table. <see cref="CFIS"/> is the kernel-virtual base; the
/// HBA itself addresses this table via the physical CTBA written into the
/// owning <see cref="HbaCommandHeader"/>.
/// </summary>
internal class HbaCommandTable
{
    /// <summary>Offset of the PRDT within the command table; the CFIS, ACMD and reserved areas occupy the first 0x80 bytes (AHCI spec 4.2.3).</summary>
    private const int PrdtOffsetBytes = 0x80;

    /// <summary>ACMD - ATAPI Command area offset within the command table (AHCI spec 4.2.3).</summary>
    private const int AcmdOffsetBytes = 0x40;

    public ulong CFIS { get; }
    public HbaPrdtEntry[] PRDTEntry { get; }

    public HbaCommandTable(ulong address, uint prdtCount)
    {
        CFIS = address;

        // Clear the first 0x80 bytes
        for (int i = 0; i < PrdtOffsetBytes / sizeof(uint); i++)
        {
            Native.MMIO.Write32(address + (ulong)(i * sizeof(uint)), 0);
        }

        PRDTEntry = new HbaPrdtEntry[prdtCount];
        for (uint i = 0; i < prdtCount; i++)
        {
            PRDTEntry[i] = new HbaPrdtEntry(address + PrdtOffsetBytes, i);
        }
    }

    public ulong ACMD => CFIS + AcmdOffsetBytes;
}

/// <summary>
/// HBA PRDT Entry.
/// </summary>
internal class HbaPrdtEntry
{
    /// <summary>Size of one PRDT entry in bytes; entry N starts at PRDT base + N * 0x10 (AHCI spec 4.2.3.3).</summary>
    private const uint PrdtEntrySizeBytes = 0x10;

    /// <summary>Number of 32-bit dwords in a PRDT entry (16 bytes / 4).</summary>
    private const int PrdtEntryDwordCount = 4;

    /// <summary>DBA - Data Base Address field offset (AHCI spec 4.2.3.3).</summary>
    private const ulong DbaOffset = 0x00;

    /// <summary>DBAU - Data Base Address Upper 32-bits field offset (AHCI spec 4.2.3.3).</summary>
    private const ulong DbauOffset = 0x04;

    /// <summary>DBC - Data Byte Count field offset within DW3 (AHCI spec 4.2.3.3).</summary>
    private const ulong DbcOffset = 0x0C;

    /// <summary>Offset of the byte holding the Interrupt on Completion bit (top byte of DW3, AHCI spec 4.2.3.3).</summary>
    private const ulong InterruptOnCompletionByteOffset = 0x0F;

    /// <summary>DBC - Data Byte Count mask, bits 21:0 of DW3 (AHCI spec 4.2.3.3).</summary>
    private const uint DbcMask = 0x3FFFFF;

    /// <summary>I - Interrupt on Completion bit position within its byte (bit 31 of DW3, AHCI spec 4.2.3.3).</summary>
    private const int InterruptOnCompletionBitShift = 7;

    private readonly ulong _address;

    public HbaPrdtEntry(ulong baseAddress, uint entry)
    {
        _address = baseAddress + PrdtEntrySizeBytes * entry;
        // Clear the entry
        for (int i = 0; i < PrdtEntryDwordCount; i++)
        {
            Native.MMIO.Write32(_address + (ulong)(i * sizeof(uint)), 0);
        }
    }

    public uint DBA
    {
        get => Native.MMIO.Read32(_address + DbaOffset);
        set => Native.MMIO.Write32(_address + DbaOffset, value);
    }

    public uint DBAU
    {
        get => Native.MMIO.Read32(_address + DbauOffset);
        set => Native.MMIO.Write32(_address + DbauOffset, value);
    }

    public uint DBC
    {
        get => Native.MMIO.Read32(_address + DbcOffset) & DbcMask;
        set => Native.MMIO.Write32(_address + DbcOffset, value);
    }

    public byte InterruptOnCompletion
    {
        get => (byte)(Native.MMIO.Read8(_address + InterruptOnCompletionByteOffset) >> InterruptOnCompletionBitShift);
        set => Native.MMIO.Write8(_address + InterruptOnCompletionByteOffset, (byte)(value << InterruptOnCompletionBitShift));
    }
}

/// <summary>
/// FIS Register Host to Device.
/// </summary>
internal class FisRegisterH2D
{
    /// <summary>Number of 32-bit dwords in a Register H2D FIS (20 bytes / 4, SATA spec 10.5.4). Internal so <see cref="Sata"/> programs it into the command header CFL field.</summary>
    internal const int FisDwordCount = 5;

    /// <summary>FIS Type field offset (SATA spec 10.5.4).</summary>
    private const ulong FisTypeOffset = 0x00;

    /// <summary>Offset of the byte holding the C (Command) bit and PM Port field (SATA spec 10.5.4).</summary>
    private const ulong FlagsOffset = 0x01;

    /// <summary>Command register field offset (SATA spec 10.5.4).</summary>
    private const ulong CommandOffset = 0x02;

    /// <summary>Features register (low byte) field offset (SATA spec 10.5.4).</summary>
    private const ulong FeatureLowOffset = 0x03;

    /// <summary>LBA bits 7:0 field offset (SATA spec 10.5.4).</summary>
    private const ulong Lba0Offset = 0x04;

    /// <summary>LBA bits 15:8 field offset (SATA spec 10.5.4).</summary>
    private const ulong Lba1Offset = 0x05;

    /// <summary>LBA bits 23:16 field offset (SATA spec 10.5.4).</summary>
    private const ulong Lba2Offset = 0x06;

    /// <summary>Device register field offset (SATA spec 10.5.4).</summary>
    private const ulong DeviceOffset = 0x07;

    /// <summary>LBA bits 31:24 field offset (SATA spec 10.5.4).</summary>
    private const ulong Lba3Offset = 0x08;

    /// <summary>LBA bits 39:32 field offset (SATA spec 10.5.4).</summary>
    private const ulong Lba4Offset = 0x09;

    /// <summary>LBA bits 47:40 field offset (SATA spec 10.5.4).</summary>
    private const ulong Lba5Offset = 0x0A;

    /// <summary>Features register (high byte) field offset (SATA spec 10.5.4).</summary>
    private const ulong FeatureHighOffset = 0x0B;

    /// <summary>Count register (low byte) field offset (SATA spec 10.5.4).</summary>
    private const ulong CountLowOffset = 0x0C;

    /// <summary>Count register (high byte) field offset (SATA spec 10.5.4).</summary>
    private const ulong CountHighOffset = 0x0D;

    /// <summary>ICC - Isochronous Command Completion field offset (SATA spec 10.5.4).</summary>
    private const ulong IccOffset = 0x0E;

    /// <summary>Control register field offset (SATA spec 10.5.4).</summary>
    private const ulong ControlOffset = 0x0F;

    /// <summary>C - Command bit position within the flags byte; set for a command FIS, clear for device control (SATA spec 10.5.4).</summary>
    private const int CommandBitShift = 7;

    private readonly ulong _address;

    public FisRegisterH2D(ulong address)
    {
        _address = address;
        // Clear the FIS (20 bytes)
        for (int i = 0; i < FisDwordCount; i++)
        {
            Native.MMIO.Write32(_address + (ulong)(i * sizeof(uint)), 0);
        }
    }

    public byte FisType
    {
        get => Native.MMIO.Read8(_address + FisTypeOffset);
        set => Native.MMIO.Write8(_address + FisTypeOffset, value);
    }

    public byte IsCommand
    {
        get => (byte)(Native.MMIO.Read8(_address + FlagsOffset) >> CommandBitShift);
        set => Native.MMIO.Write8(_address + FlagsOffset, (byte)(value << CommandBitShift));
    }

    public byte Command
    {
        get => Native.MMIO.Read8(_address + CommandOffset);
        set => Native.MMIO.Write8(_address + CommandOffset, value);
    }

    public byte FeatureLow
    {
        get => Native.MMIO.Read8(_address + FeatureLowOffset);
        set => Native.MMIO.Write8(_address + FeatureLowOffset, value);
    }

    public byte LBA0
    {
        get => Native.MMIO.Read8(_address + Lba0Offset);
        set => Native.MMIO.Write8(_address + Lba0Offset, value);
    }

    public byte LBA1
    {
        get => Native.MMIO.Read8(_address + Lba1Offset);
        set => Native.MMIO.Write8(_address + Lba1Offset, value);
    }

    public byte LBA2
    {
        get => Native.MMIO.Read8(_address + Lba2Offset);
        set => Native.MMIO.Write8(_address + Lba2Offset, value);
    }

    public byte Device
    {
        get => Native.MMIO.Read8(_address + DeviceOffset);
        set => Native.MMIO.Write8(_address + DeviceOffset, value);
    }

    public byte LBA3
    {
        get => Native.MMIO.Read8(_address + Lba3Offset);
        set => Native.MMIO.Write8(_address + Lba3Offset, value);
    }

    public byte LBA4
    {
        get => Native.MMIO.Read8(_address + Lba4Offset);
        set => Native.MMIO.Write8(_address + Lba4Offset, value);
    }

    public byte LBA5
    {
        get => Native.MMIO.Read8(_address + Lba5Offset);
        set => Native.MMIO.Write8(_address + Lba5Offset, value);
    }

    public byte FeatureHigh
    {
        get => Native.MMIO.Read8(_address + FeatureHighOffset);
        set => Native.MMIO.Write8(_address + FeatureHighOffset, value);
    }

    public byte CountL
    {
        get => Native.MMIO.Read8(_address + CountLowOffset);
        set => Native.MMIO.Write8(_address + CountLowOffset, value);
    }

    public byte CountH
    {
        get => Native.MMIO.Read8(_address + CountHighOffset);
        set => Native.MMIO.Write8(_address + CountHighOffset, value);
    }

    public byte ICC
    {
        get => Native.MMIO.Read8(_address + IccOffset);
        set => Native.MMIO.Write8(_address + IccOffset, value);
    }

    public byte Control
    {
        get => Native.MMIO.Read8(_address + ControlOffset);
        set => Native.MMIO.Write8(_address + ControlOffset, value);
    }
}
