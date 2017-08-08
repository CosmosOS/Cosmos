//#define EXT2Debug
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Hardware2;
using HW=Cosmos.Hardware2;
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
            Console.WriteLine("Start Ext2.Initialize");
            mBuffer = new byte[mBackend.BlockSize];
            fixed (byte* xBufferAddress = &mBuffer[0]) {
                mBufferAddress = xBufferAddress;
            }
            // first get the superblock;
            var mBufferAsSuperblock = (SuperBlock*)mBufferAddress;
            int xAddr = (int)mBufferAddress;
            Console.WriteLine("Buffer address: " + xAddr);
            Console.WriteLine("Start reading superblock");
            mBackend.ReadBlock(2,
                               mBuffer);
            Console.WriteLine("End reading");
            mSuperblock = *mBufferAsSuperblock;
            DebugUtil.Send_Ext2SuperBlock(mSuperblock);
            // read the group descriptors
            Console.WriteLine("INodeCount: " + mSuperblock.INodesCount);
            Console.WriteLine("INode#1: " + mBufferAddress[0]);
            Console.WriteLine("INode#2: " + mBufferAddress[1]);
            Console.WriteLine("INode#3: " + mBufferAddress[2]);
            Console.WriteLine("INode#4: " + mBufferAddress[3]);

            Console.WriteLine("BlockCount: " + mSuperblock.BlockCount);
            Console.WriteLine("INodesPerGroup: " + (int)mSuperblock.INodesPerGroup);
            if (mSuperblock.INodesPerGroup == 0x4000)
            {
                Console.WriteLine("INodesPerGroup has correct value!");
            }
            uint xGroupDescriptorCount = mSuperblock.INodesCount / mSuperblock.INodesPerGroup;
            mGroupDescriptors = new GroupDescriptor[xGroupDescriptorCount];
            var xDescriptorPtr = (GroupDescriptor*)mBufferAddress;
            Console.WriteLine("Process GroupDescriptors: " + xGroupDescriptorCount);
            //Console.ReadLine();
            for (int i = 0; i < xGroupDescriptorCount; i++) {
                Console.WriteLine("Processing GroupDescriptor " + i);
                uint xATABlock ;

                if ( BlockSize == 1024 )
                {
                    xATABlock = ( BlockSize * 2 ) / mBackend.BlockSize ;
                }
                else
                {
                    xATABlock = ( BlockSize ) / mBackend.BlockSize ;
                }

                xATABlock += (uint)(i / 16);
                if ((i % 16) == 0) {
                    Console.WriteLine("Read new GroupDescriptorBlock");
                    mBackend.ReadBlock(xATABlock,
                                       mBuffer);
                    Console.WriteLine("End Read");
                }
                mGroupDescriptors[i] = xDescriptorPtr[i % 16];
                Console.WriteLine("End of GroupDescriptor check");
            }
            Console.WriteLine("Send GroupDescriptors to log");
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

        private bool GetBitState(uint aBitmapStart,
                                 int aIndex) {
            var xPhyBlock = aBitmapStart * 8;
            xPhyBlock += (uint)aIndex % 4096;
            mBackend.ReadBlock(xPhyBlock,
                               mBuffer);
            aIndex /= 4096;
            int xBufferIndex = aIndex / 8;
            aIndex /= 8;
            return (mBufferAddress[xBufferIndex] & (1 << aIndex)) != 0;
        }

        private static uint ToUInt32(byte[] buffer, int index)
        {
            return (uint)((((buffer[index + 3] << 0x18) | (buffer[index + 2] << 0x10)) | (buffer[index + 1] << 8)) | buffer[index]);
        }

        private static ushort ToUInt16(byte[] buffer, int index)
        {
            return (ushort)((buffer[index + 1] << 8) | buffer[index]);
        }

        public override FilesystemEntry[] GetDirectoryListing(ulong aId) {
            var xBaseINodeNumber = (uint)aId;
            INode xINode;
            GetINode(xBaseINodeNumber,
                     out xINode);
            byte[] xFSBuffer = new byte[BlockSize];
            var xResult = new List<FilesystemEntry>(10);
            var xDirEntriesPerFSBlock = BlockSize / sizeof(DirectoryEntry);
            uint xBlockId = 0;
            while (ReadINodeBlock(ref xINode,
                                  xBlockId,
                                  xFSBuffer)) {
                //HW.DebugUtil.WriteBinary("Ext2",
                //                         "Directory Entry binary",
                //                         xFSBuffer,
                //                         0,
                //                         (int)BlockSize);
                //HW.DebugUtil.SendNumber("Ext2",
                //                        "First byte of datablock",
                //                        xFSBuffer[0],
                //                        8);
                int xIndex = 0;
                var xIteration = 0;
                while (xIndex < BlockSize) {
                    var xINodeNumber = ToUInt32(xFSBuffer, xIndex);
                    var xRecLength = ToUInt16(xFSBuffer, xIndex + 4);
                    
                    // only include used items
                    if (xINodeNumber > 0) {
                        // only include non ".." or "." items
                        if (xINodeNumber != xBaseINodeNumber) {
                            var xNameLength = xFSBuffer[xIndex + 6];
                            var xFileType = xFSBuffer[xIndex + 7];
                            if (!(xNameLength == 2 && xFSBuffer[xIndex + 8] == (byte)'.' && xFSBuffer[xIndex + 9] == (byte)'.')) {
                                var xFSEntry = new FilesystemEntry
                                {
                                    Id = xINodeNumber,
                                    IsDirectory = (xFileType == 2),
                                    IsReadonly = true,
                                    Filesystem = this
                                };
                                //xFSEntry.Size = GetINode(xINodeNumber).Size;
                                char[] xName = new char[xNameLength];
                                for (int c = 0; c < xName.Length; c++)
                                {
                                    xName[c] = (char)xFSBuffer[xIndex + 8 + c];
                                }
                                xFSEntry.Name = new string(xName);
                                if (!xFSEntry.Name.Equals("lost+found"))
                                {
                                    xResult.Add(xFSEntry);
                                }
                            }
                        }
                    }
                    xIndex += xRecLength;
                    xIteration++;
                    if (xIteration == 5)
                    {
                        break;
                    }
                    //break;
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
            //HW.DebugUtil.SendMessage("Ext2",
            //                         "Reading INode");
            //HW.DebugUtil.SendNumber("Ext2",
            //                        "INode Id",
            //                        (uint)xId,
            //                        32);
            //HW.DebugUtil.SendNumber("Ext2",
            //                        "Group",
            //                        xGroup,
            //                        32);
            //HW.DebugUtil.SendNumber("Ext2",
            //                        "GroupIndex",
            //                        xGroupIndex,
            //                        32);
            // read the inode:
            var xTableBlockOffset = (uint)(xGroupIndex % (ulong)(BlockSize / sizeof(INode)));
            var xTableBlock = mGroupDescriptors[xGroup].INodeTable;
            xTableBlock += xGroupIndex / (uint)(BlockSize / sizeof(INode));
            xTableBlock *= (BlockSize / mBackend.BlockSize);
            xTableBlock += xTableBlockOffset / (uint)(mBackend.BlockSize / sizeof(INode));
            xTableBlockOffset = xTableBlockOffset % (uint)(mBackend.BlockSize / sizeof(INode));
            mBackend.ReadBlock(xTableBlock,
                               mBuffer);
            INode xINode=new INode();
            fixed (byte* xTempAddress = &mBuffer[0]) {
                var xINodeAddress = (INode*)xTempAddress;
                xINode = xINodeAddress[(int)xTableBlockOffset];
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
        private bool ReadINodeBlock(ref INode aINode,
                                    uint aBlockId,
                                    byte[] aBuffer) {
            if (aBuffer.Length != BlockSize) {
                throw new Exception("Incorrect buffer size!");
            }
            if (aINode.Blocks <= aBlockId)
            {
                return false;
            }
            fixed (INode* xINodePtr = &aINode)
            {
                uint xINodeAddr = (uint)xINodePtr;
                uint* xBlocks = (uint*)(xINodeAddr + INodeConsts.BlockOffset);
                if (aBlockId >= 0 && aBlockId <= 11)
                {
                    var xBlockId = xBlocks[aBlockId];
                    if (xBlockId == 0)
                    {
                        return false;
                    }
                    ReadDataBlock(xBlockId,
                                  aBuffer);
                    return true;
                }
                else
                {
                    uint xIndirectBlockRefsPerDataBlock = BlockSize / 4;
                    if ((aBlockId - 12) < xIndirectBlockRefsPerDataBlock)
                    {
                        var xBlockId = xBlocks[12];
                        if (xBlockId == 0)
                        {
                            return false;
                        }
                        ReadDataBlock(xBlockId,
                                      aBuffer);
                        var xTheBlock = ToUInt32(aBuffer, (int)((aBlockId - 11) * 4));
                        ReadDataBlock(xTheBlock,
                                      aBuffer);
                        return true;
                    }
                    Console.WriteLine("Reading indirect blocks not yet supported!");
                }
            }
            return false;
        }

        private void ReadDataBlock(uint aBlockId,
                                   byte[] aBuffer) {
            var xPhyBlocksPerFSBlock = (BlockSize / mBackend.BlockSize);
            //aBlockId *= xPhyBlocksPerFSBlock;
            int xBlock = (int)(aBlockId * xPhyBlocksPerFSBlock);
            for (var i = 0; i < xPhyBlocksPerFSBlock; i++) {
                mBackend.ReadBlock((uint)(xBlock + i),
                                   mBuffer);
                for (int j = 0; j < (int)mBackend.BlockSize; j++)
                {
                    aBuffer[(int)((i * ((int)mBackend.BlockSize)) + j)] = mBuffer[j];
                }
                //Array.Copy(mBuffer,
                //           0,
                //           aBuffer,
                //           (mBackend.BlockSize * i),
                //           mBackend.BlockSize);
            }
        }

        public override bool ReadBlock(ulong aId,
                                       ulong aBlock,
                                       byte[] aBuffer) {
            INode xTheINode;
            uint xId = (uint)aId;
            GetINode(xId,
                     out xTheINode);
            return ReadINodeBlock(ref xTheINode,
                                  (uint)aBlock,
                                  aBuffer);
        }

        public static bool BlockDeviceContainsExt2(BlockDevice aDevice) {
            if (aDevice.BlockCount > 3)
            {
                byte[] xBuffer = new byte[aDevice.BlockSize];
                // todo: implement better detection
                aDevice.ReadBlock(2,
                                  xBuffer);
                bool xResult = (xBuffer[56] == 0x53 && xBuffer[57] == 0xEF);
                return xResult;
            }
            return false;
        }
    }
}