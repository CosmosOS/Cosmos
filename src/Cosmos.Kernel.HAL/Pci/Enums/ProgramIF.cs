// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Pci.Enums;

internal enum ProgramIf
{
    // MassStorageController:
    SataVendorSpecific = 0x00,
    SataAhci = 0x01,
    SataSerialStorageBus = 0x02,
    SasSerialStorageBus = 0x01,
    NvmNvmhci = 0x01,
    NvmNvmExpress = 0x02
}
