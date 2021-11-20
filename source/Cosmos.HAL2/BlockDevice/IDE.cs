using System;
using System.Collections.Generic;
using Cosmos.HAL.BlockDevice;

namespace Cosmos.HAL.BlockDevice
{
    public class IDE
    {
        private static PCIDevice xDevice = HAL.PCI.GetDeviceClass(HAL.ClassID.MassStorageController,
                                                                  HAL.SubclassID.IDEInterface);
        private static List<BlockDevice> ATAPIDevices = new List<BlockDevice>();
        private static List<Partition> ATAPIPartitions = new List<Partition>();
        internal static void InitDriver()
        {
            if (xDevice != null)
            {
                Console.WriteLine("ATA Primary Master");
                Initialize(Ata.ControllerIdEnum.Primary, Ata.BusPositionEnum.Master);
                Console.WriteLine("ATA Primary Slave");
                Initialize(Ata.ControllerIdEnum.Primary, Ata.BusPositionEnum.Slave);
                Console.WriteLine("ATA Secondary Master");
                Initialize(Ata.ControllerIdEnum.Secondary, Ata.BusPositionEnum.Master);
                Console.WriteLine("ATA Secondary Slave");
                Initialize(Ata.ControllerIdEnum.Secondary, Ata.BusPositionEnum.Slave);
            }

            //Add the ATAPI devices
            foreach (var item in ATAPIDevices)
            {
                BlockDevice.Devices.Add(item);
            }
            foreach (var item in ATAPIPartitions)
            {
                Partition.Partitions.Add(item);
            }
        }
        private static void Initialize(Ata.ControllerIdEnum aControllerID, Ata.BusPositionEnum aBusPosition)
        {
            var xIO = aControllerID == Ata.ControllerIdEnum.Primary ? Core.Global.BaseIOGroups.ATA1 : Core.Global.BaseIOGroups.ATA2;
            var xATA = new ATA_PIO(xIO, aControllerID, aBusPosition);
            if (xATA.DriveType == ATA_PIO.SpecLevel.Null)
            {
                return;
            }
            else if (xATA.DriveType == ATA_PIO.SpecLevel.ATA)
            {
                BlockDevice.Devices.Add(xATA);
                Ata.AtaDebugger.Send("ATA device with speclevel ATA found.");
            }
            else if (xATA.DriveType == ATA_PIO.SpecLevel.ATAPI)
            {
                var atapi = new ATAPI(xATA);

                //TODO: Replace 1000000 with proper size once ATAPI driver implements it
                //Add the atapi device to an array so we reorder them to be last
                ATAPIDevices.Add(atapi);
                ATAPIPartitions.Add(new Partition(atapi, 0, 1000000));
                Ata.AtaDebugger.Send("ATA device with speclevel ATAPI found");
                return;
            }

            ScanAndInitPartitions(xATA);
        }

        internal static void ScanAndInitPartitions(BlockDevice device)
        {
            if (GPT.IsGPTPartition(device))
            {
                var xGPT = new GPT(device);

                Ata.AtaDebugger.Send("Number of GPT partitions found:");
                Ata.AtaDebugger.SendNumber(xGPT.Partitions.Count);
                int i = 0;
                foreach (var part in xGPT.Partitions)
                {
                    Partition.Partitions.Add(new Partition(device, part.StartSector, part.SectorCount));
                    Console.WriteLine("Found partition at idx: " + i);
                    i++;
                }
            }
            else
            {
                var mbr = new MBR(device);

                if (mbr.EBRLocation != 0)
                {
                    //EBR Detected
                    var xEbrData = new byte[512];
                    device.ReadBlock(mbr.EBRLocation, 1U, ref xEbrData);
                    var xEBR = new EBR(xEbrData);

                    for (int i = 0; i < xEBR.Partitions.Count; i++)
                    {
                        //var xPart = xEBR.Partitions[i];
                        //var xPartDevice = new BlockDevice.Partition(xATA, xPart.StartSector, xPart.SectorCount);
                        //Partition.Partitions.Add(xATA, xPartDevice);
                    }
                }
                int c = 0;
                foreach (var part in mbr.Partitions)
                {
                    Partition.Partitions.Add(new Partition(device, part.StartSector, part.SectorCount));
                    Console.WriteLine("Found partition at idx: " + c);
                    c++;
                }
            }
        }
    }
}
