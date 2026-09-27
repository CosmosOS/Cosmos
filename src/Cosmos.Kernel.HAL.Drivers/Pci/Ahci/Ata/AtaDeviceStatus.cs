// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Ahci.Ata;

/// <summary>
/// ATA status bits, as PxTFD reports them.
/// </summary>
[Flags]
internal enum AtaDeviceStatus : uint
{
    /// <summary>BSY: the device is busy.</summary>
    Busy = 0x80,

    /// <summary>DRQ: the device is requesting a data transfer.</summary>
    DRQ = 0x08
}
