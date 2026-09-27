// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Ahci.Ata;

/// <summary>
/// ATA commands this driver issues or recognises.
/// </summary>
internal enum AtaCommands : byte
{
    /// <summary>READ DMA EXT.</summary>
    ReadDmaExt = 0x25,

    /// <summary>WRITE DMA EXT.</summary>
    WriteDmaExt = 0x35,

    /// <summary>FLUSH CACHE EXT.</summary>
    CacheFlushExt = 0xEA,

    /// <summary>IDENTIFY PACKET DEVICE.</summary>
    IdentifyPacket = 0xA1,

    /// <summary>IDENTIFY DEVICE DMA.</summary>
    IdentifyDma = 0xEE,

    /// <summary>IDENTIFY DEVICE.</summary>
    Identify = 0xEC
}
