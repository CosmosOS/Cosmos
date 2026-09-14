// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Pci.Enums;

internal enum ClassId
{
    PciDevice20 = 0x00,
    MassStorageController = 0x01,
    NetworkController = 0x02,
    DisplayController = 0x03,
    MultimediaDevice = 0x04,
    MemoryController = 0x05,
    BridgeDevice = 0x06,
    SimpleCommController = 0x07,
    BaseSystemPreiph = 0x08,
    InputDevice = 0x09,
    DockingStations = 0x0A,
    Proccesors = 0x0B,
    SerialBusController = 0x0C,
    WirelessController = 0x0D,
    InteligentController = 0x0E,
    SateliteCommController = 0x0F,
    EncryptionController = 0x10,
    SignalProcessingController = 0x11,
    ProcessingAccelerators = 0x12,
    NonEssentialInstsrumentation = 0x13,
    Coprocessor = 0x40,
    Unclassified = 0xFF
}
