using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	public class X86InitVmtImplementationOp: InitVmtImplementationOp {
		public X86InitVmtImplementationOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}

		protected override void Pushd(string aValue) {
			new CPUx86.Pushd(aValue);
		}

		protected override void Call(MethodDefinition aMethod) {
			new CPUx86.Call(CPU.Label.GenerateLabelName(aMethod));
		}
	}
}
