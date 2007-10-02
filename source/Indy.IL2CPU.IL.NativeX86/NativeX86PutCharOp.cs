using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.NativeX86 {
	public class NativeX86PutCharOp: IL.X86.Op {
		public MethodInformation MethodInformation;
		public NativeX86PutCharOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			MethodInformation = aMethodInfo;
		}

		private void AssembleOp(Op aOp) {
			aOp.Assembler = this.Assembler;
			aOp.Assemble();
		}

		public override void DoAssemble() {
			//(int aLine, int aPos, char aChar)
			Ldarg(mAssembler, MethodInformation.Arguments[0].VirtualAddress);
			Pushd("80");
			Multiply();
			Ldarg(mAssembler, MethodInformation.Arguments[1].VirtualAddress);
			Add();
			Pushd("2");
			Multiply();
			Pushd("0B8000h");
			Add();
			Ldarg(mAssembler, MethodInformation.Arguments[2].VirtualAddress);
			Pop("eax"); // character
			Assembler.Add(new IL2CPU.Assembler.X86.And("eax", "0FFFFFFFFh"));
			Pop("ecx");
			Assembler.Add(new CPUx86.Move("byte [ecx]", "al"));
			Assembler.Add(new CPUx86.Add("ecx", "1"));
			Assembler.Add(new CPUx86.Move("byte [ecx]", "7"));
		}
	}
}