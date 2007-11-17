using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldarga)]
	public class Ldarga: Op {
		private string mAddress;
		protected void SetArgIndex(int aIndex, MethodInformation aMethodInfo) {
			mAddress = aMethodInfo.Arguments[aIndex].VirtualAddresses.First();
		}
		public Ldarga(MethodInformation aMethodInfo, int aIndex)
			: base(null, aMethodInfo) {
			SetArgIndex(aIndex, aMethodInfo);
		}

		public Ldarga(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			if (aInstruction != null) {
				int xArgIndex;
				if (Int32.TryParse((aInstruction.Operand ?? "").ToString(), out xArgIndex)) {
					SetArgIndex(xArgIndex, aMethodInfo);
				}
				ParameterDefinition xParam = aInstruction.Operand as ParameterDefinition;
				if (xParam != null) {
					SetArgIndex(xParam.Sequence - 1, aMethodInfo);
				}
			}
		}
		public override void DoAssemble() {
			string[] mAddressParts = mAddress.Split('+');
			new CPU.Pushd(mAddressParts[0]);
			Assembler.StackSizes.Push(4);
			new CPU.Pushd(mAddressParts[1]);
			Assembler.StackSizes.Push(4);
			Add(Assembler);
		}
	}
}