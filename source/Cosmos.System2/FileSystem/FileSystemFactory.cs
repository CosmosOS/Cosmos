using Cosmos.HAL.BlockDevice;

namespace Cosmos.System.FileSystem
{
    public abstract class FileSystemFactory
    {
        /// <summary>
        /// The name of the file system.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Checks if the file system can handle the partition.
        /// </summary>
        /// <param name="aDevice">The partition.</param>
        /// <returns>Returns true if the file system can handle the partition, false otherwise.</returns>
        public abstract bool IsType(Partition aDevice);
        /// <summary>
        /// Creates a new <see cref="FileSystem"/> object for the given partition, root path, and size.
        /// </summary>
        /// <param name="aDevice">The partition.</param>
        /// <param name="aRootPath">The root path.</param>
        /// <param name="aSize">The size, in MB.</param>
        /// <returns></returns>
        public abstract FileSystem Create(Partition aDevice, string aRootPath, long aSize);
    }
}
