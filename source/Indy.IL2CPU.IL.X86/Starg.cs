using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Starg)]
	public class Starg: Op {
		private string[] mAddresses;
		private int mSize;
		protected void SetArgIndex(int aIndex, MethodInformation aMethodInfo) {
			mAddresses = aMethodInfo.Arguments[aIndex].VirtualAddresses;
		}
		public Starg(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			ParameterDefinition xParam = aInstruction.Operand as ParameterDefinition;
			if (xParam != null) {
				SetArgIndex(xParam.Sequence - 1, aMethodInfo);
			}
		}
		public override void DoAssemble() {
			if (mAddresses == null || mAddresses.Length == 0) {
				throw new Exception("No Address Specified!");
			}
			for (int i = (mAddresses.Length - 1); i >= 0; i -= 1) {
				new CPU.Pop("eax");
				new CPU.Move("[" + mAddresses[i] + "]", "eax");
			}
			Assembler.StackSizes.Pop();
		}
	}
}