namespace Cosmos.HAL;

public class PCIDeviceBridge : PCIDevice
{
    public PCIDeviceBridge(uint bus, uint slot, uint function)
        : base(bus, slot, function)
    {
        BaseAddresses = new PCIBaseAddressBar[2];
        BaseAddresses[0] = new PCIBaseAddressBar(ReadRegister32(0x10));
        BaseAddresses[1] = new PCIBaseAddressBar(ReadRegister32(0x14));

        PrimaryBusNumber = ReadRegister8(0x18);
        SecondaryBusNumber = ReadRegister8(0x19);
        SubordinateBusNumber = ReadRegister8(0x1A);
        SecondaryLatencyTimer = ReadRegister8(0x1B);

        IOBase = ReadRegister8(0x1C);
        IOLimit = ReadRegister8(0x1D);
        SecondaryStatus = ReadRegister16(0x1E);

        MemoryBase = ReadRegister16(0x20);
        MemoryLimit = ReadRegister16(0x22);

        PrefatchableMemoryBase = ReadRegister16(0x24);
        PrefatchableMemoryLimit = ReadRegister16(0x26);

        PrefatchableBaseUpper32 = ReadRegister32(0x28);
        PrefatchableLimitUpper32 = ReadRegister32(0x2C);

        IOBaseUpper16 = ReadRegister16(0x30);
        IOLimitUpper16 = ReadRegister16(0x32);

        CapabilityPointer = ReadRegister8(0x34);

        ExpansionROMBaseAddress = ReadRegister32(0x38);

        BridgeControl = ReadRegister16(0x3E);
    }

    public PCIBaseAddressBar[] BaseAddresses { get; }

    public byte PrimaryBusNumber { get; }
    public byte SecondaryBusNumber { get; }
    public byte SubordinateBusNumber { get; }
    public byte SecondaryLatencyTimer { get; }

    public byte IOBase { get; }
    public byte IOLimit { get; }
    public ushort SecondaryStatus { get; }

    public ushort MemoryBase { get; }
    public ushort MemoryLimit { get; }

    public ushort PrefatchableMemoryBase { get; }
    public ushort PrefatchableMemoryLimit { get; }

    public uint PrefatchableBaseUpper32 { get; }

    public uint PrefatchableLimitUpper32 { get; }

    public ushort IOBaseUpper16 { get; }
    public ushort IOLimitUpper16 { get; }

    public byte CapabilityPointer { get; }

    public uint ExpansionROMBaseAddress { get; }

    public ushort BridgeControl { get; }
}
