namespace Cosmos.HAL;

public class PCIDeviceCardbus : PCIDevice
{
    public PCIDeviceCardbus(uint bus, uint slot, uint function)
        : base(bus, slot, function)
    {
        CardbusBaseAddress = ReadRegister32(0x10);

        OffsetOfCapabilityList = ReadRegister8(0x14);
        SecondaryStatus = ReadRegister16(0x16);

        PCIBusNumber = ReadRegister8(0x18);
        CardbusBusNumber = ReadRegister8(0x19);
        SubordinateBusNumber = ReadRegister8(0x1A);
        CardbusLatencyTimer = ReadRegister8(0x1B);

        MemoryBaseAddress0 = ReadRegister32(0x1C);
        MemoryLimit0 = ReadRegister32(0x20);
        MemoryBaseAddress1 = ReadRegister32(0x24);
        MemoryLimit1 = ReadRegister32(0x28);

        IOBaseAddress0 = ReadRegister32(0x2C);
        IOLimit0 = ReadRegister32(0x30);
        IOBaseAddress1 = ReadRegister32(0x34);
        IOLimit1 = ReadRegister32(0x38);

        BridgeControl = ReadRegister16(0x3C);

        SubsystemDeviceID = ReadRegister16(0x40);
        SubsystemVendorID = ReadRegister16(0x42);

        PCCardBaseAddress = ReadRegister32(0x44);
    }

    public uint CardbusBaseAddress { get; }

    public byte OffsetOfCapabilityList { get; }
    public ushort SecondaryStatus { get; }

    public byte PCIBusNumber { get; }
    public byte CardbusBusNumber { get; }
    public byte SubordinateBusNumber { get; }
    public byte CardbusLatencyTimer { get; }

    public uint MemoryBaseAddress0 { get; }
    public uint MemoryLimit0 { get; }
    public uint MemoryBaseAddress1 { get; }
    public uint MemoryLimit1 { get; }

    public uint IOBaseAddress0 { get; }
    public uint IOLimit0 { get; }
    public uint IOBaseAddress1 { get; }
    public uint IOLimit1 { get; }

    public ushort BridgeControl { get; }

    public ushort SubsystemDeviceID { get; }
    public ushort SubsystemVendorID { get; }

    public uint PCCardBaseAddress { get; }
}
