//#define EXT2Debug
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Hardware;
using HW=Cosmos.Hardware;
using Cosmos.Sys.FileSystem;

namespace Cosmos.Sys.FileSystem.Ext2 {
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
            var xBaseINodeNumber = (uint)aId;
            HW.DebugUtil.SendNumber("Ext2",
                                    "Getting DirectoryListing of INode",
                                    xBaseINodeNumber,
                                    32);
            INode xINode;
            GetINode(xBaseINodeNumber,
                     out xINode);
            byte[] xFSBuffer = new byte[BlockSize];
            var xResult = new List<FilesystemEntry>(10);
            var xDirEntriesPerFSBlock = BlockSize / sizeof(DirectoryEntry);
            uint xBlockId = 0;
            while (ReadINodeBlock(xINode,
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
                while (xIndex < BlockSize) {
                    var xINodeNumber = BitConverter.ToUInt32(xFSBuffer,
                                                             xIndex);
                    var xRecLength = BitConverter.ToUInt16(xFSBuffer,
                                                           xIndex + 4);
                    if (xINodeNumber > 0) {
                        if (xINodeNumber != xBaseINodeNumber) {
                            var xNameLength = xFSBuffer[xIndex + 6];
                            var xFileType = xFSBuffer[xIndex + 7];
                            if (!(xNameLength == 2 && xFSBuffer[xIndex + 8] == (byte)'.' && xFSBuffer[xIndex + 9] == (byte)'.')) {
                                var xFSEntry = new FilesystemEntry {
                                                                       Id = xINodeNumber,
                                                                       IsDirectory = (xFileType == 2),
                                                                       IsReadonly = true,
                                                                       Filesystem = this
                                                                   };
                                //xFSEntry.Size = GetINode(xINodeNumber).Size;
                                char[] xName = new char[xNameLength];
                                for (int c = 0; c < xName.Length; c++) {
                                    xName[c] = (char)xFSBuffer[xIndex + 8 + c];
                                }
                                xFSEntry.Name = new string(xName);
                                if (!xFSEntry.Name.Equals("lost+found")) {
                                    xResult.Add(xFSEntry);
                                }
                            }
                        }
                    }
                    xIndex += xRecLength;
                }
                xBlockId++;
            }
            for (int i = 0; i < xResult.Count; i++) {
                INode xTheINode;
                GetINode((uint)xResult[i].Id,
                         out xTheINode);
                xResult[i].Size = xTheINode.Size;
            }
            return xResult.ToArray();
        }

        private void GetINode(uint aINodeNumber,
                              out INode oINode) {
            var xId = aINodeNumber - 1;
            var xGroup = (uint)(xId / mSuperblock.INodesPerGroup);
            var xGroupIndex = (uint)(xId % mSuperblock.INodesPerGroup);
            HW.DebugUtil.SendMessage("Ext2",
                                     "Reading INode");
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
            xTableBlock += xGroupIndex / (uint)(BlockSize / sizeof(INode));
            HW.DebugUtil.SendNumber("Ext2",
                                    "TableBlockOffset(1)",
                                    xTableBlockOffset,
                                    32);
            xTableBlock *= (BlockSize / mBackend.BlockSize);
            xTableBlock += xTableBlockOffset / (uint)(mBackend.BlockSize / sizeof(INode));
            xTableBlockOffset = xTableBlockOffset % (uint)(mBackend.BlockSize / sizeof(INode));
            // below these two lines, the blocks are physical blocks!
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
            oINode = xINode;
        }

        /// <summary>
        /// Reads the contents of an inode, resolving all indirect/bi-indirect/tri-indirect block arrays
        /// </summary>
        /// <param name="aINode"></param>
        /// <param name="aBlockId">This is zero-based!</param>
        /// <param name="aBuffer"></param>
        /// <returns></returns>
        private bool ReadINodeBlock(INode aINode,
                                    uint aBlockId,
                                    byte[] aBuffer) {
            if (aBuffer.Length != BlockSize) {
                throw new Exception("Incorrect buffer size!");
            }
            HW.DebugUtil.SendNumber("Ext2",
                                    "BlockId",
                                    aBlockId,
                                    32);
            uint* xBlocks = &aINode.Block;
            if (aBlockId >= 0 && aBlockId <= 11) {
                var xBlockId = xBlocks[aBlockId];
                if (xBlockId == 0) {
                    return false;
                }
                ReadDataBlock(xBlockId,
                              aBuffer);
                return true;
            } else {
                uint xIndirectBlockRefsPerDataBlock = BlockSize / 4;
                HW.DebugUtil.SendNumber("Ext2",
                                        "Indirect block reference count per data block",
                                        xIndirectBlockRefsPerDataBlock,
                                        32);
                if ((aBlockId - 12) < xIndirectBlockRefsPerDataBlock) {
                    var xBlockId = xBlocks[12];
                    if (xBlockId == 0) {
                        return false;
                    }
                    ReadDataBlock(xBlockId,
                                  aBuffer);
                    var xTheBlock = BitConverter.ToUInt32(aBuffer,
                                                          (int)((aBlockId - 11) * 4));
                    ReadDataBlock(xTheBlock,
                                  aBuffer);
                    return true;
                }
                Console.WriteLine("Reading indirect blocks not yet supported!");
            }
            return false;
        }

        private void ReadDataBlock(uint aBlockId,
                                   byte[] aBuffer) {
            var xPhyBlocksPerFSBlock = (BlockSize / mBackend.BlockSize);
            HW.DebugUtil.SendNumber("Ext2",
                                    "PhyBlocksPerFSBlock",
                                    xPhyBlocksPerFSBlock,
                                    32);
            aBlockId *= xPhyBlocksPerFSBlock;
            for (var i = 0; i < xPhyBlocksPerFSBlock; i++) {
                mBackend.ReadBlock((ulong)(aBlockId + i),
                                   mBuffer);
                Array.Copy(mBuffer,
                           0,
                           aBuffer,
                           (mBackend.BlockSize * i),
                           mBackend.BlockSize);
            }
        }

        public override bool ReadBlock(ulong aId,
                                       ulong aBlock,
                                       byte[] aBuffer) {
            INode xTheINode;
            uint xId = (uint)aId;
            HW.DebugUtil.SendNumber("Ext2",
                                    "ReadingBlock of INode",
                                    xId,
                                    32);
            HW.DebugUtil.SendNumber("Ext2",
                                    "Reading Blocknr",
                                    (uint)aBlock,
                                    32);
            GetINode(xId,
                     out xTheINode);
            return ReadINodeBlock(xTheINode,
                                  (uint)aBlock,
                                  aBuffer);
        }

        public static bool BlockDeviceContainsExt2(BlockDevice aDevice) {
            byte[] xBuffer = new byte[aDevice.BlockSize];
            // todo: implement better detection
            try
            {
                aDevice.ReadBlock(2,
                                  xBuffer);
                Hardware.DebugUtil.WriteBinary("Ext2",
                                               "Detecting Ext2 (1)",
                                               xBuffer,
                                               55,
                                               4);
                return xBuffer[56] == 0x53 && xBuffer[57] == 0xEF;
            }
            catch {
                return false;
            }
        }
    }
}