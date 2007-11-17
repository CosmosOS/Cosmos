using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86.Native {
	public class NativePutCharOp: IL.X86.Op {
		public MethodInformation MethodInformation;
		public NativePutCharOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			MethodInformation = aMethodInfo;
		}

		private void AssembleOp(IL.Op aOp) {
			aOp.Assembler = this.Assembler;
			aOp.Assemble();
		}
		private unsafe static void DoPutChar(int aLine, int aPos, byte aChar) {
			byte* xScreenPtr = (byte*)0xB8000;
			xScreenPtr += (aPos + (aLine * 80)) * 2;
			*xScreenPtr = aChar;
			xScreenPtr += 1;
			*xScreenPtr = 7;
		}

		protected void PassCall(MethodDefinition aMethod) {
			for (int i = 0; i < aMethod.Parameters.Count; i++) {
				Ldarg(Assembler, MethodInformation.Arguments[i].VirtualAddresses, MethodInformation.Arguments[i].Size);
			}
			DoQueueMethod(aMethod);
			new Indy.IL2CPU.Assembler.X86.Call(CPU.Label.GenerateLabelName(aMethod));
			if (!aMethod.ReturnType.ReturnType.FullName.StartsWith("System.Void")) {
				new Indy.IL2CPU.Assembler.X86.Pushd("eax");
			}
		}
		public override void DoAssemble() {
			TypeDefinition xType = null;
			AssemblyDefinition xAsm = AssemblyFactory.GetAssembly(typeof(NativePutCharOp).Assembly.Location);
			foreach (ModuleDefinition xMod in xAsm.Modules) {
				if (xMod.Types.Contains(typeof(NativePutCharOp).FullName)) {
					xType = xMod.Types[typeof(NativePutCharOp).FullName];
					break;
				}
			}
			if (xType == null) {
				throw new Exception("ArrayImpl type not found!");
			}
			PassCall(xType.Methods.GetMethod("DoPutChar")[0]);
			//(int aLine, int aPos, char aChar)
			//			Ldarg(mAssembler, MethodInformation.Arguments[0].VirtualAddress);
			//			Pushd("80");
			//			Multiply();
			//			Ldarg(mAssembler, MethodInformation.Arguments[1].VirtualAddress);
			//			Add();
			//			Pushd("2");
			//			Multiply();
			//			Pushd("0B8000h");
			//			Add();
			//			Ldarg(mAssembler, MethodInformation.Arguments[2].VirtualAddress);
			//			Pop("eax"); // character
			//			Assembler.Add(new IL2CPU.Assembler.X86.And("eax", "0FFFFFFFFh"));
			//			Pop("ecx");
			//			Assembler.Add(new CPUx86.Move("byte [ecx]", "al"));
			//			Assembler.Add(new CPUx86.Add("ecx", "1"));
			//			Assembler.Add(new CPUx86.Move("byte [ecx]", "7"));
		}
	}
}