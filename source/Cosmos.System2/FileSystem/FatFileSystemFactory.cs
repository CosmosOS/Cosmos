using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.FAT;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.FileSystem
{
    public class FatFileSystemFactory : FileSystemFactory
    {
        public override string Name { get => "FAT"; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FatFileSystem"/> class.
        /// </summary>
        /// <param name="aDevice">The partition.</param>
        /// <param name="aRootPath">The root path.</param>
        /// <exception cref="Exception">FAT signature not found.</exception>
        public override FileSystem Create(Partition aDevice, string aRootPath, long aSize) => new FatFileSystem(aDevice, aRootPath, aSize);

        public override bool IsType(Partition aDevice) => FatFileSystem.IsDeviceFat(aDevice);
    }
}
