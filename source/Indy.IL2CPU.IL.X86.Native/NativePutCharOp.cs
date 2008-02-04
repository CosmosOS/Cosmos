using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using System.Reflection;

namespace Indy.IL2CPU.IL.X86.Native {
	public class NativePutCharOp: IL.X86.Op {
		public MethodInformation MethodInformation;
		public NativePutCharOp(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
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

		protected void PassCall(MethodBase aMethod) {
			for (int i = 0; i < MethodInformation.Arguments.Length; i++) {
				Ldarg(Assembler, MethodInformation.Arguments[i]);
			}
			DoQueueMethod(aMethod);
			new Indy.IL2CPU.Assembler.X86.Call(CPU.Label.GenerateLabelName(aMethod));
			MethodInfo xMethodInfo = aMethod as MethodInfo;
			if (xMethodInfo != null) {
				if (!xMethodInfo.ReturnType.FullName.StartsWith("System.Void")) {
					new Indy.IL2CPU.Assembler.X86.Pushd("eax");
				}
			}
		}
		public override void DoAssemble() {
			Type xType = typeof(NativePutCharOp);
			PassCall(xType.GetMethod("DoPutChar"));
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