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
		public Ldarg(Mono.Cecil.Cil.Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo, null) {
			int xArgIndex;
			if (Int32.TryParse((aInstruction.Operand ?? "").ToString(), out xArgIndex)) {
				SetArgIndex(xArgIndex, aMethodInfo);
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