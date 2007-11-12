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
		private static void InterruptHandler(ushort aInterrupt, uint aParam) {
			Debug.Write("Interrupt Received [");
			bool xNameWritten = false;
			#region InterruptName determining
			if (aInterrupt < 19) {
				switch(aInterrupt) {
					case 0: {
							Debug.Write("Division By Zero Exception");
							xNameWritten = true;
							break;
						}
					case 1: {
							Debug.Write("Debug Exception");
							xNameWritten = true;
							break;
						}
					case 2: {
							Debug.Write("Non Maskable Interrupt Exception");
							xNameWritten = true;
							break;
						}
					case 3: {
							Debug.Write("Breakpoint Exception");
							xNameWritten = true;
							break;
						}
					case 4: {
							Debug.Write("Into Detected Overflow Exception");
							xNameWritten = true;
							break;
						}
					case 5: {
							Debug.Write("Out of Bounds Exception");
							xNameWritten = true;
							break;
						}
					case 6: {
							Debug.Write("Invalid Opcode Exception");
							xNameWritten = true;
							break;
						}
					case 7: {
							Debug.Write("No Coprocessor Exception");
							xNameWritten = true;
							break;
						}
					case 8: {
							Debug.Write("Double Fault Exception");
							xNameWritten = true;
							break;
						}
					case 9: {
							Debug.Write("Coprocessor Segment Overrun Exception");
							xNameWritten = true;
							break;
						}
					case 10: {
							Debug.Write("Bad TSS Exception");
							xNameWritten = true;
							break;
						}
					case 11: {
							Debug.Write("Segment Not Present Exception");
							xNameWritten = true;
							break;
						}
					case 12: {
							Debug.Write("Stack Fault Exception");
							xNameWritten = true;
							break;
						}
					case 13: {
							Debug.Write("General Protection Fault Exception");
							xNameWritten = true;
							break;
						}
					case 14: {
							Debug.Write("Page Fault Exception");
							xNameWritten = true;
							break;
						}
					case 15: {
							Debug.Write("Unknown Interrupt Exception");
							xNameWritten = true;
							break;
						}
					case 16: {
							Debug.Write("Coprocessor Fault Exception");
							xNameWritten = true;
							break;
						}
					case 17: {
							Debug.Write("Alignment Check Exception (486+)");
							xNameWritten = true;
							break;
						}
					case 18: {
							Debug.Write("Machine Check Exception (Pentium/586+)");
							xNameWritten = true;
							break;
						}
				}
			}
			if(!xNameWritten) {
				Debug.Write("**Unknown**");
			}
			#endregion
			Debug.WriteLine("]");
			Debug.Write("    Interrupt Number = ");
			IO.WriteSerialHexNumber(0, aInterrupt, 4);
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