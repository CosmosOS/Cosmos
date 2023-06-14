using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;
using System;
using Cosmos.System.FileSystem;
using System.Collections.Generic;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;
using Cosmos.HAL.BlockDevice;

namespace Cosmos.Kernel.Tests.DiskManager
{
    public class DiskManagerTest
    {
        /// <summary>
        /// Tests DiskManager.
        /// </summary>
        public static void Execute(Debugger mDebugger)
        {
            string driveName = @"0:\";
            Disk ourDisk = null;
            ManagedPartition ourPart = null;
            foreach (var disk in Kernel.mVFS.GetDisks())
            {
                foreach (var part in disk.Partitions)
                {
                    if (part.RootPath == driveName)
                    {
                        ourDisk = disk;
                        ourPart = part;
                        break;
                    }
                }
            }
            if (ourDisk == null)
            {
                throw new Exception("Failed to find our drive.");
            }

            mDebugger.Send("START TEST: Get Name");

            Assert.IsTrue(ourPart.RootPath == driveName, "ManagedPartition.RootPath failed drive has wrong name");

            mDebugger.Send("END TEST");

            //How to really test this? I fear the other tests relies on the fact that there are files on 0:

            mDebugger.Send("START TEST: Format");

            MBR mbr = new MBR(ourDisk.Host);

            mbr.CreateMBR(ourDisk.Host);

            mbr.WritePartitionInformation(new Partition(ourDisk.Host, 512, ourDisk.Host.BlockCount - 1024), 0);

            ourDisk.Mount();

            ourDisk.FormatPartition(0, "FAT32", true);

            mDebugger.Send("Format done testing HDD is really empty");

            var xDi = new DriveInfo(driveName);

            //If the drive is empty all Space should be free
            //Assert.IsTrue(xDi.TotalSize == xDi.TotalFreeSpace, "DiskManager.Format (quick) failed TotalFreeSpace is not the same of TotalSize");

            //Let's try to create a new file on the Root Directory
            File.Create(@"0:\newFile.txt");

            Assert.IsTrue(File.Exists(@"0:\newFile.txt") == true, "Failed to create new file after disk format");

            mDebugger.Send("END TEST");

            mDebugger.Send("Testing if you can create directories");

            Directory.CreateDirectory(@"0:\SYS\");
            Assert.IsTrue(Directory.GetDirectories(@"0:\SYS\").Length == 0, "Can create a directory and its content is emtpy");

            ourDisk.DeletePartition(0);
            mDebugger.Send("Partion is Deleted");

            mDebugger.Send("END TEST");
        }
    }
}
