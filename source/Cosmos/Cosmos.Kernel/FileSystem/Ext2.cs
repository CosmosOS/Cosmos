using System;
using Cosmos.Hardware.Storage;
using System.Collections.Generic;
using System.IO;

namespace Cosmos.Kernel.FileSystem {
	public unsafe partial class Ext2 {
		private class FileStream: Stream {
			private uint mINodeNumber;
			private Ext2 mFilesystem;
			private uint mPosition = 0;
			public FileStream(uint aINodeNumber, Ext2 aFilesystem) {
				mINodeNumber = aINodeNumber;
				mFilesystem = aFilesystem;
			}

			public override bool CanRead {
				get {
					return true;
				}
			}

			public override bool CanSeek {
				get {
					return true;
				}
			}

			public override bool CanWrite {
				get {
					return false;
				}
			}

			public override void Flush() {
			}

			public override long Length {
				get {
					INode xINode;
					mFilesystem.ReadINode(mINodeNumber, out xINode);
					long xSize = xINode.Size;
					return xSize;
				}
			}

			public override long Position {
				get {
					return mPosition;
				}
				set {
					mPosition = (uint)value;
				}
			}

			// todo: add support for reading one chunk of data which spans multiple logical blocks
			public override int Read(byte[] buffer, int offset, int count) {
				uint xBlock = (mPosition / mFilesystem.mBlockSize);
				if (xBlock != 0) {
					return 0;
				}
				INode xINode;
				if (!mFilesystem.ReadINode(mINodeNumber, out xINode)) {
					return -1;
				}
				byte* xBuffer = (byte*)Heap.MemAlloc(mFilesystem.mBlockSize);
				if (!mFilesystem.ReadINodeContents(&xINode, xBlock, xBuffer)) {
					return -2;
				}
				uint xBufferOffset = mPosition % mFilesystem.mBlockSize;
				for (int i = 0; i < count; i++) {
					buffer[offset + i] = xBuffer[xBufferOffset + i];
				}
				return count;
			}

			public override long Seek(long offset, SeekOrigin origin) {
				throw new NotImplementedException();
			}

			public override void SetLength(long value) {
				throw new NotImplementedException();
			}

			public override void Write(byte[] buffer, int offset, int count) {
				throw new NotImplementedException();
			}
		}
		private Storage mBackend;
		private SuperBlock* mSuperBlock;
		private uint mBlockSize;
		private uint mGroupsCount;
		private uint mGroupDescriptorsPerBlock;
		private GroupDescriptor*[] mGroupDescriptors;
		public const ushort EXT2_ROOT_INO = 0x02;
		

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
			mGroupDescriptors = new GroupDescriptor*[mGroupsCount];
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
				GroupDescriptor* xItem = (GroupDescriptor*)Heap.MemAlloc((uint)sizeof(GroupDescriptor));
				CopyPointers((byte*)&xDescriptorPtr[i % mGroupDescriptorsPerBlock], (byte*)xItem, (uint)sizeof(GroupDescriptor));
				mGroupDescriptors[i] = xItem;
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

		private static void CopyPointers(byte* aSource, byte* aDest, uint aLength) {
			for (int i = 0; i < aLength; i++) {
				aDest[i] = aSource[i];
			}
		}

		public Stream OpenFile(string[] xPath) {
			ushort* xBuffer = (ushort*)Heap.MemAlloc(mBackend.BlockSize);
			byte* xExt2BlockBuffer = (byte*)Heap.MemAlloc(mBlockSize);
			INode xCurrentINode;
			if (!ReadINode(EXT2_ROOT_INO, out xCurrentINode)) {
				Heap.MemFree((uint)xBuffer);
				Heap.MemFree((uint)xExt2BlockBuffer);
				return null;
			}
			bool xCurrentINodeChanged = true;
			uint xInspectedINodeCount = 0;
			uint xINodeNumber = EXT2_ROOT_INO;
			for (int i = 0; i < xPath.Length; i++) {
				Console.Write("ReadFile, Iteration ");
				ATAOld.WriteNumber((uint)i, 8);
				Console.WriteLine("");
				if (!xCurrentINodeChanged) {
					Console.WriteLine("Terminating for loop, CurrentINode didn't change");
					Heap.MemFree((uint)xBuffer);
					Heap.MemFree((uint)xExt2BlockBuffer);
					return null;
				}
				xCurrentINodeChanged = false;
				if (!ReadINodeContents(&xCurrentINode, 0, xExt2BlockBuffer)) {
					Heap.MemFree((uint)xBuffer);
					Heap.MemFree((uint)xExt2BlockBuffer);
					return null;
				}
				DirectoryEntry* xEntryPtr = (DirectoryEntry*)xExt2BlockBuffer;
				uint xTotalSize = mBlockSize;
				while (xTotalSize != 0) {
					DebugUtil.SendExt2_DirectoryEntry(xEntryPtr);
					uint xPtrAddress = (uint)xEntryPtr;
					char[] xName = new char[xEntryPtr->NameLength];
					byte* xNamePtr = &xEntryPtr->FirstNameChar;
					for (int c = 0; c < xName.Length; c++) {
						xName[c] = (char)xNamePtr[c];
					}
					xInspectedINodeCount++;
					if (EqualsName(xPath[i], xName)) {
						if (!ReadINode(xEntryPtr->INodeNumber, out xCurrentINode)) {
							Heap.MemFree((uint)xBuffer);
							Heap.MemFree((uint)xExt2BlockBuffer);
							return null;
						}
						xCurrentINodeChanged = true;
						xINodeNumber = xEntryPtr->INodeNumber;
						xTotalSize = 0;
						continue;
					}
					xPtrAddress += xEntryPtr->RecordLength;
					xTotalSize -= xEntryPtr->RecordLength;
					xEntryPtr = (DirectoryEntry*)xPtrAddress;
				}
			}
			if (xCurrentINodeChanged) {
				DebugUtil.SendMessage("Ext2", "ReadFile, for loop exited with an inode change");
			} else {
				DebugUtil.SendMessage("Ext2", "ReadFile, for loop exited without an inode change");
			}
			if ((xCurrentINode.Mode & INodeModeEnum.RegularFile) == 0) {
				Console.WriteLine("No file after for loop");
				return null;
			}
			Stream xResult = new FileStream(xINodeNumber, this);
			Heap.MemFree((uint)xBuffer);
			Heap.MemFree((uint)xExt2BlockBuffer);
			return xResult;
		}

		public unsafe byte[] ReadFile(string[] xPath) {
			ushort* xBuffer = (ushort*)Heap.MemAlloc(mBackend.BlockSize);
			byte* xExt2BlockBuffer = (byte*)Heap.MemAlloc(mBlockSize);
			INode xCurrentINode;
			if (!ReadINode(EXT2_ROOT_INO, out xCurrentINode)) {
				Heap.MemFree((uint)xBuffer);
				Heap.MemFree((uint)xExt2BlockBuffer);
				return null;
			}
			bool xCurrentINodeChanged = true;
			uint xInspectedINodeCount = 0;
			for (int i = 0; i < xPath.Length; i++) {
				Console.Write("ReadFile, Iteration ");
				ATAOld.WriteNumber((uint)i, 8);
				Console.WriteLine("");
				if (!xCurrentINodeChanged) {
					Console.WriteLine("Terminating for loop, CurrentINode didn't change");
					Heap.MemFree((uint)xBuffer);
					Heap.MemFree((uint)xExt2BlockBuffer);
					return null;
				}
				xCurrentINodeChanged = false;
				if (!ReadINodeContents(&xCurrentINode, 0, xExt2BlockBuffer)) {
					Heap.MemFree((uint)xBuffer);
					Heap.MemFree((uint)xExt2BlockBuffer);
					return null;
				}
				DirectoryEntry* xEntryPtr = (DirectoryEntry*)xExt2BlockBuffer;
				uint xTotalSize = mBlockSize;
				while (xTotalSize != 0) {
					DebugUtil.SendExt2_DirectoryEntry(xEntryPtr);
					uint xPtrAddress = (uint)xEntryPtr;
					char[] xName = new char[xEntryPtr->NameLength];
					byte* xNamePtr = &xEntryPtr->FirstNameChar;
					for (int c = 0; c < xName.Length; c++) {
						xName[c] = (char)xNamePtr[c];
					}
					xInspectedINodeCount++;
					if (EqualsName(xPath[i], xName)) {
						if (!ReadINode(xEntryPtr->INodeNumber, out xCurrentINode)) {
							Heap.MemFree((uint)xBuffer);
							Heap.MemFree((uint)xExt2BlockBuffer);
							return null;
						}
						xCurrentINodeChanged = true;
						xTotalSize = 0;
						continue;
					}
					xPtrAddress += xEntryPtr->RecordLength;
					xTotalSize -= xEntryPtr->RecordLength;
					xEntryPtr = (DirectoryEntry*)xPtrAddress;
				}
			}
			if (xCurrentINodeChanged) {
				DebugUtil.SendMessage("Ext2", "ReadFile, for loop exited with an inode change");
			} else {
				DebugUtil.SendMessage("Ext2", "ReadFile, for loop exited without an inode change");
			}
			if ((xCurrentINode.Mode & INodeModeEnum.RegularFile) == 0) {
				Console.WriteLine("No file after for loop");
				return null;
			}
			byte[] xResult;//= new byte[mBlockSize];
			if (xCurrentINode.Size < mBlockSize) {
				xResult = new byte[xCurrentINode.Size];
			} else {
				xResult = new byte[mBlockSize];
			}
			ReadINodeContents(&xCurrentINode, 0, xExt2BlockBuffer);
			for (int i = 0; i < xResult.Length; i++) {
				xResult[i] = xExt2BlockBuffer[i];
			}
			//Console.WriteLine("ReadFile, not completely implemented yet");
			//Console.Write("    Found file size = ");
			//ATAOld.WriteNumber(xCurrentINode.Size, 32);
			//Console.WriteLine("");
			Heap.MemFree((uint)xBuffer);
			Heap.MemFree((uint)xExt2BlockBuffer);
			return xResult;
		}

		private unsafe bool ReadINode(uint aINodeNumber, out INode aINode) {
			uint xGroup = (aINodeNumber - 1) / mSuperBlock->INodesPerGroup;
			uint xIndex = (aINodeNumber - 1) % mSuperBlock->INodesPerGroup;
			uint xByteIndexInGroup = (xIndex * ((byte)sizeof(INode)));
			uint xStorageBlock = (uint)((mGroupDescriptors[xGroup]->INodeTable * (1024 << mSuperBlock->LogBlockSize)) / mBackend.BlockSize);
			xStorageBlock += xByteIndexInGroup / mBackend.BlockSize;
			uint xByteIndexInBackendBlock = xByteIndexInGroup / mBackend.BlockSize;
			ushort* xBuffer = (ushort*)Heap.MemAlloc(mBackend.BlockSize);
			aINode = default(INode);
			if (!mBackend.ReadBlock(xStorageBlock, (byte*)xBuffer)) {
				Heap.MemFree((uint)xBuffer);
				Console.WriteLine("ReadINode, Reading storage block failed");
				return false;
			}
			//byte* xINodeBuff = (byte*)((uint)xBuffer) + xByteIndexInBackendBlock;
			INode* xINodePtr = (INode*)xBuffer;
			DebugUtil.SendNumber("Ext2", "ReadINode, INodePointer index", ((aINodeNumber - 1) % (mBackend.BlockSize / ((byte)sizeof(INode)))), 8);
			xINodePtr = &xINodePtr[((aINodeNumber - 1) % (byte)(mBackend.BlockSize / ((byte)sizeof(INode))))];
			aINode = *xINodePtr;
			DebugUtil.SendExt2_INode(aINodeNumber, xINodePtr);
			Heap.MemFree((uint)xBuffer);
			return true;
		}

		/// <summary>
		/// Reads the logical block at index <paramref name="aBlock"/>. Make sure the buffer is of the correct size
		/// </summary>
		private bool ReadINodeContents(INode* aINode, uint aBlock, byte* aBuffer) {
			uint xBlock;
			if (aBlock == 0) {
				xBlock = aINode->Block1;
			} else {
				Console.WriteLine("Ext2|ReadINodeContents, reading ahead of first block not yet supported!");
				return false;
			}
			uint xBase = xBlock * (mBlockSize / mBackend.BlockSize);
			for (int i = 0; i < (mBlockSize / mBackend.BlockSize); i++) {
				byte* xTempBuffer = (byte*)(((uint)aBuffer) + (i * mBackend.BlockSize));
				if (!mBackend.ReadBlock((uint)(xBase + i), xTempBuffer)) {
					Console.WriteLine("ReadINodeContents, Reading of storage block failed!");
					return false;
				}
			}
			return true;
		}

		public unsafe byte[] ReadFileOld(string[] xPath) {
			ushort* xBuffer = (ushort*)Heap.MemAlloc(mBackend.BlockSize);
			byte* xByteBuffer = (byte*)xBuffer;
			bool[] xUsedINodes = new bool[32];
			uint xCount = 0;
			INode* xINodeTable = (INode*)xBuffer;
			uint xPathPointer = 0;
			uint xCurrentINode = 2;
			int xIterations = 0;
			uint xResultSize = 0;
			DebugUtil.SendNumber("Ext2", "GroupDescriptors.Length", (uint)mGroupDescriptors.Length, 32);
			DebugUtil.SendNumber("Ext2", "ReadFile, xPath.Length", (uint)xPath.Length, 32);
			uint xOldCurrentINode = 0;
			while (xPathPointer <= xPath.Length) {
				DebugUtil.SendNumber("Ext2", "ReadFile, while iteration", (uint)xIterations, 8);
				if (xIterations == 5) {
					Console.WriteLine("DEBUG: Stopping iteration");
					break;
				}
				if (xCurrentINode == xOldCurrentINode) {
					DebugUtil.SendError("Ext2", "ReadFile, CurrentINode value didnt change in while loop!");
					return null;
				}
				xOldCurrentINode = xCurrentINode;
				DebugUtil.SendNumber("Ext2", "Current INode", xCurrentINode, 32);
				for (uint g = 0; g != (mGroupDescriptors.Length - 1); g++) {
					GroupDescriptor* xGroupDescriptor = mGroupDescriptors[g];
					DebugUtil.SendExt2_GroupDescriptor("ReadFile", 0, (int)g, (uint)xGroupDescriptor, xGroupDescriptor);
					uint xTemp = ((xGroupDescriptor->INodeBitmap) * (mBlockSize / mBackend.BlockSize));
					DebugUtil.SendNumber("Ext2", "INodeBitmap block", xTemp, 32);
					if (!mBackend.ReadBlock((uint)((xGroupDescriptor->INodeBitmap) * (mBlockSize / mBackend.BlockSize)), xByteBuffer)) {
						Heap.MemFree((uint)xBuffer);
						return null;
					}
					xTemp = *(uint*)xBuffer;
					DebugUtil.SendNumber("Ext2", "INodeBitmap", xTemp, 32);
					if (!ConvertBitmapToBoolArray((uint*)xBuffer, xUsedINodes)) {
						Heap.MemFree((uint)xBuffer);
						return null;
					}
					for (int i = 0; i != 31; i++) {
						if ((i % (mBackend.BlockSize / sizeof(INode))) == 0) {
							uint index = (uint)((i % mSuperBlock->INodesPerGroup) * sizeof(INode));
							uint offset = index / mBackend.BlockSize;
							if (!mBackend.ReadBlock((uint)((xGroupDescriptor->INodeTable * (mBlockSize / mBackend.BlockSize)) + offset), xByteBuffer)) {
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
							DebugUtil.SendExt2_INode((uint)((g * mSuperBlock->INodesPerGroup) + i), &xINodeTable[i % (mBackend.BlockSize / sizeof(INode))]);
							if (xPathPointer == xPath.Length) {
								if (!mBackend.ReadBlock((uint)(xINodeTable[i % (mBackend.BlockSize / sizeof(INode))].Block1 * (mBlockSize / mBackend.BlockSize)), xByteBuffer)) {
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
								if ((xINodeTable[i % (mBackend.BlockSize / sizeof(INode))].Mode & INodeModeEnum.Directory) != 0) {
									if (!mBackend.ReadBlock((uint)(xINodeTable[i % (mBackend.BlockSize / sizeof(INode))].Block1 * (mBlockSize / mBackend.BlockSize)), xByteBuffer)) {
										Console.WriteLine("[Ext2] Error reading INode entries");
										Heap.MemFree((uint)xBuffer);
										return null;
									}
									DirectoryEntry* xEntryPtr = (DirectoryEntry*)xBuffer;
									uint xTotalSize = xINodeTable[i % (mBackend.BlockSize / sizeof(INode))].Size;
									while (xTotalSize != 0) {
										uint xPtrAddress = (uint)xEntryPtr;
										char[] xName = new char[xEntryPtr->NameLength];
										byte* xNamePtr = &xEntryPtr->FirstNameChar;
										for (int c = 0; c < xName.Length; c++) {
											xName[c] = (char)xNamePtr[c];
										}
										if (EqualsName(xPath[xPathPointer], xName)) {
											xPathPointer++;
											xCurrentINode = xEntryPtr->INodeNumber;
											if (xPathPointer == xPath.Length) {
												xResultSize = xINodeTable[i % (mBackend.BlockSize / sizeof(INode))].Size;
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

		public string[] GetDirectoryEntries(string[] aPath) {
			List<string> xResult = new List<string>(32);
			ushort* xBuffer = (ushort*)Heap.MemAlloc(mBackend.BlockSize);
			byte* xExt2BlockBuffer = (byte*)Heap.MemAlloc(mBlockSize);
			INode xCurrentINode;
			if (!ReadINode(EXT2_ROOT_INO, out xCurrentINode)) {
				Heap.MemFree((uint)xBuffer);
				Heap.MemFree((uint)xExt2BlockBuffer);
				return null;
			}
			bool xCurrentINodeChanged = true;
			uint xInspectedINodeCount = 0;
			for (int i = 0; i < aPath.Length; i++) {
				Console.Write("ReadFile, Iteration ");
				ATAOld.WriteNumber((uint)i, 8);
				Console.WriteLine("");
				if (!xCurrentINodeChanged) {
					Console.WriteLine("Terminating for loop, CurrentINode didn't change");
					Heap.MemFree((uint)xBuffer);
					Heap.MemFree((uint)xExt2BlockBuffer);
					return null;
				}
				xCurrentINodeChanged = false;
				if (!ReadINodeContents(&xCurrentINode, 0, xExt2BlockBuffer)) {
					Heap.MemFree((uint)xBuffer);
					Heap.MemFree((uint)xExt2BlockBuffer);
					return null;
				}
				DirectoryEntry* xEntryPtr = (DirectoryEntry*)xExt2BlockBuffer;
				uint xTotalSize = mBlockSize;
				while (xTotalSize != 0) {
					DebugUtil.SendExt2_DirectoryEntry(xEntryPtr);
					uint xPtrAddress = (uint)xEntryPtr;
					char[] xName = new char[xEntryPtr->NameLength];
					byte* xNamePtr = &xEntryPtr->FirstNameChar;
					for (int c = 0; c < xName.Length; c++) {
						xName[c] = (char)xNamePtr[c];
					}
					xInspectedINodeCount++;
					if (EqualsName(aPath[i], xName)) {
						if (!ReadINode(xEntryPtr->INodeNumber, out xCurrentINode)) {
							Heap.MemFree((uint)xBuffer);
							Heap.MemFree((uint)xExt2BlockBuffer);
							return null;
						}
						xCurrentINodeChanged = true;
						xTotalSize = 0;
						continue;
					}
					xPtrAddress += xEntryPtr->RecordLength;
					xTotalSize -= xEntryPtr->RecordLength;
					xEntryPtr = (DirectoryEntry*)xPtrAddress;
				}
			}
			if (xCurrentINodeChanged) {
				DebugUtil.SendMessage("Ext2", "ReadFile, for loop exited with an inode change");
			} else {
				DebugUtil.SendMessage("Ext2", "ReadFile, for loop exited without an inode change");
			}
			if ((xCurrentINode.Mode & INodeModeEnum.Directory) == 0) {
				Console.WriteLine("Ext2|GetDirectoryEntries, No directory after for loop");
				return null;
			}
			if (!ReadINodeContents(&xCurrentINode, 0, xExt2BlockBuffer)) {
				Heap.MemFree((uint)xBuffer);
				Heap.MemFree((uint)xExt2BlockBuffer);
				return null;
			}
			DirectoryEntry* xEntry = (DirectoryEntry*)xExt2BlockBuffer;
			uint xSize = mBlockSize;
			DebugUtil.SendMessage("Ext2", "GetDirectoryEntries");
			while (xSize != 0) {
				DebugUtil.SendExt2_DirectoryEntry(xEntry);
				uint xPtrAddress = (uint)xEntry;
				char[] xName = new char[xEntry->NameLength];
				byte* xNamePtr = &xEntry->FirstNameChar;
				for (int c = 0; c < xName.Length; c++) {
					byte b= xNamePtr[c];
					xName[c] = (char)b;
				}
				xResult.Add(new String(xName));
				xPtrAddress += xEntry->RecordLength;
				xSize -= xEntry->RecordLength;
				xEntry = (DirectoryEntry*)xPtrAddress;
			}
			Heap.MemFree((uint)xBuffer);
			Heap.MemFree((uint)xExt2BlockBuffer);

			return xResult.ToArray();
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
