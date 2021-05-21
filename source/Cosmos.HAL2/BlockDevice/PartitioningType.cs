using System.Collections.Generic;
using Cosmos.HAL.BlockDevice;

namespace Cosmos.System.FileSystem
{
    public abstract class PartitioningType
    {
        /// <summary>
        /// Checks if the partition type is this device.
        /// </summary>
        /// <param name="device">The device</param>
        /// <returns>True/False</returns>
        public abstract bool IsType();
        /// <summary>
        /// Gets all partitons
        /// </summary>
        /// <returns>All of the partitons on the disk</returns>
        public abstract List<Partition> GetPartitions();
        /// <summary>
        /// Removes a partiton by an index
        /// </summary>
        /// <param name="partIndex"></param>
        public abstract void RemovePartition(int partIndex);
        /// <summary>
        /// Creates a partition
        /// </summary>
        /// <param name="sizeInMB">Partition size in MB</param>
        public abstract void CreatePartition(int sizeInMB);
        /// <summary>
        /// Removes all partitions
        /// </summary>
        public abstract void Clear();
    }
}
