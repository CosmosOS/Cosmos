using System;
using Cosmos.Hardware.Storage;

namespace Cosmos.Kernel.FileSystem {
	public unsafe partial class Ext2 {
		private Storage mBackend;
		private SuperBlock* mSuperBlock;
		private uint mBlockSize;
		private uint mGroupsCount;
		private uint mGroupDescriptorsPerBlock;
		private GroupDescriptor[] mGroupDescriptors;

		public Ext2(Storage aBackend) {
			mBackend = aBackend;
		}

		private bool ReadSuperBlock() {
			ushort* xBuffer = (ushort*)Heap.MemAlloc(mBackend.BlockSize);
			if (!mBackend.ReadBlock(2, (byte*)xBuffer)) {
				Console.WriteLine("[Ext2|SuperBlock] Error while reading SuperBlock data");
				return false;
			}
			byte* xByteBuff = (byte*)xBuffer;
			mSuperBlock = (SuperBlock*)Heap.MemAlloc((uint)sizeof(SuperBlock));
			byte* xSuperBlockByteBuff = (byte*)mSuperBlock;
			for (int i = 0; i < sizeof(SuperBlock); i++) {
				xSuperBlockByteBuff[i] = xByteBuff[i];
			}
			DebugUtil.SendExt2_SuperBlock("", mSuperBlock);
			mBlockSize = (uint)(1024 << (byte)(mSuperBlock->LogBlockSize));
			DebugUtil.SendDoubleNumber("Numbers", "", mSuperBlock->INodesCount, 32, mSuperBlock->INodesPerGroup, 32);
			mGroupsCount = mSuperBlock->INodesCount / mSuperBlock->INodesPerGroup;
			mGroupDescriptorsPerBlock = (uint)(mBlockSize / sizeof(GroupDescriptor));
			if (!ReadGroupDescriptorsOfBlock(mSuperBlock->FirstDataBlock + 1, xBuffer)) {
				return false;
			}
			Heap.MemFree((uint)xBuffer);
			return true;
		}

		private unsafe bool ReadGroupDescriptorsOfBlock(uint aBlockGroup, ushort* aBuffer) {
			mGroupDescriptors = new GroupDescriptor[mGroupsCount];
			GroupDescriptor* xDescriptorPtr = (GroupDescriptor*)aBuffer;
			for (int i = 0; i < mGroupsCount; i++) {
				uint xATABlock = (uint)(mBlockSize / mBackend.BlockSize);
				xATABlock += (uint)(i / mGroupDescriptorsPerBlock);
				if ((i % 16) == 0) {
					if (!mBackend.ReadBlock(xATABlock, (byte*)aBuffer)) {
						Console.WriteLine("[Ext2|GroupDescriptors] Error while reading GroupDescriptor data");
						return false;
					}
				}
				mGroupDescriptors[i] = xDescriptorPtr[i % mGroupDescriptorsPerBlock];
				DebugUtil.SendExt2_GroupDescriptor("ReadGroupDescriptorsOfBlock", xATABlock, i, 0, &xDescriptorPtr[i % mGroupDescriptorsPerBlock]);
			}
			return true;
		}

		public bool Initialize() {
			return ReadSuperBlock();
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

		public unsafe byte[] ReadFile(string[] xPath) {
			ushort* xBuffer = (ushort*)Heap.MemAlloc(mBackend.BlockSize);
			byte* xByteBuffer = (byte*)xBuffer;
			bool[] xUsedINodes = new bool[32];
			uint xCount = 0;
			INode* xINodeTable = (INode*)xBuffer;
			uint xPathPointer = 0;
			uint xCurrentINode = 2;
			int xIterations = 0;
			uint xResultSize = 0;
			while (xPathPointer != (xPath.Length + 1)) {
				if (xIterations == 5) {
					Console.WriteLine("DEBUG: Stopping iteration");
					break;
				}
				DebugUtil.SendNumber("Ext2", "Current INode", xCurrentINode, 32);
				for (uint g = 0; g < mGroupDescriptors.Length; g++) {
					GroupDescriptor xGroupDescriptor = mGroupDescriptors[g];
					if (!mBackend.ReadBlock((uint)((xGroupDescriptor.INodeBitmap) * (mBlockSize / mBackend.BlockSize)), xByteBuffer)) {
						Heap.MemFree((uint)xBuffer);
						return null;
					}
					if (!ConvertBitmapToBoolArray((uint*)xBuffer, xUsedINodes)) {
						Heap.MemFree((uint)xBuffer);
						return null;
					}
					for (int i = 0; i < 32; i++) {
						if ((i % (mBackend.BlockSize / sizeof(INode))) == 0) {
							uint index = (uint)((i % mSuperBlock->INodesPerGroup) * sizeof(INode));
							uint offset = index / mBackend.BlockSize;
							if (!mBackend.ReadBlock((uint)((xGroupDescriptor.INodeTable * (mBlockSize / mBackend.BlockSize)) + offset), xByteBuffer)) {
								Console.WriteLine("[Ext2] Error reading INode table entries");
								Heap.MemFree((uint)xBuffer);
								return null;
							}
						}
						uint xINodeIdentifier = (uint)((g * mSuperBlock->INodesPerGroup) + i + 1);
						if (xINodeIdentifier != xCurrentINode) {
							continue;
						}
						if (xUsedINodes[i]) {
							INode xINode = xINodeTable[i % (mBackend.BlockSize / sizeof(INode))];
							if (xPathPointer == xPath.Length) {
								#region temporary checks
								if (xINode.Block2 != 0) {
									Console.WriteLine("Multiblock files not supported yet!");
									Heap.MemFree((uint)xBuffer);
									return null;
								}
								if (xINode.Block3 != 0) {
									Console.WriteLine("Multiblock files not supported yet!");
									Heap.MemFree((uint)xBuffer);
									return null;
								}
								if (xINode.Block4 != 0) {
									Console.WriteLine("Multiblock files not supported yet!");
									Heap.MemFree((uint)xBuffer);
									return null;
								}
								if (xINode.Block5 != 0) {
									Console.WriteLine("Multiblock files not supported yet!");
									Heap.MemFree((uint)xBuffer);
									return null;
								}
								if (xINode.Block6 != 0) {
									Console.WriteLine("Multiblock files not supported yet!");
									Heap.MemFree((uint)xBuffer);
									return null;
								}
								if (xINode.Block7 != 0) {
									Console.WriteLine("Multiblock files not supported yet!");
									Heap.MemFree((uint)xBuffer);
									return null;
								}
								if (xINode.Block8 != 0) {
									Console.WriteLine("Multiblock files not supported yet!");
									Heap.MemFree((uint)xBuffer);
									return null;
								}
								if (xINode.Block9 != 0) {
									Console.WriteLine("Multiblock files not supported yet!");
									Heap.MemFree((uint)xBuffer);
									return null;
								}
								if (xINode.Block10 != 0) {
									Console.WriteLine("Multiblock files not supported yet!");
									Heap.MemFree((uint)xBuffer);
									return null;
								}
								if (xINode.Block11 != 0) {
									Console.WriteLine("Multiblock files not supported yet!");
									Heap.MemFree((uint)xBuffer);
									return null;
								}
								if (xINode.Block12 != 0) {
									Console.WriteLine("Multiblock files not supported yet!");
									Heap.MemFree((uint)xBuffer);
									return null;
								}
								if (xINode.Block13 != 0) {
									Console.WriteLine("Multiblock files not supported yet!");
									Heap.MemFree((uint)xBuffer);
									return null;
								}
								if (xINode.Block14 != 0) {
									Console.WriteLine("Multiblock files not supported yet!");
									Heap.MemFree((uint)xBuffer);
									return null;
								}
								if (xINode.Block15 != 0) {
									Console.WriteLine("Multiblock files not supported yet!");
									Heap.MemFree((uint)xBuffer);
									return null;
								}
								#endregion
								DebugUtil.SendExt2_INode((uint)((g * mSuperBlock->INodesPerGroup) + i), g, &xINodeTable[i % (mBackend.BlockSize / sizeof(INode))]);
								if (!mBackend.ReadBlock((uint)(xINode.Block1 * (mBlockSize / mBackend.BlockSize)), xByteBuffer)) {
									Console.WriteLine("[Ext2] Error reading INode entries");
									Heap.MemFree((uint)xBuffer);
									return null;
								}
								byte[] xResult = new byte[xResultSize];
								for (int b = 0; b < xResultSize; b++) {
									xResult[b] = xByteBuffer[b];
								}
								Heap.MemFree((uint)xBuffer);
								return xResult;
							} else {
								if ((xINode.Mode & INodeModeEnum.Directory) != 0) {
									if (!mBackend.ReadBlock((uint)(xINode.Block1 * (mBlockSize / mBackend.BlockSize)), xByteBuffer)) {
										Console.WriteLine("[Ext2] Error reading INode entries");
										Heap.MemFree((uint)xBuffer);
										return null;
									}
									DirectoryEntry* xEntryPtr = (DirectoryEntry*)xBuffer;
									uint xTotalSize = xINode.Size;
									while (xTotalSize != 0) {
										uint xPtrAddress = (uint)xEntryPtr;
										char[] xName = new char[xEntryPtr->NameLength];
										byte* xNamePtr = &xEntryPtr->FirstNameChar;
										for (int c = 0; c < xName.Length; c++) {
											xName[c] = (char)xNamePtr[c];
										}
										if (EqualsName(xPath[xPathPointer], xName)) {
											xPathPointer++;
											xCurrentINode = xEntryPtr->@INode;
											if (xPathPointer == xPath.Length) {
												xResultSize = xINode.Size;
											}
											continue;
										}
										xPtrAddress += xEntryPtr->RecordLength;
										xTotalSize -= xEntryPtr->RecordLength;
										xEntryPtr = (DirectoryEntry*)xPtrAddress;
									}
								}
							}
							xCount++;
						}
					}
				}
			}
			Heap.MemFree((uint)xBuffer);
			return null;
		}

		private static bool EqualsName(string a1, char[] a2) {
			if (a1 == null || a2 == null) {
				return false;
			}
			if (a1.Length != a2.Length) {
				return false;
			}
			for (int i = 0; i < a1.Length; i++) {
				if (a1[i] != a2[i]) {
					return false;
				}
			}
			return true;
		}
	}
}
