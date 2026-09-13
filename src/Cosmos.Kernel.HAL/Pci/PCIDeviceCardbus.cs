// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Pci;

internal class PciDeviceCardBus : PciDevice
{
    public uint CardBusBaseAddress { get; private set; }

    public byte OffsetOfCapabilityList { get; private set; }
    public ushort SecondaryStatus { get; private set; }

    public byte PciBusNumber { get; private set; }
    public byte CardbusBusNumber { get; private set; }
    public byte SubordinateBusNumber { get; private set; }
    public byte CardbusLatencyTimer { get; private set; }

    public uint MemoryBaseAddress0 { get; private set; }
    public uint MemoryLimit0 { get; private set; }
    public uint MemoryBaseAddress1 { get; private set; }
    public uint MemoryLimit1 { get; private set; }

    public uint IoBaseAddress0 { get; private set; }
    public uint IoLimit0 { get; private set; }
    public uint IoBaseAddress1 { get; private set; }
    public uint IoLimit1 { get; private set; }

    public ushort BridgeControl { get; private set; }

    public ushort SubsystemDeviceId { get; private set; }
    public ushort SubsystemVendorId { get; private set; }

    public uint PcCardBaseAddress { get; private set; }

    public PciDeviceCardBus(uint bus, uint slot, uint function)
        : base(bus, slot, function)
    {
        CardBusBaseAddress = ReadRegister32(0x10);

        OffsetOfCapabilityList = ReadRegister8(0x14);
        SecondaryStatus = ReadRegister16(0x16);

        PciBusNumber = ReadRegister8(0x18);
        CardbusBusNumber = ReadRegister8(0x19);
        SubordinateBusNumber = ReadRegister8(0x1A);
        CardbusLatencyTimer = ReadRegister8(0x1B);

        MemoryBaseAddress0 = ReadRegister32(0x1C);
        MemoryLimit0 = ReadRegister32(0x20);
        MemoryBaseAddress1 = ReadRegister32(0x24);
        MemoryLimit1 = ReadRegister32(0x28);

        IoBaseAddress0 = ReadRegister32(0x2C);
        IoLimit0 = ReadRegister32(0x30);
        IoBaseAddress1 = ReadRegister32(0x34);
        IoLimit1 = ReadRegister32(0x38);

        BridgeControl = ReadRegister16(0x3C);

        SubsystemDeviceId = ReadRegister16(0x40);
        SubsystemVendorId = ReadRegister16(0x42);

        PcCardBaseAddress = ReadRegister32(0x44);
    }
}
