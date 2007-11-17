using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Stloc)]
	public class Stloc: Op {
		private string[] mAddresses;
		private int mSize;

		protected void SetLocalIndex(int aIndex, MethodInformation aMethodInfo) {
			mAddresses = aMethodInfo.Locals[aIndex].VirtualAddresses;
			mSize = aMethodInfo.Locals[aIndex].Size;
		}
		public Stloc(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
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

		public string[] Addresses {
			get {
				return mAddresses;
			}
		}

		public sealed override void DoAssemble() {
			foreach (string s in mAddresses) {
				new CPU.Pop("eax");
				new CPU.Move("[" + s + "]", "eax");
			}
			Assembler.StackSizes.Pop();
		}
	}
}