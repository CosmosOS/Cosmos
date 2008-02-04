using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.PC.Bus {
    //TODO: Move to Cosmos.Hardware
    public class DeviceIDs {
        static protected Dictionary<UInt16, string> mVendors = new Dictionary<UInt16, string>();

        static DeviceIDs() {
            mVendors.Add(0x8086, "Intel");
        }

        static public string FindVendor(UInt16 aVendorID) {
            return mVendors[aVendorID];
        }
    }

    public class PCIBus : Cosmos.Hardware.Bus.PCIBus {
        protected const ushort ConfigAddr = 0xCF8;
        protected const ushort ConfigData = 0xCFC;

        static public void Init() {
            Console.WriteLine("PCI Devices");
            Console.WriteLine();
            for (byte xBus = 0; xBus <= 255; xBus++) {
                for (byte xSlot = 0; xSlot <= 31; xSlot++) {
                    for (byte xFunction = 0; xFunction <= 7; xFunction++) {
                        UInt32 xValue = Read32(xBus, xSlot, xFunction, 0);
                        //UInt16 xVendorID = (UInt16)(xValue & 0xFFFF);
                        //UInt16 xDeviceID = (UInt16)(xValue >> 16); 
                        UInt32 xVendorID = xValue & 0xFFFF;
                        UInt32 xDeviceID = xValue >> 16;
                        if (xVendorID != 0xFFFF) {
                            //string xVendorName = DeviceIDs.FindVendor(xVendorID);
                            string xVendorName = null;
                            if (xVendorName == null) {
                                xVendorName = xVendorID.ToString();
                            }
                            //Console.Write("Location: ");
                            Console.Write(xBus.ToString());
                            Console.Write("-");
                            Console.Write(xSlot.ToString());
                            Console.Write("-");
                            Console.Write(xFunction.ToString());
                            Console.Write(" ");

                            Console.Write("Value: ");
                            Console.WriteLine(xValue.ToString());

                            //Console.Write("Vendor: ");
                            //Console.WriteLine(xVendorName);

                            //Console.Write("Device: ");
                            //Console.WriteLine(xDeviceID.ToString());

                            //Console.WriteLine();
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
