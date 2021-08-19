using System;
using System.Collections.Generic;
using Cosmos.HAL.BlockDevice;

namespace Cosmos.HAL.BlockDevice
{
    public class IDE
    {
        private static PCIDevice xDevice = HAL.PCI.GetDeviceClass(HAL.ClassID.MassStorageController,
                                                                  HAL.SubclassID.IDEInterface);

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

                //Add in the atapi devices
                foreach (var item in ATAPIPartions)
                {
                    BlockDevice.Devices.Add(item);
                }
            }
        }
        private static List<Partition> ATAPIPartions = new List<Partition>();
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
                ATAPIPartions.Add(new Partition(atapi, 0, 1000000));
                Ata.AtaDebugger.Send("ATA device with speclevel ATAPI found");
                return;
            }

            if(GPT.IsGPTPartition(xATA))
            {
                var xGPT = new GPT(xATA);

                Ata.AtaDebugger.Send("Number of GPT partitions found:");
                Ata.AtaDebugger.SendNumber(xGPT.Partitions.Count);
                for (int i = 0; i < xGPT.Partitions.Count; i++)
                {
                    var xPart = xGPT.Partitions[i];
                    if (xPart == null)
                    {
                        Console.WriteLine("Null partition found at idx: " + i);
                    }
                    else
                    {
                        var xPartDevice = new Partition(xATA, xPart.StartSector, xPart.SectorCount);
                        BlockDevice.Devices.Add(xPartDevice);
                        Console.WriteLine("Found partition at idx: " + i);
                    }
                }
            }
            else
            {
                var xMbrData = new byte[512];
                xATA.ReadBlock(0UL, 1U, ref xMbrData);
                var xMBR = new MBR(xMbrData);

                if (xMBR.EBRLocation != 0)
                {
                    //EBR Detected
                    var xEbrData = new byte[512];
                    xATA.ReadBlock(xMBR.EBRLocation, 1U, ref xEbrData);
                    var xEBR = new EBR(xEbrData);

                    for (int i = 0; i < xEBR.Partitions.Count; i++)
                    {
                        //var xPart = xEBR.Partitions[i];
                        //var xPartDevice = new BlockDevice.Partition(xATA, xPart.StartSector, xPart.SectorCount);
                        //BlockDevice.BlockDevice.Devices.Add(xPartDevice);
                    }
                }
                Ata.AtaDebugger.Send("Number of MBR partitions found:");
                Ata.AtaDebugger.SendNumber(xMBR.Partitions.Count);
                int partNumb = 0;
                foreach (var part in xMBR.Partitions)
                {
                    if (part == null)
                    {
                        Console.WriteLine("Null partition found at idx: " + partNumb);
                    }
                    else
                    {
                        var xPartDevice = new Partition(xATA, part.StartSector, part.SectorCount);
                        BlockDevice.Devices.Add(xPartDevice);
                        Console.WriteLine("Found partition at idx: " + partNumb);
                    }
                    partNumb++;
                }
            }
        }
    }
}
