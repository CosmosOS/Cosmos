using System;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Boot.Glue {
	[StructLayout(LayoutKind.Sequential)]
	public struct DTPointerStruct {
		public ushort Limit;
		public uint Base;
	}
}