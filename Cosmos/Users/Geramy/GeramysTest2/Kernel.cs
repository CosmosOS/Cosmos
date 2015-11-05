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
            Cosmos.Core.IOGroup.ATA ataOne = Cosmos.Core.Global.BaseIOGroups.ATA1;
            var xATA = new Cosmos.Hardware.BlockDevice.AtaPio(ataOne, Cosmos.Hardware.BlockDevice.Ata.ControllerIdEnum.Primary, Cosmos.Hardware.BlockDevice.Ata.BusPositionEnum.Master);
            Console.WriteLine(Cosmos.Hardware.BlockDevice.BlockDevice.Devices == null ? "BlockDevice, Devices List is null" : "BlockDevice, Devices List isn't null");
            for (int i = 0; i < Cosmos.Hardware.BlockDevice.BlockDevice.Devices.Count; i++) {
                var xDevice = Cosmos.Hardware.BlockDevice.BlockDevice.Devices[i];
              if (xDevice is AtaPio) {
                xATA = (AtaPio)xDevice;
              }
            }
            Console.WriteLine();
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
            if (xPartition != null)
            {
                Console.WriteLine();
                Console.WriteLine("--------------------------");

                Console.WriteLine("Partition found.");
                var xFS = new FatFileSystem(xPartition);

                Console.WriteLine();
                Console.WriteLine("BytesPerSector : " + xFS.BytesPerSector.ToString());
                Console.WriteLine("SectorsPerCluster : " + xFS.SectorsPerCluster.ToString());
                Console.WriteLine("BytesPerCluster : " + xFS.BytesPerCluster.ToString());
    
                Console.WriteLine("ReservedSectorCount : " + xFS.ReservedSectorCount.ToString());
                Console.WriteLine("TotalSectorCount : " + xFS.TotalSectorCount.ToString());
                Console.WriteLine("ClusterCount : " + xFS.ClusterCount.ToString());
    
                Console.WriteLine("NumberOfFATs : " + xFS.NumberOfFATs.ToString());
                Console.WriteLine("FatSectorCount : " + xFS.FatSectorCount.ToString());

                Console.WriteLine("RootSector : " + xFS.RootSector.ToString());
                Console.WriteLine("RootSectorCount : " + xFS.RootSectorCount.ToString());
                Console.WriteLine("RootCluster : " + xFS.RootCluster.ToString());
                Console.WriteLine("RootEntryCount : " + xFS.RootEntryCount.ToString());

                Console.WriteLine("DataSector : " + xFS.DataSector.ToString());
                Console.WriteLine("DataSectorCount : " + xFS.DataSectorCount.ToString());
            }
            else
            {
                Console.WriteLine("Partition not found.");
            }

            Console.WriteLine();
            Console.WriteLine("--------------------------");
            Console.Write("Pausing... (Press enter to continue.)");
            Console.ReadLine();
        }
    }
}
