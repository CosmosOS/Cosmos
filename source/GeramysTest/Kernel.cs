using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using Cosmos.Hardware.BlockDevice;
using Cosmos.System.Filesystem.FAT;

namespace GeramysTest
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }

        protected override void Run()
        {
            Console.WriteLine("Test");
            Console.ReadLine();
            Cosmos.Core.IOGroup.ATA ataOne = Cosmos.Core.Global.BaseIOGroups.ATA1;
            var xATA = new Cosmos.Hardware.BlockDevice.AtaPio(ataOne, Cosmos.Hardware.BlockDevice.Ata.ControllerIdEnum.Primary, Cosmos.Hardware.BlockDevice.Ata.BusPositionEnum.Master);
            Console.WriteLine(Cosmos.Hardware.BlockDevice.BlockDevice.Devices == null ? "BlockDevice, Devices List is null" : "BlockDevice, Devices Listisnt null");
            for (int i = 0; i < Cosmos.Hardware.BlockDevice.BlockDevice.Devices.Count; i++) {
                var xDevice = Cosmos.Hardware.BlockDevice.BlockDevice.Devices[i];
              if (xDevice is AtaPio) {
                xATA = (AtaPio)xDevice;
              }
            }
            Console.WriteLine("--------------------------");
            Console.WriteLine("Type: " + (xATA.DriveType == AtaPio.SpecLevel.ATA ? "ATA" : "ATAPI"));
            Console.WriteLine("Serial No: " + xATA.SerialNo);
            Console.WriteLine("Firmware Rev: " + xATA.FirmwareRev);
            Console.WriteLine("Model No: " + xATA.ModelNo);
            Console.WriteLine("Block Size: " + xATA.BlockSize + " bytes");
            Console.WriteLine("Size: " + xATA.BlockCount * xATA.BlockSize / 1024 / 1024 + " MB");

            Partition xPartition = null;
            for (int i = 0; i < BlockDevice.Devices.Count; i++) {
              var xDevice = BlockDevice.Devices[i];
              if (xDevice is Partition) {
                xPartition = (Partition)xDevice;
              }
            }
            var xFS = new FatFileSystem(xPartition);

            Console.Write("Input: ");
            string input = Console.ReadLine();
            Console.WriteLine(input);
        }
    }
}
