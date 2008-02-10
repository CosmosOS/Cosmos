using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.FileSystem {
	partial class Ext2 {
		[StructLayout(LayoutKind.Explicit, Size = 516)]
		internal struct SuperBlock {
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
			[FieldOffset(84)]
			public uint FirstINode;


			[FieldOffset(88)]
			public uint Padding1;
			[FieldOffset(92)]
			public uint Padding2;
			[FieldOffset(96)]
			public uint Padding3;
			[FieldOffset(100)]
			public uint Padding4;
			[FieldOffset(104)]
			public uint Padding5;
			[FieldOffset(108)]
			public uint Padding6;
			[FieldOffset(112)]
			public uint Padding7;
			[FieldOffset(116)]
			public uint Padding8;
			[FieldOffset(120)]
			public uint Padding9;
			[FieldOffset(124)]
			public uint Padding10;
			[FieldOffset(128)]
			public uint Padding11;
			[FieldOffset(132)]
			public uint Padding12;
			[FieldOffset(136)]
			public uint Padding13;
			[FieldOffset(140)]
			public uint Padding14;
			[FieldOffset(144)]
			public uint Padding15;
			[FieldOffset(148)]
			public uint Padding16;
			[FieldOffset(152)]
			public uint Padding17;
			[FieldOffset(156)]
			public uint Padding18;
			[FieldOffset(160)]
			public uint Padding19;
			[FieldOffset(164)]
			public uint Padding20;
			[FieldOffset(168)]
			public uint Padding21;
			[FieldOffset(172)]
			public uint Padding22;
			[FieldOffset(176)]
			public uint Padding23;
			[FieldOffset(180)]
			public uint Padding24;
			[FieldOffset(184)]
			public uint Padding25;
			[FieldOffset(188)]
			public uint Padding26;
			[FieldOffset(192)]
			public uint Padding27;
			[FieldOffset(196)]
			public uint Padding28;
			[FieldOffset(200)]
			public uint Padding29;
			[FieldOffset(204)]
			public uint Padding30;
			[FieldOffset(208)]
			public uint Padding31;
			[FieldOffset(212)]
			public uint Padding32;
			[FieldOffset(216)]
			public uint Padding33;
			[FieldOffset(220)]
			public uint Padding34;
			[FieldOffset(224)]
			public uint Padding35;
			[FieldOffset(228)]
			public uint Padding36;
			[FieldOffset(232)]
			public uint Padding37;
			[FieldOffset(236)]
			public uint Padding38;
			[FieldOffset(240)]
			public uint Padding39;
			[FieldOffset(244)]
			public uint Padding40;
			[FieldOffset(248)]
			public uint Padding41;
			[FieldOffset(252)]
			public uint Padding42;
			[FieldOffset(256)]
			public uint Padding43;
			[FieldOffset(260)]
			public uint Padding44;
			[FieldOffset(264)]
			public uint Padding45;
			[FieldOffset(268)]
			public uint Padding46;
			[FieldOffset(272)]
			public uint Padding47;
			[FieldOffset(276)]
			public uint Padding48;
			[FieldOffset(280)]
			public uint Padding49;
			[FieldOffset(284)]
			public uint Padding50;
			[FieldOffset(288)]
			public uint Padding51;
			[FieldOffset(292)]
			public uint Padding52;
			[FieldOffset(296)]
			public uint Padding53;
			[FieldOffset(300)]
			public uint Padding54;
			[FieldOffset(304)]
			public uint Padding55;
			[FieldOffset(308)]
			public uint Padding56;
			[FieldOffset(312)]
			public uint Padding57;
			[FieldOffset(316)]
			public uint Padding58;
			[FieldOffset(320)]
			public uint Padding59;
			[FieldOffset(324)]
			public uint Padding60;
			[FieldOffset(328)]
			public uint Padding61;
			[FieldOffset(332)]
			public uint Padding62;
			[FieldOffset(336)]
			public uint Padding63;
			[FieldOffset(340)]
			public uint Padding64;
			[FieldOffset(344)]
			public uint Padding65;
			[FieldOffset(348)]
			public uint Padding66;
			[FieldOffset(352)]
			public uint Padding67;
			[FieldOffset(356)]
			public uint Padding68;
			[FieldOffset(360)]
			public uint Padding69;
			[FieldOffset(364)]
			public uint Padding70;
			[FieldOffset(368)]
			public uint Padding71;
			[FieldOffset(372)]
			public uint Padding72;
			[FieldOffset(376)]
			public uint Padding73;
			[FieldOffset(380)]
			public uint Padding74;
			[FieldOffset(384)]
			public uint Padding75;
			[FieldOffset(388)]
			public uint Padding76;
			[FieldOffset(392)]
			public uint Padding77;
			[FieldOffset(396)]
			public uint Padding78;
			[FieldOffset(400)]
			public uint Padding79;
			[FieldOffset(404)]
			public uint Padding80;
			[FieldOffset(408)]
			public uint Padding81;
			[FieldOffset(412)]
			public uint Padding82;
			[FieldOffset(416)]
			public uint Padding83;
			[FieldOffset(420)]
			public uint Padding84;
			[FieldOffset(424)]
			public uint Padding85;
			[FieldOffset(428)]
			public uint Padding86;
			[FieldOffset(432)]
			public uint Padding87;
			[FieldOffset(436)]
			public uint Padding88;
			[FieldOffset(440)]
			public uint Padding89;
			[FieldOffset(444)]
			public uint Padding90;
			[FieldOffset(448)]
			public uint Padding91;
			[FieldOffset(452)]
			public uint Padding92;
			[FieldOffset(456)]
			public uint Padding93;
			[FieldOffset(460)]
			public uint Padding94;
			[FieldOffset(464)]
			public uint Padding95;
			[FieldOffset(468)]
			public uint Padding96;
			[FieldOffset(472)]
			public uint Padding97;
			[FieldOffset(476)]
			public uint Padding98;
			[FieldOffset(480)]
			public uint Padding99;
			[FieldOffset(484)]
			public uint Padding100;
			[FieldOffset(488)]
			public uint Padding101;
			[FieldOffset(492)]
			public uint Padding102;
			[FieldOffset(496)]
			public uint Padding103;
			[FieldOffset(500)]
			public uint Padding104;
			[FieldOffset(504)]
			public uint Padding105;
			[FieldOffset(508)]
			public uint Padding106;
			[FieldOffset(512)]
			public uint Padding107;
		}

		/// <summary>
		/// For each group in the filesystem, a <see cref="GroupDescriptor"/> is created.
		/// Each represents a single "block group" within the filesystem and the information
		/// within any on of them is pertinent only to the group it is describing.
		/// </summary>
		[StructLayout(LayoutKind.Explicit, Size = 32)]
		internal struct GroupDescriptor {
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
		internal struct INode {
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
			[FieldOffset(40)]
			public uint Block1;
			[FieldOffset(44)]
			public uint Block2;
			[FieldOffset(48)]
			public uint Block3;
			[FieldOffset(52)]
			public uint Block4;
			[FieldOffset(56)]
			public uint Block5;
			[FieldOffset(60)]
			public uint Block6;
			[FieldOffset(64)]
			public uint Block7;
			[FieldOffset(68)]
			public uint Block8;
			[FieldOffset(72)]
			public uint Block9;
			[FieldOffset(76)]
			public uint Block10;
			[FieldOffset(80)]
			public uint Block11;
			[FieldOffset(84)]
			public uint Block12;
			[FieldOffset(88)]
			public uint Block13;
			[FieldOffset(92)]
			public uint Block14;
			[FieldOffset(96)]
			public uint Block15;
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

		[StructLayout(LayoutKind.Explicit, Size = 8)]
		internal struct DirectoryEntry {
			/// <summary>
			/// INode number of the entry. A value of <c>0</c> indicates that the entry is not used.
			/// </summary>
			[FieldOffset(0)]
			public uint INodeNumber;
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
}
