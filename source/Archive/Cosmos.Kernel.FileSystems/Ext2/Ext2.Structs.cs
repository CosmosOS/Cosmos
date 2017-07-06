using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Cosmos.Sys.FileSystem.Ext2
{
    partial class Ext2
    {
        [StructLayout(LayoutKind.Explicit, Size = 84)]
        internal struct SuperBlock
        {
            /// <summary>
            /// Indicates the total number of INodes, both used and free, in the filesystem.
            /// </summary>
            [FieldOffset(0)]
            public uint INodesCount;

            /// <summary>
            /// Indicates the total number of blocks, both used and free, in the filesystem.
            /// </summary>
            [FieldOffset(4)]
            public uint BlockCount;

            /// <summary>
            /// Indicates the total number of blocks reserved for the usage of the super user. this is most useful
            /// if for some reason a user, malicious or not, fills the file system to capacity; the super user will 
            /// have this specified amount of free blocks at his disposal so he can edit and save configuration files.
            /// </summary>
            [FieldOffset(8)]
            public uint RBlocksCount;

            /// <summary>
            /// Indicates the total number of free blocks, including the number of reserved blocks (<see cref="RBlocksCount"/>).
            /// This is a sum of all free blocks of all the block groups.
            /// </summary>
            [FieldOffset(12)]
            public uint FreeBlocksCount;

            /// <summary>
            /// Indicates the total number of free INodes. This is a sum of all free nodes of all the block groups.
            /// </summary>
            [FieldOffset(16)]
            public uint FreeINodesCount;

            /// <summary>
            /// Identifies the first data block, in other words the id of the block containing the superblock
            /// structure.
            /// Note that this value is always 0 for filesystems with a block size larger than 1KB,
            /// and always 1 for filesystem with a blocksize of 1KB. The superblock is always starting at the
            /// 1024th byte of the disk, which normally happens to be the first byte of the 3rd sector.
            /// </summary>
            [FieldOffset(20)]
            public uint FirstDataBlock;

            /// <summary>
            /// The block size is computed using this value as the number of bits to shift left the value 1024.
            /// This value may only be positive.
            /// <code>
            ///		int xBlockSize = 1024 &lt;&lt; LogBlockSize;
            /// </code>
            /// </summary>
            [FieldOffset(24)]
            public int LogBlockSize;

            [FieldOffset(28)]
            public uint LogFragSize;

            [FieldOffset(32)]
            public uint BlocksPerGroup;

            [FieldOffset(36)]
            public uint FragsPerGroup;

            [FieldOffset(40)]
            public uint INodesPerGroup;

            [FieldOffset(44)]
            public uint MTime;

            [FieldOffset(48)]
            public uint WTime;

            [FieldOffset(52)]
            public ushort MntCount;

            [FieldOffset(54)]
            public ushort MaxMntCount;

            [FieldOffset(56)]
            public ushort Magic;

            [FieldOffset(58)]
            public ushort State;

            [FieldOffset(60)]
            public ushort Errors;

            [FieldOffset(62)]
            public ushort MinorRevLevel;

            [FieldOffset(64)]
            public uint LastCheck;

            [FieldOffset(68)]
            public uint CheckInterval;

            [FieldOffset(72)]
            public uint CreatorOS;

            [FieldOffset(76)]
            public uint RevLevel;

            [FieldOffset(80)]
            public ushort RefResUID;

            // up to byte 84 (includes 83..)
            [FieldOffset(82)]
            public ushort DefResGID;

            public void DoSomething()
            {
            }
        }

        /// <summary>
        /// For each group in the filesystem, a <see cref="GroupDescriptor"/> is created.
        /// Each represents a single "block group" within the filesystem and the information
        /// within any on of them is pertinent only to the group it is describing.
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        internal struct GroupDescriptor
        {
            /// <summary>
            /// Block id of the first block of the "block bitmap" for the group represented.
            /// </summary>
            [FieldOffset(0)]
            public uint BlockBitmap;

            /// <summary>
            /// Block id of the first block of the "INode bitmap" for the group represented.
            /// </summary>
            [FieldOffset(4)]
            public uint INodeBitmap;

            /// <summary>
            /// Block id of the first block of the "INode table" for the group represented.
            /// </summary>
            [FieldOffset(8)]
            public uint INodeTable;

            [FieldOffset(12)]
            public ushort FreeBlocksCount;

            [FieldOffset(14)]
            public ushort FreeINodesCount;

            [FieldOffset(16)]
            public ushort UsedDirsCount;

            [FieldOffset(18)]
            public ushort Pad;

            [FieldOffset(20)]
            public uint Padding1;

            [FieldOffset(24)]
            public uint Padding2;

            [FieldOffset(28)]
            public uint Padding3;
        }

        [StructLayout(LayoutKind.Explicit, Size = 128)]
        internal struct INode
        {
            [FieldOffset(0)]
            public INodeModeEnum Mode;

            [FieldOffset(2)]
            public ushort UID;

            [FieldOffset(4)]
            public uint Size;

            [FieldOffset(8)]
            public uint ATime;

            [FieldOffset(12)]
            public uint CTime;

            [FieldOffset(16)]
            public uint MTime;

            [FieldOffset(20)]
            public uint DTime;

            [FieldOffset(24)]
            public ushort GID;

            [FieldOffset(26)]
            public ushort LinksCount;

            [FieldOffset(28)]
            public uint Blocks;

            [FieldOffset(32)]
            public uint Flags;

            [FieldOffset(36)]
            public uint OSD1;

            [FieldOffset(INodeConsts.BlockOffset)]
            public uint Block;

            [FieldOffset(100)]
            public uint Generation;

            [FieldOffset(104)]
            public uint FileACL;

            [FieldOffset(108)]
            public uint DirACL;

            [FieldOffset(112)]
            public uint FAddr;

            [FieldOffset(116)]
            public uint OSD2_1;

            [FieldOffset(120)]
            public uint OSD2_2;

            [FieldOffset(124)]
            public uint OSD2_3;
        }

        [Flags]
        public enum INodeModeEnum : ushort
        {
            FormatMask = 0xF000,
            Socket = 0xC000,
            SymbolicLink = 0xA000,
            RegularFile = 0x8000,
            BlockDevice = 0x6000,
            Directory = 0x4000,
            CharacterDevice = 0x2000,
            Fifo = 0x1000,
            SUID = 0x0800,
            SGID = 0x0400,
            StickyBit = 0x0200,
            UserAccessRightsMask = 0x01C0,
            UserAccessRightsRead = 0x0100,
            UserAccessRightsWrite = 0x0080,
            UserAccessRightsExecute = 0x0040,
            GroupAccessRightsMask = 0x0038,
            GroupAccessRightsRead = 0x0020,
            GroupAccessRightsWrite = 0x0010,
            GroupAccessRightsExecute = 0x0008,
            OthersAccessRightsMask = 0x0007,
            OthersAccessRightsRead = 0x0004,
            OthersAccessRightsWrite = 0x0002,
            OthersAccessRightsExecute = 0x0001
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct DirectoryEntry
        {
            /// <summary>
            /// INode number of the entry. A value of <c>0</c> indicates that the entry is not used.
            /// </summary>
            [FieldOffset(0)]
            public uint INode;

            [FieldOffset(4)]
            public ushort RecordLength;

            [FieldOffset(6)]
            public byte NameLength;

            [FieldOffset(7)]
            public byte FileType;

            [FieldOffset(8)]
            public byte FirstNameChar;
        }
    }

    public static class INodeConsts
    {
        public const int BlockOffset = 40;
    }
}