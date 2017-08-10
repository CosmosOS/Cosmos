using System;
using System.Collections.Generic;
using Cosmos.Hardware2;
using System.Runtime.InteropServices;

namespace Cosmos.Sys.FileSystem {
	public static class MBT {
		private class MBTPartition: BlockDevice {
			private readonly BlockDevice mBackend;
			private readonly uint mBlockStart;
            private readonly uint mBlockCount;
			private readonly string mName;
            public MBTPartition(BlockDevice aBackend, uint aBlockStart, uint aBlockCount, string aName)
            {
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

            public override ulong BlockCount
            {
				get {
					return mBlockCount;
				}
			}

            private ulong GetActualBlock(ulong aBlock)
            {
				return aBlock + mBlockStart;
			}

            public override void ReadBlock(ulong aBlock, byte[] aBuffer)
            {
				mBackend.ReadBlock(GetActualBlock(aBlock), aBuffer);
			}

            public override void WriteBlock(ulong aBlock, byte[] aContents)
            {
				mBackend.WriteBlock(GetActualBlock(aBlock), aContents);
			}

			public override string Name {
				get {
					return mName;
				}
			}
		}
        public static void Init() 
        {
            //DebugUtil.SendMessage("MBT", "Initializing");
            //DebugUtil.SendNumber("MBT", "DeviceCount", (uint)Device.Devices.Count, 32);

            //List<Device> mbtPartitions = new List<Device>();
			for (int i = 0; i < Device.Devices.Count; i++) {
				if (Device.Devices[i].Type == Device.DeviceType.Storage) {
					var xTest = getPartitions((BlockDevice)Device.Devices[i]);
					if (xTest != null) {
						for (int j = 0; j < xTest.Count; j++) {
							Device.Add(xTest[j]);
						}
					}
				}
			}
        }

        private static List<Device> getPartitions(BlockDevice xBlockDev)
        {
            if (xBlockDev == null)
                return default(List<Device>);

            List<Device> partitionList = new List<Device>();
            int xContentsIndex = 0x1BE;
            //DebugUtil.SendMessage("MBT", "Found Device");
            byte[] xBlockContents = new byte[xBlockDev.BlockSize];
            xBlockDev.ReadBlock(0, xBlockContents);
            // detecting whether MBT or not
            //DebugUtil.SendNumber("MBT", "xBlockDev.BlockSize", xBlockDev.BlockSize, 32);

            //DebugUtil.SendDoubleNumber("MBT", "Last bytes of block", xBlockContents[510], 8, xBlockContents[511], 8);
            if (!(xBlockContents[xBlockDev.BlockSize - 2] == 0x55 && xBlockContents[xBlockDev.BlockSize - 1] == 0xAA))
            {
                //DebugUtil.SendMessage("MBT", "Does not contain MBR");
                //Hardware.DebugUtil.SendATA_BlockReceived(255, 255, 0, xBlockContents);
                return default(List<Device>);
            }

            for (byte j = 0; j < 4; j++)
            {
                //DebugUtil.SendNumber("MBT", "Partition Status", xBlockContents[xContentsIndex], 8);
                if (!(xBlockContents[xContentsIndex] == 0x80 || xBlockContents[xContentsIndex] == 0))
                {
                    xContentsIndex += 16;
                    continue;
                }
                xContentsIndex += 8;
                uint xStart = BitConverter.ToUInt32(xBlockContents, xContentsIndex);
                xContentsIndex += 4;
                uint xLength = BitConverter.ToUInt32(xBlockContents, xContentsIndex);
                xContentsIndex += 4;
                if (xStart > 0 && xLength > 0)
                {
                    //DebugUtil.SendDoubleNumber("MBT", "Entry Found. Start, Length in blocks", xStart, 32, xLength, 32);
                    xStart += 2;
                    Console.WriteLine("Add Partition to Device list");
                    partitionList.Add(new MBTPartition(xBlockDev, xStart, xLength, "Partition")); //Causes System Fault on the HTC Shift!
                    //DebugUtil.SendMessage("MBT", "FoundPartition");
                }
            }

            return partitionList;
        }
	}
}
