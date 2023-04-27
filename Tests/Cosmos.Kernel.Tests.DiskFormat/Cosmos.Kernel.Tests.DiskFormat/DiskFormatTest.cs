using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;
using System;
using Cosmos.System.FileSystem;
using Cosmos.HAL.BlockDevice;

namespace Cosmos.Kernel.Tests.DiskFormat
{
    public class DiskFormatTest
    {
        /// <summary>
        /// Tests DiskManager.
        /// </summary>
        public static void Execute(Debugger mDebugger)
        {
            string driveName = @"0:\";
            Disk ourDisk = new Disk(Ata.Devices[0]);
            MBR ourMBR = null;
            
            //How to really test this? I fear the other tests relies on the fact that there are files on 0:

            mDebugger.Send("START TEST: Format");

            ourMBR = new MBR(ourDisk.Host);

            ourMBR.CreateMBR(ourDisk.Host);

            ourMBR.WritePartitionInformation(new Partition(ourDisk.Host, 512, ourDisk.Host.BlockCount - 1024), 0);

            ourDisk.Mount();

            ourDisk.FormatPartition(0, "FAT32", true);

            mDebugger.Send("Format done testing HDD is really empty");

            var xDi = new DriveInfo(driveName);

            //If the drive is empty all Space should be free
            Assert.IsTrue(xDi.TotalSize == xDi.TotalFreeSpace, "DiskManager.Format (quick) failed TotalFreeSpace is not the same of TotalSize");

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
