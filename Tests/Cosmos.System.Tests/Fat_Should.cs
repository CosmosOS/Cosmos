using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.FAT;
using NUnit.Framework;

namespace Cosmos.System.Tests
{
    public class Fat_Should
    {
        private FatFileSystem mFS;
        private FatFileSystem.Fat mFat;

        [SetUp]
        public void Setup()
        {
            var xDevice = new TestBlockDevice();
            var xPartition = new Partition(xDevice, 0, xDevice.BlockCount);
            var xFactory = new FatFileSystemFactory();
            mFS = (FatFileSystem)xFactory.Create(xPartition, "0:\\", (long)(xPartition.BlockSize * xPartition.BlockCount));
            mFat = mFS.GetFat(0);
        }

        [Test]
        public void Add_New_Clusters_To_Chain_When_Needed()
        {
            uint xStartCluster = mFS.RootCluster;
            mFat.SetFatEntry(xStartCluster, mFat.FatEntryEofValue());
            mFat.SetFatEntry(xStartCluster + 2, mFat.FatEntryEofValue());
            mFat.SetFatEntry(xStartCluster + 5, mFat.FatEntryEofValue());

            uint[] xChain = mFat.GetFatChain(xStartCluster, mFS.BytesPerCluster);
            Assert.AreEqual(xChain.Length, 1);

            xChain = mFat.GetFatChain(xStartCluster, mFS.BytesPerCluster * 3);
            Assert.AreEqual(3, xChain.Length);
            Assert.AreEqual(2, xChain[0]);
            Assert.AreEqual(3, xChain[1]);
            Assert.AreEqual(5, xChain[2]);

            xChain = mFat.GetFatChain(xStartCluster, mFS.BytesPerCluster * 5);
            Assert.AreEqual(5, xChain.Length);
            Assert.AreEqual(2, xChain[0]);
            Assert.AreEqual(3, xChain[1]);
            Assert.AreEqual(5, xChain[2]);
            Assert.AreEqual(6, xChain[3]);
            Assert.AreEqual(8, xChain[4]);
        }
    }
}
