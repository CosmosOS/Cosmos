using System;

namespace Cosmos.Kernel.Boot.Glue {
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct BootInformationStruct {
		public struct MMapEntry {
			public uint Size;
			public uint AddrLow;
			public uint AddrHigh;
			public uint LengthLow;
			public uint LengthHigh;
			public uint Type;
		}
		public uint Flags;
		public uint MemLower;
		public uint MemUpper;
		public uint BootDevice;
		public uint CmdLine;
		public uint ModsCount;
		public uint ModsAddr;
		public uint Syms_Num;
		public uint Syms_Size;
		public uint Syms_Addr;
		public uint Syms_Shndx;
		public uint MMapLength;
		public uint MMapAddr;
		public uint DrivesLength;
		public uint DrivesAddr;
		public uint ConfigTable;
		public uint BootLoaderName;
		public uint ApmTable;
		public uint VbeControlInfo;
		public uint VbeModeInfo;
//		public ushort VbeMode;
//		public ushort VbeInterfaceSeg;
//		public ushort VbeInterfaceOff;
//		public ushort VbeInterfaceLen;
		public uint VbeMode_IntfSeg;
		public uint VbeIntfOff_Len;
	}
}
