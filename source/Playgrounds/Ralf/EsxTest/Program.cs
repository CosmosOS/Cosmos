using System;
using Cosmos.Build.Windows;
using Cosmos.Hardware;
using Cosmos.Kernel;
using Cosmos.Sys;

namespace EsxTest
{
    unsafe class Program
    {
        static bool Test = false;

        #region Cosmos Builder logic
        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        static void Main(string[] args)
        {
            //if (!Test)
            //{
                BuildUI.Run();
            //}
            //else
            //{
            //    Init();
            //}
        }

        #endregion

        // Main entry point of the kernel
        public static void Init()
        {
            
            if (!Test)
            {
//                Cosmos.Sys.Boot.Default();
                var xBoot = new Cosmos.Sys.Boot();
                xBoot.Execute();

            }


            //RandomTest();
            //PciTest();
            HeapTest();
        }



        private static void RandomTest()
        {
            while (true)
            {
                Console.WriteLine(MTRandom.Next(100));
            }
        }

        private static UInt32[] pointer = new UInt32[1000];

        //private static byte* memPtr;

        private static UInt32 Size=200000;

        private static void HeapTest()
        {
            Console.WriteLine("HeapTest");
            byte[] Memory = new byte[Size];
//            memPtr =
//                (byte*)Cosmos.Kernel.Heap.MemAlloc(HeaderSize);

            fixed (byte* memPtr = &Memory[0])
            {
                Heap.Init((UInt32) memPtr, Size);
                Heap.DebugActive = false;
                UInt32 p0 = 0;
                UInt32 p1 = 0;
                UInt32 p2 = 0;
                UInt32 p3 = 0;

                while (true)
                {
                    Console.Clear();
                    HeapCounter.Print();
                    for (int i = 0; i < 1000; i++)
                    {
                        uint index = MTRandom.Next(1000);
                        if (pointer[index]==0)
                        {
                            pointer[index] = Heap.MemAlloc(MTRandom.Next(100) + 1);
                        }
                        else
                        {
                            Heap.MemFree(pointer[index]);
                            pointer[index] = 0;                            
                        }
                        
                    }

//                    //Heap.DebugActive = true;
//                    for (int i = 0; i < 4; i++)
//                    {
//                        UInt32 n = MTRandom.Next(20) + 1;
//                        switch (i)
//                        {
//                            case 0:
//                                p0 = Heap.MemAlloc(n);
//                                break;
//                            case 1:
//                                p1 = Heap.MemAlloc(n);
//                                break;
//                            case 2:
//                                p2 = Heap.MemAlloc(n);
//                                break;
//                            case 3:
//                                p3 = Heap.MemAlloc(n);
//                                break;
//                        }
//                    }
//                    for (int i = 0; i < 4; i++)
//                    {
//                        switch (i)
//                        {
//                            case 0:
//                                Heap.MemFree(p0);
//                                break;
//                            case 1:
//                                Heap.MemFree(p1);
//                                break;
//                            case 2:
//                                Heap.MemFree(p2);
//                                break;
//                            case 3:
//                                Heap.MemFree(p3);
//                                break;
//                        }
//                    }
                }
            }

            //            Console.WriteLine("Press Enter for Reboot");
            //            Console.ReadLine();
            //            Deboot.Reboot();
        }

        private static void PciTest()
        {
            PCIBus.Init();
            var deviceIDs = new PCIBus.DeviceIDs();
            foreach (var device in PCIBus.Devices)
            {
                if (deviceIDs.FindVendor(device.VendorID) != null)
                {
                    Console.WriteLine("Vendor: " + deviceIDs.FindVendor(device.VendorID));
                }
                else
                {
                    Console.WriteLine("Vendor: unknown");
                }
                if (deviceIDs.FindDevice(device.VendorID,device.DeviceID) != null)
                {
                    Console.WriteLine("Device: " + deviceIDs.FindDevice(device.VendorID, device.DeviceID));
                }
                else
                {
                    Console.WriteLine("Device: unknown 0x" + device.VendorID.ToHex(4)+device.DeviceID.ToHex(4));
                }
                Console.WriteLine("VendorID: 0x" + device.VendorID.ToHex(4));
                Console.WriteLine("DeviceID: 0x" + device.DeviceID.ToHex(4));
                Console.WriteLine("Type: " + device.HeaderType);
                Console.WriteLine("IRQ: " + device.InterruptLine);
                Console.WriteLine("Classcode: " + device.ClassCode);
                Console.WriteLine("SubClass:  " + device.SubClass);
                Console.WriteLine(device.GetClassInfo());
                Console.ReadLine();
            }
            Console.WriteLine("No more devices");
            Console.WriteLine("Press Enter for Reboot");
            Console.ReadLine();
            Deboot.Reboot();
        }
    }
}
