using System;
using Cosmos.Build.Windows;
using Cosmos.Hardware;
using Cosmos.Kernel;
using Cosmos.Sys;

namespace EsxTest
{
    class Program
    {
        static bool Test = false;

        #region Cosmos Builder logic
        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        static void Main(string[] args)
        {
            if (!Test)
            {
                BuildUI.Run();
            }
            else
            {
                Init();
            }
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


            //SimpleTest();
            //RandomTest();
            //PciTest();
            HeapTest();
        }


        private static void SimpleTest()
        {
            int i = 0;
            while (true)
            {
                Console.WriteLine(i);
                ++i;
            }
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

        private static UInt32 Size=3000000;

        private unsafe static void HeapTest()
        {
            Console.WriteLine("HeapTest");
            byte[] Memory = new byte[Size];
            fixed (byte* memPtr = &Memory[0])
            {
                byte counter = 0;
                UInt32 testSize;
                byte* testPointer;
                Heap.Init((UInt32) memPtr, Size,65536);
                //Heap.DebugActive = true;
                while (true)
                {
                    ++counter;

                    Console.CursorLeft = 0;
                    Console.CursorTop = 1;
                    HeapCounter.Print();

                    testSize = MTRandom.Next(5000) + 1;
                    testPointer = (byte*)Heap.MemAlloc(testSize);
                    var incPointer = testPointer;
                    for (int i = 0; i < testSize; i++)
                    {
                        (*incPointer) = counter;
                        ++incPointer;
                    }

                    for (int i = 0; i < 500; i++)
                    {
                        incPointer = testPointer;
                        for (int j = 0; j < testSize; j++)
                        {
                            if ((*incPointer) != counter)
                            {
                                Console.WriteLine("Heap failure!");
                                while (true)
                                {

                                }
                            }
                            ++incPointer;
                        }
                        uint index = MTRandom.Next(1000);
                        if (pointer[index]==0)
                        {
                            pointer[index] = Heap.MemAlloc(MTRandom.Next(5000) + 1);
                        }
                        else
                        {
                            Heap.MemFree(pointer[index]);
                            pointer[index] = 0;                            
                        }
                    }
                    Heap.MemFree((UInt32) testPointer);
                }
            }
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
