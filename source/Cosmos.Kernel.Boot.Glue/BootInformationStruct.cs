using System;

namespace Cosmos.Kernel.Boot.Glue {
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct BootInformationStruct {
		public uint Flags;
		public uint MemLower;
		public uint MemUpper;
		public uint BootDevice;
		public uint CmdLine;
		public uint ModsCount;
		public uint ModsAddr;
		public uint Syms_1;
		public uint Syms_2;
		public uint Syms_3;
		public uint MMapLength;
		public uint MMapAddr;
		public uint DrivesLength;
		public uint DrivesAddr;
		public uint ConfigTable;
		public uint BootLoaderName;
		public uint ApmTable;
		public uint VbeControlInfo;
		public uint VbeModeInfo;
		public ushort VbeMode;
		public ushort VbeInterfaceSeg;
		public ushort VbeInterfaceOff;
		public ushort VbeInterfaceLen;
	}
}
