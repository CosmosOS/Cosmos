using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SentinelKernel.System.FileSystem.VFS;
using Sys = Cosmos.System;

namespace SentinelKernel
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully.");
           VFSManager.RegisterVFS(new System.FileSystem.VFS.SentinelVFS());
        }

        protected override void Run()
        {
            bool xTest = Directory.Exists("0:\\test");
            Stop();
        }

        /*
protected void InitializeHardware()
{
    Console.WriteLine();
    Console.WriteLine("Initializing hardware...");
    if (BlockDevice.Devices.Count > 0)
    {
        Console.WriteLine("Block devices found: " + BlockDevice.Devices.Count);
        InitializeATADevices();
        if (this.mATA != null)
        {
            InitializePartitions();
        }
    }
    else
    {
        Console.WriteLine("No block devices found!");
    }
    Console.WriteLine("Complete...");
    Console.WriteLine("Press enter.");
    Console.ReadLine();
}
*/

        /*
        protected void InitializeATADevices()
        {
            Console.WriteLine("Initializing ATA Dedices...");
            try
            {
                for (int i = 0; i < BlockDevice.Devices.Count; i++)
                {
                    if (BlockDevice.Devices[i] is AtaPio)
                    {
                        this.mATA = (AtaPio)BlockDevice.Devices[i];
                        break;
                    }
                }

                if (mATA != null)
                {
                    Console.WriteLine();
                    Console.WriteLine("--------ATA Devices-------");
                    Console.WriteLine("Type: " + (mATA.DriveType == AtaPio.SpecLevel.ATA ? "ATA" : "ATAPI"));
                    Console.WriteLine("Serial No: " + mATA.SerialNo);
                    Console.WriteLine("Firmware Rev: " + mATA.FirmwareRev);
                    Console.WriteLine("Model No: " + mATA.ModelNo);
                    Console.WriteLine("Block Size: " + mATA.BlockSize + " bytes");
                    Console.WriteLine("Size: " + mATA.BlockCount * mATA.BlockSize / 1024 / 1024 + " MB");
                    Console.WriteLine("--------------------------");
                }
                else
                {
                    Console.WriteLine("No ATA devices found!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }
            Console.WriteLine("Complete...");
            Console.WriteLine("Press enter.");
            Console.ReadLine();
        }
        */
    }
}