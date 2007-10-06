using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Indy.IL2CPU.IL.NativeX86 {
	partial class RuntimeEngineImpl {
		public const ushort GDT_SystemSelector = 0;
		public const ushort GDT_CodeSelector = 8;
		public const ushort GDT_DataSelector = 0x10;
		private static DTPointerStruct mGDTPointer;

//		private static DTPointer* gdtPointer = (DTPointer*)Kernel.LabelledAlloc("GDTPointer", DTPointer.SizeOf);
//		private static Entry* gdt = (Entry*)Kernel.StaticAlloc(Entry.SizeOf * GDTEntries);
//
//		[StructLayout(LayoutKind.Sequential)]
//		public struct Entry {
//			public enum Type: ushort {
//				Accessed = 1,
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
//			}
//
//			public const uint SizeOf = 8;
//
//			public ushort LimitLow;
//			public ushort BaseLow;
//			public byte BaseMiddle;
//			public byte Access;
//			public byte Granularity;
//			public byte BaseHigh;
//
//			public void Setup(uint _base, uint _limit, ushort flags) {
//				this.BaseLow = (ushort)(_base & 0xFFFF);
//				this.BaseMiddle = (byte)((_base >> 16) & 0xFF);
//				this.BaseHigh = (byte)((_base >> 24) & 0xFF);
//
//				// The limits
//				this.LimitLow = (ushort)(_limit & 0xFFFF);
//				this.Granularity = (byte)((_limit >> 16) & 0x0F);
//
//				// Granularity and Access
//				this.Granularity |= (byte)((flags >> 4) & 0xF0);
//				this.Access = (byte)(flags & 0xFF);
//			}
//		}


		private static void GDT_LoadArray() {
			// implemented using bare assembly
		}

		private static void GDT_RegisterGDT() {
			// implemented using bare assembly
		}

		private static void SetupGlobalDescriptorTable() {
			GDT_LoadArray();
			GDT_RegisterGDT();
		}
	}
}

