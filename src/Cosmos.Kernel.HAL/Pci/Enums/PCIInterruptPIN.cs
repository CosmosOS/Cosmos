// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Pci.Enums;

internal enum PciInterruptPin : byte
{
    None = 0x00,
    Inta = 0x01,
    Intb = 0x02,
    Intc = 0x03,
    Intd = 0x04
};
