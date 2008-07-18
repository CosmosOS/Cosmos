using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Hardware;

namespace Cosmos.FileSystem.Ext2 {
    public unsafe partial class Ext2 : Filesystem {
        private readonly BlockDevice mBackend;
        private SuperBlock mSuperblock;
        private GroupDescriptor[] mGroupDescriptors;
        private byte[] mBuffer;
        private byte* mBufferAddress;
       

        public Ext2(BlockDevice aBackend) {
            mBackend = aBackend;
            Initialize();
            Console.Write("BlockSize: ");
            Console.WriteLine(BlockSize.ToString());
        }

        private void Initialize() {
            mBuffer= new byte[mBackend.BlockSize];
            fixed(byte* xBufferAddress = &mBuffer[0]) {
                mBufferAddress = xBufferAddress;
            }
            // first get the superblock;
            var mBufferAsSuperblock = (SuperBlock*)mBufferAddress;
            mBackend.ReadBlock(2, mBuffer);
            mSuperblock = *mBufferAsSuperblock;
            mSuperblock.DoSomething();
            Console.Write("INode count: ");
            Console.WriteLine(mSuperblock.INodesCount.ToString());
            Console.Write("LogBlockSize: ");
            Console.WriteLine(mSuperblock.LogBlockSize.ToString());
            Console.Write("BlockCount: ");
            Console.WriteLine(mSuperblock.BlockCount.ToString());
            Console.Write("Reserved Block Count: ");
            Console.WriteLine(mSuperblock.RBlocksCount.ToString());
            Console.Write("Free Block Count: ");
            Console.WriteLine(mSuperblock.FreeBlocksCount.ToString());
            Console.Write("Free INodes Count: ");
            Console.WriteLine(mSuperblock.FreeINodesCount.ToString());
            Console.Write("First Data Block: ");
            Console.WriteLine(mSuperblock.FirstDataBlock.ToString());
            Console.Write("Log Block Size: ");
            Console.WriteLine(mSuperblock.LogBlockSize.ToString());
            Console.Write("Log Frag Size: ");
            Console.WriteLine(mSuperblock.LogFragSize.ToString());
            Console.Write("Blocks per group: ");
            Console.WriteLine(mSuperblock.BlocksPerGroup.ToString());
        }

        public override uint BlockSize {
            get {
                uint xTemp = 1;
                return xTemp << mSuperblock.LogBlockSize;
            }
        }

        public override unsafe FilesystemEntry[] GetDirectoryListing(ulong aId) {
//            byte[] xBuffer = new byte[mBackend.BlockSize];
//            fixed (byte* xBufferAddress = &xBuffer[0]) {
//                mBackend.ReadBlock(2,
//                                   xBuffer);
//                var xSuperBlock = *(SuperBlock*)xBufferAddress;
//                int xBlockSize = (int)(1024 << (byte)(xSuperBlock.LogBlockSize));
//                uint xGroupsCount = xSuperBlock.INodesCount / xSuperBlock.INodesPerGroup;
//                uint xGroupDescriptorsPerBlock = (uint)(xBlockSize / sizeof(GroupDescriptor));
//                GroupDescriptor[] xGroupDescriptors = ReadGroupDescriptorsOfBlock(xSuperBlock.FirstDataBlock + 1,
//                                                                                  xSuperBlock,
//                                                                                  xBuffer);
//                if (xGroupDescriptors == null) {
//                    throw new Exception("Error reading GroupDescriptors!");
//                }
//                bool[] xUsedINodes = new bool[32];
//                uint xCount = 0;
//                INode* xINodeTable = (INode*)xBufferAddress;
//                for (uint g = 0; g < xGroupDescriptors.Length; g++) {
//                    GroupDescriptor xGroupDescriptor = xGroupDescriptors[g];
//                    mBackend.ReadBlock(((xGroupDescriptor.INodeBitmap) * 8), xBuffer);
//                    if (!ConvertBitmapToBoolArray((uint*)xBuffer,
//                                                  xUsedINodes)) {
//                        return;
//                    }
//                    for (int i = 0; i < 32; i++) {
//                        if ((i % 4) == 0) {
//                            uint index = (uint)((i % xSuperBlock.INodesPerGroup) * sizeof(INode));
//                            uint offset = index / 512;
//                            if (!Hardware.Storage.ATAOld.ReadDataNew(aController,
//                                                                     aDrive,
//                                                                     (int)((xGroupDescriptor.INodeTable * 8) + offset),
//                                                                     xBuffer)) {
//                                Console.WriteLine("[Ext2] Error reading INode table entries");
//                                return;
//                            }
//                        }
//                        uint xINodeIdentifier = (uint)((g * xSuperBlock.INodesPerGroup) + i + 1);
//                        if (xINodeIdentifier == 0xB) {
//                            continue;
//                        }
//                        if (xUsedINodes[i]) {
//                            INode xINode = xINodeTable[i % 4];
//                            //DebugUtil.SendExt2_INode((uint)((g * xSuperBlock.INodesPerGroup) + i), g, &xINodeTable[i % 4]);
//                            if ((xINode.Mode & INodeModeEnum.Directory) != 0) {
//                                //DebugUtil.SendNumber("Ext2", "Directory found", (uint)((g * xSuperBlock.INodesPerGroup) + i + 1), 32);
//                                if (!Hardware.Storage.ATAOld.ReadDataNew(aController,
//                                                                         aDrive,
//                                                                         (int)(xINode.Block1 * 8),
//                                                                         xBuffer)) {
//                                    Console.WriteLine("[Ext2] Error reading INode entries");
//                                    return;
//                                }
//                                DirectoryEntry* xEntryPtr = (DirectoryEntry*)xBuffer;
//                                uint xTotalSize = xINode.Size;
//                                while (xTotalSize != 0 /* && xEntryPtr->@INode != 0*/) {
//                                    //DebugUtil.SendExt2_DirectoryEntry(xEntryPtr);
//                                    uint xPtrAddress = (uint)xEntryPtr;
//                                    //if (xEntryPtr->@INode == 0xC) {
//                                    char[] xName = new char[xEntryPtr->NameLength];
//                                    byte* xNamePtr = &xEntryPtr->FirstNameChar;
//                                    for (int c = 0; c < xName.Length; c++) {
//                                        xName[c] = (char)xNamePtr[c];
//                                    }
//                                    string s = new String(xName);
//                                    //								DebugUtil.SendMessage("Ext2, DirectoryEntryName", s);
//                                    //}
//                                    xPtrAddress += xEntryPtr->RecordLength;
//                                    xTotalSize -= xEntryPtr->RecordLength;
//                                    xEntryPtr = (DirectoryEntry*)xPtrAddress;
//                                }
//                            }
//                            xCount++;
//                        }
//                    }
//                }
//            }
////			DebugUtil.SendNumber("Ext2", "Used INode count", xCount, 32);
            return null;
        }

        public override void ReadBlock(ulong aId,
                                       ulong aBlock,
                                       byte[] aBuffer) {
            throw new System.NotImplementedException();
        }
    }
}