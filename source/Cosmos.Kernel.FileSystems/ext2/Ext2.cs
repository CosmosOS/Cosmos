#define EXT2Debug
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Hardware;
using HW=Cosmos.Hardware;

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
        }

        private void Initialize() {
            mBuffer = new byte[mBackend.BlockSize];
            fixed (byte* xBufferAddress = &mBuffer[0]) {
                mBufferAddress = xBufferAddress;
            }
            // first get the superblock;
            var mBufferAsSuperblock = (SuperBlock*)mBufferAddress;
            mBackend.ReadBlock(2,
                               mBuffer);
            mSuperblock = *mBufferAsSuperblock;
            DebugUtil.Send_Ext2SuperBlock(mSuperblock);
            // read the group descriptors
            uint xGroupDescriptorCount = mSuperblock.INodesCount / mSuperblock.INodesPerGroup;
            mGroupDescriptors = new GroupDescriptor[xGroupDescriptorCount];
            var xDescriptorPtr = (GroupDescriptor*)mBufferAddress;
            for (int i = 0; i < xGroupDescriptorCount; i++) {
                uint xATABlock = 8;
                xATABlock += (uint)(i / 16);
                if ((i % 16) == 0) {
                    mBackend.ReadBlock(xATABlock,
                                       mBuffer);
                }
                mGroupDescriptors[i] = xDescriptorPtr[i % 16];
            }
            DebugUtil.Send_Ext2GroupDescriptors(mGroupDescriptors);
        }

        public override uint BlockSize {
            get {
                return ((uint)1024) << mSuperblock.LogBlockSize;
            }
        }

        public override ulong RootId {
            get {
                return 2;
            }
        }

        private bool GetBitState(ulong aBitmapStart,
                                 int aIndex) {
            var xPhyBlock = aBitmapStart * 8;
            xPhyBlock += (ulong)aIndex % 4096;
            mBackend.ReadBlock(xPhyBlock,
                               mBuffer);
            aIndex /= 4096;
            int xBufferIndex = aIndex / 8;
            aIndex /= 8;
            return (mBufferAddress[xBufferIndex] & (1 << aIndex)) != 0;
        }

        public override FilesystemEntry[] GetDirectoryListing(ulong aId) {
            var xId = (uint)(aId - 1);
            var xGroup = (uint)(xId / mSuperblock.INodesPerGroup);
            var xGroupIndex = (uint)(xId % mSuperblock.INodesPerGroup);
            HW.DebugUtil.SendNumber("Ext2",
                                    "INode Id",
                                    (uint)xId,
                                    32);
            HW.DebugUtil.SendNumber("Ext2",
                                    "Group",
                                    xGroup,
                                    32);
            HW.DebugUtil.SendNumber("Ext2",
                                    "GroupIndex",
                                    xGroupIndex,
                                    32);
            // read the inode:
            var xTableBlockOffset = (uint)(xGroupIndex % (ulong)(BlockSize / sizeof(INode)));
            var xTableBlock = mGroupDescriptors[xGroup].INodeTable;
            HW.DebugUtil.SendNumber("Ext2",
                                    "TableBlockOffset(1)",
                                    xTableBlockOffset,
                                    32);
            xTableBlock *= (BlockSize / mBackend.BlockSize);
            // below these two lines, the blocks are physical blocks!
            HW.DebugUtil.SendNumber("Ext2",
                                    "TableBlockOffset(2)",
                                    xTableBlockOffset,
                                    32);
            HW.DebugUtil.SendNumber("Ext2",
                                    "Physical TableBlock",
                                    xTableBlock,
                                    32);
            HW.DebugUtil.SendNumber("Ext2",
                                    "TableBlockOffset(Final)",
                                    xTableBlockOffset,
                                    32);
            mBackend.ReadBlock(xTableBlock,
                               mBuffer);
            INode xINode;
            fixed (byte* xTempAddress = &mBuffer[0]) {
                var xINodeAddress = (INode*)xTempAddress;
                xINode = xINodeAddress[(int)xTableBlockOffset];
                HW.DebugUtil.SendNumber("Ext2",
                                        "INode mode first byte",
                                        mBufferAddress[sizeof(INode)],
                                        32);
                HW.DebugUtil.SendNumber("Ext2",
                                        "INode mode",
                                        (ushort)xINode.Mode,
                                        32);
            }
            byte[] xFSBuffer = new byte[BlockSize];
            var xResult = new List<FilesystemEntry>(10);
            var xDirEntriesPerFSBlock = BlockSize / sizeof(DirectoryEntry);
            uint xBlockId = 0;
            while (ReadDataBlock(xINode,
                                 xBlockId,
                                 xFSBuffer)) {
                HW.DebugUtil.WriteBinary("Ext2",
                                         "Directory Entry binary",
                                         xFSBuffer,
                                         0,
                                         (int)BlockSize);
                HW.DebugUtil.SendNumber("Ext2",
                                        "First byte of datablock",
                                        xFSBuffer[0],
                                        8);
                int xIndex = 0;
                while(xIndex < BlockSize) {
                    var xINodeNumber = BitConverter.ToUInt32(xFSBuffer,
                                                       xIndex);
                    var xRecLength = BitConverter.ToUInt16(xFSBuffer,
                                                           xIndex+4);
                    if (xINodeNumber > 0) {
                        var xNameLength = xFSBuffer[xIndex + 6];
                        var xFileType = xFSBuffer[xIndex + 7];
                        var xFSEntry = new FilesystemEntry();
                        xFSEntry.Id = xINodeNumber;
                        xFSEntry.IsDirectory = xFileType == 2; // 2 == directory
                        xFSEntry.IsReadonly = true;
                        char[] xName = new char[xNameLength];
                        for (int c = 0; c < xName.Length; c++) {
                            xName[c] = (char)xFSBuffer[xIndex + 8 + c];
                        }
                        xFSEntry.Name = new string(xName);
                        xResult.Add(xFSEntry);
                    }
                    xIndex += xRecLength;
                }
                xBlockId++;
            }
            return xResult.ToArray();
        }

        /// <summary>
        /// Reads the contents of an inode, resolving all indirect/bi-indirect/tri-indirect block arrays
        /// </summary>
        /// <param name="aINode"></param>
        /// <param name="aBlockId">This is zero-based!</param>
        /// <param name="aBuffer"></param>
        /// <returns></returns>
        private bool ReadDataBlock(INode aINode,
                                   uint aBlockId,
                                   byte[] aBuffer) {
            if (aBuffer.Length != BlockSize) {
                throw new Exception("Incorrect buffer size!");
            }
            HW.DebugUtil.SendNumber("Ext2",
                                    "BlockId",
                                    aBlockId,
                                    32);
            if (aBlockId >= 0 && aBlockId <= 11) {
                uint* xBlocks = &aINode.Block;
                var xBlockId = xBlocks[aBlockId];
                if (xBlockId == 0) {
                    return false;
                }
                var xPhyBlocksPerFSBlock = (BlockSize / mBackend.BlockSize);
                HW.DebugUtil.SendNumber("Ext2",
                                        "PhyBlocksPerFSBlock",
                                        xPhyBlocksPerFSBlock,
                                        32);
                xBlockId *= xPhyBlocksPerFSBlock;
                for (var i = 0; i < xPhyBlocksPerFSBlock; i++) {
                    mBackend.ReadBlock((ulong)(xBlockId + i),
                                       mBuffer);
                    Array.Copy(mBuffer,
                               0,
                               aBuffer,
                               (mBackend.BlockSize * i),
                               mBackend.BlockSize);
                }
                HW.DebugUtil.SendNumber("Ext2",
                                        "BlockAddress",
                                        xBlockId,
                                        32);
                return true;
            }
            return false;
        }

        public override bool ReadBlock(ulong aId,
                                       ulong aBlock,
                                       byte[] aBuffer) {
            throw new System.NotImplementedException();
        }
    }
}