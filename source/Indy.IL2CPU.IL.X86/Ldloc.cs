using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldloc)]
	public class Ldloc: Op {
		private string[] mAddresses;
		private int mSize;
		protected void SetLocalIndex(int aIndex, MethodInformation aMethodInfo) {
			mAddresses = aMethodInfo.Locals[aIndex].VirtualAddresses;
			mSize = aMethodInfo.Locals[aIndex].Size;
		}
		public Ldloc(MethodInformation aMethodInfo, int aIndex):base(null, aMethodInfo) {
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

		public string[] Addresses {
			get {
				return mAddresses;
			}
		}

		public sealed override void DoAssemble() {
			foreach (string s in mAddresses) {
				Move(Assembler, "eax", "[" + s + "]");
				Assembler.Add(new CPU.Push("eax"));
			}
			Assembler.StackSizes.Push(mSize);
		}
	}
}