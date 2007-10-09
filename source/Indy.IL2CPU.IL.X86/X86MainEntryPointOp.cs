using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL.X86 {
	public class X86MainEntryPointOp: MainEntryPointOp {
		public X86MainEntryPointOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}

		public override void Pushd(string aValue) {
			Assembler.Add(new CPUx86.Pushd(aValue));
		}

		public override void Call(MethodDefinition aMethod) {
			Assembler.Add(new CPUx86.Call(new CPU.Label(aMethod).Name));
			if(!aMethod.ReturnType.ReturnType.FullName.StartsWith("System.Void")) {
				Assembler.Add(new CPUx86.Pushd("eax"));
			}
		}

		public override void Enter(string aName) {
			X86MethodHeaderOp.AssembleHeader(Assembler, aName, new int[0]);
		}

		public override void Exit() {
			X86MethodFooterOp.AssembleFooter(0, Assembler, new int[0], 0);
		}
	}
}
