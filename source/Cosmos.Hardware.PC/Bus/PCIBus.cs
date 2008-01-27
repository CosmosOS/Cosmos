using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.PC.Bus {
    public class PCIBus : Cosmos.Hardware.Bus.PCIBus {
        protected const ushort ConfigAddr = 0xCF8;
        protected const ushort ConfigData = 0xCFC;

        static public void Init() {
        }

        static public UInt32 Read32(byte aBus, byte aSlot
            , byte aFunc, byte aRegister) {
            ulong xAddr = (ulong)(
                ((UInt32)aBus << 16)
                | ((UInt32)aSlot << 11)
                | ((UInt32)aFunc << 8)
                // Strip last 2 bits. Register must be on even 32 bits, just in case
                // user passes us bad data
                | ((UInt32)aRegister & 0xFC)
                // Enable bit
                | 0x80000000); 
            //CPUBus.WriteLong(ConfigAddr, xAddr);
            //return CPUBus.ReadLong(ConfigData);
            return 0;
        }

    }
}
