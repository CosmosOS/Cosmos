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
            Cosmos.Sys.Boot.Default();
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
