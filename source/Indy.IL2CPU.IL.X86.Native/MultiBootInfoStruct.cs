using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Indy.IL2CPU.IL.X86.Native {
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct MultiBootInfoStruct {
		[Flags]
		public enum FlagsEnum: uint {
			None = 0,
			MemoryInfo = 1,
			BootDeviceInfo = 1 << 1,
			CmdLineInfo = 1 << 2,
			ModulesInfo = 1 << 3,
			AOutSymbolTableInfo = 1 << 4,
			ELFSectionHeaderInfo = 1 << 5,
			MMapInfo = 1 << 6,
			DrivesInfo = 1 << 7,
			ConfigTableInfo = 1 << 8,
			BootLoaderNameInfo = 1 << 9,
			ApmTableInfo = 1 << 10,
			VbeInfo = 1 << 11
		}

		public readonly FlagsEnum Flags;
		public readonly uint MemLower;
		public readonly uint MemUpper;
		public readonly uint BootDevice;
		public readonly void* CmdLine;
		public readonly uint ModsCount;
		public readonly void* ModsAddress;
		public readonly uint TabSizeOrNumber;
		public readonly uint StrSizeOrSize;
		public readonly void* Address;
		public readonly uint ReservedOrShndx;
		public readonly uint MMapLength;
		public readonly void* MMapAddress;
		public readonly uint DrivesLength;
		public readonly void* DrivesAddress;
		public readonly void* ConfigTable;
		public readonly void* BootLoaderName;
		public readonly void* ApmTable;
		public readonly void* VbeControlInfo;
		public readonly void* VbeModeInfo;
		public readonly ushort VbeMode;
		public readonly ushort VbeInterfaceSegment;
		public readonly ushort VbeInterfaceOffset;
		public readonly ushort VbeInterfaceLength;
	}
}
