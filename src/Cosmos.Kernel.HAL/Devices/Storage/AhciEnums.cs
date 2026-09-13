// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Devices.Storage;

/// <summary>
/// Port type enumeration.
/// </summary>
internal enum PortType
{
    Nothing = 0x00,
    Sata = 0x01,
    Satapi = 0x02,
    Semb = 0x03,
    PM = 0x04
}

/// <summary>
/// FIS (Frame Information Structure) type enumeration.
/// </summary>
internal enum FisType : byte
{
    RegisterH2D = 0x27,  // Register FIS: Host to Device
    RegisterD2H = 0x34,  // Register FIS: Device to Host
    DmaActivate = 0x39, // DMA Activate
    DmaSetup = 0x41,    // DMA Setup: Device to Host
    Data = 0x46,         // Data FIS: Bidirectional
    Bist = 0x58,         // BIST
    PioSetup = 0x5F,    // PIO Setup: Device to Host
    DeviceBits = 0xA1    // Device bits
}

/// <summary>
/// AHCI drive signature to identify what drive is plugged to Port.
/// </summary>
internal enum AhciSignature : uint
{
    Sata = 0x0000,
    PortMultiplier = 0x9669,
    Satapi = 0xEB14,
    Semb = 0xC33C,
    Nothing = 0xFFFF
}

/// <summary>
/// SATA Status: Interface Power Management Status.
/// </summary>
internal enum InterfacePowerManagementStatus : uint
{
    NotPresent = 0x00,
    Active = 0x01,
    Partial = 0x02,
    Slumber = 0x06,
    DeviceSleep = 0x08
}

/// <summary>
/// SATA Status: Current Interface Speed.
/// </summary>
internal enum CurrentInterfaceSpeedStatus : uint
{
    NotPresent = 0x00,
    Gen1Rate = 0x01,
    Gen2Rate = 0x02,
    Gen3Rate = 0x03
}

/// <summary>
/// SATA Status: Device Detection Status.
/// </summary>
internal enum DeviceDetectionStatus : uint
{
    NotDetected = 0x00,
    DeviceDetectedNoPhy = 0x01,
    DeviceDetectedWithPhy = 0x03,
    PhyOffline = 0x04
}

/// <summary>
/// ATA Device Status bits.
/// </summary>
internal enum AtaDeviceStatus : uint
{
    Busy = 0x80,
    DRQ = 0x08
}

/// <summary>
/// Command and Status register bits.
/// </summary>
internal enum CommandAndStatus : uint
{
    ICC_Reserved0 = 0x0000000F,
    // PxCMD.ICC lives in bits 31:28 (AHCI 1.3.1 s3.3.7) — the values are
    // shifted into place so writing them programs the actual ICC field
    // instead of colliding with ST/SUD/POD in the low bits.
    ICC_DevSleep = 0x8U << 28,
    ICC_Slumber = 0x6U << 28,
    ICC_Partial = 0x2U << 28,
    ICC_Active = 0x1U << 28,
    ICC_Idle = 0x00000000,
    ASP = 01 << 27,
    ALPE = 01 << 26,
    EnableATAPILED = 01 << 25,
    ATAPIDevice = 01 << 24,
    APSTE = 01 << 23,
    FISSwitchPort = 01 << 22,
    ExternalSATAPort = 01 << 21,
    ColdPresenceDetect = 01 << 20,
    MPSP = 01 << 19,
    HotPlugCapPort = 01 << 18,
    PortMultAttach = 01 << 17,
    ColdPresenceState = 01 << 16,
    CMDListRunning = 01 << 15,
    FISReceiveRunning = 01 << 14,
    MPSS = 01 << 13,
    // PxCMD.CCS is a 5-bit field (bits 12:08), not a single flag: mask and
    // shift to extract the slot index of the command currently being issued.
    CurrentCMDSlotMask = 0x1F << 08,
    Reserved0 = 01 << 07,
    FISReceiveEnable = 01 << 04,
    CMDListOverride = 01 << 03,
    PowerOnDevice = 01 << 02,
    SpinUpDevice = 01 << 01,
    StartProcess = 01 << 00,
    Null = 0xFFFF
}

/// <summary>
/// Interrupt Status bits.
/// </summary>
internal enum InterruptStatus : int
{
    ColdPortDetectStatus = 01 << 31,
    TaskFileErrorStatus = 01 << 30,
    HostBusFatalErrorStatus = 01 << 29,
    HostBusDataErrorStatus = 01 << 28,
    InterfaceFatalErrorStatus = 01 << 27,
    InterfaceNFatalErrorStatus = 01 << 26,
    OverflowStatus = 01 << 24,
    IncorrectPMStatus = 01 << 23,
    PhyRdyChangeStatus = 01 << 22,
    DevMechanicalPresenceStatus = 01 << 07,
    PortConnectChangeStatus = 01 << 06,
    DescriptorProcessed = 01 << 05,
    UnknownFISInterrupt = 01 << 04,
    SetDeviceBitsInterrupt = 01 << 03,
    DMASetupFISInterrupt = 01 << 02,
    PIOSetupFISInterrupt = 01 << 01,
    D2HRegFISInterrupt = 01 << 00,
    Null = 0xFFFF
}

/// <summary>
/// ATA Commands.
/// </summary>
internal enum AtaCommands : byte
{
    ReadDma = 0xC8,
    ReadDmaExt = 0x25,
    WriteDma = 0xCA,
    WriteDmaExt = 0x35,
    CacheFlush = 0xE7,
    CacheFlushExt = 0xEA,
    Packet = 0xA0,
    IdentifyPacket = 0xA1,
    IdentifyDma = 0xEE,
    Identify = 0xEC,
    Read = 0xA8,
    Eject = 0x1B
}
