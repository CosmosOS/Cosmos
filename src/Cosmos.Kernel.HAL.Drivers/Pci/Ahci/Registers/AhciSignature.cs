// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Ahci.Registers;

/// <summary>
/// The high word of PxSIG, which identifies what drive is plugged into a port.
/// </summary>
internal enum AhciSignature : uint
{
    /// <summary>A SATA disk.</summary>
    Sata = 0x0000,

    /// <summary>A port multiplier.</summary>
    PortMultiplier = 0x9669,

    /// <summary>A SATAPI device.</summary>
    Satapi = 0xEB14,

    /// <summary>An enclosure management bridge.</summary>
    Semb = 0xC33C,

    /// <summary>No device.</summary>
    Nothing = 0xFFFF
}
