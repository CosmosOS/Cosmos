using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldarg)]
	public class Ldarg: Op {
		private string[] mAddresses;
		private int mSize;
		protected void SetArgIndex(int aIndex, MethodInformation aMethodInfo) {
			mAddresses = aMethodInfo.Arguments[aIndex].VirtualAddresses;
			mSize = aMethodInfo.Arguments[aIndex].Size;
		}
		public Ldarg(MethodInformation aMethodInfo, int aIndex):base(null, aMethodInfo) {
			SetArgIndex(aIndex, aMethodInfo);
		}

		public Ldarg(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			int xArgIndex;
			if (aInstruction != null) {
				if (Int32.TryParse((aInstruction.Operand ?? "").ToString(), out xArgIndex)) {
					SetArgIndex(xArgIndex, aMethodInfo);
				}
				ParameterDefinition xParam = aInstruction.Operand as ParameterDefinition;
				if(xParam!=null) {
					SetArgIndex(xParam.Sequence-1, aMethodInfo);
				}
			}
		}
		public string[] Addresses {
			get {
				return mAddresses;
			}
		}

		public override void DoAssemble() {
			Ldarg(Assembler, mAddresses, mSize);
		}
	}
}