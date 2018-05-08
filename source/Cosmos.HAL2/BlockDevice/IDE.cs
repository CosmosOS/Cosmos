using System;
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
                //Console.WriteLine("ATA Primary Slave");
                //Initialize(Ata.ControllerIdEnum.Primary, Ata.BusPositionEnum.Slave);
                Console.WriteLine("ATA Secondary Master");
                Initialize(Ata.ControllerIdEnum.Secondary, Ata.BusPositionEnum.Master);
                //Console.WriteLine("ATA Secondary Slave");
                //Initialize(Ata.ControllerIdEnum.Secondary, Ata.BusPositionEnum.Slave);
            }
        }

        private static void Initialize(Ata.ControllerIdEnum aControllerID, Ata.BusPositionEnum aBusPosition)
        {
            var xIO = aControllerID == Ata.ControllerIdEnum.Primary ? Core.Global.BaseIOGroups.ATA1 : Core.Global.BaseIOGroups.ATA2;
            var xATA = new AtaPio(xIO, aControllerID, aBusPosition);
            if (xATA.DriveType == AtaPio.SpecLevel.Null)
                return;
            else if (xATA.DriveType == AtaPio.SpecLevel.ATA)
            {
                BlockDevice.Devices.Add(xATA);
                Ata.AtaDebugger.Send("ATA device with speclevel ATA found.");
            }
            else if (xATA.DriveType == AtaPio.SpecLevel.ATAPI)
            {
                /*  Not sure how this would effect the Filesystem - should be fine unless drive master-slave options are changed  */
                BlockDevice.Devices.Add(xATA);
                ATAPI.Init();
                ATAPI.Test();
                Ata.AtaDebugger.Send("ATA device with speclevel ATAPI found, Support is currently WIP!");
                return;
            }
            var xMbrData = new byte[512];
            xATA.ReadBlock(0UL, 1U, xMbrData);
            var xMBR = new MBR(xMbrData);

            if (xMBR.EBRLocation != 0)
            {
                //EBR Detected
                var xEbrData = new byte[512];
                xATA.ReadBlock(xMBR.EBRLocation, 1U, xEbrData);
                var xEBR = new EBR(xEbrData);

                for (int i = 0; i < xEBR.Partitions.Count; i++)
                {
                    //  Updated code to match new Partition class, not sure if this code works at all however,
                    //  so I've left it commented out

                    /*
                    var xPart = xEBR.Partitions[i];
                    var xPartDevice = new Partition(xATA, xPart.StartSector, xPart.SectorCount);
                    BlockDevice.Devices.Add(xPartDevice);
                    */
                }
            }

            // TODO: Change this to foreach when foreach is supported
            // Isn't it supported now?
            Ata.AtaDebugger.Send("Number of MBR partitions found:");
            Ata.AtaDebugger.SendNumber(xMBR.Partitions.Count);
            /*
            foreach (MBR.PartInfo partition in xMBR.Partitions)
            {
                if (partition == null)
                {
                    Ata.AtaDebugger.Send("Null partition found! " + partition);
                }
                else
                {
                    var xPartDevice = new Partition(xATA, partition.StartSector, partition.SectorCount);
                    BlockDevice.Devices.Add(xPartDevice);
                    Ata.AtaDebugger.Send("Found partition! " + partition);
                }
            }
            */
            for(int i = 0; i < xMBR.Partitions.Count; i++)
            {
                var xPart = xMBR.Partitions[i];
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
    }
}
