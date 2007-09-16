using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldsfld)]
	public class Ldsfld: Op {
		private bool IsIntPtrZero = false;
		public Ldsfld(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			IsIntPtrZero = aInstruction.Operand.ToString() == "System.IntPtr System.IntPtr::Zero";
		}
		public override void DoAssemble() {
			if(IsIntPtrZero) {
				Pushd("0");
				return;
			}
			throw new NotImplementedException();
		}
	}
}