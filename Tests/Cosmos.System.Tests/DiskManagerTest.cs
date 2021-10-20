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
    class DiskManagerTest
    {

        [SetUp]
        public void Setup()
        {
            DebuggerFactory.WriteToConsole = true;
            var xDevice = new TestBlockDevice();
            var xPartition = new Partition(xDevice, 0, xDevice.BlockCount);
            BlockDevice.Devices.Clear();
            BlockDevice.Devices.Add(xPartition);
            CosmosVFS cosmosVFS = new CosmosVFS();
            VFSManager.RegisterVFS(cosmosVFS, true);
            cosmosVFS.Initialize(false);
        }

        [TearDown]
        public void CleanUp()
        {
            VFSManager.Reset();
        }

        [Test]
        public void TestDiskManagerQuickFormat()
        {
            string driveName = @"C:\";
            var drive = new DiskManager(driveName);
            drive.Format("FAT32", aQuick: true);

            var xDi = new DriveInfo(driveName);

            /* If the drive is emptry all Space should be free */
            Assert.IsTrue(DriveInfoImpl.get_TotalSize(xDi) == DriveInfoImpl.get_TotalFreeSpace(xDi));

            /* Let's try to create a new file on the Root Directory */
            FileStreamImpl.InitializeStream(@"C:\newFile.txt", FileMode.Create);

            Assert.IsTrue(CosmosFileSystem.FileExists(@"C:\newFile.txt"), "Failed to create new file after disk format");

            DirectoryImpl.CreateDirectory(@"C:\SYS");
            Assert.IsTrue(DirectoryImpl.GetDirectories(@"C:\SYS").Length == 0, "Can create new empty directory");
        }
    }
}
