using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldarg)]
	public class Ldarg: Op {
		private string mAddress;
		protected void SetArgIndex(int aIndex, MethodInformation aMethodInfo) {
			mAddress = aMethodInfo.Arguments[aIndex].VirtualAddress;
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
		public string Address {
			get {
				return mAddress;
			}
		}

		public sealed override void DoAssemble() {
			Ldarg(Assembler, mAddress);
		}
	}
}