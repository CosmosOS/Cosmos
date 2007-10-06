using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Indy.IL2CPU.IL.NativeX86 {
	public partial class RuntimeEngineImpl {
		[StructLayout(LayoutKind.Sequential)]
		public struct IDTEntryStruct {
			public enum FlagsEnum: byte {
				NotPresent = 0xE,
				Present = 0x4E
			}
			public ushort BaseLow;
			public ushort Sel;
			public byte AlwaysZero;
			public byte Flags;
			public ushort BaseHi;
		}
		[StructLayout(LayoutKind.Sequential)]
		public struct DTPointerStruct {
			public ushort Limit;
			public uint Base;
		}

		// Do not rename, it is being referenced by name string
		private static IDTEntryStruct[] mIDTEntries;
		private static DTPointerStruct mIDTPointer;
		private static void IDT_LoadArray() {
			// implemented using bare assembly
		}

		private static void IDT_RegisterIDT() {
			// implemented using bare assembly
		}

		private static void IDT_SetHandler(byte aInterruptNumber, uint aBase, ushort aSel, IDTEntryStruct.FlagsEnum aFlags) {
			ushort xBaseLow = (ushort)(aBase & 0xFFFF);
			ushort xBaseHigh = (ushort)((aBase >> 16) & 0xFFFF);
			mIDTEntries[aInterruptNumber].AlwaysZero = 0;
			mIDTEntries[aInterruptNumber].Sel = GDT_CodeSelector;
			mIDTEntries[aInterruptNumber].BaseLow = xBaseLow;
			mIDTEntries[aInterruptNumber].BaseHi = xBaseHigh;
			mIDTEntries[aInterruptNumber].Flags = 0x80 /*present*/| 0x8 /*32-bit*/| 0x6 /*interrupt gate*/;
		}

		#region interrupt handlers

		[InterruptServiceRoutine(0)]
		private static void Interrupt_0() {
			Console.WriteLine("Interrupt 0 occurred!");
		}
		#endregion

		private static void SetupInterruptDescriptorTable() {
			bool aFalse = false;
			if (aFalse) {
				Interrupt_0();
				int theLen = mIDTEntries.Length;
			}
			IDT_LoadArray();
			IDT_RegisterIDT();
		}
	}
}