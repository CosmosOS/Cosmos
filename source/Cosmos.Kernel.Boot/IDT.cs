using System;
using System.Diagnostics;
using Cosmos.Kernel.Boot.Glue;

namespace Cosmos.Kernel.Boot {
	public static class IDT {
		// Do not rename, it is being referenced by name string
		[GlueField(FieldType=GlueFieldTypeEnum.IDT_Array)]
		private static IDTEntryStruct[] mIDTEntries;
		[GlueField(FieldType = GlueFieldTypeEnum.IDT_Pointer)]
		private static DTPointerStruct mIDTPointer;

		[GluePlaceholderMethod(MethodType = GluePlaceholderMethodTypeEnum.IDT_LoadArray)]
		private static void IDT_LoadArray() {
			// implemented using bare assembly
		}

		[GluePlaceholderMethod(MethodType = GluePlaceholderMethodTypeEnum.IDT_Register)]
		private static void IDT_RegisterIDT() {
			// implemented using bare assembly
		}

		[GlueMethod(MethodType = GlueMethodTypeEnum.IDT_SetHandler)]
		private static void IDT_SetHandler(byte aInterruptNumber, uint aBase, ushort aSel, IDTEntryStruct.FlagsEnum aFlags) {
			mIDTEntries[aInterruptNumber].AlwaysZero = 0;
			mIDTEntries[aInterruptNumber].Sel = 0;
			mIDTEntries[aInterruptNumber].BaseLow = (ushort)(aBase);
			mIDTEntries[aInterruptNumber].BaseHi = (ushort)(aBase >> 16);
			mIDTEntries[aInterruptNumber].Flags = 128 /*Present*/| 0 /*Ring0*/| 8 /*32-bit*/| 0xF /*interrupt gate*/;
		}	
						   
		[GlueMethod(MethodType = GlueMethodTypeEnum.IDT_InterruptHandler)]
		private static void InterruptHandler(byte aInterrupt, byte aParam) {
			//System.Diagnostics.Debugger.Break();
			Debug.WriteLine("Interrupt received:");
			//CustomImplementations.System.ConsoleImpl.Write("    ");
			//CustomImplementations.System.ConsoleImpl.OutputByteValue(aInterrupt);
			//CustomImplementations.System.ConsoleImpl.WriteLine("");
			//CustomImplementations.System.ConsoleImpl.Write("    ");
			//CustomImplementations.System.ConsoleImpl.OutputByteValue(aParam);
			//if (aInterrupt >= 40 && aInterrupt <= 47) {
			//	WriteToPort(0xA0, 0x20);
			//}
			//if (aInterrupt >= 32 && aInterrupt <= 47) {
			//	WriteToPort(0x20, 0x20);
			//}
			//CustomImplementations.System.ConsoleImpl.WriteLine("");
		}

		public static void SetupInterruptDescriptorTable() {
			Debug.WriteLine("Start setting up Interrupt Descriptor Table");
			Debug.WriteLine("Load the array");
			IDT_LoadArray();
			Debug.WriteLine("Register the IDT");
			//System.Diagnostics.Debugger.Break();
			IDT_RegisterIDT();
		}
	}
}