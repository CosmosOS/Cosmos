using System;

using Cosmos.HAL.BlockDevice;

namespace Cosmos.System.FileSystem.FAT
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

            var bootSector = ReadBootSector(aDevice);
            var bpb = new BiosParameterBlock(bootSector);

            return IsFatPartition(bpb);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FatFileSystem"/> class.
        /// </summary>
        /// <param name="aDevice">The partition.</param>
        /// <param name="aRootPath">The root path.</param>
        /// <exception cref="Exception">FAT signature not found.</exception>
        public override FileSystem Create(Partition aDevice, string aRootPath, long aSize)
        {
            var bootSector = ReadBootSector(aDevice);
            var bpb = new BiosParameterBlock(bootSector);

            if (!IsFatPartition(bpb))
            {
                throw new InvalidOperationException("Partition file system is not FAT!");
            }

            return new FatFileSystem(bpb, aDevice, aRootPath, aSize);
        }

        private byte[] ReadBootSector(Partition partition)
        {
            var bootSector = partition.NewBlockArray(1);
            partition.ReadBlock(0, 1, bootSector);

            return bootSector;
        }

        private bool IsFatPartition(BiosParameterBlock bpb) =>
            bpb.GetValue(BiosParameterBlock.Signature) == 0xAA55;
    }
}
