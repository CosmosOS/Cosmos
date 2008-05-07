using System;
using Cosmos.Build.Windows;
using Cosmos.Hardware.PC.Bus;

namespace SteveKernel
{
    class Program
    {
        #region Cosmos Builder logic
        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        static void Main(string[] args) {
            BuildUI.Run();
        }
        #endregion




        // Main entry point of the kernel
        public static void Init()
        {
            Console.WriteLine("Done booting");

			Cosmos.Sys.Boot.Default();
            System.Diagnostics.Debugger.Break();
            Cosmos.Hardware.PC.Bus.PCIBus.DebugLSPCI();
            
            
            PCIDeviceNormal rtlpci = PCIBus.GetPCIDevice(0, 3, 0) as PCIDeviceNormal;
            if (rtlpci != null)
                Console.WriteLine("got rtl pci");

            MemoryAddressSpace mas = rtlpci.GetAddressSpace(1) as MemoryAddressSpace;
            if (mas != null)
                Console.WriteLine("got mas");
            
            Console.WriteLine("dumping memory space:");
            
            for (uint i = 0; i < mas.Size; i++) 
                Console.Write(PCIBus.ToHex(mas.Read8Unchecked(i),2) +" ");


            while (true)
                ;
        }
    }
}