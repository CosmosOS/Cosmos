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

		private static unsafe GroupDescriptor[] ReadGroupDescriptorsOfBlock(byte aController, byte aDrive, uint aBlockGroup, SuperBlock aSuperBlock, ushort* aBuffer) {
			uint xGroupDescriptorCount = aSuperBlock.INodesCount / aSuperBlock.INodesPerGroup;
			GroupDescriptor[] xResult = new GroupDescriptor[xGroupDescriptorCount];
			GroupDescriptor* xDescriptorPtr = (GroupDescriptor*)aBuffer;
			for (int i = 0; i < xGroupDescriptorCount; i++) {
				int xATABlock = (int)((8));
				xATABlock += (i / 16);
				if ((i % 16) == 0) {
					if (!Hardware.Storage.ATA.ReadDataNew(aController, aDrive, xATABlock, aBuffer)) {
						Console.WriteLine("[Ext2|GroupDescriptors] Error while reading GroupDescriptor data");
						return null;
					}
				}
				xResult[i] = xDescriptorPtr[i % 16];
				DebugUtil.SendExt2_GroupDescriptor("ReadGroupDescriptorsOfBlock", xATABlock, i, 0, &xDescriptorPtr[i % 16]);
			}
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
			uint xGroupDescriptorsPerBlock = (uint)(xBlockSize / sizeof(GroupDescriptor));
			GroupDescriptor[] xGroupDescriptors = ReadGroupDescriptorsOfBlock(aController, aDrive, xSuperBlock.FirstDataBlock + 1, xSuperBlock, xBuffer);
			if (xGroupDescriptors == null) {
				return;
			}
			bool[] xUsedINodes = new bool[32];
			uint xCount = 0;
			INode* xINodeTable = (INode*)xBuffer;
			for (uint g = 0; g < xGroupDescriptors.Length; g++) {
				GroupDescriptor xGroupDescriptor = xGroupDescriptors[g];
				if (!Hardware.Storage.ATA.ReadDataNew(aController, aDrive, (int)((xGroupDescriptor.INodeBitmap) * 8), xBuffer)) {
					return;
				}
				if (!ConvertBitmapToBoolArray((uint*)xBuffer, xUsedINodes)) {
					return;
				}
				for (int i = 0; i < 32; i++) {
					if ((i % 4) == 0) {
						uint index = (uint)((i % xSuperBlock.INodesPerGroup) * sizeof(INode));
						uint offset = index / 512;
						if (!Hardware.Storage.ATA.ReadDataNew(aController, aDrive, (int)((xGroupDescriptor.INodeTable * 8) + offset), xBuffer)) {
							Console.WriteLine("[Ext2] Error reading INode table entries");
							return;
						}
					}
					uint xINodeIdentifier = (uint)((g * xSuperBlock.INodesPerGroup) + i + 1);
					if (xINodeIdentifier == 0xB) {
						continue;
					}
					if (xUsedINodes[i]) {
						INode xINode = xINodeTable[i % 4];
						//DebugUtil.SendExt2_INode((uint)((g * xSuperBlock.INodesPerGroup) + i), g, &xINodeTable[i % 4]);
						if ((xINode.Mode & INodeModeEnum.Directory) != 0) {
							//DebugUtil.SendNumber("Ext2", "Directory found", (uint)((g * xSuperBlock.INodesPerGroup) + i + 1), 32);
							DebugUtil.SendExt2_INode(xINodeIdentifier, g, &xINodeTable[i % 4]);
							DebugUtil.SendExt2_INodeMode(xINodeIdentifier, (ushort)xINode.Mode);
							if (!Hardware.Storage.ATA.ReadDataNew(aController, aDrive, (int)(xINode.Block1 * 8), xBuffer)) {
								Console.WriteLine("[Ext2] Error reading INode entries");
								return;
							}
							DirectoryEntry* xEntryPtr = (DirectoryEntry*)xBuffer;
							uint xTotalSize = xINode.Size;
							while (xTotalSize != 0/* && xEntryPtr->@INode != 0*/) {
								DebugUtil.SendExt2_DirectoryEntry(xEntryPtr);
								uint xPtrAddress = (uint)xEntryPtr;
								xPtrAddress += xEntryPtr->RecordLength;
								DebugUtil.SendNumber("Ext2", "xEntryPtr.RecordLength", xEntryPtr->RecordLength, 16);
								DebugUtil.SendNumber("Ext2", "xTotalSize", xTotalSize, 32);
								xTotalSize -= xEntryPtr->RecordLength;
								DebugUtil.SendNumber("Ext2", "xEntryPtr.RecordLength", xEntryPtr->RecordLength, 16);
								DebugUtil.SendNumber("Ext2", "xTotalSize", xTotalSize, 32);
								xEntryPtr = (DirectoryEntry*)xPtrAddress;
							}
							//								DebugUtil.SendNumber("Ext2", "Directory Entries", xEntryCount, 32);
						}
						xCount++;
					}
				}
			}

			DebugUtil.SendNumber("Ext2", "Used INode count", xCount, 32);
		}

		private static unsafe bool ConvertBitmapToBoolArray(uint* aBitmap, bool[] aArray) {
			if (aArray == null || aArray.Length != 32) {
				Console.WriteLine("[ConvertBitmapToBoolArray] Incorrect Array");
				return false;
			}
			uint xValue = *aBitmap;
			for (byte b = 0; b < 32; b++) {
				uint xCheckBit = (uint)(1 << b);
				aArray[b] = (xValue & xCheckBit) != 0;
			}
			return true;
		}
	}
}
