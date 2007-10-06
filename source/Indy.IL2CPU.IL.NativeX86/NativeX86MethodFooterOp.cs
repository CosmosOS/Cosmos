using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.IL.X86;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.NativeX86 {
	public class NativeX86MethodFooterOp: X86MethodFooterOp {
		private string mLabelName;
		public NativeX86MethodFooterOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			mLabelName = aMethodInfo.LabelName;
		}

		public override void DoAssemble() {
			base.DoAssemble();
		}
	}
}