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
            if (xDevice.DeviceExists == true)
            {
                Console.WriteLine("ATA Primary Master");
                Initialize(ATA.ControllerIdEnum.Primary, ATA.BusPositionEnum.Master);
                Console.ReadKey(true);
                Console.WriteLine("ATA Primary Slave");
                Initialize(ATA.ControllerIdEnum.Primary, ATA.BusPositionEnum.Slave);
                Console.ReadKey(true);
                Console.WriteLine("ATA Secondary Master");
                Initialize(ATA.ControllerIdEnum.Secondary, ATA.BusPositionEnum.Master);
                Console.ReadKey(true);
                Console.WriteLine("ATA Secondary Slave");
                Initialize(ATA.ControllerIdEnum.Secondary, ATA.BusPositionEnum.Slave);
                Console.ReadKey(true);
            }
        }

        private static void Initialize(ATA.ControllerIdEnum aControllerID, ATA.BusPositionEnum aBusPosition)
        {
            var xIO = aControllerID == ATA.ControllerIdEnum.Primary ? Core.Global.BaseIOGroups.ATA1 : Core.Global.BaseIOGroups.ATA2;
            var xATA = new ATA_PIO(xIO, aControllerID, aBusPosition);
            if (xATA.DriveType == ATA_PIO.SpecLevel.Null)
            {
                //ATA.ATADebugger.Send
                Console.WriteLine("No ATA device found @ " + aControllerID + ", " + aBusPosition);
                return;
            }
            else if (xATA.DriveType == ATA_PIO.SpecLevel.ATA)
            {
                BlockDevice.Devices.Add(xATA);
                //ATA.ATADebugger.Send
                Console.WriteLine("ATA device with speclevel ATA found @ " + aControllerID + ", " + aBusPosition);

                #region ATA_MBR
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
                ATA.ATADebugger.Send("Number of MBR partitions found:");
                ATA.ATADebugger.SendNumber(xMBR.Partitions.Count);
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
                        BlockDevice.Devices.Add(xPartDevice);
                        Console.WriteLine("Found partition at idx: " + i);
                    }
                }
                #endregion

            }
            else if (xATA.DriveType == ATA_PIO.SpecLevel.ATAPI)
            {
                BlockDevice.Devices.Add(xATA);
                ATAPI.ATAPIDevices.Add(xATA);
                //ATA.ATADebugger.Send
                Console.WriteLine("ATA device with speclevel ATAPI found @ " + aControllerID + ", " + aBusPosition + ". Support WIP");
            }
            
        }
    }
}
