using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.PC.Bus {

    public class PCIBus : Cosmos.Hardware.Bus.PCIBus {
        protected const ushort ConfigAddr = 0xCF8;
        protected const ushort ConfigData = 0xCFC;

        static public void Init() {
            var xDeviceIDs = new DeviceIDs();
            Console.WriteLine("PCI Devices");
            Console.WriteLine();
            for (byte xBus = 0; xBus <= 255; xBus++) {
                for (byte xSlot = 0; xSlot <= 31; xSlot++) {
                    for (byte xFunction = 0; xFunction <= 7; xFunction++) {
                        UInt32 xUInt32 = Read32(xBus, xSlot, xFunction, 0);
                        //UInt16 xVendorID = (UInt16)(xUInt32 & 0xFFFF);
                        //UInt16 xDeviceID = (UInt16)(xUInt32 >> 16); 
                        UInt32 xVendorID = xUInt32 & 0xFFFF;
                        UInt32 xDeviceID = xUInt32 >> 16;
                        if (xVendorID != 0xFFFF) {
                            string xVendor = xDeviceIDs.FindVendor(xVendorID);
                            if (xVendor == null) {
                                xVendor = xVendorID.ToString();
                            }
                            Console.Write(xBus.ToString());
                            Console.Write("-");
                            Console.Write(xSlot.ToString());
                            Console.Write("-");
                            Console.Write(xFunction.ToString());
                            Console.Write(" ");
                            Console.Write(xVendor);
                            Console.Write(": ");
                            Console.Write(xDeviceID.ToString());

                            Console.WriteLine();
                        }
                    }
                }
            }
        }

        static public UInt32 Read32(byte aBus, byte aSlot
            , byte aFunc, byte aRegister) {
            UInt32 xAddr = (UInt32)(
                // Bits 23-16
                ((UInt32)aBus << 16)
                // Bits 15-11
                | (((UInt32)aSlot & 0x1F) << 11)
                // Bits 10-8
                | (((UInt32)aFunc & 0x07) << 8)
                // Bits 7-0
                // Strip last 2 bits. Register must be on even 32 bits, just in case
                // user passes us bad data
                | ((UInt32)aRegister & 0xFC)
                // Enable bit - must be set
                | 0x80000000);
            CPUBus.Write32(ConfigAddr, xAddr);
            return CPUBus.Read32(ConfigData);
        }

    }
}
