using System;
using System.Collections.Generic;

namespace Cosmos.HAL.BlockDevice
{
	// This class should not support selecting a device or sub device.
	// Each instance must control exactly one device. For example with ATA
	// master/slave, each one needs its own device instance. For ATA
	// this complicates things a bit because they share IO ports, but this
	// is an intentional decision.

	public abstract class BlockDevice : Device
	{
		// TODO: Need to protect this from changes except by Hardware ring
		static public List<BlockDevice> Devices = new List<BlockDevice>();

		public byte[] NewBlockArray(UInt32 aBlockCount)
		{
			return new byte[aBlockCount * mBlockSize];
		}

		protected UInt64 mBlockCount = 0;
        public UInt64 BlockCount => mBlockCount;

        protected UInt64 mBlockSize = 0;
        public UInt64 BlockSize => mBlockSize;

        // Only allow reading and writing whole blocks because many of the hardware
        // command work that way and we dont want to add complexity at the BlockDevice level.
        // public abstract void ReadBlock(UInt64 aBlockNo, UInt32 aBlockCount, byte[] aData);
        public abstract void ReadBlock(UInt64 aBlockNo, UInt64 aBlockCount, byte[] aData);
		public abstract void WriteBlock(UInt64 aBlockNo, UInt64 aBlockCount, byte[] aData);

		protected void CheckDataSize(byte[] aData, UInt64 aBlockCount)
		{
		    if ((ulong)aData.Length != aBlockCount * mBlockSize)
			{
				throw new Exception("Invalid data size.");
			}
		}

		protected void CheckBlockNo(UInt64 aBlockNo, UInt64 aBlockCount)
		{
			if (aBlockNo + aBlockCount >= mBlockCount)
			{
				//throw new Exception("Invalid block number.");
			}
		}
	}
}
