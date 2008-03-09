using System;
using System.Collections.Generic;
using Cosmos.Hardware;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.New.Partitioning {
	public static class MBT {
		private class MBTPartition: BlockDevice {
			private readonly BlockDevice mBackend;
			private readonly ulong mBlockStart;
			private readonly ulong mBlockCount;
			private readonly string mName;
			public MBTPartition(BlockDevice aBackend, ulong aBlockStart, ulong aBlockCount, string aName) {
				mBlockStart = aBlockStart;
				mBlockCount = aBlockCount;
				mBackend = aBackend;
				mName = aName;
			}

			public override uint BlockSize {
				get {
					return mBackend.BlockSize;
				}
			}

			public override ulong BlockCount {
				get {
					return mBlockCount;
				}
			}

			private ulong GetActualBlock(ulong aBlock) {
				return aBlock;
			}

			public override byte[] ReadBlock(ulong aBlock) {
				return mBackend.ReadBlock(GetActualBlock(aBlock));
			}

			public override void WriteBlock(ulong aBlock, byte[] aContents) {
				mBackend.WriteBlock(GetActualBlock(aBlock), aContents);
			}

			public override string Name {
				get {
					return mName;
				}
			}
		}
		public static void Initialize() {
			DebugUtil.SendMessage("MBT", "Initializing");
			DebugUtil.SendNumber("MBT", "DeviceCount", (uint)Device.Devices.Count, 32);
			for (int i = 0; i < Device.Devices.Count; i++) {
				var xDevice = Device.Devices[i];
				var xBlockDev = xDevice as BlockDevice;
				if (xBlockDev == null) {
					continue;
				}
				DebugUtil.SendMessage("MBT", "Found Device");
				var xBlockContents = xBlockDev.ReadBlock(0);
				// detecting whether MBT or not
				DebugUtil.SendNumber("MBT", "xBlockDev.BlockSize", xBlockDev.BlockSize, 32);
				DebugUtil.SendDoubleNumber("MBT", "Last bytes of block", xBlockContents[510], 8, xBlockContents[511], 8);
				if (!(xBlockContents[xBlockDev.BlockSize - 2] == 0x55 && xBlockContents[xBlockDev.BlockSize - 1] == 0xAA)) {
					DebugUtil.SendMessage("MBT", "Does not contain MBR");
					Hardware.DebugUtil.SendATA_BlockReceived(255, 255, 0, xBlockContents);
					continue;
				}
				int xContentsIndex = 0;
				xContentsIndex = 0x1BE;
				for (byte j = 0; j < 4; j++) {
					if (!(xBlockContents[xContentsIndex] == 0x80 || xBlockContents[xContentsIndex] == 0)) {
						DebugUtil.SendNumber("MBT", "Partition Status", xBlockContents[xContentsIndex], 8);
						xContentsIndex += 16;
						continue;
					}
					xContentsIndex += 8;					
					uint xStart = BitConverter.ToUInt32(xBlockContents, xContentsIndex);
					xContentsIndex += 4;
					uint xLength = BitConverter.ToUInt32(xBlockContents, xContentsIndex);
					xContentsIndex += 4;
					DebugUtil.SendDoubleNumber("MBT", "Entry Found. Start, Length in blocks", xStart, 32, xLength, 32);
					if (xStart > 0 && xLength > 0) {
						xStart += 2;
						//Device.Add(new MBTPartition(xBlockDev, xStart, xLength, "Partition"));
						DebugUtil.SendMessage("MBT", "FoundPartition");
					}
				}
			}
		}
	}
}
