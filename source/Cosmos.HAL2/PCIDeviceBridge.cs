using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.HAL
{
    public class PCIDeviceBridge : PCIDevice
    {
        public PCIBaseAddressBar[] BaseAddresses { get; private set; }

        public byte PrimaryBusNumber { get; private set; }
        public byte SecondaryBusNumber { get; private set; }
        public byte SubordinateBusNumber { get; private set; }
        public byte SecondaryLatencyTimer { get; private set; }

        public byte IOBase { get; private set; }
        public byte IOLimit { get; private set; }
        public ushort SecondaryStatus { get; private set; }

        public ushort MemoryBase { get; private set; }
        public ushort MemoryLimit { get; private set; }

        public ushort PrefatchableMemoryBase { get; private set; }
        public ushort PrefatchableMemoryLimit { get; private set; }

        public uint PrefatchableBaseUpper32 { get; private set; }

        public uint PrefatchableLimitUpper32 { get; private set; }

        public ushort IOBaseUpper16 { get; private set; }
        public ushort IOLimitUpper16 { get; private set; }

        public byte CapabilityPointer { get; private set; }

        public uint ExpansionROMBaseAddress { get; private set; }

        public ushort BridgeControl { get; private set; }

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
    }
}
