using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.PC.Bus {
    //TODO: Move to Cosmos.Hardware
    public class DeviceID
    {
        UInt32 key;

        public UInt32 Key
        {
            get { return key; }
            set { key = value; }
        }

        String value;

        public String Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public DeviceID(UInt32 pkey, String pvalue)
        {
            key = pkey;
            value = pvalue;
        }
    }

    public class DeviceIDs
    {
        protected Dictionary<String> mVendors = new Dictionary<String>();

        public DeviceIDs() {
            mVendors.Add(0x8086, "Intel");
        }

        public string FindVendor(UInt32 aVendorID) {
            return mVendors[aVendorID];
        }

        /*protected List<DeviceID> mVendors = new List<DeviceID>();

        public DeviceIDs()
        {
            mVendors.Add(new DeviceID(0x8086, "Intel"));
        }

        public string FindVendor(UInt32 aVendorID)
        {
            for (int i = 0; i < mVendors.Count; i++)
            {

                if (mVendors[i].Key == aVendorID)
                    return mVendors[i].Value;
            }
            return null;
        }*/
    }

    public class PCIBus : Cosmos.Hardware.Bus.PCIBus {
        protected const ushort ConfigAddr = 0xCF8;
        protected const ushort ConfigData = 0xCFC;

        static public void Init() {
            DeviceIDs deviceIDs = new DeviceIDs();
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
                            string xVendorName = deviceIDs.FindVendor(xVendorID);
                            //string xVendorName = null;
                            //if (xVendorName == null) {
                            //    xVendorName = xVendorID.ToString();
                            //}
                            //Console.Write("Location: ");
                            Console.Write(xBus.ToString());
                            Console.Write("-");
                            Console.Write(xSlot.ToString());
                            Console.Write("-");
                            Console.Write(xFunction.ToString());
                            if(xVendorName != null)
                            {
                                Console.Write("(");
                                Console.Write(xVendorName);
                                Console.Write(")");
                            }
                            Console.Write(" ");

                            Console.Write("UInt32: ");
                            Console.WriteLine(xUInt32.ToString());

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
