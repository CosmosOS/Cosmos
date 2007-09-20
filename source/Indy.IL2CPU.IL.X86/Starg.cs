using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Starg)]
	public class Starg: Op {
		private string mAddress;
		protected void SetArgIndex(int aIndex, MethodInformation aMethodInfo) {
			mAddress = aMethodInfo.Arguments[aIndex].VirtualAddress;
		}
		public Starg(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo, null) {
			ParameterDefinition xParam = aInstruction.Operand as ParameterDefinition;
			if (xParam != null) {
				SetArgIndex(xParam.Sequence - 1, aMethodInfo);
			}
		}
		public override void DoAssemble() {
			if (String.IsNullOrEmpty(mAddress)) {
				throw new Exception("No Address Specified!");
			}
			Pop("eax");
			Move(Assembler, "[" + mAddress + "]", "eax");
		}
	}
}