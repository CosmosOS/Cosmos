using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Indy.IL2CPU.IL.NativeX86 {
	public partial class RuntimeEngineImpl {
		[StructLayout(LayoutKind.Sequential)]
		public struct DTPointerStruct {
			public ushort Limit;
			public uint Base;
		}
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
			mIDTEntries[aInterruptNumber].AlwaysZero = 0;
			mIDTEntries[aInterruptNumber].Sel = 8;
			mIDTEntries[aInterruptNumber].BaseLow = (ushort)(aBase & 0xFFFF);
			mIDTEntries[aInterruptNumber].BaseHi = (ushort)((aBase >> 16) & 0xFFFF);
			mIDTEntries[aInterruptNumber].Flags = 128 /*Present*/| 0 /*Ring0*/| 8 /*32-bit*/| 0xF /*interrupt gate*/;
		}																					  // was E

		private static void InterruptHandler(byte aInterrupt, uint aParam) {
			if(aInterrupt >= 40 && aInterrupt <= 47) {
				WriteToPort(0xA0, 0x20);
			}
			if (aInterrupt >= 32 && aInterrupt <= 47) {
				WriteToPort(0x20, 0x20);
			}
			//Debug.WriteLine("Interrupt received");
		}

		private static void SetupInterruptDescriptorTable() {
			Debug.WriteLine("Start setting up Interrupt Descriptor Table");
			bool aFalse = false;
			if (aFalse) {
				// code is never executed, but neccessary for IL2CPU to detect the methods
				InterruptHandler(0, 0);
				int theLen = mIDTEntries.Length;
			}
			Debug.WriteLine("Load the array");
			IDT_LoadArray();
			Debug.WriteLine("Register the IDT");
			System.Diagnostics.Debugger.Break();
			IDT_RegisterIDT();
		}
	}
}