using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldloca)]
	public class Ldloca: Op {
		private string mAddress;
		private bool mIsReferenceTypeField;
		protected void SetLocalIndex(int aIndex, MethodInformation aMethodInfo) {
			mAddress = aMethodInfo.Locals[aIndex].VirtualAddress;
		}
		public Ldloca(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			int xLocalIndex;
			if (Int32.TryParse((aInstruction.Operand ?? "").ToString(), out xLocalIndex)) {
				SetLocalIndex(xLocalIndex, aMethodInfo);
				//
				return;
			}
			VariableDefinition xVarDef = aInstruction.Operand as VariableDefinition;
			if (xVarDef != null) {
				mIsReferenceTypeField = Engine.GetDefinitionFromTypeReference(xVarDef.VariableType).IsClass;
				SetLocalIndex(xVarDef.Index, aMethodInfo);
			}
		}

		public string Address {
			get {
				return mAddress;
			}
		}

		public sealed override void DoAssemble() {
			//if (mIsReferenceTypeField) {
			//	Move(Assembler, "eax", "[" + mAddress + "]");
			//	Push(Assembler, "eax");
			//} else {
				string[] xAddressParts = mAddress.Split('-');
				Move(Assembler, "edx", "ebp");
				Assembler.Add(new CPU.Sub("edx", xAddressParts[1]));
				Push(Assembler, "edx");
			//}
		}
	}
}