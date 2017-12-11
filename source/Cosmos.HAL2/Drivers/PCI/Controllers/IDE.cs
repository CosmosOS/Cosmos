using System;
using Cosmos.HAL.BlockDevice;

namespace Cosmos.HAL.Drivers.PCI.Controllers
{
    public class IDE
    {
        public static void InitAta(Ata.ControllerIdEnum aControllerID, Ata.BusPositionEnum aBusPosition)
        {
            var xIO = aControllerID == Ata.ControllerIdEnum.Primary ? Core.Global.BaseIOGroups.ATA1 : Core.Global.BaseIOGroups.ATA2;
            var xATA = new AtaPio(xIO, aControllerID, aBusPosition);
            if (xATA.DriveType == AtaPio.SpecLevel.Null)
            {
                return;
            }
            if (xATA.DriveType == AtaPio.SpecLevel.ATA)
            {
                BlockDevice.BlockDevice.Devices.Add(xATA);
                Ata.AtaDebugger.Send("ATA device with speclevel ATA found.");
            }
            else
            {
                //Ata.AtaDebugger.Send("ATA device with spec level " + (int)xATA.DriveType +
                //                     " found, which is not supported!");
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
                    //var xPart = xEBR.Partitions[i];
                    //var xPartDevice = new BlockDevice.Partition(xATA, xPart.StartSector, xPart.SectorCount);
                    //BlockDevice.BlockDevice.Devices.Add(xPartDevice);
                }
            }

            // TODO Change this to foreach when foreach is supported
            Ata.AtaDebugger.Send("Number of MBR partitions found:");
            Ata.AtaDebugger.SendNumber(xMBR.Partitions.Count);
            for (int i = 0; i < xMBR.Partitions.Count; i++)
            {
                var xPart = xMBR.Partitions[i];
                if (xPart == null)
                {
                    Console.WriteLine("Null partition found at idx: " + i);
                }
                else
                {
                    var xPartDevice = new Partition(xATA, xPart.StartSector, xPart.SectorCount);
                    BlockDevice.BlockDevice.Devices.Add(xPartDevice);
                    Console.WriteLine("Found partition at idx" + i);
                }
            }
        }
    }
}
