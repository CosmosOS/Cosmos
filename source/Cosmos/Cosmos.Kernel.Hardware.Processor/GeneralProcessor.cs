using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Cosmos.Hardware;
using Indy.IL2CPU.Assembler;
using Mono.Cecil;

namespace Cosmos.Kernel.Hardware.Processor {
	public static class GeneralProcessor {

		private static MethodDefinition GetMethodDef(Assembly aAssembly, string aType, string aMethodName, bool aErrorWhenNotFound) {
			AssemblyDefinition xAssembly = AssemblyFactory.GetAssembly(aAssembly.Location);
			foreach (ModuleDefinition xMod in xAssembly.Modules) {
				if (xMod.Types.Contains(aType)) {
					TypeDefinition xType = xMod.Types[aType];
					foreach (MethodDefinition xMethod in xType.Methods) {
						if (xMethod.Name == aMethodName) {
							return xMethod;
						}
					}
				}
			}
			if (aErrorWhenNotFound) {
				throw new Exception("Method '" + aType + "::" + aMethodName + "' not found!");
			}
			return null;
		}

		private static MethodDefinition GetInterruptHandler(byte aInterrupt) {
			return GetMethodDef(typeof(Interrupts).Assembly, typeof(Interrupts).FullName, "HandleInterrupt_" + aInterrupt.ToString("X2"), false);
		}

		public static void InitializeFields(StreamWriter aOutput) {
			#region generate GDT
			string xFieldName = "_NATIVE_GDT_Contents";
			/* format
			 *	ushort LimitLow
			 *	ushort BaseLow
			 *	byte BaseMiddle
			 *	byte Access
			 *	byte Granularity
			 *	byte BaseHigh
			 */
			string xFieldData = "0,0,0,0,0,0,0,0,"; // system entry, all zeros
			// code entry
			xFieldData += "255,255,0,0,0,0x99,0xCF,0,";
			// data entry
			xFieldData += "255,255,0,0,0,0x93,0xCF,0";
			aOutput.WriteLine("{0} db {1}", xFieldName, xFieldData);
			xFieldName = "_NATIVE_GDT_Pointer";
			xFieldData = "0x00170000,_NATIVE_GDT_Contents";
			aOutput.WriteLine("{0} dd {1}", xFieldName, xFieldData);
			#endregion
			#region generate IDT
			xFieldData = "";
			xFieldName = "_NATIVE_IDT_Contents";
			for (int i = 0; i < 256; i++) {
				xFieldData += "(__ISR_Handler_" + i.ToString("X2") + " and 0xFF),";
				xFieldData += "((__ISR_Handler_" + i.ToString("X2") + " shr 8) and 0xFF),";
				xFieldData += "0x8,0,";
				xFieldData += "0,";
				xFieldData += "0x8E,";
				xFieldData += "((__ISR_Handler_" + i.ToString("X2") + " shr 16) and 0xFF),";
				xFieldData += "((__ISR_Handler_" + i.ToString("X2") + " shr 24) and 0xFF),";
			}
			aOutput.WriteLine("{0} db {1}", xFieldName, xFieldData.TrimEnd(','));
			xFieldName = "_NATIVE_IDT_Pointer";
			xFieldData = "0x07FF0000,_NATIVE_IDT_Contents";
			aOutput.WriteLine("{0} dd {1}", xFieldName, xFieldData);
			#endregion
		}

		public static void InitializeEntryPoint(StreamWriter aOutput) {
			aOutput.WriteLine("mov eax, _NATIVE_GDT_Pointer");
			aOutput.WriteLine("add eax, 2");
			aOutput.WriteLine("lgdt [eax]");
			aOutput.WriteLine("mov ax, 0x10");
			aOutput.WriteLine("mov ds, ax");
			aOutput.WriteLine("mov es, ax");
			aOutput.WriteLine("mov fs, ax");
			aOutput.WriteLine("mov gs, ax");
			aOutput.WriteLine("mov ss, ax");
			aOutput.WriteLine("jmp 0x8:flush__GDT__table");
			aOutput.WriteLine("flush__GDT__table:");
			aOutput.WriteLine("nop");
			aOutput.WriteLine("; setup idt");
			aOutput.WriteLine("mov eax, _NATIVE_IDT_Pointer");
			aOutput.WriteLine("add eax, 2");
			aOutput.WriteLine("lidt [eax]");
			aOutput.WriteLine("nop");
			aOutput.WriteLine("; reprogram PIC to be on 0x20-0x37");
			Action<int, int> WriteToPort = delegate(int port, int value) {
				aOutput.WriteLine("mov dx, 0x" + port.ToString("X"));
				aOutput.WriteLine("mov ax, 0x" + value.ToString("X"));
				aOutput.WriteLine("out dx, ax");
			};
			WriteToPort(0x20, 0x11);
			WriteToPort(0xA0, 0x11);
			WriteToPort(0x21, 0x20);
			WriteToPort(0xA1, 0x28);
			WriteToPort(0x21, 0x04);
			WriteToPort(0xA1, 0x02);
			WriteToPort(0x21, 0x01);
			WriteToPort(0xA1, 0x01);
			WriteToPort(0x21, 0x0);
			WriteToPort(0xA1, 0x0);
			aOutput.WriteLine("sti");
		}

		public static void EmitHelperCode(StreamWriter aOutput) {
			int[] xInterruptsWithParam = new int[] { 8, 10, 11, 12, 13, 14 };
			for (int j = 0; j < 256; j++) {
				aOutput.WriteLine("__ISR_Handler_" + j.ToString("X2") + ":");
				aOutput.WriteLine("    cli");
				aOutput.WriteLine("    xchg bx, bx");
				aOutput.WriteLine("    pushad");
				if (!xInterruptsWithParam.Contains(j)) {
					aOutput.WriteLine("    pushd 0");
				}
				MethodDefinition xHandler = GetInterruptHandler((byte)j);
				if (xHandler == null) {
					xHandler = GetMethodDef(typeof(Interrupts).Assembly, typeof(Interrupts).FullName, "HandleInterrupt_Default", true);
					aOutput.WriteLine("    pushd 0x" + j.ToString("X"));
				}
				aOutput.WriteLine("    call " + Label.GenerateLabelName(xHandler));
				if (j >= 0x20 && j <= 0x2F) {
					if (j >= 0x28) {
						aOutput.WriteLine("mov dx, 0xA0");
						aOutput.WriteLine("mov ax, 0x20");
						aOutput.WriteLine("out dx, ax");
					}
					aOutput.WriteLine("mov dx, 0x20");
					aOutput.WriteLine("mov ax, 0x20");
					aOutput.WriteLine("out dx, ax");
				}
				aOutput.WriteLine("    popad");
				aOutput.WriteLine("    sti");
				aOutput.WriteLine("    iretd");
			}
		}
	}
}
