using System.Collections.Generic;
using System.Text;
using Cosmos.Debug.Kernel;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.FAT;
using Cosmos.System.FileSystem.VFS;
using NUnit.Framework;

namespace Cosmos.System.Tests
{
    public class FatFileSystem_Should
    {
        private FatFileSystem mFS;

        [SetUp]
        public void Setup()
        {
            DebuggerFactory.WriteToConsole = true;
            var xDevice = new TestBlockDevice();
            var xPartition = new Partition(xDevice, 0, xDevice.BlockCount);
            var xFactory = new FatFileSystemFactory();
            mFS = (FatFileSystem)xFactory.Create(xPartition, "0:\\", (long)(xPartition.BlockSize * xPartition.BlockCount));
        }

        [Test]
        public void Load_Root_Directory_Entry()
        {
            var xRootDirectory = mFS.GetRootDirectory();
            Assert.NotNull(xRootDirectory);
        }

        [Test]
        public void Create_A_Directory_Entry()
        {
            string xNewDirectoryEntryName = "NEW";

            var xRootDirectory = mFS.GetRootDirectory();
            Assert.NotNull(xRootDirectory);

            var xRootDirectoryListing = mFS.GetDirectoryListing(xRootDirectory);
            Assert.AreEqual(xRootDirectoryListing.Count, 0);

            mFS.CreateDirectory(xRootDirectory, xNewDirectoryEntryName);

            xRootDirectoryListing = mFS.GetDirectoryListing(xRootDirectory);
            Assert.AreEqual(xRootDirectoryListing.Count, 1);

            var xNewDirectoryEntry = xRootDirectoryListing[0];
            Assert.AreEqual(xNewDirectoryEntry.mName, xNewDirectoryEntryName);

            var dirDirectoryListing = mFS.GetDirectoryListing(xNewDirectoryEntry);
            Assert.AreEqual(0, dirDirectoryListing.Count); //the . and .. directories should not be included
        }

        [Test]
        public void Create_A_Subdirectory_Entry()
        {
            var xRootDirectory = mFS.GetRootDirectory();
            mFS.CreateDirectory(xRootDirectory, "First");
            var root = mFS.GetDirectoryListing(xRootDirectory);
            mFS.CreateDirectory(root[0], "Sub");
            Assert.AreEqual(mFS.GetDirectoryListing(root[0]).Count, 1);
        }

        [Test]
        public void Create_A_Subdirectory_Entry_With_Files()
        {
            var xRootDirectory = mFS.GetRootDirectory();
            mFS.CreateDirectory(xRootDirectory, "First");
            var root = mFS.GetDirectoryListing(xRootDirectory);
            mFS.CreateDirectory(root[0], "Sub");
            Assert.AreEqual(mFS.GetDirectoryListing(root[0]).Count, 1);
            var dir = root[0];
            Assert.AreEqual("First", dir.mName);
            mFS.CreateFile(dir, "test.txt");
            Assert.AreEqual(mFS.GetDirectoryListing(dir).Count, 2);
            mFS.CreateFile(dir, "test2.txt");
            Assert.AreEqual(mFS.GetDirectoryListing(dir).Count, 3);
            Assert.AreEqual("test.txt", mFS.GetDirectoryListing(dir)[1].mName);
            Assert.AreEqual(@"0:\First\test.txt", mFS.GetDirectoryListing(dir)[1].mFullPath);
        }
    }
}
