// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Pci.Enums;

internal enum PciCommand : short
{
    Io = 0x1, /* Enable response in I/O space */
    Memory = 0x2, /* Enable response in Memory space */
    Master = 0x4, /* Enable bus mastering */
    Special = 0x8, /* Enable response to special cycles */
    Invalidate = 0x10, /* Use memory write and invalidate */
    VgaPallete = 0x20, /* Enable palette snooping */
    Parity = 0x40, /* Enable parity checking */
    Wait = 0x80, /* Enable address/data stepping */
    Serr = 0x100, /* Enable SERR */
    FastBack = 0x200, /* Enable back-to-back writes */
    InterruptDisable = 0x400 /* Disable INTx interrupts */
}
