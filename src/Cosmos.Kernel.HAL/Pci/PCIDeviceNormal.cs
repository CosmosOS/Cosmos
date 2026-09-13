// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Pci;

internal class PciDeviceNormal : PciDevice
{
    public PciBaseAddressBar[] BaseAddresses { get; private set; }

    public uint CardbusCisPointer { get; private set; }

    public ushort SubsystemVendorId { get; private set; }
    public ushort SubsystemId { get; private set; }

    public uint ExpansionRomBaseAddress { get; private set; }

    public byte CapabilitiesPointer { get; private set; }

    public byte MinGrant { get; private set; }
    public byte MaxLatency { get; private set; }

    public PciDeviceNormal(uint bus, uint slot, uint function)
        : base(bus, slot, function)
    {
        BaseAddresses = new PciBaseAddressBar[6];
        BaseAddresses[0] = new PciBaseAddressBar(ReadRegister32(0x10));
        BaseAddresses[1] = new PciBaseAddressBar(ReadRegister32(0x14));
        BaseAddresses[2] = new PciBaseAddressBar(ReadRegister32(0x18));
        BaseAddresses[3] = new PciBaseAddressBar(ReadRegister32(0x1C));
        BaseAddresses[4] = new PciBaseAddressBar(ReadRegister32(0x20));
        BaseAddresses[5] = new PciBaseAddressBar(ReadRegister32(0x24));

        CardbusCisPointer = ReadRegister32(0x28);

        SubsystemVendorId = ReadRegister16(0x2C);
        SubsystemId = ReadRegister16(0x2E);

        ExpansionRomBaseAddress = ReadRegister32(0x30);

        CapabilitiesPointer = ReadRegister8(0x34);

        MinGrant = ReadRegister8(0x3E);
        MaxLatency = ReadRegister8(0x3F);
    }
}
