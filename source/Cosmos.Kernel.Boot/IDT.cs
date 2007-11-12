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

		[GluePlaceholderMethod(MethodType = GluePlaceholderMethodTypeEnum.IDT_EnableInterrupts)]
		public static void IDT_EnableInterrupts() {
		}

		[GluePlaceholderMethod(MethodType = GluePlaceholderMethodTypeEnum.IDT_Register)]
		private static void IDT_RegisterIDT() {
			// implemented using bare assembly
		}

		[GlueMethod(MethodType = GlueMethodTypeEnum.IDT_SetHandler)]
		private static void IDT_SetHandler(byte aInterruptNumber, uint aBase, ushort aSel, IDTFlagsEnum aFlags) {
			mIDTEntries[aInterruptNumber].AlwaysZero = 0;
			mIDTEntries[aInterruptNumber].Sel = 0x8;
			mIDTEntries[aInterruptNumber].BaseLow = (ushort)(aBase & 0xFFFF);
			mIDTEntries[aInterruptNumber].BaseHi = (ushort)((aBase >> 16) & 0xFFFF);
			mIDTEntries[aInterruptNumber].Flags = 0x8E;//128 /*Present*/| 0 /*Ring0*/| 8 /*32-bit*/| 0xF /*interrupt gate*/;
			Debug.Write("Registering Interrupt ");
			IO.WriteSerialHexNumber(0, aInterruptNumber, 2);
			Debug.WriteLine("");
			Debug.Write("    BaseLow = ");
			IO.WriteSerialHexNumber(0, mIDTEntries[aInterruptNumber].BaseLow, 4);
			Debug.WriteLine("");
			Debug.Write("    BaseHigh = ");
			IO.WriteSerialHexNumber(0, mIDTEntries[aInterruptNumber].BaseHi, 4);
			Debug.WriteLine("");
			Debug.Write("    Base = ");
			IO.WriteSerialHexNumber(0, aBase);
			Debug.WriteLine("");
		}	
						   
		[GlueMethod(MethodType = GlueMethodTypeEnum.IDT_InterruptHandler)]
		private static void InterruptHandler(uint aInterrupt, uint aParam) {
			Debug.WriteLine("Interrupt Received");
			Debug.Write("    Interrupt Number = ");
			IO.WriteSerialHexNumber(0, aInterrupt, 8);
			Debug.WriteLine("");
			Debug.Write("    Interrupt Params = ");
			IO.WriteSerialHexNumber(0, aParam, 8);
			Debug.WriteLine("");
			//System.Diagnostics.Debugger.Break();
			//Console.WriteLine("Interrupt received:");
			//CustomImplementations.System.ConsoleImpl.Write("    ");
			//CustomImplementations.System.ConsoleImpl.OutputByteValue(aInterrupt);
			//CustomImplementations.System.ConsoleImpl.WriteLine("");
			//CustomImplementations.System.ConsoleImpl.Write("    ");
			//CustomImplementations.System.ConsoleImpl.OutputByteValue(aParam);
			if (aInterrupt >= 40 && aInterrupt <= 47) {
				IO.WriteToPort(0xA0, 0x20);
			}
			if (aInterrupt >= 32 && aInterrupt <= 47) {
				IO.WriteToPort(0x20, 0x20);
			}
			//CustomImplementations.System.ConsoleImpl.WriteLine("");
		}

		public static void Setup() {
			Console.WriteLine("Start setting up Interrupt Descriptor Table");
			Console.WriteLine("Load the array");
			IDT_LoadArray();
			Console.WriteLine("Register the IDT");
			//System.Diagnostics.Debugger.Break();
			IDT_RegisterIDT();
		}
	}
}