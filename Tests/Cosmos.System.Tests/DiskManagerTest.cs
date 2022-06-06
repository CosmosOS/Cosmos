using Cosmos.Debug.Kernel;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.FAT;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System_Plugs.System.IO;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cosmos.System.Tests
{
    public class DiskManagerTest
    {
        static ManagedPartition ourPart;
        static Disk ourDisk;
        [SetUp]
        public void Setup()
        {
            DebuggerFactory.WriteToConsole = true;
            var xDevice = new TestBlockDevice();
            var xPartition = new Partition(xDevice, 0, xDevice.BlockCount);
            BlockDevice.Devices.Clear();
            Partition.Partitions.Clear();

            BlockDevice.Devices.Add(xDevice);
            Partition.Partitions.Add(xPartition);
            CosmosVFS cosmosVFS = new CosmosVFS();
            VFSManager.RegisterVFS(cosmosVFS, true, true);

            foreach (var disk in VFSManager.GetDisks())
            {
                foreach (var part in disk.Partitions)
                {
                    if (part.RootPath == @"0:\")
                    {
                        ourDisk = disk;
                        ourPart = part;
                        break;
                    }
                }
            }
            if (ourDisk == null)
            {
                throw new Exception("Failed to find our disk.");
            }
        }

        [TearDown]
        public void CleanUp()
        {
            VFSManager.Reset();
        }

        [Test]
        public void Execute()
        {
            string driveName = @"0:\";

            Assert.IsTrue(ourPart.RootPath == driveName, "ManagedPartition.RootPath failed drive has wrong name");

            ourDisk.FormatPartition(0, "FAT32", true);

            var xDi = new DriveInfo(driveName);

            //If the drive is empty all Space should be free
            Assert.IsTrue(xDi.TotalSize == xDi.TotalFreeSpace, "DiskManager.Format (quick) failed TotalFreeSpace is not the same of TotalSize");

            //Let's try to create a new file on the Root Directory
            File.Create(@"0:\newFile.txt");

            Assert.IsTrue(File.Exists(@"0:\newFile.txt") == true, "Failed to create new file after disk format");

            Directory.CreateDirectory(@"0:\SYS\");

            Assert.IsTrue(Directory.GetDirectories(@"0:\SYS\").Length == 0, "Can create a directory and its content is emtpy");
        }
    }
}
