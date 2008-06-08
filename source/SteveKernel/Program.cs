using System;
using Cosmos.Build.Windows;
using Cosmos.Hardware.PC.Bus;
using Cosmos.FileSystem;
using Cosmos.Hardware;
using System.Diagnostics;

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
            Cosmos.Sys.Boot.Default();
            Console.WriteLine("Done booting");


            Console.WriteLine("looking for devices");
            Debugger.Break();
            Cosmos.Hardware.Storage.ATA2.ATA.Initialize(Cosmos.Hardware.Global.Sleep);

            Console.WriteLine("looking for mbr");

            for (int i = 0; i < Cosmos.Hardware.Device.Devices.Count; i++)
            {
                Device dev = Cosmos.Hardware.Device.Devices[i];


                if (dev is Disk)
                {
                    Disk bd = dev as Disk;
                    MBR mbr = new MBR(bd);

                    Console.WriteLine("WARNING: ABOUT TO REWRITE MBR OF " + bd.Name);
                    Console.ReadLine();

                    mbr.DiskSignature = 0x12345678;
                    mbr.Partition[0].StartLBA = 1;
                    mbr.Partition[0].LengthLBA = (uint)(bd.BlockCount - 1);
                    mbr.Partition[0].PartitionType = 0x0c;
                    mbr.Partition[0].Bootable = true;

                    mbr.Save();

                    Console.WriteLine("wrote a fat partition of size + " + (mbr.Partition[0].LengthLBA) * 512); ;


                    Console.ReadLine();
                }
                else
                {

                    Console.WriteLine("skipping " +dev.Name);
                }
            }

            
			System.Diagnostics.Debugger.Break();
            // Kudzu: Moved to Kudzu.PCITest - maybe should go somehwere common, but dont want debug
            // in main Cosmos code
            //Cosmos.Hardware.PC.Bus.PCIBus.DebugLSPCI();
            
            
            PCIDeviceNormal rtlpci = PCIBus.GetPCIDevice(0, 3, 0) as PCIDeviceNormal;
            if (rtlpci != null)
                Console.WriteLine("got rtl pci");

            Cosmos.Kernel.MemoryAddressSpace mas = rtlpci.GetAddressSpace(1) as Cosmos.Kernel.MemoryAddressSpace;
            if (mas != null)
                Console.WriteLine("got mas");
            
            Console.WriteLine("dumping memory space:");
            
            //for (uint i = 0; i < mas.Size; i++) 
                // Conver to extensino method as per your commetns. :)
                //Console.Write(PCIBus.ToHex(mas.Read8Unchecked(i),2) +" ");


            while (true)
                ;
        }
    }
}