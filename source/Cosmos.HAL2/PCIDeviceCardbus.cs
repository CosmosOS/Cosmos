using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.HAL
{
    public class PCIDeviceCardbus : PCIDevice
    {
        public uint CardbusBaseAddress { get; private set; }

        public byte OffsetOfCapabilityList { get; private set; }
        public ushort SecondaryStatus { get; private set; }

        public byte PCIBusNumber { get; private set; }
        public byte CardbusBusNumber { get; private set; }
        public byte SubordinateBusNumber { get; private set; }
        public byte CardbusLatencyTimer { get; private set; }

        public uint MemoryBaseAddress0 { get; private set; }
        public uint MemoryLimit0 { get; private set; }
        public uint MemoryBaseAddress1 { get; private set; }
        public uint MemoryLimit1 { get; private set; }

        public uint IOBaseAddress0 { get; private set; }
        public uint IOLimit0 { get; private set; }
        public uint IOBaseAddress1 { get; private set; }
        public uint IOLimit1 { get; private set; }

        public ushort BridgeControl { get; private set; }

        public ushort SubsystemDeviceID { get; private set; }
        public ushort SubsystemVendorID { get; private set; }

        public uint PCCardBaseAddress { get; private set; }

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
    }
}
