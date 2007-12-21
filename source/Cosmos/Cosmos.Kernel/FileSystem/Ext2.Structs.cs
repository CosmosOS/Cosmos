using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.FileSystem {
	partial class Ext2 {
		[StructLayout(LayoutKind.Sequential)]
		internal struct SuperBlock {
			/// <summary>
			/// Indicates the total number of INodes, both used and free, in the filesystem.
			/// </summary>
			public uint INodesCount;
			/// <summary>
			/// Indicates the total number of blocks, both used and free, in the filesystem.
			/// </summary>
			public uint BlockCount;
			/// <summary>
			/// Indicates the total number of blocks reserved for the usage of the super user. this is most useful
			/// if for some reason a user, malicious or not, fills the file system to capacity; the super user will 
			/// have this specified amount of free blocks at his disposal so he can edit and save configuration files.
			/// </summary>
			public uint RBlocksCount;
			/// <summary>
			/// Indicates the total number of free blocks, including the number of reserved blocks (<see cref="RBlocksCount"/>).
			/// This is a sum of all free blocks of all the block groups.
			/// </summary>
			public uint FreeBlocksCount;
			/// <summary>
			/// Indicates the total number of free INodes. This is a sum of all free nodes of all the block groups.
			/// </summary>
			public uint FreeINodesCount;
			/// <summary>
			/// Identifies the first data block, in other words the id of the block containing the superblock
			/// structure.
			/// Note that this value is always 0 for filesystems with a block size larger than 1KB,
			/// and always 1 for filesystem with a blocksize of 1KB. The superblock is always starting at the
			/// 1024th byte of the disk, which normally happens to be the first byte of the 3rd sector.
			/// </summary>
			public uint FirstDataBlock;
			/// <summary>
			/// The block size is computed using this value as the number of bits to shift left the value 1024.
			/// This value may only be positive.
			/// <code>
			///		int xBlockSize = 1024 &lt;&lt; LogBlockSize;
			/// </code>
			/// </summary>
			public int LogBlockSize;
			public uint LogFragSize;
			public uint BlocksPerGroup;
			public uint FragsPerGroup;
			public uint INodesPerGroup;
			public uint MTime;
			public uint WTime;
			public ushort MntCount;
			public ushort MaxMntCount;
			public ushort Magic;
			public ushort State;
			public ushort Errors;
			public ushort MinorRevLevel;
			public uint LastCheck;
			public uint CheckInterval;
			public uint CreatorOS;
			public uint RevLevel;
			public ushort RefResUID;
			// up to byte 84 (includes 83..)
			public ushort DefResGID;

			public uint Padding1;
			public uint Padding2;
			public uint Padding3;
			public uint Padding4;
			public uint Padding5;
			public uint Padding6;
			public uint Padding7;
			public uint Padding8;
			public uint Padding9;
			public uint Padding10;
			public uint Padding11;
			public uint Padding12;
			public uint Padding13;
			public uint Padding14;
			public uint Padding15;
			public uint Padding16;
			public uint Padding17;
			public uint Padding18;
			public uint Padding19;
			public uint Padding20;
			public uint Padding21;
			public uint Padding22;
			public uint Padding23;
			public uint Padding24;
			public uint Padding25;
			public uint Padding26;
			public uint Padding27;
			public uint Padding28;
			public uint Padding29;
			public uint Padding30;
			public uint Padding31;
			public uint Padding32;
			public uint Padding33;
			public uint Padding34;
			public uint Padding35;
			public uint Padding36;
			public uint Padding37;
			public uint Padding38;
			public uint Padding39;
			public uint Padding40;
			public uint Padding41;
			public uint Padding42;
			public uint Padding43;
			public uint Padding44;
			public uint Padding45;
			public uint Padding46;
			public uint Padding47;
			public uint Padding48;
			public uint Padding49;
			public uint Padding50;
			public uint Padding51;
			public uint Padding52;
			public uint Padding53;
			public uint Padding54;
			public uint Padding55;
			public uint Padding56;
			public uint Padding57;
			public uint Padding58;
			public uint Padding59;
			public uint Padding60;
			public uint Padding61;
			public uint Padding62;
			public uint Padding63;
			public uint Padding64;
			public uint Padding65;
			public uint Padding66;
			public uint Padding67;
			public uint Padding68;
			public uint Padding69;
			public uint Padding70;
			public uint Padding71;
			public uint Padding72;
			public uint Padding73;
			public uint Padding74;
			public uint Padding75;
			public uint Padding76;
			public uint Padding77;
			public uint Padding78;
			public uint Padding79;
			public uint Padding80;
			public uint Padding81;
			public uint Padding82;
			public uint Padding83;
			public uint Padding84;
			public uint Padding85;
			public uint Padding86;
			public uint Padding87;
			public uint Padding88;
			public uint Padding89;
			public uint Padding90;
			public uint Padding91;
			public uint Padding92;
			public uint Padding93;
			public uint Padding94;
			public uint Padding95;
			public uint Padding96;
			public uint Padding97;
			public uint Padding98;
			public uint Padding99;
			public uint Padding100;
			public uint Padding101;
			public uint Padding102;
			public uint Padding103;
			public uint Padding104;
			public uint Padding105;
			public uint Padding106;
			public uint Padding107;
		}

		/// <summary>
		/// For each group in the filesystem, a <see cref="GroupDescriptor"/> is created.
		/// Each represents a single "block group" within the filesystem and the information
		/// within any on of them is pertinent only to the group it is describing.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		internal struct GroupDescriptor {
			/// <summary>
			/// Block id of the first block of the "block bitmap" for the group represented.
			/// </summary>
			public uint BlockBitmap;
			/// <summary>
			/// Block id of the first block of the "INode bitmap" for the group represented.
			/// </summary>
			public uint INodeBitmap;
			/// <summary>
			/// Block id of the first block of the "INode table" for the group represented.
			/// </summary>
			public uint INodeTable;
			public ushort FreeBlocksCount;
			public ushort FreeINodesCount;
			public ushort UsedDirsCount;
			public ushort Pad;
			public uint Padding1;
			public uint Padding2;
			public uint Padding3;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct INode {
			public INodeModeEnum Mode;
			public ushort UID;
			public uint Size;
			public uint ATime;
			public uint CTime;
			public uint MTime;
			public uint DTime;
			public ushort GID;
			public ushort LinksCount;
			public uint Blocks;
			public uint Flags;
			public uint OSD1;
			public uint Block1;
			public uint Block2;
			public uint Block3;
			public uint Block4;
			public uint Block5;
			public uint Block6;
			public uint Block7;
			public uint Block8;
			public uint Block9;
			public uint Block10;
			public uint Block11;
			public uint Block12;
			public uint Block13;
			public uint Block14;
			public uint Block15;
			public uint Generation;
			public uint FileACL;
			public uint DirACL;
			public uint FAddr;
			public uint OSD2_1;
			public uint OSD2_2;
			public uint OSD2_3;
		}

		[Flags]
		public enum INodeModeEnum: ushort {
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

		[StructLayout(LayoutKind.Sequential)]
		internal struct DirectoryEntry {
			/// <summary>
			/// INode number of the entry. A value of <c>0</c> indicates that the entry is not used.
			/// </summary>
			public uint INode;
			public ushort RecordLength;
			public byte NameLength;
			public byte FileType;
			public byte FirstNameChar;
		}
	}
}