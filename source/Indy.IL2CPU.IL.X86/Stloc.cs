using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Stloc)]
	public class Stloc: Op {
		private int mOffset;
		protected void SetLocalIndex(int aIndex, MethodInformation aMethodInfo) {
			mOffset = aMethodInfo.Locals[aIndex].Offset + aMethodInfo.Locals[aIndex].Size + 4;
		}
		public Stloc(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			int xLocalIndex;
			if (Int32.TryParse((aInstruction.Operand ?? "").ToString(), out xLocalIndex)) {
				SetLocalIndex(xLocalIndex, aMethodInfo);
			}
		}

		public int Offset {
			get {
				return mOffset;
			}
		}

		public sealed override void Assemble() {
			Pop("eax");
			Move("[esp+" + mOffset + "]", "eax");
		}
	}
}