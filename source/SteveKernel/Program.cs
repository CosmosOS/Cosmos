using System;
using Cosmos.Build.Windows;
using Cosmos.Driver.RTL8139;
using Cosmos.Hardware.PC.Bus;

namespace SteveKernel
{
    class Program
    {
        #region Cosmos Builder logic
        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        static void Main(string[] args)
        {
            var xBuilder = new Builder();
            xBuilder.Build();
        }
        #endregion




        // Main entry point of the kernel
        public static void Init()
        {
            Console.WriteLine("Done booting");


			Cosmos.Kernel.Boot.Default();
            //Cosmos.Hardware.PC.Bus.PCIBus.DebugLSPCI();


            UInt32 j = 0xfffffff0; Console.WriteLine(PCIBus.ToHex(j,8));
            UInt32 k = 0x44324441; Console.WriteLine(PCIBus.ToHex(k,8));
            Console.WriteLine(PCIBus.ToHex(j & k,8));
            Console.ReadLine();
            RTL8139 rtl = RTL8139.FindDevices()[0] as RTL8139;
            if (rtl != null)
                Console.WriteLine("got rtl");
            
            PCIDeviceNormal rtlpci = PCIBus.GetPCIDevice(0, 3, 0) as PCIDeviceNormal;
            if (rtlpci != null)
                Console.WriteLine("got rtl pci");

            AddressSpace mas = rtlpci.GetAddressSpace(1) as AddressSpace;
            if (mas != null)
                Console.WriteLine("got as");
            MemoryAddressSpace mmas = mas as MemoryAddressSpace;

            if (mmas != null)
                Console.WriteLine("got mas");
            Console.WriteLine(mas.Size.ToString());

            Console.ReadLine();
            for (uint i = 0; i < 256; i++) 
                Console.Write(PCIBus.ToHex(mmas.Read8Unchecked(i),2) +" ");


            while (true)
                ;
        }
    }
}