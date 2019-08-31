using System.Text;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.FAT;
using NUnit.Framework;

namespace Cosmos.System.Tests
{
    public class FatFileSystem_Should
    {
        private FatFileSystem mFS;

        [SetUp]
        public void Setup()
        {
            var xDevice = new TestBlockDevice();
            var xPartition = new Partition(xDevice, 0, xDevice.BlockCount);
            var xFactory = new FatFileSystemFactory();
            mFS = (FatFileSystem) xFactory.Create(xPartition, "0:\\", (long) (xPartition.BlockSize * xPartition.BlockCount));
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
        }
    }
}
