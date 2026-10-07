// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Pci.Storage.Ahci;

/// <summary>
/// The AHCI register offsets, field layouts, signatures, ATA opcodes and
/// the command region layout (AHCI 1.3.1) for <see cref="AhciDriver"/>,
/// <see cref="AhciState"/> and <see cref="AhciPort"/>. The three on-wire
/// structures follow in this file, laid out sequentially with no padding so
/// a whole-struct write into DMA memory is the wire format; every field is
/// little-endian on the architectures the kernel runs on. Constants and
/// the four offset helpers for the region and the port banks; any context.
/// </summary>
internal static class AhciProtocol
{
    // --- Generic host control registers (section 3.1) ---

    /// <summary>CAP, 32-bit: the HBA's capabilities.</summary>
    public const ulong Cap = 0x00;

    /// <summary>GHC, 32-bit: global host control.</summary>
    public const ulong Ghc = 0x04;

    /// <summary>IS, 32-bit: the per-port interrupt status summary.</summary>
    public const ulong Is = 0x08;

    /// <summary>PI, 32-bit: one bit per implemented port.</summary>
    public const ulong Pi = 0x0C;

    /// <summary>VS, 32-bit: the AHCI version, major in bits 31:16.</summary>
    public const ulong Vs = 0x10;

    /// <summary>GHC.AE, bit 31: AHCI mode enabled.</summary>
    public const uint GhcAhciEnable = 1u << 31;

    /// <summary>GHC.HR, bit 0: HBA reset, self-clearing.</summary>
    public const uint GhcHbaReset = 1;

    /// <summary>Mask of CAP.NP, bits 4:0: the number of ports minus one.</summary>
    public const uint CapPortsMask = 0x1F;

    /// <summary>Shift of CAP.NCS, bits 12:8: the number of command slots minus one.</summary>
    public const int CapSlotsShift = 8;

    /// <summary>Mask of CAP.NCS after the shift.</summary>
    public const uint CapSlotsMask = 0x1F;

    /// <summary>CAP.SCLO, bit 24: the HBA supports the command list override.</summary>
    public const uint CapCommandListOverride = 1u << 24;

    /// <summary>CAP.S64A, bit 31: the HBA addresses 64 bits.</summary>
    public const uint Cap64BitAddressing = 1u << 31;

    /// <summary>The highest CAP.NP value: 32 ports, so every PI bit.</summary>
    public const uint CapPortsAll = 31;

    /// <summary>Shift of the major version within VS.</summary>
    public const int VersionMajorShift = 16;

    /// <summary>Shift of the minor version within VS; the byte below it is the patch level.</summary>
    public const int VersionMinorShift = 8;

    /// <summary>Mask of one version byte.</summary>
    public const uint VersionByteMask = 0xFF;

    // --- Port registers (section 3.3), bank n at 0x100 + 0x80 * n ---

    /// <summary>Offset of port 0's register bank.</summary>
    public const ulong PortBankBase = 0x100;

    /// <summary>Bytes of one port's register bank.</summary>
    public const ulong PortBankBytes = 0x80;

    /// <summary>Ports an HBA can implement: one bit of PI each.</summary>
    public const int MaxPorts = 32;

    /// <summary>PxCLB: the command list base, low 32 bits.</summary>
    public const ulong PxClb = 0x00;

    /// <summary>PxCLBU: the command list base, high 32 bits.</summary>
    public const ulong PxClbu = 0x04;

    /// <summary>PxFB: the received FIS base, low 32 bits.</summary>
    public const ulong PxFb = 0x08;

    /// <summary>PxFBU: the received FIS base, high 32 bits.</summary>
    public const ulong PxFbu = 0x0C;

    /// <summary>PxIS: the port's interrupt status, RW1C.</summary>
    public const ulong PxIs = 0x10;

    /// <summary>PxIE: the port's interrupt enables.</summary>
    public const ulong PxIe = 0x14;

    /// <summary>PxCMD: the port's command and status.</summary>
    public const ulong PxCmd = 0x18;

    /// <summary>PxTFD: the task file data, the status byte in bits 7:0.</summary>
    public const ulong PxTfd = 0x20;

    /// <summary>PxSIG: the device's signature from its first D2H FIS.</summary>
    public const ulong PxSig = 0x24;

    /// <summary>PxSSTS: the SATA status.</summary>
    public const ulong PxSsts = 0x28;

    /// <summary>PxSCTL: the SATA control.</summary>
    public const ulong PxSctl = 0x2C;

    /// <summary>PxSERR: the SATA error, RW1C.</summary>
    public const ulong PxSerr = 0x30;

    /// <summary>PxSACT: the native command queuing active slots.</summary>
    public const ulong PxSact = 0x34;

    /// <summary>PxCI: the command issue bits, one per slot.</summary>
    public const ulong PxCi = 0x38;

    /// <summary>PxCMD.ST, bit 0: the command engine runs.</summary>
    public const uint CmdStart = 1;

    /// <summary>PxCMD.CLO, bit 3: the command list override, self-clearing.</summary>
    public const uint CmdCommandListOverride = 1 << 3;

    /// <summary>PxCMD.FRE, bit 4: FIS receive enabled.</summary>
    public const uint CmdFisReceiveEnable = 1 << 4;

    /// <summary>Shift of PxCMD.CCS, bits 12:8: the slot being issued.</summary>
    public const int CmdCurrentSlotShift = 8;

    /// <summary>Mask of PxCMD.CCS after the shift.</summary>
    public const uint CmdCurrentSlotMask = 0x1F;

    /// <summary>PxCMD.FR, bit 14: the FIS receive engine runs.</summary>
    public const uint CmdFisReceiveRunning = 1 << 14;

    /// <summary>PxCMD.CR, bit 15: the command list engine runs.</summary>
    public const uint CmdListRunning = 1 << 15;

    /// <summary>PxCMD.ATAPI, bit 24: the attached device is ATAPI.</summary>
    public const uint CmdAtapi = 1u << 24;

    /// <summary>PxTFD status byte BSY, bit 7.</summary>
    public const uint TfdBusy = 1 << 7;

    /// <summary>PxTFD status byte DRQ, bit 3.</summary>
    public const uint TfdDataRequest = 1 << 3;

    /// <summary>PxIS.TFES, bit 30: a task file error.</summary>
    public const uint IsTaskFileError = 1u << 30;

    /// <summary>Mask of PxSSTS.DET, bits 3:0.</summary>
    public const uint SstsDetMask = 0xF;

    /// <summary>PxSSTS.DET value 3: a device is present and the PHY is up.</summary>
    public const uint SstsDetPresent = 3;

    /// <summary>Shift of PxSSTS.IPM, bits 11:8.</summary>
    public const int SstsIpmShift = 8;

    /// <summary>Mask of PxSSTS.IPM after the shift.</summary>
    public const uint SstsIpmMask = 0xF;

    /// <summary>PxSSTS.IPM value 1: the interface is active.</summary>
    public const uint SstsIpmActive = 1;

    /// <summary>Mask of PxSCTL.DET, bits 3:0.</summary>
    public const uint SctlDetMask = 0xF;

    /// <summary>PxSCTL.DET value 1: COMRESET.</summary>
    public const uint SctlDetComreset = 1;

    /// <summary>The all-ones write that clears every bit of an RW1C register.</summary>
    public const uint Rw1CClearAll = 0xFFFFFFFF;

    // --- Signatures (PxSIG high 16 bits) ---

    /// <summary>A SATA disk.</summary>
    public const uint SignatureSata = 0x0000;

    /// <summary>A SATAPI device.</summary>
    public const uint SignatureSatapi = 0xEB14;

    /// <summary>An enclosure management bridge.</summary>
    public const uint SignatureSemb = 0xC33C;

    /// <summary>A port multiplier.</summary>
    public const uint SignaturePortMultiplier = 0x9669;

    /// <summary>PxSIG before any D2H FIS arrived.</summary>
    public const uint InvalidSignature = 0xFFFFFFFF;

    /// <summary>Shift of the classifying high word within PxSIG.</summary>
    public const int SignatureHighShift = 16;

    // --- ATA commands and FIS types ---

    /// <summary>READ DMA EXT.</summary>
    public const byte AtaReadDmaExt = 0x25;

    /// <summary>WRITE DMA EXT.</summary>
    public const byte AtaWriteDmaExt = 0x35;

    /// <summary>FLUSH CACHE EXT.</summary>
    public const byte AtaCacheFlushExt = 0xEA;

    /// <summary>IDENTIFY DEVICE.</summary>
    public const byte AtaIdentify = 0xEC;

    /// <summary>The register host to device FIS type.</summary>
    public const byte FisTypeRegisterH2D = 0x27;

    /// <summary>C, bit 7 of the H2D FIS flags: the FIS carries a command.</summary>
    public const byte FisCommandFlag = 0x80;

    /// <summary>The device register's LBA mode bit, bit 6, set for a 48-bit command.</summary>
    public const byte DeviceLbaMode = 0x40;

    /// <summary>Bytes of one sector.</summary>
    public const uint SectorBytes = 512;

    // --- Command region: one DMA run for all 32 port indices ---

    /// <summary>Bytes of the command region: the command lists, the FIS areas and the command tables of 32 ports.</summary>
    public const int CommandRegionBytes = 0x4A000;

    /// <summary>The command region's alignment: the command list's 1 KiB, which covers the FIS area's 256 bytes and the table's 128.</summary>
    public const int CommandRegionAlignment = 1024;

    /// <summary>Bytes of one port's command list: 32 headers of 32 bytes.</summary>
    public const int CommandListBytes = 1024;

    /// <summary>Bytes of one port's received FIS area.</summary>
    public const int FisReceiveBytes = 256;

    /// <summary>Bytes of one command table: the H2D FIS, the ATAPI command and 8 PRDT entries.</summary>
    public const int CommandTableBytes = 0x100;

    /// <summary>Offset of the H2D FIS within a command table.</summary>
    public const int CommandTableFisOffset = 0x00;

    /// <summary>Offset of the ATAPI command within a command table.</summary>
    public const int CommandTableAtapiOffset = 0x40;

    /// <summary>Offset of the PRDT within a command table.</summary>
    public const int CommandTablePrdtOffset = 0x80;

    /// <summary>PRDT entries a command table holds.</summary>
    public const ushort PrdtEntriesPerTable = 8;

    /// <summary>Command headers in one command list, and the most slots CAP.NCS can announce.</summary>
    public const int CommandSlotsPerPort = 32;

    /// <summary>Bytes of one command header.</summary>
    public const int CommandHeaderBytes = 32;

    /// <summary>Bytes of one PRDT entry.</summary>
    public const int PrdtEntryBytes = 16;

    /// <summary>Bytes of a register H2D FIS.</summary>
    public const int FisRegisterH2DBytes = 20;

    /// <summary>Offset of port 0's FIS area within the region.</summary>
    private const int FisReceiveBase = 0x8000;

    /// <summary>Offset of port 0's command tables within the region.</summary>
    private const int CommandTableBase = 0xA000;

    /// <summary>Bytes of one port's block of command tables: 32 tables of 256 bytes.</summary>
    private const int CommandTablesPerPortBytes = 0x2000;

    /// <summary>Offset of port <paramref name="port"/>'s command list within the region. Any context.</summary>
    /// <param name="port">The port index.</param>
    public static int CommandListOffset(uint port) => CommandListBytes * (int)port;

    /// <summary>Offset of port <paramref name="port"/>'s received FIS area within the region. Any context.</summary>
    /// <param name="port">The port index.</param>
    public static int FisReceiveOffset(uint port) => FisReceiveBase + FisReceiveBytes * (int)port;

    /// <summary>Offset of the command table of <paramref name="slot"/> on port <paramref name="port"/> within the region. Any context.</summary>
    /// <param name="port">The port index.</param>
    /// <param name="slot">The command slot.</param>
    public static int CommandTableOffset(uint port, int slot) => CommandTableBase + CommandTablesPerPortBytes * (int)port + CommandTableBytes * slot;

    /// <summary>Offset of port <paramref name="port"/>'s register bank within the window. Any context.</summary>
    /// <param name="port">The port index.</param>
    public static ulong PortBank(uint port) => PortBankBase + PortBankBytes * port;

    // --- Command header fields ---

    /// <summary>CFL, bits 4:0 of the header's first byte: the H2D FIS length in dwords.</summary>
    public const byte HeaderFisDwords = 5;

    /// <summary>ATAPI, bit 5 of the header's first byte.</summary>
    public const byte HeaderAtapi = 1 << 5;

    /// <summary>W, bit 6 of the header's first byte: the data goes to the device.</summary>
    public const byte HeaderWrite = 1 << 6;

    /// <summary>P, bit 7 of the header's first byte: prefetchable.</summary>
    public const byte HeaderPrefetchable = 1 << 7;

    /// <summary>I, bit 31 of a PRDT entry's last dword: interrupt on completion.</summary>
    public const uint PrdtInterruptOnCompletion = 1u << 31;

    /// <summary>Mask of a PRDT entry's byte count minus one, bits 21:0.</summary>
    public const uint PrdtByteCountMask = 0x3FFFFF;

    // --- IDENTIFY DEVICE words ---

    /// <summary>Words the IDENTIFY data holds.</summary>
    public const int IdentifyWords = 256;

    /// <summary>The first word of the serial number.</summary>
    public const int IdentifySerialWord = 10;

    /// <summary>Words of the serial number: 10 to 19.</summary>
    public const int IdentifySerialWords = 10;

    /// <summary>The first word of the firmware revision.</summary>
    public const int IdentifyFirmwareWord = 23;

    /// <summary>Words of the firmware revision: 23 to 26.</summary>
    public const int IdentifyFirmwareWords = 4;

    /// <summary>The first word of the model number.</summary>
    public const int IdentifyModelWord = 27;

    /// <summary>Words of the model number: 27 to 46.</summary>
    public const int IdentifyModelWords = 20;

    /// <summary>The low word of the 28-bit sector count.</summary>
    public const int IdentifyLba28CountWord = 60;

    /// <summary>The word whose bit 10 announces the 48-bit address feature set.</summary>
    public const int IdentifyCommandSetsWord = 83;

    /// <summary>Bit 10 of word 83: the 48-bit address feature set is supported.</summary>
    public const ushort IdentifyLba48Supported = 1 << 10;

    /// <summary>The first word of the 48-bit sector count: 100 to 103.</summary>
    public const int IdentifyLba48CountWord = 100;
}

/// <summary>
/// A command header (AHCI 1.3.1 section 4.2.2): 32 bytes, the hardware
/// layout, so the fields stay public. Written whole from a fresh instance
/// with the fields it needs set, so every other field is zero.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct HbaCommandHeader
{
    /// <summary>CFL in bits 4:0, ATAPI bit 5, W bit 6, P bit 7.</summary>
    public byte Flags0;

    /// <summary>R bit 0, B bit 1, C bit 2, PMP in bits 7:4.</summary>
    public byte Flags1;

    /// <summary>PRDTL: the PRDT entries the table holds.</summary>
    public ushort PrdtLength;

    /// <summary>PRDBC: the bytes transferred, written by the HBA.</summary>
    public uint PrdByteCount;

    /// <summary>CTBA: the command table's physical address, low 32 bits.</summary>
    public uint CommandTableBase;

    /// <summary>CTBAU: the command table's physical address, high 32 bits.</summary>
    public uint CommandTableBaseUpper;

    /// <summary>Reserved.</summary>
    public uint Reserved0;

    /// <summary>Reserved.</summary>
    public uint Reserved1;

    /// <summary>Reserved.</summary>
    public uint Reserved2;

    /// <summary>Reserved.</summary>
    public uint Reserved3;
}

/// <summary>
/// A physical region descriptor (AHCI 1.3.1 section 4.2.3.3): 16 bytes,
/// the hardware layout, so the fields stay public. Written whole.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct HbaPrdtEntry
{
    /// <summary>DBA: the data's physical address, low 32 bits.</summary>
    public uint DataBase;

    /// <summary>DBAU: the data's physical address, high 32 bits.</summary>
    public uint DataBaseUpper;

    /// <summary>Reserved.</summary>
    public uint Reserved;

    /// <summary>DBC in bits 21:0: the byte count minus one; I in bit 31.</summary>
    public uint ByteCountAndInterrupt;
}

/// <summary>
/// A register host to device FIS (Serial ATA section 10.3.4): 20 bytes,
/// the hardware layout, so the fields stay public. Written whole at the
/// start of a command table.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct FisRegisterH2D
{
    /// <summary>The FIS type, 0x27.</summary>
    public byte FisType;

    /// <summary>C in bit 7, the port multiplier port in bits 3:0.</summary>
    public byte Flags;

    /// <summary>The ATA command.</summary>
    public byte Command;

    /// <summary>The features register, low byte.</summary>
    public byte FeaturesLow;

    /// <summary>LBA bits 7:0.</summary>
    public byte Lba0;

    /// <summary>LBA bits 15:8.</summary>
    public byte Lba1;

    /// <summary>LBA bits 23:16.</summary>
    public byte Lba2;

    /// <summary>The device register.</summary>
    public byte Device;

    /// <summary>LBA bits 31:24.</summary>
    public byte Lba3;

    /// <summary>LBA bits 39:32.</summary>
    public byte Lba4;

    /// <summary>LBA bits 47:40.</summary>
    public byte Lba5;

    /// <summary>The features register, high byte.</summary>
    public byte FeaturesHigh;

    /// <summary>The sector count, low byte.</summary>
    public byte CountLow;

    /// <summary>The sector count, high byte.</summary>
    public byte CountHigh;

    /// <summary>The isochronous command completion field.</summary>
    public byte Icc;

    /// <summary>The control register.</summary>
    public byte Control;

    /// <summary>Reserved.</summary>
    public uint Reserved;
}
