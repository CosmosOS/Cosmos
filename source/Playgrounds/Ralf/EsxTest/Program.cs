using System;
using System.Threading;
using Cosmos.Build.Windows;
using Cosmos.Hardware;
using Cosmos.Kernel;
using Cosmos.Sys;

namespace EsxTest
{
    class Program
    {
        #region Cosmos Builder logic
        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        static void Main(string[] args)
        {
            BuildUI.Run();
        }

        #endregion

        // Main entry point of the kernel
        public static void Init()
        {
//            bool xTest = false;
//            if (xTest)
//            {
//                Cosmos.Sys.Boot.Default();
//            }
            var xBoot = new Cosmos.Sys.Boot();
            xBoot.Execute();

            RandomTest();
            //PciTest();
            //HeapTest();
        }

        /// <summary>
        /// http://en.wikipedia.org/wiki/Mersenne_Twister
        /// The following generates uniformly 32-bit integers in the range [0, 2^32 − 1] with the MT19937 algorithm:
        /// 
        ///        // Create a length 624 array to store the state of the generator
        ///         int[0..623] MT
        ///         int index = 0
        ///         
        ///         // Initialize the generator from a seed
        ///         function initializeGenerator(int seed) {
        ///             MT[0] := seed
        ///             for i from 1 to 623 { // loop over each other element
        ///                 MT[i] := last 32 bits of(1812433253 * (MT[i-1] xor (right shift by 30 bits(MT[i-1]))) + i) // 0x6c078965
        ///             }
        ///         }
        ///         
        ///         // Extract a tempered pseudorandom number based on the index-th value,
        ///         // calling generateNumbers() every 624 numbers
        ///         function extractNumber() {
        ///             if index == 0 {
        ///                 generateNumbers()
        ///             }
        ///             
        ///             int y := MT[index]
        ///             y := y xor (right shift by 11 bits(y))
        ///             y := y xor (left shift by 7 bits(y) and (2636928640)) // 0x9d2c5680
        ///             y := y xor (left shift by 15 bits(y) and (4022730752)) // 0xefc60000
        ///             y := y xor (right shift by 18 bits(y))
        ///             
        ///             index := (index + 1) mod 624
        ///             return y
        ///         }
        ///         
        ///         // Generate an array of 624 untempered numbers
        ///         function generateNumbers() {
        ///             for i from 0 to 623 {
        ///                 int y := 32nd bit of(MT[i]) + last 31 bits of(MT[(i+1) mod 624])
        ///                 MT[i] := MT[(i + 397) mod 624] xor (right shift by 1 bit(y))
        ///                 if (y mod 2) == 1 { // y is odd
        ///                     MT[i] := MT[i] xor (2567483615) // 0x9908b0df
        ///                 }
        ///             }
        ///         }    
        /// 
        /// 
        /// 
        /// 
        /// 
        /// </summary>
        public static class MTRandom
        {
            private static readonly UInt32[] MT = new UInt32[624];
            private static int index;
            private static bool Initialized;

            internal static void Initialize(UInt32 seed)
            {
                MT[0] = seed;
                for (int i = 1; i < 624; i++)
                {
                    MT[i] = (UInt32)(1812433253 * (MT[i - 1] ^ (MT[i - 1] >> 30)) + i);
                }
                Initialized = true;
            }

            private static void GenerateNumbers()
            {
                for (int i = 0; i < 624; i++)
                {
                    UInt32 y = MT[i] + (MT[i + 1] % 624) & 0x7FFFFFFF;
                    MT[i] = MT[(i + 397) % 624] ^ (y >> 1);
                    if ((y % 2) == 1)
                    {
                        MT[i] = (MT[i] ^ 0x9908b0df);
                    }
                }
            }

            internal static UInt32 Next()
            {
                if (!Initialized)
                {
                    Initialize((UInt32)(RTC.GetHours() * 60 * 60 + RTC.GetMinutes() * 60 + RTC.GetSeconds()));
                }
                if (index == 0)
                {
                    GenerateNumbers();
                }
                UInt32 y = MT[index];
                y = y ^ (y >> 11);
                y = (y ^ ((y << 7) & 2636928640));
                y = (y ^ ((y << 15) & 0xefc60000));
                y = y ^ (y >> 18);
                return y;
            }

            internal static UInt32 Next(UInt32 maxValue)
            {
                return Next() % maxValue;
            }
        }


        private static void RandomTest()
        {
            while (true)
            {
                Console.WriteLine(MTRandom.Next()%100);
            }
        }

        private static UInt32[] pointer = new UInt32[1000];

//        private static void HeapTest()
//        {
//            Console.WriteLine("HeapTest");
//            
//
//            UInt32 ptr;
//            while (true)
//            {
//                //Heap.DebugActive = true;
//                for (int i = 0; i < 10; i++)
//                {
//                    UInt32 n = (UInt32) MTRandom.Next(1024*1024) + 1;                    
//                    //UInt32 n = (UInt32) i + 1;
//                    pointer[i] = Heap.MemAlloc(n);
//                    Console.Clear();
//                    HeapCounter.Print();
//                    
//                }
//                for (int i = 0; i < 10; i++)
//                {
//                    Heap.MemFree(pointer[i]);
//                    Console.Clear();
//                    HeapCounter.Print();
//                }
//
//            }
//
//            Console.WriteLine("Press Enter for Reboot");
//            Console.ReadLine();
//            Deboot.Reboot();
//        }

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
