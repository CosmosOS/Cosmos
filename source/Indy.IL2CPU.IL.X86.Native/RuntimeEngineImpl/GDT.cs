using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Indy.IL2CPU.IL.X86.Native {
	public partial class RuntimeEngineImpl {
		[StructLayout(LayoutKind.Sequential)]
		public struct GDTEntry {
			public ushort LimitLow;
			public ushort BaseLow;
			public byte BaseMiddle;
			public byte Access;
			public byte Granularity;
			public byte BaseHigh;
		}

		private static void GDT_InitEntry(ref GDTEntry aEntry, uint aBase, uint aLimit, ushort aFlags) {
			aEntry.BaseLow = (ushort)(aBase & 0xFFFF);
			aEntry.BaseMiddle = (byte)((aBase >> 16) & 0xFF);
			aEntry.BaseHigh = (byte)((aBase >> 24) & 0xFF);
			aEntry.LimitLow = (ushort)(aLimit & 0xFFFF);
			aEntry.Granularity = (byte)((aLimit >> 16) & 0x0F);
			aEntry.Granularity |= (byte)((aFlags >> 4) & 0xF0);
			aEntry.Access = (byte)(aFlags & 0xFF);
		}

		private static GDTEntry[] mGDTEntries = new GDTEntry[3];

		public const ushort CodeSelector = 8;
		public const ushort DataSelector = 16;


		private static DTPointerStruct mGDTPointer;


		private static void GDT_RegisterGDT() {
		}

		private static void GDT_LoadArray() {
		}

		public static void SetupGDT() {
			GDT_LoadArray();
			GDT_InitEntry(ref mGDTEntries[0], 0, 0, 0);
			GDT_InitEntry(ref mGDTEntries[CodeSelector >> 3], 0, 0xFFFFFFFF, 0x89A);

//						Accessed = 1,
//				Writable = 2,
//				Expansion = 4,
//				Executable = 8,
//				Descriptor = 16,
//				Privilege_Ring_0 = 0,
//				Privilege_Ring_1 = 32,
//				Privilege_Ring_2 = 64,
//				Privilege_Ring_3 = 96,
//				Present = 128,
//				OperandSize_16Bit = 0,
//				OperandSize_32Bit = 1024,
//				Granularity_Byte = 0,
//				Granularity_4K = 2048
//Entry.Type.Granularity_4K |
//				Entry.Type.OperandSize_32Bit |
//				Entry.Type.Present |
//				Entry.Type.Descriptor |
//				Entry.Type.Writable));
//
//
			GDT_InitEntry(ref mGDTEntries[DataSelector >> 3], 0, 0xFFFFFFFF, 0x92);
			Console.WriteLine("Register now:");
			System.Diagnostics.Debugger.Break();
			GDT_RegisterGDT();
		}
	}
}
