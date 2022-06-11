namespace Cosmos.HAL;

public class PCIDeviceNormal : PCIDevice
{
    public PCIDeviceNormal(uint bus, uint slot, uint function)
        : base(bus, slot, function)
    {
        BaseAddresses = new PCIBaseAddressBar[6];
        BaseAddresses[0] = new PCIBaseAddressBar(ReadRegister32(0x10));
        BaseAddresses[1] = new PCIBaseAddressBar(ReadRegister32(0x14));
        BaseAddresses[2] = new PCIBaseAddressBar(ReadRegister32(0x18));
        BaseAddresses[3] = new PCIBaseAddressBar(ReadRegister32(0x1C));
        BaseAddresses[4] = new PCIBaseAddressBar(ReadRegister32(0x20));
        BaseAddresses[5] = new PCIBaseAddressBar(ReadRegister32(0x24));

        CardbusCISPointer = ReadRegister32(0x28);

        SubsystemVendorID = ReadRegister16(0x2C);
        SubsystemID = ReadRegister16(0x2E);

        ExpansionROMBaseAddress = ReadRegister32(0x30);

        CapabilitiesPointer = ReadRegister8(0x34);

        MinGrant = ReadRegister8(0x3E);
        MaxLatency = ReadRegister8(0x3F);
    }

    public PCIBaseAddressBar[] BaseAddresses { get; }

    public uint CardbusCISPointer { get; }

    public ushort SubsystemVendorID { get; }
    public ushort SubsystemID { get; }

    public uint ExpansionROMBaseAddress { get; }

    public byte CapabilitiesPointer { get; }

    public byte MinGrant { get; }
    public byte MaxLatency { get; }

    //public void EnableDevice()
    //{
    //    Command |= PCICommand.Master | PCICommand.IO | PCICommand.Memory;
    //}
}
