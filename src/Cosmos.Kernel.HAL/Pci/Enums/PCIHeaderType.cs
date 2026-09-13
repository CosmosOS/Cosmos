// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Pci.Enums;

internal enum PciHeaderType : byte
{
    Normal = 0x00,
    Bridge = 0x01,
    Cardbus = 0x02
};
