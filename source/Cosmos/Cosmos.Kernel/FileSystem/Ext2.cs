using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel.FileSystem {
	public partial class Ext2: FileSystem {
		//public static unsafe byte[] ReadFileContents(byte aController, byte aDrive, string[] aPath) {
		//    ushort* xBuffer = (ushort*)Heap.MemAlloc(512);
		//    if (!Hardware.Storage.ATA.ReadDataNew(aController, aDrive, 2, xBuffer)) {
		//        Console.WriteLine("[Ext2|SuperBlock] Error while reading SuperBlock data");
		//        return null;
		//    }
		//    SuperBlock xSuperBlock = *(SuperBlock*)xBuffer;
		//    DebugUtil.SendExt2_SuperBlock("ReadFileContents", &xSuperBlock);
		//    int xBlockSize = (int)(1024 << (byte)(xSuperBlock.LogBlockSize));
		//    uint xGroupDescriptorCount = xSuperBlock.BlockCount / xSuperBlock.BlocksPerGroup;
		//    DebugUtil.SendNumber("Ext2", "GroupDescriptorCount", xGroupDescriptorCount, 32);
		//    GroupDescriptor[] xGroupDescriptors = ReadGroupDescriptorsOfBlock(aController, aDrive, xSuperBlock.FirstDataBlock + 1, xSuperBlock, xBuffer);
		//    if (xGroupDescriptors == null) {
		//        return null;
		//    }
		//    for (uint i = 0; i < xGroupDescriptors.Length; i++) {
		//        GroupDescriptor xGroupDescriptor = xGroupDescriptors[i];
		//        DebugUtil.SendExt2_GroupDescriptor("ReadFileContents", xSuperBlock.FirstDataBlock + 1, i, &xGroupDescriptor);
		//    }
		//    return null;
		//}

		private static unsafe GroupDescriptor[] ReadGroupDescriptorsOfBlock(byte aController, byte aDrive, uint aBlock, SuperBlock aSuperBlock, ushort* aBuffer) {
			uint xGroupDescriptorCount = aSuperBlock.INodesCount / aSuperBlock.INodesPerGroup;
			//int xBlockSize = (int)(1024 << (byte)(xSuperBlock.LogBlockSize));
			//uint xGroupDescriptorsPerBlock = (uint)(xBlockSize / sizeof(GroupDescriptor));
			GroupDescriptor[] xResult = new GroupDescriptor[xGroupDescriptorCount];
			for (int i = 0; i < xGroupDescriptorCount; i++) {
				DebugUtil.SendNumber("Ext2", "i", (uint)i, 8);
				DebugUtil.SendNumber("Ext2", "Buffer address", (uint)aBuffer, 32);
				if ((i % 16) == 0) {
					DebugUtil.SendMessage("Ext2", "Reading Block");
					if (!Hardware.Storage.ATA.ReadDataNew(aController, aDrive, (int)(aBlock + (byte)(i / 16)), aBuffer)) {
						Console.WriteLine("[Ext2|GroupDescriptors] Error while reading GroupDescriptor data");
						return null;
					}
				}
				GroupDescriptor* xDescriptorPtr = (GroupDescriptor*)(aBuffer + ((int)(i - (byte)(i / 16)) * 32));
				DebugUtil.SendNumber("Ext2", "Offset", (uint)xDescriptorPtr, 32);
				xResult[i] = xDescriptorPtr[0];
				DebugUtil.SendExt2_GroupDescriptor("ReadGroupDescriptorsOfBlock", (int)(aBlock + (byte)(i / 16)), i, xDescriptorPtr);
			}
			//GroupDescriptor* xDescriptorPtr = (GroupDescriptor*)aBuffer;
			//for (uint i = 0; i < xGroupDescriptorCount; i++) {
			//    xResult[i] = xDescriptorPtr[i];
			//    DebugUtil.SendExt2_GroupDescriptor("ReadGroupDescriptorsOfBlock", aSuperBlock.FirstDataBlock + 1, i, &xDescriptorPtr[i]);
			//}
			return xResult;
		}

		public static unsafe void PrintAllFilesAndDirectories(byte aController, byte aDrive) {
			ushort* xBuffer = (ushort*)Heap.MemAlloc(512);
			if (!Hardware.Storage.ATA.ReadDataNew(aController, aDrive, 2, xBuffer)) {
				Console.WriteLine("[Ext2|SuperBlock] Error while reading SuperBlock data");
				return;
			}
			SuperBlock xSuperBlock = *(SuperBlock*)xBuffer;
			DebugUtil.SendExt2_SuperBlock("ReadFileContents", &xSuperBlock);
			int xBlockSize = (int)(1024 << (byte)(xSuperBlock.LogBlockSize));
			uint xGroupsCount = xSuperBlock.INodesCount / xSuperBlock.INodesPerGroup;
			DebugUtil.SendNumber("Ext2", "GroupsCount", xGroupsCount, 32);
			uint xGroupDescriptorsPerBlock = (uint)(xBlockSize / sizeof(GroupDescriptor));
			GroupDescriptor[] xGroupDescriptors = ReadGroupDescriptorsOfBlock(aController, aDrive, xSuperBlock.FirstDataBlock + 1, xSuperBlock, xBuffer);
			if (xGroupDescriptors == null) {
				return;
			}
			DebugUtil.SendNumber("Ext2", "GroupDescriptor Count", (uint)xGroupDescriptors.Length, 32);
			//bool[] xBitmap = new bool[32];
			uint xTempInt = 0;
			for (uint i = 0; i < xGroupDescriptors.Length; i++) {
				GroupDescriptor xGroupDescriptor = xGroupDescriptors[i];
				xTempInt = xTempInt + xGroupDescriptor.FreeINodesCount;
			}
			DebugUtil.SendNumber("Ext2", "Sum of FreeINodesCount", xTempInt, 32);
		}

		private static bool ConvertBitmapToBoolArray(uint aBitmap, bool[] aArray) {
			if (aArray == null || aArray.Length != 32) {
				Console.WriteLine("[ConvertBitmapToBoolArray] Incorrect Array");
				return false;
			}
			for (byte b = 0; b < 32; b++) {
				uint xCheckBit = (uint)(1 << b);
				aArray[b] = (aBitmap & xCheckBit) != 0;
			}
			return true;
		}
	}
}
