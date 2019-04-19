using System;

using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.FAT;

namespace Cosmos.System.FileSystem
{
    public class FatFileSystemFactory : FileSystemFactory
    {
        public override string Name => "FAT";

        public override bool IsType(Partition aDevice)
        {
            if (aDevice == null)
            {
                throw new ArgumentNullException(nameof(aDevice));
            }

            var xBPB = aDevice.NewBlockArray(1);
            aDevice.ReadBlock(0UL, 1U, ref xBPB);

            var xSig = BitConverter.ToUInt16(xBPB, 510);
            return xSig == 0xAA55;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FatFileSystem"/> class.
        /// </summary>
        /// <param name="aDevice">The partition.</param>
        /// <param name="aRootPath">The root path.</param>
        /// <exception cref="Exception">FAT signature not found.</exception>
        public override FileSystem Create(Partition aDevice, string aRootPath, long aSize) => new FatFileSystem(aDevice, aRootPath, aSize);
    }
}
