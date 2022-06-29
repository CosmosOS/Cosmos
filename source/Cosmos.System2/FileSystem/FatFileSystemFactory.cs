using System;

using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.FAT;

namespace Cosmos.System.FileSystem
{
    public class FatFileSystemFactory : FileSystemFactory
    {
        public override string Name => "FAT";

        /// <summary>
        /// Checks if the file system can handle the partition.
        /// </summary>
        /// <param name="aDevice">The partition.</param>
        /// <returns>Returns true if the file system can handle the partition, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if partition is null.</exception>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">Thrown on memory error.</exception>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error.</exception>
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
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown when aDevice is null.</item>
        /// <item>Thrown when FatFileSystem is null.</item>
        /// <item>Thrown on fatal error (contact support).</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown when aRootPath is null.</item>
        /// <item>Thrown on fatal error (contact support).</item>
        /// </list>
        /// </exception>
        /// <exception cref="OverflowException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown on fatal error (contact support).</item>
        /// <item>>FAT signature not found.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        public override FileSystem Create(Partition aDevice, string aRootPath, long aSize) => new FatFileSystem(aDevice, aRootPath, aSize);
    }
}
