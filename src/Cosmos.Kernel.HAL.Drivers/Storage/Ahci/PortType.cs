// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Storage.Ahci;

/// <summary>
/// What a port's signature says is attached to it.
/// </summary>
internal enum PortType
{
    /// <summary>Nothing, or a signature this driver does not recognise.</summary>
    Nothing = 0x00,

    /// <summary>A SATA disk.</summary>
    Sata = 0x01,

    /// <summary>A SATAPI device, such as an optical drive.</summary>
    Satapi = 0x02,

    /// <summary>An enclosure management bridge.</summary>
    Semb = 0x03,

    /// <summary>A port multiplier.</summary>
    PM = 0x04
}
