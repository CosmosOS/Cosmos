using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL.Drivers.PCI.Controllers;
using Cosmos.HAL.Drivers.PCI.Video;

namespace Cosmos.HAL.Drivers
{
    public abstract class PCIDriver
    {
        internal abstract PCIDevice GetDevice();

        public static void InitializeAll()
        {
            IDE.InitDriver();
            //AHCI.InitDriver();
            //SCSI.InitDriver();
            //RAID.InitDriver();
            //EHCI.InitDriver();
            VMWareSVGAII.InitDriver();
            
            // Class 0x01 = Mass Storage Controllers' Class code
            // Subclass 0x00 = Subclass code of SCSI Controller
            // Subclass 0x01 = Subclass code of IDE Controller
            // Subclass 0x04 = Subclass code of RAID Controller
            // Subclass 0x06 = Subclass code of AHCI Controller

            if (HAL.PCI.GetDeviceClass(0x01, 0x00) != null)
            {
                Global.mDebugger.Send("SCSI isn't supported yet");
                Console.WriteLine("SCSI Controller not supported yet");
                if(HAL.PCI.GetDeviceClass(0x01, 0x01) == null)
                {
                    Console.WriteLine("Booting without ATA Initialization");
                    Console.WriteLine("FAT cannot be used while ATA isn't initialized");
                }
            }
            else if (HAL.PCI.GetDeviceClass(0x01, 0x04) != null)
            {
                Global.mDebugger.Send("RAID isn't supported yet");
                Console.WriteLine("RAID Controller is not supported yet");
                Console.WriteLine("Booting without ATA Initialization");
                Console.WriteLine("FAT cannot be used while ATA isn't initialized");
                if (HAL.PCI.GetDeviceClass(0x01, 0x01) == null)
                {
                    Console.WriteLine("Booting without ATA Initialization");
                    Console.WriteLine("FAT cannot be used while ATA isn't initialized");
                }
            }
            else if (HAL.PCI.GetDeviceClass(0x01, 0x06) != null)
            {
                Global.mDebugger.Send("AHCI isn't supported yet");
                Console.WriteLine("AHCI Controller is not supported yet");
                if (HAL.PCI.GetDeviceClass(0x01, 0x01) == null)
                {
                    Console.WriteLine("Booting without ATA Initialization");
                    Console.WriteLine("FAT cannot be used while ATA isn't initialized");
                }
            }
            else
            {
                Console.WriteLine("Booting without ATA Initialization");
                Console.WriteLine("FAT cannot be used while ATA isn't initialized");
            }
        }
    }
}
