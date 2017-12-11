using System;
using Cosmos.HAL;
using Cosmos.Debug.Kernel;
using Cosmos.HAL.BlockDevice;

namespace Cosmos.HAL.Drivers.PCI.Controllers
{
    public class AHCI
    {
        internal Debugger mAHCIDebugger = new Debugger("HAL", "AHCI");

        public AHCI(uint aABARAddress)
        {
            var xSATA = new SATA(aABARAddress);
            var xMBRData = new byte[512];
            Console.WriteLine("Before");
            xSATA.ReadBlock(0UL, 1U, xMBRData);
            Console.WriteLine(xMBRData[0]);
            Console.WriteLine(xMBRData[1]);
            Console.WriteLine(xMBRData[2]);
            Console.WriteLine(xMBRData[3]);
            Console.WriteLine(xMBRData[4]);
            Console.WriteLine("Done reading");
            var xMBR = new MBR(xMBRData);

            if (xMBR.EBRLocation != 0)
            {
                // EBR Detected!
                var xEBRData = new byte[512];
                xSATA.ReadBlock(xMBR.EBRLocation, 1U, xEBRData);
                var xEBR = new EBR(xEBRData);
                for (int i = 0; i < xEBR.Partitions.Count; i++)
                {
                    //var xPart = xEBR.Partitions[i];
                    //var xPartDevice = new Partition(xSATA, xPart.StartSector, xPart.SectorCount);
                    //Devices.Add(xPartDevice);
                }
            }

            mAHCIDebugger.Send("Number of MBR partitions found: ");
            mAHCIDebugger.SendNumber(xMBR.Partitions.Count);
            for (int i = 0; i < xMBR.Partitions.Count; i++)
            {
                var xPart = xMBR.Partitions[i];
                if (xPart == null)
                {
                    Console.WriteLine("Null partition found at idx: " + i);
                }
                else
                {
                    var xPartDevice = new Partition(xSATA, xPart.StartSector, xPart.SectorCount);
                    BlockDevice.BlockDevice.Devices.Add(xPartDevice);
                    Console.WriteLine("Found partition at idx: " + i);
                }
            }
        }
    }
}
