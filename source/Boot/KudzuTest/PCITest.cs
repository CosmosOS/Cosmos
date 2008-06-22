using System;
using System.Collections.Generic;
using System.Text;
using HW = Cosmos.Hardware;
using Cosmos.Kernel;

namespace KudzuTest {
    public class PCITest {
        public static void Test() {
            HW.PCIBus.Init();
            WriteDevices();
        }

        public static void WriteDevices() {
            var xDeviceIDs = new HW.PCIBus.DeviceIDs();

            Console.WriteLine(HW.PCIBus.Devices.Length);
            for (int d = 0; d < HW.PCIBus.Devices.Length; d++) {
                var xPCIDevice = HW.PCIBus.Devices[d];
            //foreach(
                string xVendor = xDeviceIDs.FindVendor(xPCIDevice.VendorID);

                if (xVendor == null) {
                    xVendor = xPCIDevice.VendorID.ToHex(4);
                }

                Console.Write(xPCIDevice.Bus + "-" + xPCIDevice.Slot + "-" + xPCIDevice.Function);
                Console.ReadLine();
                Console.Write(" " + xVendor + ":" + xPCIDevice.DeviceID.ToHex(4));
                Console.WriteLine(" Type: " + xPCIDevice.HeaderType + " IRQ: " + xPCIDevice.InterruptLine);
                //                                /*Enum.GetName(typeof(PCIHeaderType), */ xPCIDevice.HeaderType/*) */);
                //                           Console.WriteLine(" Status: " + xPCIDevice.Status + " " +
                //                               /*Enum.GetName(typeof(PCIStatus),  */xPCIDevice.Status/*) */);
                //                            Console.WriteLine(" Command: " + xPCIDevice.Command + " " +
                //                                /*Enum.GetName(typeof(PCICommand), */xPCIDevice.Command /* ) */);
                uint xClass = (UInt32)((xPCIDevice.ClassCode << 8) | xPCIDevice.SubClass);
                System.Console.Write(" Class [" + xClass.ToHex(4) + "] " + xPCIDevice.GetClassInfo());
                System.Console.WriteLine();
                System.Console.Write(" Memory: ");

                for (byte i = 0; i < xPCIDevice.NumberOfBaseAddresses(); i++) {
                    var a = xPCIDevice.GetAddressSpace(i);

                    if (a != null) {
                        System.Console.Write("register " + i + " @ " + a.Offset.ToHex(8) + " (" + a.Size + "b) ");
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

    }
}
