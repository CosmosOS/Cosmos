using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Stloc)]
	public class Stloc: Op {
		private string mAddress;
		private bool mNeedsPop = true;
		protected void SetLocalIndex(int aIndex, MethodInformation aMethodInfo) {
			mAddress = aMethodInfo.Locals[aIndex].VirtualAddress;
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

			if(aInstruction.Previous != null &&
				(aInstruction.Previous.OpCode.Code == Code.Call ||
				aInstruction.Previous.OpCode.Code == Code.Calli ||
				aInstruction.Previous.OpCode.Code == Code.Callvirt)) {
				//mNeedsPop = false;
			}
		}

		public string Address {
			get {
				return mAddress;
			}
		}
		public bool NeedsPop {
			get {
				return mNeedsPop;
			}
		}

		public sealed override void Assemble() {
			if(String.IsNullOrEmpty(mAddress)) {
				throw new Exception("No address specified!");
			}
			if (mNeedsPop) {
				Pop("eax");
			}
			Move(Assembler, "[" + mAddress + "]", "eax");
		}
	}
}