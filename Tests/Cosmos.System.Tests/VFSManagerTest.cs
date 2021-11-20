using Cosmos.Debug.Kernel;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.FAT;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.Tests
{
    class VFSManagerTest
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
        public void Test_Disk_Manager()
        {
            var disks = VFSManager.GetDisks();
            Assert.IsTrue(disks.Count != 0);
            foreach (var item in disks)
            {
                Assert.IsTrue(item.Size != 0);
                Assert.IsTrue(item.Partitions.Count != 0);
            }

            const string root = @"C:\";
            long initialSize = VFSManager.GetTotalSize(root);
            VFSManager.Format("C", "FAT32", true);
            Assert.AreEqual(initialSize, VFSManager.GetAvailableFreeSpace(root));
            Assert.AreEqual(0, VFSManager.GetDirectoryListing(root).Count);
            VFSManager.CreateFile(root + "test.txt");
            Assert.IsNotNull(VFSManager.GetFile(root + "test.txt"));
        }

        [Test]
        public void Test_Disk_Manager_Reformating()
        {
            const string root = @"C:\";
            long initialSize = VFSManager.GetTotalSize(root);
            VFSManager.Format("C", "FAT32", true);
            Assert.AreEqual(initialSize, VFSManager.GetAvailableFreeSpace(root));
            Assert.AreEqual(0, VFSManager.GetDirectoryListing(root).Count);
            VFSManager.CreateFile(root + "test.txt");
            VFSManager.CreateFile(root + "test1.txt");
            VFSManager.CreateFile(root + "test2.txt");
            VFSManager.CreateFile(root + "test3.txt");
            VFSManager.CreateFile(root + "test4.txt");
            VFSManager.CreateFile(root + "newfile.txt");
            VFSManager.CreateDirectory(root + "SubDir");
            VFSManager.CreateFile(root + "SubDir\\file.txt");
            Assert.IsNotNull(VFSManager.GetFile(root + "SubDir\\file.txt"));
            List<DirectoryEntry> lists = VFSManager.GetDirectoryListing(root);
            Assert.AreEqual(7, lists.Count);
            Assert.AreEqual(DirectoryEntryTypeEnum.File, lists[0].mEntryType);
            Assert.AreEqual(DirectoryEntryTypeEnum.File, lists[5].mEntryType);
            Assert.AreEqual(DirectoryEntryTypeEnum.Directory, lists[6].mEntryType);
            VFSManager.Format("C", "FAT32", true);
            Assert.AreEqual(0, VFSManager.GetDirectoryListing(root).Count);
            VFSManager.CreateDirectory(root + "dir");
            Assert.IsNull(VFSManager.GetFile(root + "newfile.txt"));
            VFSManager.CreateFile(root + "newfile.txt");
            Assert.IsNotNull(VFSManager.GetFile(root + "newfile.txt"));
            Assert.AreEqual(2, VFSManager.GetDirectoryListing(root).Count);
        }

        [Test]
        public void Test_Disk_Manager_Reformating_First_Directories()
        {
            const string root = @"C:\";
            long initialSize = VFSManager.GetTotalSize(root);
            VFSManager.Format("C", "FAT32", true);
            Assert.AreEqual(initialSize, VFSManager.GetAvailableFreeSpace(root));
            Assert.AreEqual(0, VFSManager.GetDirectoryListing(root).Count);
            VFSManager.CreateDirectory(root + "SubDir");
            VFSManager.CreateFile(root + "SubDir\\filet.txt");
            Assert.IsNotNull(VFSManager.GetFile(root + "SubDir\\filet.txt"));
            Assert.AreEqual(1, VFSManager.GetDirectoryListing(root).Count);
            VFSManager.Format("C", "FAT32", true);
            Assert.AreEqual(0, VFSManager.GetDirectoryListing(root).Count);
            VFSManager.CreateDirectory(root + "dir");
            Assert.AreEqual(1, VFSManager.GetDirectoryListing(root).Count);
            Assert.AreEqual(0, VFSManager.GetDirectoryListing(root + "dir").Count);
        }
    }
}
