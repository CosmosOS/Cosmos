using System;
using Indy.IL2CPU.Assembler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Stloc)]
	public class Stloc: Op {
		private bool mNeedsGC = false;
		private MethodInformation.Variable mLocal;
		private string mBaseLabel;

		protected void SetLocalIndex(int aIndex, MethodInformation aMethodInfo) {
			mLocal = aMethodInfo.Locals[aIndex];
			mNeedsGC = aMethodInfo.Locals[aIndex].IsReferenceType;
			mNeedsGC &= aMethodInfo.Locals[aIndex].VariableType.FullName != "System.String";
		}

		public Stloc(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			int xLocalIndex;
			mBaseLabel = GetInstructionLabel(aInstruction);
			if (Int32.TryParse((aInstruction.Operand ?? "").ToString(), out xLocalIndex)) {
				SetLocalIndex(xLocalIndex, aMethodInfo);
			}
			VariableDefinition xVarDef = aInstruction.Operand as VariableDefinition;
			if (xVarDef != null) {
				SetLocalIndex(xVarDef.Index, aMethodInfo);
			}
		}

		public sealed override void DoAssemble() {
			if (mNeedsGC) {
				new CPUx86.Pushd("[" + mLocal.VirtualAddresses[0] + "]");				
				Engine.QueueMethodRef(GCImplementationRefs.DecRefCountRef);
				new CPUx86.Call(Label.GenerateLabelName(GCImplementationRefs.DecRefCountRef));
			}
			foreach (string s in mLocal.VirtualAddresses) {
				new CPUx86.Pop(CPUx86.Registers.EAX);
				new CPUx86.Move("[" + s + "]", "eax");
			}
			// no need to inc again, items on the transient stack are also counted
			Assembler.StackSizes.Pop();
		}
	}
}