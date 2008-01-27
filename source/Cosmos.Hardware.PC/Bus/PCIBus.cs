using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.PC.Bus {
    public class PCIBus : Cosmos.Hardware.Bus.PCIBus {
        protected const ushort ConfigAddr = 0xCF8;
        protected const ushort ConfigData = 0xCFC;

        static public void Init() {
            for (byte xBus = 0; xBus <= 255; xBus++) {
                for (byte xSlot = 0; xSlot <= 31; xSlot++) {
                    for (byte xFunction = 0; xFunction <= 7; xFunction++) {
                        UInt32 xValue = Read32(xBus, xSlot, xFunction, 0);
                        UInt32 xVendorID = xValue & 0xFFFF;
                        if (xVendorID != 0xFFFF) {
                            Console.WriteLine("PCI Device found");
                        }
                    }
                }
            }
        }

        static public UInt32 Read32(byte aBus, byte aSlot
            , byte aFunc, byte aRegister) {
            UInt32 xAddr = (UInt32)(
                ((UInt32)aBus << 16)
                | ((UInt32)aSlot << 11)
                | ((UInt32)aFunc << 8)
                // Strip last 2 bits. Register must be on even 32 bits, just in case
                // user passes us bad data
                | ((UInt32)aRegister & 0xFC)
                // Enable bit
                | 0x80000000); 
            CPUBus.Write32(ConfigAddr, xAddr);
            return CPUBus.Read32(ConfigData);
        }

    }
}
