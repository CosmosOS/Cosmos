using System;
using System.IO;
using Indy.IL2CPU.Assembler;


using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldloc)]
	public class Ldloc: Op {
		private MethodInformation.Variable mLocal;
		protected void SetLocalIndex(int aIndex, MethodInformation aMethodInfo) {
			mLocal = aMethodInfo.Locals[aIndex];
		}
		public Ldloc(MethodInformation aMethodInfo, int aIndex)
			: base(null, aMethodInfo) {
			SetLocalIndex(aIndex, aMethodInfo);
		}

		public Ldloc(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			SetLocalIndex(aReader.OperandValueInt32, aMethodInfo);
			//VariableDefinition xVarDef = aReader.Operand as VariableDefinition;
			//if (xVarDef != null) {
			//    SetLocalIndex(xVarDef.Index, aMethodInfo);
			//}
		}

		public sealed override void DoAssemble() {
			Ldloc(Assembler, mLocal);
		}
	}
}