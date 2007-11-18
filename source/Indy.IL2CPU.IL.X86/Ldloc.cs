using System;
using System.IO;
using Indy.IL2CPU.Assembler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldloc)]
	public class Ldloc: Op {
		private MethodInformation.Variable mLocal;
		protected void SetLocalIndex(int aIndex, MethodInformation aMethodInfo) {
			mLocal = aMethodInfo.Locals[aIndex];
		}
		public Ldloc(MethodInformation aMethodInfo, int aIndex)
			: base(null, aMethodInfo) {
			SetLocalIndex(aIndex, aMethodInfo);
		}

		public Ldloc(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			int xLocalIndex;
			if (Int32.TryParse((aInstruction.Operand ?? "").ToString(), out xLocalIndex)) {
				SetLocalIndex(xLocalIndex, aMethodInfo);
			}
			VariableDefinition xVarDef = aInstruction.Operand as VariableDefinition;
			if (xVarDef != null) {
				SetLocalIndex(xVarDef.Index, aMethodInfo);
			}
		}

		public sealed override void DoAssemble() {
			Ldloc(Assembler, mLocal);
		}
	}
}