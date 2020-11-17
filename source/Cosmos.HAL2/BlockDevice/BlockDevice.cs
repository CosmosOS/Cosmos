using System;
using System.Collections.Generic;

namespace Cosmos.HAL.BlockDevice
{
    // This class should not support selecting a device or sub device.
    // Each instance must control exactly one device. For example with ATA
    // master/slave, each one needs its own device instance. For ATA
    // this complicates things a bit because they share IO ports, but this
    // is an intentional decision.
    /// <summary>
    /// BlockDevice abstract class. See also: <seealso cref="Device"/>.
    /// </summary>
    public abstract class BlockDevice : Device
    {
        // TODO: Need to protect this from changes except by Hardware ring
        /// <summary>
        /// Devices list.
        /// </summary>
        static public List<BlockDevice> Devices = new List<BlockDevice>();

        /// <summary>
        /// Create new block array.
        /// </summary>
        /// <param name="aBlockCount">Number of blocks to alloc.</param>
        /// <returns>byte array.</returns>
        public byte[] NewBlockArray(UInt32 aBlockCount)
        {
            return new byte[aBlockCount * mBlockSize];
        }

        /// <summary>
        /// Block count.
        /// </summary>
        protected UInt64 mBlockCount = 0;
        /// <summary>
        /// Get block count.
        /// </summary>
        public UInt64 BlockCount => mBlockCount;

        /// <summary>
        /// Block size.
        /// </summary>
        protected UInt64 mBlockSize = 0;
        /// <summary>
        /// Get block size.
        /// </summary>
        public UInt64 BlockSize => mBlockSize;

        // Only allow reading and writing whole blocks because many of the hardware
        // command work that way and we dont want to add complexity at the BlockDevice level.
        // public abstract void ReadBlock(UInt64 aBlockNo, UInt32 aBlockCount, byte[] aData);
        /// <summary>
        /// Read block from partition.
        /// </summary>
        /// <param name="aBlockNo">A block to read from.</param>
        /// <param name="aBlockCount">A number of blocks in the partition.</param>
        /// <param name="aData">A data that been read.</param>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">Thrown when data size invalid.</exception>
        public abstract void ReadBlock(UInt64 aBlockNo, UInt64 aBlockCount, ref byte[] aData);

        /// <summary>
        /// Write block to partition.
        /// </summary>
        /// <param name="aBlockNo">A block number to write to.</param>
        /// <param name="aBlockCount">A number of blocks in the partition.</param>
        /// <param name="aData">A data to write.</param>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">Thrown when data size invalid.</exception>
        public abstract void WriteBlock(UInt64 aBlockNo, UInt64 aBlockCount, ref byte[] aData);

        /// <summary>
        /// Check data size.
        /// </summary>
        /// <param name="aData">A data to check the size of.</param>
        /// <param name="aBlockCount">Number of blocks used to store the data.</param>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">Thrown when data size invalid.</exception>
        protected void CheckDataSize(byte[] aData, UInt64 aBlockCount)
        {
            if ((ulong)aData.Length != aBlockCount * mBlockSize)
            {
                throw new Exception("Invalid data size.");
            }
        }

        /// <summary>
        /// Check block number.
        /// Not implemented.
        /// </summary>
        /// <param name="aBlockNo">A block number to be checked.</param>
        /// <param name="aBlockCount">A block count.</param>
        protected void CheckBlockNo(UInt64 aBlockNo, UInt64 aBlockCount)
        {
            if (aBlockNo + aBlockCount >= mBlockCount)
            {
                //throw new Exception("Invalid block number.");
            }
        }
    }
}
