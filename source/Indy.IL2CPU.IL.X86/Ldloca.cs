using System;
using System.Linq;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldloca)]
	public class Ldloca: Op {
		private string mAddress;
		private bool mIsReferenceTypeField;
		protected void SetLocalIndex(int aIndex, MethodInformation aMethodInfo) {
			mAddress = aMethodInfo.Locals[aIndex].VirtualAddresses.LastOrDefault();
		}
		public Ldloca(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			int xLocalIndex;
			if (Int32.TryParse((aInstruction.Operand ?? "").ToString(), out xLocalIndex)) {
				SetLocalIndex(xLocalIndex, aMethodInfo);
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
			string[] xAddressParts = mAddress.Split('-');
			new CPUx86.Move(CPUx86.Registers.EDX, CPUx86.Registers.EBP);
			new CPUx86.Sub(CPUx86.Registers.EDX, xAddressParts[1]);
			new CPUx86.Push(CPUx86.Registers.EDX);
			Assembler.StackSizes.Push(4);
		}
	}
}