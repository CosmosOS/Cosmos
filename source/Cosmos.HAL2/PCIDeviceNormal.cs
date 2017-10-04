using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.HAL
{
    public class PCIDeviceNormal : PCIDevice
    {
        public PCIBaseAddressBar[] BaseAddresses { get; private set; }

        public uint CardbusCISPointer { get; private set; }

        public ushort SubsystemVendorID { get; private set; }
        public ushort SubsystemID { get; private set; }

        public uint ExpansionROMBaseAddress { get; private set; }

        public byte CapabilitiesPointer { get; private set; }

        public byte MinGrant { get; private set; }
        public byte MaxLatency { get; private set; }

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

        //public void EnableDevice()
        //{
        //    Command |= PCICommand.Master | PCICommand.IO | PCICommand.Memory;
        //}
    }
}
