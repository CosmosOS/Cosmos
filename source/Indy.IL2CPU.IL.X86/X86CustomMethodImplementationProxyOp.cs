using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	public class X86CustomMethodImplementationProxyOp: CustomMethodImplementationProxyOp {
		public X86CustomMethodImplementationProxyOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}

		protected override void Ldarg(int index) {
			Op.Ldarg(Assembler, MethodInfo.Arguments[index].VirtualAddress);
		}

		protected override void Ldfld(TypeInformation.Field aField) {
			Assembler.Add(new CPUx86.Pop("eax"));
			Assembler.Add(new CPUx86.Pushd("[eax " + aField.RelativeAddress + "]"));
		}

		protected override void CallProxiedMethod() {
			Op x = new Call(ProxiedMethod);
			x.Assembler = Assembler;
			x.Assemble();
		}

		protected override void Ldloc(int index) {
			Op x = new Ldloc(MethodInfo, index);
			x.Assembler = Assembler;
			x.Assemble();
		}
	}
}
