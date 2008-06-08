using System;
using System.Collections.Generic;
using System.Text;
using HW = Cosmos.Hardware;

namespace KudzuTest {
    public class PCITest {
        public static void Test() {
            HW.PC.Bus.PCIBus.Init();
            WriteDevices();
        }

        public static void WriteDevices() {
            var xDeviceIDs = new HW.PC.Bus.PCIBus.DeviceIDs();

            for (int d = 0; d < Cosmos.Hardware.PC.Bus.PCIBus.Devices.Length; d++) {
                var xPCIDevice = Cosmos.Hardware.PC.Bus.PCIBus.Devices[d];
                string xVendor = xDeviceIDs.FindVendor(xPCIDevice.VendorID);

                if (xVendor == default(string))
                    xVendor = ToHex(xPCIDevice.VendorID, 4);

                System.Console.Write(xPCIDevice.Bus + "-" + xPCIDevice.Slot + "-" + xPCIDevice.Function);
                System.Console.Write(" " + xVendor + ":" + ToHex(xPCIDevice.DeviceID, 4));
                System.Console.WriteLine(" Type: " + xPCIDevice.HeaderType + " IRQ: " + xPCIDevice.InterruptLine);
                //                                /*Enum.GetName(typeof(PCIHeaderType), */ xPCIDevice.HeaderType/*) */);
                //                           Console.WriteLine(" Status: " + xPCIDevice.Status + " " +
                //                               /*Enum.GetName(typeof(PCIStatus),  */xPCIDevice.Status/*) */);
                //                            Console.WriteLine(" Command: " + xPCIDevice.Command + " " +
                //                                /*Enum.GetName(typeof(PCICommand), */xPCIDevice.Command /* ) */);
                System.Console.Write(" Class [" + ToHex((UInt32)((xPCIDevice.ClassCode << 8) | xPCIDevice.SubClass), 4) + "] " + xPCIDevice.GetClassInfo());
                System.Console.WriteLine();
                System.Console.Write(" Memory: ");

                for (byte i = 0; i < xPCIDevice.NumberOfBaseAddresses(); i++) {
                    var a = xPCIDevice.GetAddressSpace(i);

                    if (a != null) {
                        System.Console.Write("register " + i + " @ " + ToHex(a.Offset, 8) + " (" + a.Size + "b) ");
                        if (a is Cosmos.Kernel.MemoryAddressSpace) {
                            Console.WriteLine("mem");
                        } else {
                            Console.WriteLine("io");
                        }
                    }
                }
                System.Console.WriteLine();
                System.Console.WriteLine();
            }
        }

        [Obsolete("Use extensionmethod in Cosmos.Hardware.Extension.Numbersystem. See MACAddress.cs for example")]
        public static string ToHex(UInt32 num, int length) {
            char[] ret = new char[length];
            UInt32 cpy = num;

            for (int index = length - 1; index >= 0; index--) {
                ret[index] = hex(cpy & 0xf);
                cpy = cpy / 16;
            }

            return "0x" + new string(ret);
        }

        [Obsolete]
        private static char hex(uint p) {
            switch (p) {
                case 0:
                    return '0';
                case 1:
                    return '1';
                case 2:
                    return '2';
                case 3:
                    return '3';
                case 4:
                    return '4';
                case 5:
                    return '5';
                case 6:
                    return '6';
                case 7:
                    return '7';
                case 8:
                    return '8';
                case 9:
                    return '9';
                case 10:
                    return 'a';
                case 11:
                    return 'b';
                case 12:
                    return 'c';
                case 13:
                    return 'd';
                case 14:
                    return 'e';
                case 15:
                    return 'f';
            }
            return ' ';
        }
    }
}
